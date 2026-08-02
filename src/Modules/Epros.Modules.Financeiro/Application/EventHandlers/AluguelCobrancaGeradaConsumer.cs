using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Outbox;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.EventHandlers
{
    /// <summary>
    /// TRANSVERSAL T1 — Consumidor REAL do evento <see cref="CatalogoEventosIntegracao.Imobiliaria.AluguelCobrancaGerada"/>
    /// (<c>imo.aluguel.cobranca_gerada</c>) publicado pelo módulo Imobiliária no Outbox.
    ///
    /// Origina o título em CONTAS A RECEBER (modelo FIEL: ContasAReceber + FatoGeradorFinanceiro +
    /// PlanoDeContasItem padrão) — o recebível vive no Financeiro (NF-01), não na Imobiliária.
    /// Idempotente por (tenant + cobrança) via <see cref="ContasAReceber.Documento"/> determinístico.
    ///
    /// NÃO governa juros/multa/desconto nem a baixa (esses são refletidos de volta por eventos próprios);
    /// aqui apenas cria o título em aberto com o vencimento informado na cobrança.
    /// </summary>
    public class AluguelCobrancaGeradaConsumer : IOutboxConsumer
    {
        private readonly ContextFinanceiro _context;

        public AluguelCobrancaGeradaConsumer(ContextFinanceiro context) => _context = context;

        public string EventType => CatalogoEventosIntegracao.Imobiliaria.AluguelCobrancaGerada;

        public async Task ConsumeAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            var payload = JsonSerializer.Deserialize<AluguelCobrancaGeradaPayload>(
                message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (payload == null || payload.CobrancaId == Guid.Empty)
                return;

            var tenantId = string.IsNullOrWhiteSpace(payload.TenantId) ? message.TenantId : payload.TenantId;

            // Documento determinístico (máx. 30 chars) → âncora de idempotência.
            var documento = $"ALG{payload.CobrancaId:N}".Substring(0, 30);

            var jaProcessado = await _context.ContasAReceberAgregado
                .IgnoreQueryFilters()
                .AnyAsync(cr => cr.TenantId == tenantId && cr.Documento == documento, cancellationToken);

            if (jaProcessado)
                return;

            // Resolve/garante um cliente para o tenant (Cliente Balcão determinístico) — mesma
            // convenção do consumidor de Venda; a vinculação fina do locatário fica a cargo do
            // fechamento posterior (valida-negócio), sem travar a origem do título.
            var cliente = await _context.PessoasLookup
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.EhCliente, cancellationToken);

            Guid clienteId;
            if (cliente != null)
            {
                clienteId = cliente.Id;
            }
            else
            {
                clienteId = FinanceiroFielFactory.GetDeterministicGuid($"DefaultCliente_{tenantId}");
                var clienteExiste = await _context.PessoasLookup
                    .IgnoreQueryFilters()
                    .AnyAsync(p => p.Id == clienteId, cancellationToken);

                if (!clienteExiste)
                {
                    _context.PessoasLookup.Add(new PessoaLookup
                    {
                        Id = clienteId,
                        EhCliente = true,
                        Status = 3,
                        TipoPessoa = 1,
                        TenantId = tenantId,
                        CriadoPor = "system_outbox",
                        CriadoEm = DateTime.UtcNow
                    });
                    _context.PessoasFisicasLookup.Add(new PessoaFisicaLookup
                    {
                        PessoaId = clienteId,
                        Cpf = "00000000000",
                        Nome = "Cliente Balcão",
                        TenantId = tenantId,
                        CriadoPor = "system_outbox",
                        CriadoEm = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            var planoItemId = await FinanceiroFielFactory.GarantirPlanoItemPadraoAsync(
                _context, tenantId, "system_outbox", cancellationToken);

            var competencia = ParseCompetencia(payload.Competencia);
            var vencimento = CalcularVencimento(competencia, payload.VencimentoDia);

            var fato = FinanceiroFielFactory.CriarFatoGerador(
                _context, EOrigem.ManualContasAReceber, null, null,
                $"Aluguel {payload.Competencia} (cobrança {payload.CobrancaId.ToString().Substring(0, 8)})",
                tenantId, "system_outbox");

            var conta = new ContasAReceber(
                pessoaId: clienteId,
                planoDeContasFinanceiroItemId: planoItemId,
                fatoGeradorFinanceiroId: fato.Id,
                nomePessoa: cliente == null ? "Cliente Balcão" : null,
                situacao: ESituacao.Aberto,
                dataVencimento: vencimento,
                dataEmissao: competencia,
                dataBaixa: null,
                documento: documento,
                valorTitulo: payload.Valor,
                valorInicialDesconto: 0m,
                valorInicialMulta: 0m,
                valorInicialJuros: 0m,
                valorInicialAcrescimo: 0m,
                numeroParcela: 1,
                detalhamento: $"Gerada automaticamente a partir da cobrança de aluguel {payload.CobrancaId} (locação {payload.LocacaoId}).",
                justificativaCancelamento: null,
                tenantId: tenantId,
                criadoPor: "system_outbox");

            if (!conta.IsValid)
                return;

            _context.ContasAReceberAgregado.Add(conta);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static DateTime ParseCompetencia(string? competencia)
        {
            if (!string.IsNullOrWhiteSpace(competencia)
                && DateTime.TryParse(competencia + "-01", out var dt))
                return dt.Date;
            return DateTime.UtcNow.Date;
        }

        private static DateTime CalcularVencimento(DateTime competencia, int? dia)
        {
            var d = dia is > 0 and <= 31 ? dia.Value : competencia.Day;
            var maxDia = DateTime.DaysInMonth(competencia.Year, competencia.Month);
            if (d > maxDia) d = maxDia;
            return new DateTime(competencia.Year, competencia.Month, d);
        }

        private class AluguelCobrancaGeradaPayload
        {
            public Guid CobrancaId { get; set; }
            public Guid LocacaoId { get; set; }
            public Guid ImovelId { get; set; }
            public string? Competencia { get; set; }
            public string? Tipo { get; set; }
            public decimal Valor { get; set; }
            public int? VencimentoDia { get; set; }
            public string? TenantId { get; set; }
        }
    }
}
