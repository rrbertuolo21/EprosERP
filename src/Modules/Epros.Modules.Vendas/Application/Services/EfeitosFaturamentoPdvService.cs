using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Services
{
    /// <summary>
    /// TEMA T3 — Liga os EFEITOS REAIS do faturamento de uma venda de PDV (caminho fiscal-interativo).
    /// Quando a venda é transmitida/faturada, este serviço, em UMA unidade de trabalho do
    /// <see cref="ContextVendas"/> (o chamador dá o SaveChanges):
    ///
    ///   1) CREDITA o caixa por FORMA DE PAGAMENTO — grava um <see cref="CaixaMovimento"/> (Tipo "Venda")
    ///      por forma da venda (quebra Dinheiro/Cartão/Pix… que faltava). Não soma ao total físico
    ///      Suprimento/Sangria (o fechamento já contabiliza a venda pelo Total — anti-dupla-contagem).
    ///   2) ENFILEIRA o evento <c>VendaFaturada</c> no Outbox de Vendas (mesmo contrato do fluxo de
    ///      sincronização), de onde o <c>VendasOutboxProcessorJob</c> despacha para os consumidores REAIS:
    ///      Financeiro (título em Contas a Receber) e Estoque (baixa pelo MOTOR ÚNICO).
    ///
    /// IDEMPOTENTE: não reenfileira <c>VendaFaturada</c> nem recria o movimento de caixa por forma se já
    /// existirem para a mesma venda (re-transmissão segura). Os efeitos a jusante (estoque/financeiro)
    /// têm idempotência própria nos consumidores.
    /// </summary>
    public class EfeitosFaturamentoPdvService
    {
        private readonly ContextVendas _context;

        public EfeitosFaturamentoPdvService(ContextVendas context) => _context = context;

        /// <summary>
        /// Aplica os efeitos de faturamento da <paramref name="venda"/> (já carregada com Itens e
        /// Pagamentos). NÃO chama SaveChanges — o handler chamador controla a transação única.
        /// Retorna a quantidade de <see cref="CaixaMovimento"/> de venda criados.
        /// </summary>
        public async Task<int> AplicarAsync(Venda venda, string tenantId, string usuario, CancellationToken ct = default)
        {
            if (venda == null) throw new ArgumentNullException(nameof(venda));

            await EnfileirarVendaFaturadaAsync(venda, tenantId, ct);
            return await CreditarCaixaPorFormaAsync(venda, tenantId, usuario, ct);
        }

        /// <summary>
        /// Enfileira o evento <c>VendaFaturada</c> no Outbox de Vendas (idempotente por venda). Mesmo
        /// payload do <c>SincronizarVendasCommandHandler</c>, para o <c>VendasOutboxProcessorJob</c>
        /// rotear a Financeiro e Estoque.
        /// </summary>
        private async Task EnfileirarVendaFaturadaAsync(Venda venda, string tenantId, CancellationToken ct)
        {
            var vendaIdText = venda.Id.ToString();
            var jaEnfileirado = await _context.OutboxMessages
                .IgnoreQueryFilters()
                .AnyAsync(m => m.EventType == "VendaFaturada" && m.Payload.Contains(vendaIdText), ct);

            if (jaEnfileirado)
                return;

            var payloadObj = new
            {
                VendaId = venda.Id,
                TenantId = tenantId,
                Total = venda.Total,
                CriadoEm = venda.CriadoEm,
                Itens = venda.Itens.Select(i => new
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario
                }).ToList()
            };

            var payloadJson = JsonSerializer.Serialize(payloadObj);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "VendaFaturada", payloadJson));
        }

        /// <summary>
        /// Grava um <see cref="CaixaMovimento"/> (Tipo "Venda") por forma de pagamento da venda. Usa a
        /// coleção de <see cref="VendaPagamento"/> quando existir; senão, cai na forma escalar
        /// <see cref="Venda.FormaPagamento"/> com o Total. Idempotente: pula (venda + forma) já lançados.
        /// Só credita se a venda estiver vinculada a um caixa existente (CaixaId é GUID de caixa).
        /// </summary>
        private async Task<int> CreditarCaixaPorFormaAsync(Venda venda, string tenantId, string usuario, CancellationToken ct)
        {
            if (!Guid.TryParse(venda.CaixaId, out var caixaId) || caixaId == Guid.Empty)
                return 0; // Venda sem caixa vinculado (ex.: rótulo textual): sem crédito de gaveta.

            var caixaExiste = await _context.Caixas
                .IgnoreQueryFilters()
                .AnyAsync(c => c.TenantId == tenantId && c.Id == caixaId, ct);
            if (!caixaExiste)
                return 0;

            var ancoraVenda = $"Venda {venda.Id}";
            var criados = 0;

            if (venda.Pagamentos.Count > 0)
            {
                foreach (var pg in venda.Pagamentos)
                {
                    // Valor líquido recebido na forma (o troco só existe no dinheiro).
                    var valor = pg.ValorPagamento - pg.ValorTroco;
                    if (valor <= 0m) continue;

                    var forma = pg.TipoPagamento.ToString();
                    if (await MovimentoVendaJaExisteAsync(caixaId, tenantId, ancoraVenda, forma, ct))
                        continue;

                    _context.CaixaMovimentos.Add(new CaixaMovimento(
                        Guid.NewGuid(), Guid.NewGuid(), caixaId, "Venda", valor,
                        $"{ancoraVenda}|Forma:{forma}", tenantId, usuario, DateTime.UtcNow));
                    criados++;
                }
            }
            else if (venda.Total > 0m)
            {
                var forma = string.IsNullOrWhiteSpace(venda.FormaPagamento)
                    ? ETipoPagamento.PagamentoEletronicoNaoInformado.ToString()
                    : venda.FormaPagamento!;

                if (!await MovimentoVendaJaExisteAsync(caixaId, tenantId, ancoraVenda, forma, ct))
                {
                    _context.CaixaMovimentos.Add(new CaixaMovimento(
                        Guid.NewGuid(), Guid.NewGuid(), caixaId, "Venda", venda.Total,
                        $"{ancoraVenda}|Forma:{forma}", tenantId, usuario, DateTime.UtcNow));
                    criados++;
                }
            }

            return criados;
        }

        private async Task<bool> MovimentoVendaJaExisteAsync(Guid caixaId, string tenantId, string ancoraVenda, string forma, CancellationToken ct)
        {
            var marca = $"{ancoraVenda}|Forma:{forma}";
            return await _context.CaixaMovimentos
                .IgnoreQueryFilters()
                .AnyAsync(m => m.TenantId == tenantId
                            && m.CaixaId == caixaId
                            && m.Tipo == "Venda"
                            && m.Observacao != null
                            && m.Observacao == marca, ct);
        }
    }
}
