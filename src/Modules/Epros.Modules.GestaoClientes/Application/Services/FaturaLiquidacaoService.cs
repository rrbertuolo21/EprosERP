using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Services
{
    /// <summary>
    /// 1.08B — Serviço de LIQUIDAÇÃO de fatura reutilizado por todos os caminhos de conciliação real
    /// (webhook unificado do Mercado Pago = PIX/boleto/checkout; cobrança recorrente no cartão-on-file).
    /// Centraliza o "caminho da passada A": baixa da fatura + split de comissão + liquidação do
    /// <see cref="PagamentoFatura"/> + ativação da assinatura/tenant + pagamento global + recibo.
    ///
    /// NÃO chama <c>SaveChanges</c> (quem orquestra decide o commit). Idempotente por fatura já paga
    /// (retorna null sem duplicar). Ao ativar a assinatura, ANCORA o ciclo de cobrança
    /// (<see cref="AssinaturaCliente.ProximaCobrancaEm"/>) conforme a duração do plano — fechando o
    /// vínculo assinatura↔recorrência (Mensal → +1 mês; Anual → +1 ano; Vitalicia → sem renovação).
    ///
    /// 1.08I — MECANISMO de reconhecimento de receita por competência (diferimento anual = 12 avos, CPC 47
    /// RN05) e APURAÇÃO de comissão parametrizável (base/momento = PARÂMETRO). ⚠️ Contas contábeis, política
    /// de pró-rata/vitalício e base/momento de comissão = PARÂMETRO do cliente/contador (VALIDA CONTADOR).
    /// NÃO emite NFS-e nem inventa número fiscal.
    /// </summary>
    public class FaturaLiquidacaoService
    {
        private readonly ContextGestaoClientes _context;
        private readonly ReconhecimentoReceitaService _reconhecimentoReceita;
        private readonly ApuracaoComissaoService _apuracaoComissao;

        public FaturaLiquidacaoService(ContextGestaoClientes context)
        {
            _context = context;
            _reconhecimentoReceita = new ReconhecimentoReceitaService(context);
            _apuracaoComissao = new ApuracaoComissaoService(context);
        }

        /// <summary>
        /// Liquida a fatura com os valores exatamente informados pelo gateway (fidelidade de dado).
        /// Retorna o recibo emitido, ou null se a fatura já estava paga (idempotência).
        /// </summary>
        public async Task<ReciboPagamento?> LiquidarAsync(
            Fatura fatura,
            string? paymentId,
            string meioPagamento,
            string gatewayNome,
            decimal valorBruto,
            decimal? tarifa,
            decimal? liquido,
            DateTime? dataAprovacao,
            string alteradoPor,
            CancellationToken cancellationToken)
        {
            if (fatura.Status == FaturaStatus.Paga)
                return null;

            liquido ??= valorBruto - (tarifa ?? 0m);

            // 1) Baixa da fatura + split de comissão (idêntico ao BaixarFaturaCommandHandler).
            await BaixarFaturaComComissaoAsync(fatura, alteradoPor, cancellationToken);

            // 2) Liquida o PagamentoFatura pendente (PIX/Boleto) ou cria já liquidado (cartão/checkout).
            var pagamento = await _context.PagamentosFaturas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.FaturaId == fatura.Id
                                          && (p.IdentificadorPagamento == paymentId
                                              || ((p.TipoPagamento == "PIX" || p.TipoPagamento == "Boleto") && p.Status == PagamentoFaturaStatus.Pending))
                                          && p.DeletadoEm == null, cancellationToken);

            if (pagamento == null)
            {
                pagamento = new PagamentoFatura(
                    faturaId: fatura.Id,
                    tipoPagamento: meioPagamento,
                    status: PagamentoFaturaStatus.Paid,
                    valorPago: valorBruto,
                    valorTarifa: tarifa,
                    identificadorPagamento: paymentId,
                    pagoManualmente: false,
                    dataPagamento: DateTime.UtcNow,
                    tenantId: fatura.TenantId,
                    criadoPor: alteradoPor);
                pagamento.Liquidar(valorBruto, tarifa, alteradoPor, liquido, dataAprovacao);
                _context.PagamentosFaturas.Add(pagamento);
            }
            else
            {
                pagamento.Liquidar(valorBruto, tarifa, alteradoPor, liquido, dataAprovacao);
            }

            // 3) Ativa a assinatura mais recente do cliente + move o StatusSaaS do tenant para Ativo (§12).
            var assinatura = await _context.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.ClienteId == fatura.ClienteId && a.DeletadoEm == null && !a.Arquivada)
                .OrderByDescending(a => a.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);

            var cliente = await _context.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == fatura.ClienteId && c.DeletadoEm == null, cancellationToken);

            if (assinatura != null)
            {
                assinatura.Ativar(alteradoPor);
                await AncorarCicloCobrancaAsync(assinatura, alteradoPor, cancellationToken);

                // 1.08I — DIFERIMENTO: gera o cronograma de reconhecimento de receita conforme a duração do
                // plano (Anual = 12 avos; Mensal = 1; Vitalício = ponto único por default). A parcela ainda
                // não apropriada é passivo de receita diferida; a rotina mensal a reconhece. [CPC 47 RN04/05/07]
                var planoRef = await _context.Planos.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == assinatura.PlanoId, cancellationToken);
                if (planoRef != null)
                {
                    var competenciaInicial = dataAprovacao ?? DateTime.UtcNow;
                    await _reconhecimentoReceita.GerarCronogramaAsync(
                        fatura, planoRef.Duration, competenciaInicial, alteradoPor, cancellationToken);
                }
            }

            // 1.08I — APURAÇÃO de comissão parametrizável (base/momento lidos de ConfiguracaoGlobal; default
            // seguro Bruto/Caixa). Complementa o ComissaoApuradaEvent factual, registrando o resultado do
            // mecanismo. ⚠️ base/momento/% = PARÂMETRO, VALIDA CONTADOR.
            await _apuracaoComissao.ApurarAsync(fatura, liquido, alteradoPor, cancellationToken);

            if (cliente != null)
            {
                if (assinatura != null)
                    cliente.AlterarPlano(assinatura.PlanoId, alteradoPor);
                cliente.AtualizarStatusSaaS(StatusSaaS.Ativo, alteradoPor);
            }

            // 4) Pagamento global (histórico consolidado) + recibo do cliente.
            if (assinatura != null)
            {
                _context.PagamentosGlobais.Add(new PagamentoGlobal(
                    assinaturaId: assinatura.Id,
                    pedidoId: null,
                    faturaId: fatura.Id,
                    dataPagamento: DateTime.UtcNow,
                    valor: valorBruto,
                    gateway: gatewayNome,
                    transactionId: paymentId ?? Guid.NewGuid().ToString("N"),
                    tenantId: fatura.TenantId,
                    criadoPor: alteradoPor));
            }

            var recibo = ReciboPagamento.Emitir(
                fatura: fatura,
                pagamentoFaturaId: pagamento.Id,
                valorPago: valorBruto,
                meioPagamento: meioPagamento,
                pagadorNome: cliente?.RazaoSocial,
                pagadorDocumento: cliente?.Cnpj,
                criadoPor: alteradoPor);
            _context.RecibosPagamento.Add(recibo);

            // 1.08C — ENTREGA do recibo: além de registrar o ReciboPagamento, enfileira ReciboEmitidoEvent no
            // Outbox (committado junto pelo orquestrador). O GestaoClientesOutboxProcessorJob dispara a
            // notificação com o resumo do recibo via INotificacaoService. (PDF do recibo = outra fatia; o
            // gancho de anexo/link do PDF está marcado no processador.)
            var reciboPayload = JsonSerializer.Serialize(new
            {
                ClienteId = fatura.ClienteId,
                TenantId = fatura.TenantId,
                FaturaId = fatura.Id,
                ReciboNumero = recibo.Numero,
                Valor = valorBruto,
                MeioPagamento = meioPagamento,
                DataPagamento = recibo.DataPagamento
            });
            _context.OutboxMessages.Add(new OutboxMessage(fatura.TenantId, "ReciboEmitidoEvent", reciboPayload));

            return recibo;
        }

        /// <summary>
        /// 1.08B — Ancora o próximo vencimento do ciclo conforme a duração do plano (fecha o vínculo
        /// assinatura↔recorrência). Só define quando ainda não há âncora (não reescreve um ciclo em andamento).
        /// </summary>
        private async Task AncorarCicloCobrancaAsync(AssinaturaCliente assinatura, string alteradoPor, CancellationToken cancellationToken)
        {
            if (assinatura.ProximaCobrancaEm != null)
                return; // ciclo já ancorado — o job de renovação avança a data.

            var plano = await _context.Planos.IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == assinatura.PlanoId, cancellationToken);
            if (plano == null || plano.Preco <= 0)
                return; // plano free → sem ciclo de cobrança.

            var proxima = CalcularProximaCobranca(plano.Duration, DateTime.UtcNow);
            assinatura.DefinirProximaCobranca(proxima, alteradoPor);
        }

        /// <summary>
        /// Próximo vencimento do ciclo: Mensal → +1 mês; Anual → +1 ano; Vitalicia → null (cobrança única).
        /// </summary>
        public static DateTime? CalcularProximaCobranca(PlanoDuration duration, DateTime referencia) => duration switch
        {
            PlanoDuration.Mensal => referencia.AddMonths(1),
            PlanoDuration.Anual => referencia.AddYears(1),
            _ => (DateTime?)null // Vitalicia
        };

        private async Task BaixarFaturaComComissaoAsync(Fatura fatura, string alteradoPor, CancellationToken cancellationToken)
        {
            var cliente = await _context.Clientes.IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == fatura.ClienteId, cancellationToken);

            decimal percentualRevenda = 0, percentualVendedor = 0;
            if (cliente != null)
            {
                if (cliente.RevendaId.HasValue)
                {
                    var revenda = await _context.Revendas.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(r => r.Id == cliente.RevendaId.Value && r.Ativo, cancellationToken);
                    if (revenda != null) percentualRevenda = revenda.PercentualComissao;
                }
                if (cliente.VendedorId.HasValue)
                {
                    var vendedor = await _context.Vendedores.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(v => v.Id == cliente.VendedorId.Value && v.Ativo, cancellationToken);
                    if (vendedor != null) percentualVendedor = vendedor.PercentualComissao;
                }
            }

            fatura.CalcularSplitComissao(percentualRevenda, percentualVendedor);
            fatura.Baixar(alteradoPor);

            var payloadComissao = new
            {
                FaturaId = fatura.Id,
                ClienteId = fatura.ClienteId,
                ValorTotal = fatura.Valor,
                PercentualComissaoRevenda = fatura.PercentualComissaoRevenda,
                PercentualComissaoVendedor = fatura.PercentualComissaoVendedor,
                ValorComissaoRevenda = fatura.ValorComissaoRevenda,
                ValorComissaoVendedor = fatura.ValorComissaoVendedor,
                ApuradoEm = DateTime.UtcNow
            };
            _context.OutboxMessages.Add(new OutboxMessage(fatura.TenantId, "ComissaoApuradaEvent", JsonSerializer.Serialize(payloadComissao)));
        }
    }
}
