using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Interfaces;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Gateways;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// 1.08E — Cupom de desconto RECORRENTE aplicado nas faturas do ciclo (renovação) + ESTORNO/refund
    /// de um PagamentoFatura (área Landlord/operador interno). Gateway sempre MOCKADO (nunca o MP real).
    /// </summary>
    public class CupomRecorrenteEstorno108ETests
    {
        #region Doubles

        private ContextGestaoClientes CreateContext(string db, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>().UseInMemoryDatabase(db).Options;
            return new ContextGestaoClientes(options, new TestTenantProvider(tenantId), new TestCurrentUser(userId));
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t; public TestTenantProvider(string t) => _t = t; public string GetTenantId() => _t;
        }
        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u; public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u; public string? GetUserName() => "Operador"; public string? GetUserEmail() => "op@epros.com";
        }
        private sealed class TestMediator : IMediator
        {
            private readonly Func<object, Task<object>> _h;
            public TestMediator(Func<object, Task<object>> h) => _h = h;
            public async Task<TResponse> Send<TResponse>(IRequest<TResponse> r, CancellationToken c = default) => (TResponse)await _h(r);
            public async Task Send<TRequest>(TRequest r, CancellationToken c = default) where TRequest : IRequest => await _h(r!);
            public async Task<object?> Send(object r, CancellationToken c = default) => await _h(r);
            public Task Publish(object n, CancellationToken c = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification n, CancellationToken c = default) where TNotification : INotification => Task.CompletedTask;
            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> r, CancellationToken c = default) => throw new NotImplementedException();
            public IAsyncEnumerable<object?> CreateStream(object r, CancellationToken c = default) => throw new NotImplementedException();
        }

        /// <summary>Fake do gateway que conta chamadas de estorno (idempotência) e permite forçar falha.</summary>
        private sealed class FakeGateway : IPaymentGateway
        {
            public int EstornosChamados = 0;
            public bool FalharEstorno = false;

            public Task<CommandResult> EstornarPagamentoAsync(string paymentId, ConfiguracaoGatewayPagamento c, decimal? valor, CancellationToken ct = default)
            {
                EstornosChamados++;
                return FalharEstorno
                    ? Task.FromResult(CommandResult.Falha("gateway recusou o estorno"))
                    : Task.FromResult(CommandResult.Ok("ok", new EstornoResultado("refund-1", paymentId, valor, "approved")));
            }

            public Task<CommandResult> GerarCobrancaPixAsync(Fatura f, ConfiguracaoGatewayPagamento c, DadosPagador p, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> ConsultarPagamentoAsync(string paymentId, ConfiguracaoGatewayPagamento c, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> TestarConexaoAsync(ConfiguracaoGatewayPagamento c, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> GerarBoletoAsync(Fatura f, ConfiguracaoGatewayPagamento c, DadosPagador p, DateTime v, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> CriarCartaoOnFileAsync(ConfiguracaoGatewayPagamento c, DadosPagador p, string t, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> CobrarCartaoAsync(Fatura f, ConfiguracaoGatewayPagamento c, string cus, string card, DadosPagador p, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> CriarPreferenciaCheckoutAsync(Fatura f, ConfiguracaoGatewayPagamento c, string d, DadosPagador p, string? u, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
        }

        private static ConfiguracaoGatewayPagamento GatewayGlobal(string userId)
            => new ConfiguracaoGatewayPagamento(EProvedorGateway.MercadoPago, EAmbienteGateway.Sandbox,
                "tok", null, "segredo", "BRL", null, null, true, userId);

        private ProcessarRenovacaoAssinaturasCommandHandler NovaRenovacao(ContextGestaoClientes ctx, string user)
            => new ProcessarRenovacaoAssinaturasCommandHandler(ctx, new TestCurrentUser(user),
                new CobrancaRecorrenteGatewayNoop(), new TestMediator(_ => Task.FromResult<object>(CommandResult.Ok("pix ok"))));

        #endregion

        // ===== 1) Cupom recorrente na fatura do ciclo =====

        [Fact]
        public async Task Renovacao_Deve_Aplicar_Cupom_Recorrente_Valido_E_Registrar_Uso()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-cuprec"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Mensal", 100m, tenant, user); ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            var cupom = new Cupom("RECORR10", "Percentual", 10m, null, null, tenant, user);
            ctx.Cupons.Add(cupom);
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.Ativa,
                DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(-1).AddDays(30), null, "PIX", null, null, tenant, user);
            assinatura.VincularCupom(cupom.Id, user);
            assinatura.DefinirProximaCobranca(DateTime.UtcNow.AddDays(-1), user); // ciclo vencido
            ctx.AssinaturasClientes.Add(assinatura);
            await ctx.SaveChangesAsync();

            var r = await NovaRenovacao(ctx, user).Handle(new ProcessarRenovacaoAssinaturasCommand(DateTime.UtcNow), CancellationToken.None);

            Assert.True(r.Sucesso, string.Join(",", r.Erros ?? Array.Empty<string>()));
            var fatura = await ctx.Faturas.IgnoreQueryFilters().FirstAsync(f => f.ClienteId == cliente.Id);
            Assert.Equal(90m, fatura.Valor); // 100 - 10%
            var uso = await ctx.UsosCupons.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.FaturaId == fatura.Id && u.CupomId == cupom.Id);
            Assert.NotNull(uso);
            Assert.Null(uso!.PedidoId);
            var cupomDb = await ctx.Cupons.IgnoreQueryFilters().FirstAsync(c => c.Id == cupom.Id);
            Assert.Equal(1, cupomDb.QuantidadeUsos);
        }

        [Fact]
        public async Task Renovacao_Nao_Deve_Aplicar_Cupom_Expirado()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-cupexp"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Mensal", 100m, tenant, user); ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            var cupom = new Cupom("EXPIRADO", "Percentual", 10m, null, DateTime.UtcNow.AddDays(-1), tenant, user);
            ctx.Cupons.Add(cupom);
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.Ativa,
                DateTime.UtcNow.AddMonths(-1), null, null, "PIX", null, null, tenant, user);
            assinatura.VincularCupom(cupom.Id, user);
            assinatura.DefinirProximaCobranca(DateTime.UtcNow.AddDays(-1), user);
            ctx.AssinaturasClientes.Add(assinatura);
            await ctx.SaveChangesAsync();

            var r = await NovaRenovacao(ctx, user).Handle(new ProcessarRenovacaoAssinaturasCommand(DateTime.UtcNow), CancellationToken.None);

            Assert.True(r.Sucesso);
            var fatura = await ctx.Faturas.IgnoreQueryFilters().FirstAsync(f => f.ClienteId == cliente.Id);
            Assert.Equal(100m, fatura.Valor); // sem desconto
            Assert.False(await ctx.UsosCupons.IgnoreQueryFilters().AnyAsync(u => u.FaturaId == fatura.Id));
            var cupomDb = await ctx.Cupons.IgnoreQueryFilters().FirstAsync(c => c.Id == cupom.Id);
            Assert.Equal(0, cupomDb.QuantidadeUsos);
        }

        [Fact]
        public async Task Renovacao_Nao_Deve_Aplicar_Cupom_No_Limite_De_Uso()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-cuplim"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Mensal", 100m, tenant, user); ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            var cupom = new Cupom("LIMITE1", "Fixo", 25m, 1, null, tenant, user);
            cupom.IncrementarUso(user); // já atingiu o limite (1/1)
            ctx.Cupons.Add(cupom);
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.Ativa,
                DateTime.UtcNow.AddMonths(-1), null, null, "PIX", null, null, tenant, user);
            assinatura.VincularCupom(cupom.Id, user);
            assinatura.DefinirProximaCobranca(DateTime.UtcNow.AddDays(-1), user);
            ctx.AssinaturasClientes.Add(assinatura);
            await ctx.SaveChangesAsync();

            var r = await NovaRenovacao(ctx, user).Handle(new ProcessarRenovacaoAssinaturasCommand(DateTime.UtcNow), CancellationToken.None);

            Assert.True(r.Sucesso);
            var fatura = await ctx.Faturas.IgnoreQueryFilters().FirstAsync(f => f.ClienteId == cliente.Id);
            Assert.Equal(100m, fatura.Valor);
            Assert.False(await ctx.UsosCupons.IgnoreQueryFilters().AnyAsync(u => u.FaturaId == fatura.Id));
        }

        // ===== 2) Estorno / refund =====

        private async Task<(Cliente cliente, Fatura fatura, PagamentoFatura pagamento, PedidoSaaS pedido)> SemearPagamentoLiquidado(
            ContextGestaoClientes ctx, string tenant, string user, bool comConfigGateway = true, string? paymentId = "pay-123", bool manual = false)
        {
            var plano = new Plano("Mensal", 100m, tenant, user); ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            cliente.AtualizarStatusSaaS(StatusSaaS.Ativo, user);
            ctx.Clientes.Add(cliente);
            if (comConfigGateway) ctx.ConfiguracoesGatewayPagamento.Add(GatewayGlobal(user));
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.Ativa,
                DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null, "PIX", null, null, tenant, user);
            ctx.AssinaturasClientes.Add(assinatura);
            var pedido = new PedidoSaaS(cliente.Id, plano.Id, null, 100m, 0m, "BRL", "PIX", tenant, user);
            pedido.Liquidar(assinatura.Id, user);
            ctx.PedidosSaaS.Add(pedido);
            var fatura = new Fatura(cliente.Id, 100m, DateTime.UtcNow.AddDays(5), tenant, user);
            fatura.Baixar(user);
            ctx.Faturas.Add(fatura);
            var pagamento = new PagamentoFatura(fatura.Id, "PIX", PagamentoFaturaStatus.Paid, 100m, 2m, paymentId, manual, DateTime.UtcNow, tenant, user);
            ctx.PagamentosFaturas.Add(pagamento);
            await ctx.SaveChangesAsync();
            return (cliente, fatura, pagamento, pedido);
        }

        private EstornarPagamentoFaturaCommandHandler NovoEstorno(ContextGestaoClientes ctx, string tenantOperador, string user, FakeGateway gw)
            => new EstornarPagamentoFaturaCommandHandler(ctx, new TestTenantProvider(tenantOperador), new TestCurrentUser(user), gw);

        [Fact]
        public async Task Estorno_Deve_Reverter_Pagamento_Fatura_Pedido_E_Enfileirar_Evento()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-estorno"; var user = "op";
            using var ctx = CreateContext(db, "system", user);
            var (cliente, fatura, pagamento, pedido) = await SemearPagamentoLiquidado(ctx, tenant, user);
            var gw = new FakeGateway();

            var r = await NovoEstorno(ctx, "system", user, gw).Handle(new EstornarPagamentoFaturaCommand(pagamento.Id), CancellationToken.None);

            Assert.True(r.Sucesso, string.Join(",", r.Erros ?? Array.Empty<string>()));
            Assert.Equal(1, gw.EstornosChamados);
            var pagDb = await ctx.PagamentosFaturas.IgnoreQueryFilters().FirstAsync(p => p.Id == pagamento.Id);
            Assert.Equal(PagamentoFaturaStatus.Refunded, pagDb.Status);
            Assert.Equal("refund-1", pagDb.IdentificadorEstorno);
            Assert.NotNull(pagDb.DataEstorno);
            var fatDb = await ctx.Faturas.IgnoreQueryFilters().FirstAsync(f => f.Id == fatura.Id);
            Assert.Equal(FaturaStatus.Estornada, fatDb.Status);
            Assert.False(fatDb.Quitada);
            var pedDb = await ctx.PedidosSaaS.IgnoreQueryFilters().FirstAsync(p => p.Id == pedido.Id);
            Assert.Equal(PedidoSaaSStatus.Refunded, pedDb.Status);
            var cliDb = await ctx.Clientes.IgnoreQueryFilters().FirstAsync(c => c.Id == cliente.Id);
            Assert.Equal(StatusSaaS.AguardandoPagamento, cliDb.StatusSaaS);
            Assert.Equal(1, await ctx.OutboxMessages.IgnoreQueryFilters().CountAsync(m => m.EventType == "PagamentoEstornadoEvent"));
        }

        [Fact]
        public async Task Estorno_Deve_Ser_Idempotente_Nao_Duplica()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-estorno-idem"; var user = "op";
            using var ctx = CreateContext(db, "system", user);
            var (_, _, pagamento, _) = await SemearPagamentoLiquidado(ctx, tenant, user);
            var gw = new FakeGateway();

            var r1 = await NovoEstorno(ctx, "system", user, gw).Handle(new EstornarPagamentoFaturaCommand(pagamento.Id), CancellationToken.None);
            var r2 = await NovoEstorno(ctx, "system", user, gw).Handle(new EstornarPagamentoFaturaCommand(pagamento.Id), CancellationToken.None);

            Assert.True(r1.Sucesso);
            Assert.True(r2.Sucesso); // idempotente
            Assert.Equal(1, gw.EstornosChamados); // segunda chamada NÃO toca o gateway
            Assert.Equal(1, await ctx.OutboxMessages.IgnoreQueryFilters().CountAsync(m => m.EventType == "PagamentoEstornadoEvent"));
        }

        [Fact]
        public async Task Estorno_Exige_Operador_Interno_1_11()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-estorno-sec"; var user = "op";
            using var ctx = CreateContext(db, "system", user);
            var (_, _, pagamento, _) = await SemearPagamentoLiquidado(ctx, tenant, user);
            var gw = new FakeGateway();

            // Operador NÃO interno (tenant de cliente) → proibido.
            var r = await NovoEstorno(ctx, tenant, user, gw).Handle(new EstornarPagamentoFaturaCommand(pagamento.Id), CancellationToken.None);

            Assert.False(r.Sucesso);
            Assert.Equal(0, gw.EstornosChamados);
            var pagDb = await ctx.PagamentosFaturas.IgnoreQueryFilters().FirstAsync(p => p.Id == pagamento.Id);
            Assert.Equal(PagamentoFaturaStatus.Paid, pagDb.Status); // não estornou
        }

        [Fact]
        public async Task Estorno_Sem_Gateway_Configurado_Faz_NoOp_Controlado_E_Reverte_Local()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-estorno-noop"; var user = "op";
            using var ctx = CreateContext(db, "system", user);
            // Sem ConfiguracaoGatewayPagamento → sem credencial/ambiente.
            var (_, fatura, pagamento, _) = await SemearPagamentoLiquidado(ctx, tenant, user, comConfigGateway: false);
            var gw = new FakeGateway();

            var r = await NovoEstorno(ctx, "system", user, gw).Handle(new EstornarPagamentoFaturaCommand(pagamento.Id), CancellationToken.None);

            Assert.True(r.Sucesso, string.Join(",", r.Erros ?? Array.Empty<string>()));
            Assert.Equal(0, gw.EstornosChamados); // no-op controlado: não chama o gateway sem credencial
            var pagDb = await ctx.PagamentosFaturas.IgnoreQueryFilters().FirstAsync(p => p.Id == pagamento.Id);
            Assert.Equal(PagamentoFaturaStatus.Refunded, pagDb.Status);
            Assert.Null(pagDb.IdentificadorEstorno); // sem refund id do gateway
            var fatDb = await ctx.Faturas.IgnoreQueryFilters().FirstAsync(f => f.Id == fatura.Id);
            Assert.Equal(FaturaStatus.Estornada, fatDb.Status);
        }

        [Fact]
        public async Task Estorno_Nao_Estorna_Pagamento_Nao_Liquidado()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-estorno-pend"; var user = "op";
            using var ctx = CreateContext(db, "system", user);
            var plano = new Plano("Mensal", 100m, tenant, user); ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            var fatura = new Fatura(cliente.Id, 100m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(fatura);
            var pagamento = new PagamentoFatura(fatura.Id, "PIX", PagamentoFaturaStatus.Pending, 100m, null, "pix-x", false, null, tenant, user);
            ctx.PagamentosFaturas.Add(pagamento);
            await ctx.SaveChangesAsync();
            var gw = new FakeGateway();

            var r = await NovoEstorno(ctx, "system", user, gw).Handle(new EstornarPagamentoFaturaCommand(pagamento.Id), CancellationToken.None);

            Assert.False(r.Sucesso);
            Assert.Equal(0, gw.EstornosChamados);
        }
    }
}
