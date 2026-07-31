using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Interfaces;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Application.Services;
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
    /// 1.08B — Cartão tokenizado (recorrente), boleto, checkout online real e recorrência anual/vitalícia.
    /// Todos os testes usam gateway MOCKADO (nunca a API real do Mercado Pago).
    /// </summary>
    public class CartaoBoletoCheckout108BTests
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
            public string? GetUserId() => _u; public string? GetUserName() => "User"; public string? GetUserEmail() => "u@epros.com";
        }
        private sealed class IdentityCofre : ISegredoCofreService
        {
            public Task<string> CriptografarAsync(string v) => Task.FromResult(v);
            public Task<string> DescriptografarAsync(string v) => Task.FromResult(v);
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

        /// <summary>Fake configurável do gateway (nunca chama a API real do MP).</summary>
        private sealed class FakeGateway : IPaymentGateway
        {
            public string CartaoStatus = "approved";
            public bool FalharCartao = false;
            public bool FalharCheckout = false;

            public Task<CommandResult> GerarCobrancaPixAsync(Fatura f, ConfiguracaoGatewayPagamento c, DadosPagador p, CancellationToken ct = default)
                => Task.FromResult(CommandResult.Ok("ok", new CobrancaPixResultado("pix-1", "qr", "qrb64", "ticket", DateTime.UtcNow.AddMinutes(30), "pending")));
            public Task<CommandResult> ConsultarPagamentoAsync(string paymentId, ConfiguracaoGatewayPagamento c, CancellationToken ct = default)
                => Task.FromResult(CommandResult.Ok("ok", new ConsultaPagamentoResultado(paymentId, "approved", 100m, 2m, DateTime.UtcNow, null, 98m)));
            public Task<CommandResult> TestarConexaoAsync(ConfiguracaoGatewayPagamento c, CancellationToken ct = default)
                => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> GerarBoletoAsync(Fatura f, ConfiguracaoGatewayPagamento c, DadosPagador p, DateTime venc, CancellationToken ct = default)
                => Task.FromResult(CommandResult.Ok("ok", new CobrancaBoletoResultado("boleto-1", "00190500954", "0339912345", venc, "https://mp/boleto.pdf", "pending")));
            public Task<CommandResult> CriarCartaoOnFileAsync(ConfiguracaoGatewayPagamento c, DadosPagador p, string token, CancellationToken ct = default)
                => Task.FromResult(CommandResult.Ok("ok", new CartaoOnFileResultado("cus_1", "card_1", "visa", "4242", 12, 2030)));
            public Task<CommandResult> CobrarCartaoAsync(Fatura f, ConfiguracaoGatewayPagamento c, string cus, string card, DadosPagador p, CancellationToken ct = default)
                => FalharCartao
                    ? Task.FromResult(CommandResult.Falha("gateway fora do ar"))
                    : Task.FromResult(CommandResult.Ok("ok", new ConsultaPagamentoResultado("cardpay-1", CartaoStatus, f.Valor, 3m, DateTime.UtcNow, f.Id.ToString(), f.Valor - 3m)));
            public Task<CommandResult> CriarPreferenciaCheckoutAsync(Fatura f, ConfiguracaoGatewayPagamento c, string d, DadosPagador p, string? u, CancellationToken ct = default)
                => FalharCheckout
                    ? Task.FromResult(CommandResult.Falha("sem checkout"))
                    : Task.FromResult(CommandResult.Ok("ok", new PreferenciaCheckoutResultado("pref-123", "https://mp/checkout/pref-123")));
        }

        private static ConfiguracaoGatewayPagamento GatewayGlobal(string userId)
            => new ConfiguracaoGatewayPagamento(EProvedorGateway.MercadoPago, EAmbienteGateway.Sandbox,
                "tok", null, "segredo", "BRL", null, null, true, userId);

        #endregion

        // ===== 1) Cartão tokenizado (on-file) =====

        [Fact]
        public async Task Adicionar_Cartao_Deve_Salvar_Somente_Token_E_Marcar_Padrao()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-cartao"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Pago", 100m, tenant, user); ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            ctx.ConfiguracoesGatewayPagamento.Add(GatewayGlobal(user));
            await ctx.SaveChangesAsync();

            var handler = new AdicionarCartaoCommandHandler(ctx, new TestTenantProvider(tenant), new TestCurrentUser(user), new FakeGateway());
            var r = await handler.Handle(new AdicionarCartaoCommand("CARD_TOKEN_DO_FRONT"), CancellationToken.None);

            Assert.True(r.Sucesso, string.Join(",", r.Erros ?? Array.Empty<string>()));
            var meio = await ctx.MeiosPagamentoClientes.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.ClienteId == cliente.Id);
            Assert.NotNull(meio);
            Assert.Equal("card_1", meio!.CardIdGateway);
            Assert.Equal("cus_1", meio.CustomerIdGateway);
            Assert.Equal("4242", meio.UltimosQuatro);
            Assert.True(meio.Padrao);
            // ⛔ PCI: a entidade NÃO tem campo de PAN/CVV — só metadados + identificadores opacos.
        }

        [Fact]
        public async Task Definir_Padrao_Deve_Zerar_Padrao_Anterior_E_Listar_E_Remover()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-cartao2"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Pago", 100m, tenant, user); ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            ctx.ConfiguracoesGatewayPagamento.Add(GatewayGlobal(user));
            await ctx.SaveChangesAsync();

            var tp = new TestTenantProvider(tenant); var cu = new TestCurrentUser(user);
            var add = new AdicionarCartaoCommandHandler(ctx, tp, cu, new FakeGateway());
            var r1 = await add.Handle(new AdicionarCartaoCommand("TOK1"), CancellationToken.None);
            var r2 = await add.Handle(new AdicionarCartaoCommand("TOK2"), CancellationToken.None);
            Assert.True(r1.Sucesso && r2.Sucesso);

            var lista = await new ListarMeiosPagamentoQueryHandler(ctx, tp).Handle(new ListarMeiosPagamentoQuery(), CancellationToken.None);
            Assert.Equal(2, lista.Count);
            Assert.Single(lista.Where(m => m.Padrao)); // só um padrão

            var primeiro = lista.First(m => !m.Padrao);
            var setPadrao = await new DefinirMeioPagamentoPadraoCommandHandler(ctx, tp, cu).Handle(new DefinirMeioPagamentoPadraoCommand(primeiro.Id), CancellationToken.None);
            Assert.True(setPadrao.Sucesso);
            var padroes = await ctx.MeiosPagamentoClientes.IgnoreQueryFilters().CountAsync(m => m.Padrao && m.Ativo);
            Assert.Equal(1, padroes);

            var rem = await new RemoverMeioPagamentoCommandHandler(ctx, tp, cu).Handle(new RemoverMeioPagamentoCommand(primeiro.Id), CancellationToken.None);
            Assert.True(rem.Sucesso);
            var ativos = await ctx.MeiosPagamentoClientes.IgnoreQueryFilters().CountAsync(m => m.Ativo);
            Assert.Equal(1, ativos);
        }

        [Fact]
        public async Task Cobranca_Recorrente_Cartao_Aprovada_Deve_Liquidar_Fatura()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-rec"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Pago", 100m, tenant, user); ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.AguardandoAprovacao,
                DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null, "Cartao", null, null, tenant, user);
            ctx.AssinaturasClientes.Add(assinatura);
            ctx.ConfiguracoesGatewayPagamento.Add(GatewayGlobal(user));
            var cartao = new MeioPagamentoCliente(cliente.Id, "cus_1", "card_1", "visa", "4242", 12, 2030, true, tenant, user);
            ctx.MeiosPagamentoClientes.Add(cartao);
            var fatura = new Fatura(cliente.Id, 100m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            var gw = new FakeGateway { CartaoStatus = "approved" };
            var rec = new CobrancaRecorrenteGatewayMercadoPago(ctx, gw, new FaturaLiquidacaoService(ctx));

            Assert.True(await rec.PossuiCartaoOnFileAsync(cliente.Id));
            var r = await rec.CobrarCartaoRecorrenteAsync(fatura, cliente.Id, CancellationToken.None);

            Assert.True(r.Sucesso, string.Join(",", r.Erros ?? Array.Empty<string>()));
            var faturaDb = await ctx.Faturas.IgnoreQueryFilters().FirstAsync(f => f.Id == fatura.Id);
            Assert.Equal(FaturaStatus.Paga, faturaDb.Status);
            var pag = await ctx.PagamentosFaturas.IgnoreQueryFilters().FirstAsync(p => p.FaturaId == fatura.Id);
            Assert.Equal("Cartao", pag.TipoPagamento);
            Assert.Equal(PagamentoFaturaStatus.Paid, pag.Status);
            var assinaturaDb = await ctx.AssinaturasClientes.IgnoreQueryFilters().FirstAsync(a => a.Id == assinatura.Id);
            Assert.Equal(AssinaturaStatus.Ativa, assinaturaDb.Status);
            Assert.NotNull(await ctx.RecibosPagamento.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.FaturaId == fatura.Id));
        }

        [Fact]
        public async Task Cobranca_Recorrente_Cartao_Recusada_Nao_Liquida()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-rec2"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Pago", 100m, tenant, user); ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            ctx.ConfiguracoesGatewayPagamento.Add(GatewayGlobal(user));
            ctx.MeiosPagamentoClientes.Add(new MeioPagamentoCliente(cliente.Id, "cus_1", "card_1", "visa", "4242", 12, 2030, true, tenant, user));
            var fatura = new Fatura(cliente.Id, 100m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            var gw = new FakeGateway { CartaoStatus = "rejected" };
            var rec = new CobrancaRecorrenteGatewayMercadoPago(ctx, gw, new FaturaLiquidacaoService(ctx));
            var r = await rec.CobrarCartaoRecorrenteAsync(fatura, cliente.Id, CancellationToken.None);

            Assert.False(r.Sucesso);
            var faturaDb = await ctx.Faturas.IgnoreQueryFilters().FirstAsync(f => f.Id == fatura.Id);
            Assert.NotEqual(FaturaStatus.Paga, faturaDb.Status); // cai na inadimplência
        }

        // ===== 2) Boleto =====

        [Fact]
        public async Task Gerar_Boleto_Deve_Persistir_Pagamento_Pendente_Com_Linha_Digitavel()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-bol"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", Guid.NewGuid(), tenant, user);
            ctx.Clientes.Add(cliente);
            ctx.ConfiguracoesGatewayPagamento.Add(GatewayGlobal(user));
            var fatura = new Fatura(cliente.Id, 150m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            var handler = new GerarBoletoCommandHandler(ctx, new TestTenantProvider(tenant), new TestCurrentUser(user), new FakeGateway());
            var r = await handler.Handle(new GerarBoletoCommand(fatura.Id), CancellationToken.None);

            Assert.True(r.Sucesso, string.Join(",", r.Erros ?? Array.Empty<string>()));
            var pag = await ctx.PagamentosFaturas.IgnoreQueryFilters().FirstAsync(p => p.FaturaId == fatura.Id && p.TipoPagamento == "Boleto");
            Assert.Equal(PagamentoFaturaStatus.Pending, pag.Status);
            Assert.Equal("00190500954", pag.LinhaDigitavel);
            Assert.False(string.IsNullOrEmpty(pag.UrlBoleto));
        }

        [Fact]
        public async Task Boleto_Deve_Conciliar_Pelo_Mesmo_Webhook_Unificado()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-bol2"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", Guid.NewGuid(), tenant, user);
            ctx.Clientes.Add(cliente);
            const string secret = "segredo";
            ctx.ConfiguracoesGatewayPagamento.Add(new ConfiguracaoGatewayPagamento(EProvedorGateway.MercadoPago, EAmbienteGateway.Sandbox,
                "tok", null, secret, "BRL", null, null, true, user));
            var fatura = new Fatura(cliente.Id, 150m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            // Emite o boleto (pending).
            await new GerarBoletoCommandHandler(ctx, new TestTenantProvider(tenant), new TestCurrentUser(user), new FakeGateway())
                .Handle(new GerarBoletoCommand(fatura.Id), CancellationToken.None);

            // Webhook do MP concilia o pagamento do boleto (approved, external_reference = fatura).
            var consulta = new WebhookFakeGateway("approved", 150m, 3.5m, fatura.Id.ToString(), 146.5m);
            var wh = new ProcessarWebhookPagamentoCommandHandler(ctx, consulta, new IdentityCofre());
            var cmd = BuildWebhook(secret, "boletopay-99", "payment.updated");
            var r = await wh.Handle(cmd, CancellationToken.None);

            Assert.True(r.Sucesso, string.Join(",", r.Erros ?? Array.Empty<string>()));
            var faturaDb = await ctx.Faturas.IgnoreQueryFilters().FirstAsync(f => f.Id == fatura.Id);
            Assert.Equal(FaturaStatus.Paga, faturaDb.Status);
            var recibo = await ctx.RecibosPagamento.IgnoreQueryFilters().FirstAsync(x => x.FaturaId == fatura.Id);
            Assert.Equal("Boleto", recibo.MeioPagamento);
        }

        // ===== 3) Recorrência anual/vitalícia + vínculo assinatura↔ciclo =====

        [Fact]
        public async Task Liquidacao_Deve_Ancorar_Proxima_Cobranca_Anual_Ao_Ativar()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-anual"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Anual", 1200m, null, 10, 2, null, tenant, user, duration: PlanoDuration.Anual);
            ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.AguardandoAprovacao,
                DateTime.UtcNow, DateTime.UtcNow.AddDays(30), null, "PIX", null, null, tenant, user);
            ctx.AssinaturasClientes.Add(assinatura);
            var fatura = new Fatura(cliente.Id, 1200m, DateTime.UtcNow.AddDays(5), tenant, user);
            ctx.Faturas.Add(fatura);
            await ctx.SaveChangesAsync();

            var recibo = await new FaturaLiquidacaoService(ctx).LiquidarAsync(fatura, "pix-1", "PIX", "MercadoPago", 1200m, null, null, DateTime.UtcNow, "sys", CancellationToken.None);
            await ctx.SaveChangesAsync();

            Assert.NotNull(recibo);
            var assinaturaDb = await ctx.AssinaturasClientes.IgnoreQueryFilters().FirstAsync(a => a.Id == assinatura.Id);
            Assert.NotNull(assinaturaDb.ProximaCobrancaEm);
            Assert.True(assinaturaDb.ProximaCobrancaEm > DateTime.UtcNow.AddMonths(11)); // ~1 ano
        }

        [Fact]
        public async Task Renovacao_Deve_Gerar_Fatura_Anual_E_Avancar_Ciclo_Idempotente()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-renov"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Anual", 1200m, null, 10, 2, null, tenant, user, duration: PlanoDuration.Anual);
            ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.Ativa,
                DateTime.UtcNow.AddYears(-1), DateTime.UtcNow.AddYears(-1).AddDays(30), null, "PIX", null, null, tenant, user);
            assinatura.DefinirProximaCobranca(DateTime.UtcNow.AddDays(-1), user); // ciclo vencido
            ctx.AssinaturasClientes.Add(assinatura);
            await ctx.SaveChangesAsync();

            var mediator = new TestMediator(_ => Task.FromResult<object>(CommandResult.Ok("pix ok")));
            var handler = new ProcessarRenovacaoAssinaturasCommandHandler(ctx, new TestCurrentUser(user), new CobrancaRecorrenteGatewayNoop(), mediator);

            var r1 = await handler.Handle(new ProcessarRenovacaoAssinaturasCommand(DateTime.UtcNow), CancellationToken.None);
            Assert.True(r1.Sucesso);
            Assert.Equal(1, await ctx.Faturas.CountAsync(f => f.ClienteId == cliente.Id));
            var assinaturaDb = await ctx.AssinaturasClientes.FindAsync(assinatura.Id);
            Assert.True(assinaturaDb!.ProximaCobrancaEm > DateTime.UtcNow.AddMonths(11)); // avançou ~1 ano

            // Idempotente: não gera fatura de novo no mesmo ciclo já avançado.
            var r2 = await handler.Handle(new ProcessarRenovacaoAssinaturasCommand(DateTime.UtcNow), CancellationToken.None);
            Assert.True(r2.Sucesso);
            Assert.Equal(1, await ctx.Faturas.CountAsync(f => f.ClienteId == cliente.Id));
        }

        [Fact]
        public async Task Renovacao_Nao_Deve_Cobrar_Plano_Vitalicio()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-vit"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var plano = new Plano("Vitalicio", 5000m, null, 10, 2, null, tenant, user, duration: PlanoDuration.Vitalicia);
            ctx.Planos.Add(plano);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", plano.Id, tenant, user);
            ctx.Clientes.Add(cliente);
            // Vitalícia: ProximaCobrancaEm permanece null → nunca entra na renovação.
            var assinatura = new AssinaturaCliente(cliente.Id, plano.Id, AssinaturaStatus.Ativa,
                DateTime.UtcNow, null, null, "PIX", null, null, tenant, user);
            ctx.AssinaturasClientes.Add(assinatura);
            await ctx.SaveChangesAsync();

            var handler = new ProcessarRenovacaoAssinaturasCommandHandler(ctx, new TestCurrentUser(user),
                new CobrancaRecorrenteGatewayNoop(), new TestMediator(_ => Task.FromResult<object>(CommandResult.Ok("ok"))));
            var r = await handler.Handle(new ProcessarRenovacaoAssinaturasCommand(DateTime.UtcNow), CancellationToken.None);

            Assert.True(r.Sucesso);
            Assert.Equal(0, await ctx.Faturas.CountAsync(f => f.ClienteId == cliente.Id));
        }

        // ===== 4) Checkout online REAL (sem stub) =====

        [Fact]
        public async Task Checkout_Deve_Criar_Preferencia_Real_E_Retornar_InitPoint()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-chk"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var tp = new TestTenantProvider(tenant); var cu = new TestCurrentUser(user);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", Guid.NewGuid(), tenant, user);
            ctx.Clientes.Add(cliente);
            var plano = new Plano("Pago", 299m, null, 10, 2, null, tenant, user);
            ctx.Planos.Add(plano);
            ctx.ConfiguracoesGatewayPagamento.Add(GatewayGlobal(user));
            await ctx.SaveChangesAsync();

            // Cria o pedido (gera a fatura AguardandoAprovacao).
            var criar = new Epros.Modules.Aplicativo.Application.Handlers.CriarPedidoSaaSCommandHandler(ctx, tp, cu);
            var rp = await criar.Handle(new Epros.Modules.Aplicativo.Application.Commands.CriarPedidoSaaSCommand(plano.Id, null, "Gateway"), CancellationToken.None);
            Assert.True(rp.Sucesso);
            var pedidoId = (Guid)rp.Dados.GetType().GetProperty("PedidoId")!.GetValue(rp.Dados)!;

            var handler = new Epros.Modules.Aplicativo.Application.Handlers.IniciarCheckoutCommandHandler(ctx, tp, cu, new FakeGateway());
            var r = await handler.Handle(new Epros.Modules.Aplicativo.Application.Commands.IniciarCheckoutCommand(pedidoId), CancellationToken.None);

            Assert.True(r.Sucesso, string.Join(",", r.Erros ?? Array.Empty<string>()));
            var url = (string)r.Dados.GetType().GetProperty("CheckoutUrl")!.GetValue(r.Dados)!;
            var pref = (string)r.Dados.GetType().GetProperty("PreferenceId")!.GetValue(r.Dados)!;
            Assert.Equal("https://mp/checkout/pref-123", url); // URL real do gateway, não mais stub epros.com
            Assert.Equal("pref-123", pref);
            var sessao = await ctx.SessoesPagamentos.IgnoreQueryFilters().FirstAsync(s => s.PedidoId == pedidoId);
            Assert.Equal("pref-123", sessao.GatewayRef);
        }

        [Fact]
        public async Task Checkout_Deve_Falhar_Sem_Gateway_Configurado()
        {
            var db = Guid.NewGuid().ToString(); var tenant = "t-chk2"; var user = "u";
            using var ctx = CreateContext(db, tenant, user);
            var tp = new TestTenantProvider(tenant); var cu = new TestCurrentUser(user);
            var cliente = new Cliente("Cli", "00.000.000/0001-00", "cli@epros.com", Guid.NewGuid(), tenant, user);
            ctx.Clientes.Add(cliente);
            var plano = new Plano("Pago", 299m, null, 10, 2, null, tenant, user);
            ctx.Planos.Add(plano);
            await ctx.SaveChangesAsync();
            // Sem ConfiguracaoGatewayPagamento → fail-closed.

            var criar = new Epros.Modules.Aplicativo.Application.Handlers.CriarPedidoSaaSCommandHandler(ctx, tp, cu);
            var rp = await criar.Handle(new Epros.Modules.Aplicativo.Application.Commands.CriarPedidoSaaSCommand(plano.Id, null, "Gateway"), CancellationToken.None);
            var pedidoId = (Guid)rp.Dados.GetType().GetProperty("PedidoId")!.GetValue(rp.Dados)!;

            var handler = new Epros.Modules.Aplicativo.Application.Handlers.IniciarCheckoutCommandHandler(ctx, tp, cu, new FakeGateway());
            var r = await handler.Handle(new Epros.Modules.Aplicativo.Application.Commands.IniciarCheckoutCommand(pedidoId), CancellationToken.None);

            Assert.False(r.Sucesso);
        }

        #region webhook helpers

        private static ProcessarWebhookPagamentoCommand BuildWebhook(string secret, string paymentId, string action)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var requestId = Guid.NewGuid().ToString();
            var header = MercadoPagoWebhookSignature.MontarHeader(secret, paymentId, requestId, ts);
            return new ProcessarWebhookPagamentoCommand(action, new WebhookData(paymentId), header, requestId);
        }

        private sealed class WebhookFakeGateway : IPaymentGateway
        {
            private readonly string _status; private readonly decimal _valor; private readonly decimal? _tarifa;
            private readonly string? _ext; private readonly decimal? _liquido;
            public WebhookFakeGateway(string status, decimal valor, decimal? tarifa, string? ext, decimal? liquido)
            { _status = status; _valor = valor; _tarifa = tarifa; _ext = ext; _liquido = liquido; }
            public Task<CommandResult> ConsultarPagamentoAsync(string paymentId, ConfiguracaoGatewayPagamento c, CancellationToken ct = default)
                => Task.FromResult(CommandResult.Ok("ok", new ConsultaPagamentoResultado(paymentId, _status, _valor, _tarifa, DateTime.UtcNow, _ext, _liquido)));
            public Task<CommandResult> GerarCobrancaPixAsync(Fatura f, ConfiguracaoGatewayPagamento c, DadosPagador p, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> TestarConexaoAsync(ConfiguracaoGatewayPagamento c, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> GerarBoletoAsync(Fatura f, ConfiguracaoGatewayPagamento c, DadosPagador p, DateTime v, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> CriarCartaoOnFileAsync(ConfiguracaoGatewayPagamento c, DadosPagador p, string t, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> CobrarCartaoAsync(Fatura f, ConfiguracaoGatewayPagamento c, string cus, string card, DadosPagador p, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
            public Task<CommandResult> CriarPreferenciaCheckoutAsync(Fatura f, ConfiguracaoGatewayPagamento c, string d, DadosPagador p, string? u, CancellationToken ct = default) => Task.FromResult(CommandResult.Ok("ok"));
        }

        #endregion
    }
}
