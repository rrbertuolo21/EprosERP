using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// 1.08D — Mudança de plano self-service (upgrade/downgrade + proração pro-rata por dias) e
    /// cancelamento/reativação self-service governados. Testes de handler no provider InMemory,
    /// no mesmo padrão de AssinaturasPlanosTests.
    /// </summary>
    public class MudancaPlanoCancelamento108DTests
    {
        #region Helpers

        private static ContextGestaoClientes CreateGestao(string dbName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ContextGestaoClientes(options, new TestTenantProvider(tenantId), new TestCurrentUser(userId));
        }

        private static ContextAplicativo CreateApp(string dbName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ContextAplicativo(options, new TestTenantProvider(tenantId), new TestCurrentUser(userId));
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
            public string? GetUserName() => "User Test";
            public string? GetUserEmail() => "test@epros.com";
        }

        private static AssinaturaCliente NovaAssinaturaAtiva(Guid clienteId, Guid planoId, DateTime agora, string tenantId, string userId)
        {
            // Ciclo de 30 dias: 15 já corridos, 15 restantes.
            return new AssinaturaCliente(
                clienteId: clienteId,
                planoId: planoId,
                status: AssinaturaStatus.Ativa,
                dataInicio: agora.AddDays(-15),
                dataFim: agora.AddDays(15),
                trialAte: null,
                metodoPagamento: "PIX",
                transacaoId: "tx-1",
                detalhesPacoteJson: "{}",
                tenantId: tenantId,
                criadoPor: userId);
        }

        #endregion

        [Fact]
        public async Task Upgrade_Deve_Trocar_Plano_Gerar_Proracao_Debito_Fatura_Diferenca_E_Evento()
        {
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-upgrade";
            var userId = "user-upgrade";

            using var context = CreateGestao(dbName, tenantId, userId);

            var planoBasico = new Plano("Básico", 100.00m, tenantId, userId);
            var planoPremium = new Plano("Premium", 300.00m, tenantId, userId);
            context.Planos.AddRange(planoBasico, planoPremium);

            var cliente = new Cliente("Cliente Upgrade", "00.000.000/0001-00", "up@epros.com", planoBasico.Id,
                null, null, 10, StatusSaaS.Ativo, tenantId, userId);
            context.Clientes.Add(cliente);

            var agora = DateTime.UtcNow;
            var assinatura = NovaAssinaturaAtiva(cliente.Id, planoBasico.Id, agora, tenantId, userId);
            context.AssinaturasClientes.Add(assinatura);
            await context.SaveChangesAsync();

            var handler = new MudarPlanoCommandHandler(context, new TestTenantProvider(tenantId), new TestCurrentUser(userId));

            // Act
            var result = await handler.Handle(new MudarPlanoCommand(planoPremium.Id), CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso, string.Join(",", result.Erros ?? Array.Empty<string>()));

            var clienteDb = await context.Clientes.FindAsync(cliente.Id);
            Assert.Equal(planoPremium.Id, clienteDb!.PlanoId);

            var assinaturaDb = await context.AssinaturasClientes.FindAsync(assinatura.Id);
            Assert.Equal(planoPremium.Id, assinaturaDb!.PlanoId);

            var ajuste = await context.AjustesProracao.FirstOrDefaultAsync(a => a.AssinaturaClienteId == assinatura.Id);
            Assert.NotNull(ajuste);
            Assert.Equal(TipoAjusteProracao.Debito, ajuste!.Tipo);
            // (300 - 100) / 30 dias * 15 restantes = 100.00 (mecanismo pro-rata por dias; arredondamento default).
            Assert.Equal(100.00m, ajuste.ValorAjuste);
            Assert.Equal(15, ajuste.DiasRestantes);
            Assert.Equal(30, ajuste.DiasCiclo);
            Assert.NotNull(ajuste.FaturaId);

            // Fatura de diferença gerada com o valor do débito.
            var fatura = await context.Faturas.FirstOrDefaultAsync(f => f.Id == ajuste.FaturaId);
            Assert.NotNull(fatura);
            Assert.Equal(100.00m, fatura!.Valor);
            Assert.Equal(FaturaStatus.Pendente, fatura.Status);

            // DTO retornado marca valida-contador.
            var dto = Assert.IsType<MudancaPlanoResultadoDto>(result.Dados);
            Assert.True(dto.ValidaContador);
            Assert.Equal("Upgrade", dto.TipoMudanca);

            // Evento de notificação enfileirado no Outbox.
            var evento = await context.OutboxMessages.FirstOrDefaultAsync(o => o.EventType == "PlanoAlteradoEvent");
            Assert.NotNull(evento);
            Assert.Contains(planoPremium.Id.ToString(), evento!.Payload);
        }

        [Fact]
        public async Task Downgrade_Com_Uso_Acima_Do_Novo_Limite_Bloqueia_Novas_Criacoes_Mas_Nao_Apaga()
        {
            var dbName = Guid.NewGuid().ToString();
            var appDbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-downgrade";
            var userId = "user-downgrade";

            using var context = CreateGestao(dbName, tenantId, userId);
            using var appContext = CreateApp(appDbName, tenantId, userId);

            // Plano grande (5 usuários) → plano pequeno (1 usuário).
            var planoGrande = new Plano("Grande", 300.00m, null, 5, 2, null, tenantId, userId);
            var planoPequeno = new Plano("Pequeno", 100.00m, null, 1, 1, null, tenantId, userId);
            context.Planos.AddRange(planoGrande, planoPequeno);

            var cliente = new Cliente("Cliente Downgrade", "00.000.000/0001-00", "dg@epros.com", planoGrande.Id,
                null, null, 10, StatusSaaS.Ativo, tenantId, userId);
            context.Clientes.Add(cliente);

            var agora = DateTime.UtcNow;
            var assinatura = NovaAssinaturaAtiva(cliente.Id, planoGrande.Id, agora, tenantId, userId);
            context.AssinaturasClientes.Add(assinatura);
            await context.SaveChangesAsync();

            // Dois usuários ativos (acima do limite do plano pequeno).
            appContext.Usuarios.Add(new Usuario(tenantId, "Usuário 1", "u1@epros.com", "hash", UsuarioTipo.Company, userId));
            appContext.Usuarios.Add(new Usuario(tenantId, "Usuário 2", "u2@epros.com", "hash", UsuarioTipo.Company, userId));
            await appContext.SaveChangesAsync();

            var handler = new MudarPlanoCommandHandler(context, new TestTenantProvider(tenantId), new TestCurrentUser(userId));

            // Act: downgrade
            var result = await handler.Handle(new MudarPlanoCommand(planoPequeno.Id), CancellationToken.None);

            // Assert: troca ocorreu
            Assert.True(result.Sucesso, string.Join(",", result.Erros ?? Array.Empty<string>()));
            var clienteDb = await context.Clientes.FindAsync(cliente.Id);
            Assert.Equal(planoPequeno.Id, clienteDb!.PlanoId);

            // Proração de CRÉDITO (plano mais barato), sem fatura de diferença.
            var ajuste = await context.AjustesProracao.FirstOrDefaultAsync(a => a.AssinaturaClienteId == assinatura.Id);
            Assert.NotNull(ajuste);
            Assert.Equal(TipoAjusteProracao.Credito, ajuste!.Tipo);
            Assert.Equal(-100.00m, ajuste.ValorAjuste);
            Assert.Null(ajuste.FaturaId);
            Assert.Equal(0, await context.Faturas.CountAsync(f => f.ClienteId == cliente.Id));

            // Excedente NÃO apagado: os 2 usuários continuam.
            Assert.Equal(2, await appContext.Usuarios.CountAsync(u => u.TenantId == tenantId && u.DeletadoEm == null));

            // Reavaliação de entitlement: o ValidadorLimitesSaaS agora BLOQUEIA novas criações (2 >= 1).
            var validador = new ValidadorLimitesSaaS(appContext, context);
            var (excedido, msg) = await validador.ValidarLimiteUsuariosAsync(tenantId);
            Assert.True(excedido);
            Assert.Contains("limite de usuários", msg, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Cancelamento_Deve_Ir_Para_Cancelado_Registrar_Governanca_E_Enfileirar_Evento()
        {
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-cancel";
            var userId = "user-cancel";

            using var context = CreateGestao(dbName, tenantId, userId);

            var plano = new Plano("Plano", 200.00m, tenantId, userId);
            context.Planos.Add(plano);
            var cliente = new Cliente("Cliente Cancel", "00.000.000/0001-00", "cc@epros.com", plano.Id,
                null, null, 10, StatusSaaS.Ativo, tenantId, userId);
            context.Clientes.Add(cliente);
            var agora = DateTime.UtcNow;
            var assinatura = NovaAssinaturaAtiva(cliente.Id, plano.Id, agora, tenantId, userId);
            context.AssinaturasClientes.Add(assinatura);
            await context.SaveChangesAsync();

            var handler = new CancelarAssinaturaCommandHandler(context, new TestTenantProvider(tenantId), new TestCurrentUser(userId));

            // Act
            var result = await handler.Handle(new CancelarAssinaturaCommand("Custo alto"), CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso, string.Join(",", result.Erros ?? Array.Empty<string>()));

            var clienteDb = await context.Clientes.FindAsync(cliente.Id);
            Assert.Equal(StatusSaaS.Cancelado, clienteDb!.StatusSaaS);
            // Âncora da janela somente-leitura/export de 30 dias (REG-021) foi atualizada agora.
            Assert.NotNull(clienteDb.StatusSaaSAtualizadoEm);
            Assert.True((DateTime.UtcNow - clienteDb.StatusSaaSAtualizadoEm!.Value) < TimeSpan.FromMinutes(1));

            var assinaturaDb = await context.AssinaturasClientes.FindAsync(assinatura.Id);
            Assert.Equal(AssinaturaStatus.Cancelada, assinaturaDb!.Status);
            Assert.NotNull(assinaturaDb.CanceladaEm);
            Assert.Equal("Custo alto", assinaturaDb.MotivoCancelamento);
            Assert.Equal(userId, assinaturaDb.CanceladaPor);

            var evento = await context.OutboxMessages.FirstOrDefaultAsync(o => o.EventType == "AssinaturaCanceladaEvent");
            Assert.NotNull(evento);
            Assert.Contains("Custo alto", evento!.Payload);
        }

        [Fact]
        public async Task Reativacao_Dentro_Da_Janela_Deve_Voltar_Para_Ativo()
        {
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-reativa";
            var userId = "user-reativa";

            using var context = CreateGestao(dbName, tenantId, userId);

            var plano = new Plano("Plano", 200.00m, tenantId, userId);
            context.Planos.Add(plano);
            var cliente = new Cliente("Cliente Reativa", "00.000.000/0001-00", "rr@epros.com", plano.Id,
                null, null, 10, StatusSaaS.Ativo, tenantId, userId);
            context.Clientes.Add(cliente);
            var agora = DateTime.UtcNow;
            var assinatura = NovaAssinaturaAtiva(cliente.Id, plano.Id, agora, tenantId, userId);
            context.AssinaturasClientes.Add(assinatura);
            await context.SaveChangesAsync();

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // Cancela
            await new CancelarAssinaturaCommandHandler(context, tenantProvider, currentUser)
                .Handle(new CancelarAssinaturaCommand("teste"), CancellationToken.None);

            // Act: reativa dentro da janela
            var result = await new ReativarAssinaturaCommandHandler(context, tenantProvider, currentUser)
                .Handle(new ReativarAssinaturaCommand(), CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso, string.Join(",", result.Erros ?? Array.Empty<string>()));

            var clienteDb = await context.Clientes.FindAsync(cliente.Id);
            Assert.Equal(StatusSaaS.Ativo, clienteDb!.StatusSaaS);

            var assinaturaDb = await context.AssinaturasClientes.FindAsync(assinatura.Id);
            Assert.Equal(AssinaturaStatus.Ativa, assinaturaDb!.Status);
            Assert.Null(assinaturaDb.CanceladaEm);

            var evento = await context.OutboxMessages.FirstOrDefaultAsync(o => o.EventType == "AssinaturaReativadaEvent");
            Assert.NotNull(evento);
        }

        [Fact]
        public async Task Reativacao_Deve_Falhar_Se_Nao_Estiver_Cancelada()
        {
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-reativa-guard";
            var userId = "user-reativa-guard";

            using var context = CreateGestao(dbName, tenantId, userId);
            var plano = new Plano("Plano", 200.00m, tenantId, userId);
            context.Planos.Add(plano);
            var cliente = new Cliente("Cliente Ativo", "00.000.000/0001-00", "ag@epros.com", plano.Id,
                null, null, 10, StatusSaaS.Ativo, tenantId, userId);
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();

            var result = await new ReativarAssinaturaCommandHandler(context, new TestTenantProvider(tenantId), new TestCurrentUser(userId))
                .Handle(new ReativarAssinaturaCommand(), CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("não está cancelada", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Mudar_Plano_Deve_Falhar_Se_Ja_Estiver_No_Plano()
        {
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-mesmo-plano";
            var userId = "user-mesmo-plano";

            using var context = CreateGestao(dbName, tenantId, userId);
            var plano = new Plano("Plano", 200.00m, tenantId, userId);
            context.Planos.Add(plano);
            var cliente = new Cliente("Cliente", "00.000.000/0001-00", "mp@epros.com", plano.Id,
                null, null, 10, StatusSaaS.Ativo, tenantId, userId);
            context.Clientes.Add(cliente);
            var agora = DateTime.UtcNow;
            context.AssinaturasClientes.Add(NovaAssinaturaAtiva(cliente.Id, plano.Id, agora, tenantId, userId));
            await context.SaveChangesAsync();

            var result = await new MudarPlanoCommandHandler(context, new TestTenantProvider(tenantId), new TestCurrentUser(userId))
                .Handle(new MudarPlanoCommand(plano.Id), CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("já está neste plano", StringComparison.OrdinalIgnoreCase));
        }
    }
}
