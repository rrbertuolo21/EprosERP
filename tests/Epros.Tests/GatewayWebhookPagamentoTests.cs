using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Handlers;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Domain.Services;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Estrutura de baixa por webhook (FIN-SF gap 1): registro de gateway, validacao de assinatura HMAC,
    /// idempotencia/dedup por (gateway x evento) e baixa da fatura pelo nosso numero. Nao chama provedor externo.
    /// </summary>
    public class GatewayWebhookPagamentoTests
    {
        private const string Tenant = "tenant-wh";
        private const string User = "user-wh";
        private const string Chave = "segredo-super-secreto";

        private static ContextFinanceiro Ctx(string db)
        {
            var options = new DbContextOptionsBuilder<ContextFinanceiro>().UseInMemoryDatabase(db).Options;
            var c = new ContextFinanceiro(options, new TP(Tenant), new CU(User));
            c.Database.EnsureCreated();
            return c;
        }

        // Cria gateway + sacado + fatura com nosso numero e devolve (gatewayId, nossoNumero).
        private static async Task<(Guid gateway, long nossoNumero)> Semear(string db)
        {
            using var ctx = Ctx(db);
            var gw = new GatewayPagamento(EProvedorPagamento.MercadoPago, "MP Teste", Chave, "app-1", Tenant, User);
            ctx.GatewaysPagamento.Add(gw);

            var sacado = new Sacado(null, null, "Cliente X", null, null, null, null, null, null, null, null, null, null, null, null, null, 0m, Tenant, User);
            ctx.Sacados.Add(sacado);
            await ctx.SaveChangesAsync();

            var fatura = new FaturaCobranca(sacado.Id, null, "REF-1", "DOC-1", DateTime.UtcNow, DateTime.UtcNow.AddDays(5), 250m, null, ETipoFaturaCobranca.Avulsa, Tenant, User);
            fatura.AtribuirBoleto(778899, Guid.NewGuid(), User); // seta NossoNumero
            ctx.FaturasCobranca.Add(fatura);
            await ctx.SaveChangesAsync();
            return (gw.Id, 778899);
        }

        private static ProcessarWebhookPagamentoCommandHandler Handler(ContextFinanceiro ctx)
            => new(ctx, new TP(Tenant), new CU(User));

        [Fact]
        public async Task Webhook_Valido_Baixa_Fatura_Por_Nosso_Numero()
        {
            const string db = nameof(Webhook_Valido_Baixa_Fatura_Por_Nosso_Numero);
            var (gateway, nn) = await Semear(db);
            const string payload = "{\"id\":\"evt-1\",\"nn\":778899}";
            var assinatura = ValidadorAssinaturaWebhook.Assinar(payload, Chave);

            using (var ctx = Ctx(db))
            {
                var r = await Handler(ctx).Handle(new ProcessarWebhookPagamentoCommand(
                    gateway, "evt-1", "payment.updated", nn, 250m, DateTime.UtcNow, payload, assinatura), CancellationToken.None);
                Assert.True(r.Sucesso);
            }

            using (var ctx = Ctx(db))
            {
                var fatura = await ctx.FaturasCobranca.FirstAsync(f => f.NossoNumero == nn);
                Assert.Equal(ESituacaoFaturaCobranca.Baixada, fatura.Situacao);
                var wh = await ctx.WebhooksPagamento.FirstAsync();
                Assert.Equal(EStatusWebhookPagamento.Processado, wh.Status);
                Assert.Contains(ctx.OutboxMessages, o => o.EventType == "fin.cobranca.webhook_processado");
            }
        }

        [Fact]
        public async Task Webhook_Duplicado_Nao_Reprocessa()
        {
            const string db = nameof(Webhook_Duplicado_Nao_Reprocessa);
            var (gateway, nn) = await Semear(db);
            const string payload = "{\"id\":\"evt-dup\"}";
            var assinatura = ValidadorAssinaturaWebhook.Assinar(payload, Chave);

            using (var ctx = Ctx(db))
                await Handler(ctx).Handle(new ProcessarWebhookPagamentoCommand(
                    gateway, "evt-dup", "payment.updated", nn, 250m, DateTime.UtcNow, payload, assinatura), CancellationToken.None);

            using (var ctx = Ctx(db))
            {
                var r = await Handler(ctx).Handle(new ProcessarWebhookPagamentoCommand(
                    gateway, "evt-dup", "payment.updated", nn, 250m, DateTime.UtcNow, payload, assinatura), CancellationToken.None);
                Assert.True(r.Sucesso); // idempotente
            }

            using (var ctx = Ctx(db))
                Assert.Equal(1, await ctx.WebhooksPagamento.CountAsync()); // dedup: um unico registro.
        }

        [Fact]
        public async Task Webhook_Assinatura_Invalida_Nao_Baixa()
        {
            const string db = nameof(Webhook_Assinatura_Invalida_Nao_Baixa);
            var (gateway, nn) = await Semear(db);

            using (var ctx = Ctx(db))
            {
                var r = await Handler(ctx).Handle(new ProcessarWebhookPagamentoCommand(
                    gateway, "evt-bad", "payment.updated", nn, 250m, DateTime.UtcNow, "{\"id\":\"evt-bad\"}", "assinatura-errada"), CancellationToken.None);
                Assert.False(r.Sucesso);
            }

            using (var ctx = Ctx(db))
            {
                var fatura = await ctx.FaturasCobranca.FirstAsync(f => f.NossoNumero == nn);
                Assert.NotEqual(ESituacaoFaturaCobranca.Baixada, fatura.Situacao);
                var wh = await ctx.WebhooksPagamento.FirstAsync();
                Assert.Equal(EStatusWebhookPagamento.AssinaturaInvalida, wh.Status);
            }
        }

        [Fact]
        public async Task Webhook_Fatura_Inexistente_Retorna_Falha()
        {
            const string db = nameof(Webhook_Fatura_Inexistente_Retorna_Falha);
            var (gateway, _) = await Semear(db);
            const string payload = "{\"id\":\"evt-nf\"}";
            var assinatura = ValidadorAssinaturaWebhook.Assinar(payload, Chave);

            using var ctx = Ctx(db);
            var r = await Handler(ctx).Handle(new ProcessarWebhookPagamentoCommand(
                gateway, "evt-nf", "payment.updated", 999999, 100m, DateTime.UtcNow, payload, assinatura), CancellationToken.None);
            Assert.False(r.Sucesso);
            var wh = await ctx.WebhooksPagamento.FirstAsync();
            Assert.Equal(EStatusWebhookPagamento.FaturaNaoLocalizada, wh.Status);
        }

        private sealed class TP : ITenantProvider
        {
            private readonly string _t; public TP(string t) => _t = t;
            public string GetTenantId() => _t;
        }
        private sealed class CU : ICurrentUser
        {
            private readonly string _u; public CU(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "wh";
            public string? GetUserEmail() => "wh@epros.local";
        }
    }
}
