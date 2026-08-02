using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Application.Handlers;
using Epros.Modules.Producao.Application.Queries;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class ProducaoModuleTests
    {
        [Fact]
        public async Task Deve_Criar_Ficha_Tecnica_BOM_E_Inativar_Anteriores()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextProducao>()
                .UseInMemoryDatabase("db_producao_bom")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextProducao(options, tenantProvider, currentUser);

            // Criar primeira lista
            var lista1 = new ListaMateriais("PROD-ACABADO", "Lista 1", "1.0", "tenant-1", "user-1");
            context.ListasMateriais.Add(lista1);
            await context.SaveChangesAsync();

            var handler = new CriarListaMateriaisCommandHandler(context, tenantProvider, currentUser);
            var items = new List<CriarListaMateriaisItem>
            {
                new("INSUMO-001", 2.5m, "KG"),
                new("INSUMO-002", 1.0m, "UN")
            };
            var command = new CriarListaMateriaisCommand("PROD-ACABADO", "Lista 2", "2.0", items);

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var listas = await context.ListasMateriais.Include(l => l.Itens).ToListAsync();
            Assert.Equal(2, listas.Count);

            var antiga = listas.First(l => l.Versao == "1.0");
            Assert.False(antiga.Ativa);

            var nova = listas.First(l => l.Versao == "2.0");
            Assert.True(nova.Ativa);
            Assert.Equal(2, nova.Itens.Count);
            Assert.Contains(nova.Itens, i => i.InsumoSku == "INSUMO-001" && i.QuantidadeNecessaria == 2.5m);
        }

        [Fact]
        public async Task Deve_Falhar_Criar_OP_Sem_BOM()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextProducao>()
                .UseInMemoryDatabase("db_producao_op_sem_bom")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextProducao(options, tenantProvider, currentUser);

            var handler = new AbrirOrdemProducaoCommandHandler(context, tenantProvider, currentUser);
            var command = new AbrirOrdemProducaoCommand("OP-123", "PROD-ACABADO-INEXISTENTE", 10.0m);

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.False(result.Sucesso);
            Assert.Contains("não há nenhuma ficha técnica (BOM) ativa cadastrada", result.Erros.FirstOrDefault() ?? "");
        }

        [Fact]
        public async Task Deve_Criar_OP_Iniciar_E_Apontar_Producao()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextProducao>()
                .UseInMemoryDatabase("db_producao_op_apontamento")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextProducao(options, tenantProvider, currentUser);

            // Criar BOM ativa
            var bom = new ListaMateriais("PROD-ACABADO", "Lista do Produto", "1.0", "tenant-1", "user-1");
            context.ListasMateriais.Add(bom);
            await context.SaveChangesAsync();

            var abrirHandler = new AbrirOrdemProducaoCommandHandler(context, tenantProvider, currentUser);
            var abrirResult = await abrirHandler.Handle(new AbrirOrdemProducaoCommand("OP-2026-01", "PROD-ACABADO", 100m), CancellationToken.None);
            Assert.True(abrirResult.Sucesso);

            var opId = (Guid)abrirResult.Dados!.GetType().GetProperty("OrdemProducaoId")!.GetValue(abrirResult.Dados)!;

            var iniciarHandler = new IniciarProducaoCommandHandler(context, currentUser);
            var iniciarResult = await iniciarHandler.Handle(new IniciarProducaoCommand(opId), CancellationToken.None);
            Assert.True(iniciarResult.Sucesso);

            var apontarHandler = new RegistrarApontamentoCommandHandler(context, currentUser);

            // Agir
            var apontarResult1 = await apontarHandler.Handle(new RegistrarApontamentoCommand(opId, 40m, 5m, "Operador A"), CancellationToken.None);
            var apontarResult2 = await apontarHandler.Handle(new RegistrarApontamentoCommand(opId, 50m, 2m, "Operador B"), CancellationToken.None);

            // Assertiva
            Assert.True(apontarResult1.Sucesso);
            Assert.True(apontarResult2.Sucesso);

            var opFinal = await context.OrdensProducao.Include(o => o.Apontamentos).FirstOrDefaultAsync(o => o.Id == opId);
            Assert.NotNull(opFinal);
            Assert.Equal("EmProducao", opFinal!.Status);
            Assert.Equal(90m, opFinal.QuantidadeProduzida);
            Assert.Equal(7m, opFinal.QuantidadeRefugada);
            Assert.Equal(2, opFinal.Apontamentos.Count);
        }

        [Fact]
        public async Task Deve_Encerrar_OP_E_Gerar_Outbox_Message()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextProducao>()
                .UseInMemoryDatabase("db_producao_op_encerrar")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextProducao(options, tenantProvider, currentUser);

            // Criar BOM ativa
            var bom = new ListaMateriais("PROD-ACABADO", "Lista do Produto", "1.0", "tenant-1", "user-1");
            bom.AdicionarItem("INSUMO-1", 1.5m, "KG", "user-1");
            context.ListasMateriais.Add(bom);

            // Criar OP e apontamentos
            var op = new OrdemProducao("OP-X", "PROD-ACABADO", 10m, "tenant-1", "user-1");
            op.IniciarProducao("user-1");
            op.RegistrarApontamento(8m, 2m, "Operador 1", "user-1"); // total fabricado = 10m
            context.OrdensProducao.Add(op);
            await context.SaveChangesAsync();

            var handler = new EncerrarOrdemProducaoCommandHandler(context, tenantProvider, currentUser);

            // Agir
            var result = await handler.Handle(new EncerrarOrdemProducaoCommand(op.Id), CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var opEncerrada = await context.OrdensProducao.FindAsync(op.Id);
            Assert.Equal("Encerrada", opEncerrada!.Status);
            Assert.NotNull(opEncerrada.DataFim);

            var outboxMsg = await context.OutboxMessages.FirstOrDefaultAsync();
            Assert.NotNull(outboxMsg);
            Assert.Equal("OrdemProducaoEncerrada", outboxMsg!.EventType);
            Assert.Contains("INSUMO-1", outboxMsg.Payload);
            // Quantidade Consumida = 1.5 (BOM) * 10 (Produzida + Refugada) = 15m
            Assert.Contains("\"QuantidadeConsumida\":15", outboxMsg.Payload);
        }

        [Fact]
        public async Task Deve_Atualizar_Estoques_E_Custos_Ao_Processar_OrdemProducaoEncerrada()
        {
            // Organizar
            var optionsEstoque = new DbContextOptionsBuilder<ContextEstoque>()
                .UseInMemoryDatabase("db_estoque_producao_integracao")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var contextEstoque = new ContextEstoque(optionsEstoque, tenantProvider, currentUser);

            // Cadastrar insumo no estoque com saldo 50 e custo médio 10
            var insumo = new Produto("INSUMO-1", "Materia Prima 1", 0m, "tenant-1", "user-1");
            contextEstoque.Produtos.Add(insumo);

            // Cadastrar produto acabado no estoque com saldo 0
            var produtoAcabado = new Produto("PROD-ACABADO", "Produto Acabado", 50m, "tenant-1", "user-1");
            contextEstoque.Produtos.Add(produtoAcabado);

            await contextEstoque.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(contextEstoque, "tenant-1", "user-1", insumo.Id, 50m, 10m);

            var handler = new OrdemProducaoEncerradaEstoqueHandler(contextEstoque);

            // Consumido 10 unidades do INSUMO-1 para fabricar 5 unidades do PROD-ACABADO
            var insumosConsumidos = new List<InsumoConsumidoNotification>
            {
                new("INSUMO-1", 10m)
            };

            var notification = new OrdemProducaoEncerradaEventNotification(
                OrdemProducaoId: Guid.NewGuid(),
                Codigo: "OP-XYZ",
                ProdutoAcabadoSku: "PROD-ACABADO",
                QuantidadeProduzida: 5m,
                QuantidadeRefugada: 1m,
                TenantId: "tenant-1",
                InsumosConsumidos: insumosConsumidos
            );

            // Agir
            await handler.Handle(notification, CancellationToken.None);

            // Assertiva
            var insumoAtualizado = await contextEstoque.Produtos.FirstOrDefaultAsync(p => p.Sku == "INSUMO-1");
            Assert.Equal(40m, insumoAtualizado!.SaldoEstoque); // Baixou 10 unidades (50 - 10)

            var acabadoAtualizado = await contextEstoque.Produtos.FirstOrDefaultAsync(p => p.Sku == "PROD-ACABADO");
            Assert.Equal(5m, acabadoAtualizado!.SaldoEstoque); // Recebeu 5 unidades fabricadas
            // Custo Total Consumido = 10 unidades * R$ 10 (custo do insumo) = R$ 100
            // Custo Unitário do Acabado = R$ 100 / 5 unidades = R$ 20
            Assert.Equal(20m, acabadoAtualizado.CustoMedio);
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
