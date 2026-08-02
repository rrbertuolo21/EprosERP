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

        // ======================================================================
        // T5 — MES MOVE ESTOQUE DE VERDADE: finalizar a ordem MES emite o evento
        // canônico prd.ordem.concluida no Outbox e o consumidor do Estoque baixa os
        // insumos e dá entrada do acabado pelo motor único (idempotente).
        // ======================================================================
        [Fact]
        public async Task Finalizar_Mes_Deve_Emitir_Evento_prd_ordem_concluida_No_Outbox()
        {
            var options = new DbContextOptionsBuilder<ContextProducao>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextProducao(options, tenantProvider, currentUser);

            var acabadoId = Guid.NewGuid();
            var insumoId = Guid.NewGuid();
            var estruturaId = Guid.NewGuid();

            var ordemId = await MontarOrdemMesAtivaAsync(context, tenantProvider, currentUser, acabadoId, insumoId, estruturaId, quantidadeProduzida: 5m, quantidadeComponente: 10m);

            var finalizar = new FinalizarMesOrdemCommandHandler(context, tenantProvider, currentUser);
            var res = await finalizar.Handle(new FinalizarMesOrdemCommand(ordemId, DateTime.UtcNow, Guid.NewGuid(), 200m, 0m, null, null), CancellationToken.None);
            Assert.True(res.Sucesso, string.Join(";", res.Erros));

            var outbox = await context.OutboxMessages.FirstOrDefaultAsync(m => m.EventType == "prd.ordem.concluida");
            Assert.NotNull(outbox);
            Assert.Contains(insumoId.ToString(), outbox!.Payload);
            Assert.Contains(acabadoId.ToString(), outbox.Payload);
        }

        [Fact]
        public async Task Consumidor_Estoque_Deve_Baixar_Insumo_E_Entrar_Acabado_Ao_Concluir_Mes_De_Forma_Idempotente()
        {
            // --- Producao: monta e finaliza a ordem MES, gerando o evento no outbox ---
            var optProd = new DbContextOptionsBuilder<ContextProducao>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var contextProd = new ContextProducao(optProd, tenantProvider, currentUser);

            // --- Estoque: insumo com saldo 50 @ custo 10; acabado com saldo 0 ---
            var optEst = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var contextEst = new ContextEstoque(optEst, tenantProvider, currentUser);
            var insumo = new Produto("MP-1", "Materia Prima 1", 0m, "tenant-1", "user-1");
            var acabado = new Produto("PA-1", "Produto Acabado 1", 0m, "tenant-1", "user-1");
            contextEst.Produtos.Add(insumo);
            contextEst.Produtos.Add(acabado);
            await contextEst.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(contextEst, "tenant-1", "user-1", insumo.Id, 50m, 10m);

            var ordemId = await MontarOrdemMesAtivaAsync(contextProd, tenantProvider, currentUser, acabado.Id, insumo.Id, Guid.NewGuid(), quantidadeProduzida: 5m, quantidadeComponente: 10m);
            var finalizar = new FinalizarMesOrdemCommandHandler(contextProd, tenantProvider, currentUser);
            var res = await finalizar.Handle(new FinalizarMesOrdemCommand(ordemId, DateTime.UtcNow, Guid.NewGuid(), 200m, 0m, null, null), CancellationToken.None);
            Assert.True(res.Sucesso, string.Join(";", res.Erros));
            var outbox = await contextProd.OutboxMessages.FirstAsync(m => m.EventType == "prd.ordem.concluida");

            // --- Consome o evento no Estoque (motor único) ---
            var consumer = new Epros.Modules.Estoque.Application.Outbox.MesOrdemConcluidaEstoqueConsumer(contextEst);
            await consumer.ConsumeAsync(outbox, CancellationToken.None);

            var insumoDepois = await contextEst.Produtos.FirstAsync(p => p.Id == insumo.Id);
            var acabadoDepois = await contextEst.Produtos.FirstAsync(p => p.Id == acabado.Id);
            Assert.Equal(40m, insumoDepois.SaldoEstoque);   // 50 - 10
            Assert.Equal(5m, acabadoDepois.SaldoEstoque);   // 5 fabricadas
            Assert.Equal(20m, acabadoDepois.CustoMedio);    // 100 / 5

            // --- ANTI-DUPLA-CONTAGEM: reprocessar a MESMA mensagem não move nada ---
            await consumer.ConsumeAsync(outbox, CancellationToken.None);
            var insumoRepetido = await contextEst.Produtos.FirstAsync(p => p.Id == insumo.Id);
            var acabadoRepetido = await contextEst.Produtos.FirstAsync(p => p.Id == acabado.Id);
            Assert.Equal(40m, insumoRepetido.SaldoEstoque); // permanece 40 (não baixou de novo)
            Assert.Equal(5m, acabadoRepetido.SaldoEstoque); // permanece 5 (não entrou de novo)
        }

        /// <summary>Cria uma ordem MES no estado Ativo (Rascunho->EmAnalise->Ativo), com item produzido e
        /// um componente de BOM apontando para o insumo — pronta para finalizar.</summary>
        private static async Task<Guid> MontarOrdemMesAtivaAsync(
            ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser,
            Guid acabadoId, Guid insumoId, Guid estruturaId, decimal quantidadeProduzida, decimal quantidadeComponente)
        {
            var criar = new CriarMesOrdemCommandHandler(context, tenantProvider, currentUser);
            var criarCmd = new CriarMesOrdemCommand(
                Guid.NewGuid(), "REF-1", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), estruturaId, acabadoId, null,
                200m, null, null,
                new List<CriarMesOrdemItemInput> { new(acabadoId, quantidadeProduzida, null, 0m) });
            var criarRes = await criar.Handle(criarCmd, CancellationToken.None);
            Assert.True(criarRes.Sucesso, string.Join(";", criarRes.Erros));

            var ordem = await context.MesOrdens.Include(o => o.Itens).FirstAsync();

            // Componente de BOM da estrutura ativa (o insumo a ser baixado).
            context.BomComponentes.Add(new BomComponente(estruturaId, insumoId, quantidadeComponente, "tenant-1", "user-1"));
            await context.SaveChangesAsync();

            // Registra produção no item (quantidade efetiva do acabado).
            var registrar = new RegistrarMesProducaoItemCommandHandler(context, currentUser);
            var itemId = ordem.Itens.First().Id;
            await registrar.Handle(new RegistrarMesProducaoItemCommand(itemId, quantidadeProduzida, quantidadeProduzida, 0m), CancellationToken.None);

            // Rascunho -> EmAnalise -> Ativo.
            await new SubmeterMesOrdemCommandHandler(context, tenantProvider, currentUser).Handle(new SubmeterMesOrdemCommand(ordem.Id), CancellationToken.None);
            await new AprovarMesOrdemCommandHandler(context, tenantProvider, currentUser).Handle(new AprovarMesOrdemCommand(ordem.Id), CancellationToken.None);

            return ordem.Id;
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
