using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Interfaces;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Gateways;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>
    /// 1.08A — MOTOR DE COBRANÇA UNIFICADO (webhook do Mercado Pago).
    ///
    /// Endpoint canônico: <c>POST /api/v1/plataforma/clientes/webhooks/mercadopago</c>. Este é o ÚNICO
    /// caminho que concilia o PIX real do gateway. Ele:
    ///   (a) valida o <c>x-signature</c> REAL do Mercado Pago (esquema oficial ts + manifesto),
    ///   (b) lê <c>data.id</c> como o id NUMÉRICO do pagamento (não como GUID da fatura),
    ///   (c) consulta o pagamento no gateway (status + valor bruto + tarifa real + líquido + external_reference),
    ///   (d) resolve a Fatura por <c>external_reference</c>,
    ///   (e) liquida o <see cref="PagamentoFatura"/> + baixa a Fatura + ativa a <see cref="AssinaturaCliente"/>/tenant,
    ///   (f) é idempotente via <see cref="WebhookEventoProcessado"/> (1.06).
    ///
    /// O segredo do webhook vem SEMPRE do <see cref="ConfiguracaoGatewayPagamento.WebhookSecret"/> CIFRADO
    /// (descriptografado pelo cofre), nunca de texto plano. Fail-closed: sem config ativa / sem segredo, o
    /// webhook é rejeitado.
    ///
    /// ⛔ DIFERIDO (skills de negócio vazias — pedidos abertos): NÃO aplica reconhecimento de receita por
    /// competência/diferimento (REG-025), NÃO apura comissão de revenda como regra fiscal, NÃO emite NFS-e.
    /// Registra apenas os fatos financeiros (bruto/tarifa/líquido exatamente como o gateway informou).
    /// </summary>
    public class ProcessarWebhookPagamentoCommandHandler : ICommandHandler<ProcessarWebhookPagamentoCommand>
    {
        private const string ProvedorWebhook = "mercadopago";

        private readonly ContextGestaoClientes _context;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ISegredoCofreService _cofreService;
        private readonly ILogger<ProcessarWebhookPagamentoCommandHandler>? _logger;

        public ProcessarWebhookPagamentoCommandHandler(
            ContextGestaoClientes context,
            IPaymentGateway paymentGateway,
            ISegredoCofreService cofreService,
            ILogger<ProcessarWebhookPagamentoCommandHandler>? logger = null)
        {
            _context = context;
            _paymentGateway = paymentGateway;
            _cofreService = cofreService;
            _logger = logger;
        }

        public async Task<CommandResult> Handle(ProcessarWebhookPagamentoCommand request, CancellationToken cancellationToken)
        {
            if (request.Action != "payment.created" && request.Action != "payment.updated")
                return CommandResult.Falha("Ação não suportada pelo webhook.");

            if (request.Data == null || string.IsNullOrEmpty(request.Data.Id))
                return CommandResult.Falha("Identificador do pagamento não fornecido no webhook.");

            var paymentId = request.Data.Id;

            // 1) Config ativa do gateway (global/plataforma) — fonte ÚNICA do segredo (cifrado) e do token.
            var config = await ResolverConfigAtivaAsync(cancellationToken);
            if (config == null)
            {
                _logger?.LogWarning("Webhook rejeitado: nenhuma configuração de gateway ativa encontrada (fail-closed).");
                return CommandResult.Falha("Gateway de pagamento não configurado. Webhook rejeitado.");
            }

            if (string.IsNullOrWhiteSpace(config.WebhookSecret))
            {
                _logger?.LogWarning("Webhook rejeitado: WebhookSecret (cifrado) ausente na configuração do gateway (fail-closed).");
                return CommandResult.Falha("Segredo do webhook não configurado. Webhook rejeitado.");
            }

            // 2) Descriptografa o segredo do cofre e valida a assinatura REAL do Mercado Pago.
            string secret;
            try
            {
                secret = await _cofreService.DescriptografarAsync(config.WebhookSecret!);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Falha ao descriptografar o WebhookSecret do gateway {ConfigId}.", config.Id);
                return CommandResult.Falha("Não foi possível ler o segredo do webhook (falha no cofre).");
            }

            if (string.IsNullOrEmpty(request.Signature))
                return CommandResult.Falha("Assinatura do webhook não fornecida (x-signature).");

            if (!MercadoPagoWebhookSignature.Validar(secret, request.Signature, request.RequestId, paymentId))
                return CommandResult.Falha("Assinatura do webhook inválida. Acesso não autorizado.");

            // 3) Idempotência (REG-016): se este pagamento já foi conciliado, ignora a reentrega.
            var jaProcessado = await _context.WebhookEventosProcessados
                .IgnoreQueryFilters()
                .AnyAsync(w => w.Provedor == ProvedorWebhook && w.EventoId == paymentId && w.DeletadoEm == null, cancellationToken);
            if (jaProcessado)
                return CommandResult.Ok("Evento de webhook já processado anteriormente (idempotência).");

            // 4) Consulta o pagamento no gateway: status + valores + tarifa REAL + external_reference.
            var consulta = await _paymentGateway.ConsultarPagamentoAsync(paymentId, config, cancellationToken);
            if (!consulta.Sucesso || consulta.Dados is not ConsultaPagamentoResultado pg)
                return CommandResult.Falha(consulta.Erros?.Any() == true ? consulta.Erros : new[] { "Falha ao consultar o pagamento no gateway." },
                    "Não foi possível conciliar o pagamento.");

            // Só liquidamos pagamentos efetivamente aprovados. Status não-liquidado NÃO marca idempotência
            // (uma reentrega posterior 'approved' do mesmo pagamento ainda deve ser conciliada).
            var status = (pg.Status ?? string.Empty).ToLowerInvariant();
            if (status != "approved" && status != "paid" && status != "succeeded")
                return CommandResult.Ok($"Webhook recebido com status '{pg.Status}'. Nenhuma liquidação executada.");

            // 5) Resolve a Fatura pelo external_reference (que é o Id da Fatura, gravado ao gerar a cobrança).
            if (!Guid.TryParse(pg.ExternalReference, out var faturaId))
                return CommandResult.Falha("external_reference do pagamento não corresponde a uma Fatura válida.");

            var fatura = await _context.Faturas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.Id == faturaId && f.DeletadoEm == null, cancellationToken);
            if (fatura == null)
                return CommandResult.Falha($"Fatura {faturaId} (external_reference) não encontrada.");

            var alteradoPor = "system-webhook-mp";

            // Se a fatura já está paga, apenas registra idempotência e sai.
            if (fatura.Status == FaturaStatus.Paga)
            {
                await RegistrarEventoProcessadoAsync(paymentId, request.Action, cancellationToken);
                return CommandResult.Ok("Fatura já se encontra baixada/paga.");
            }

            // 6) Valores exatamente como o gateway informou (fidelidade de dado — sem regra contábil).
            var valorBruto = pg.ValorTransacao ?? fatura.Valor;
            var tarifa = pg.ValorTarifa;
            var liquido = pg.ValorLiquido ?? (valorBruto - (tarifa ?? 0m));

            // 6a) Baixa da fatura + split de comissão (mesma apuração do BaixarFaturaCommandHandler, porém
            // sem depender de contexto de tenant HTTP — o webhook chega anônimo).
            await BaixarFaturaComComissaoAsync(fatura, alteradoPor, cancellationToken);

            // 6b) Liquida o PagamentoFatura PIX pendente (criado em GerarCobrancaPixCommandHandler) com os
            // valores reais do gateway. Se não existir, cria o registro já liquidado.
            var pagamento = await _context.PagamentosFaturas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.FaturaId == fatura.Id
                                          && (p.IdentificadorPagamento == paymentId
                                              || (p.TipoPagamento == "PIX" && p.Status == PagamentoFaturaStatus.Pending))
                                          && p.DeletadoEm == null, cancellationToken);

            if (pagamento == null)
            {
                pagamento = new PagamentoFatura(
                    faturaId: fatura.Id,
                    tipoPagamento: "PIX",
                    status: PagamentoFaturaStatus.Paid,
                    valorPago: valorBruto,
                    valorTarifa: tarifa,
                    identificadorPagamento: paymentId,
                    pagoManualmente: false,
                    dataPagamento: DateTime.UtcNow,
                    tenantId: fatura.TenantId,
                    criadoPor: alteradoPor);
                pagamento.Liquidar(valorBruto, tarifa, alteradoPor, liquido, pg.DataAprovacao);
                _context.PagamentosFaturas.Add(pagamento);
            }
            else
            {
                pagamento.Liquidar(valorBruto, tarifa, alteradoPor, liquido, pg.DataAprovacao);
            }

            // 6c) Ativa a assinatura mais recente do cliente + move o StatusSaaS do tenant para Ativo (§12).
            var assinatura = await _context.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.ClienteId == fatura.ClienteId && a.DeletadoEm == null && !a.Arquivada)
                .OrderByDescending(a => a.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);

            var cliente = await _context.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == fatura.ClienteId && c.DeletadoEm == null, cancellationToken);

            if (assinatura != null)
                assinatura.Ativar(alteradoPor);

            if (cliente != null)
            {
                if (assinatura != null)
                    cliente.AlterarPlano(assinatura.PlanoId, alteradoPor);
                cliente.AtualizarStatusSaaS(StatusSaaS.Ativo, alteradoPor);
            }

            // 6d) Pagamento global (histórico consolidado) + recibo do cliente.
            if (assinatura != null)
            {
                _context.PagamentosGlobais.Add(new PagamentoGlobal(
                    assinaturaId: assinatura.Id,
                    pedidoId: null,
                    faturaId: fatura.Id,
                    dataPagamento: DateTime.UtcNow,
                    valor: valorBruto,
                    gateway: "MercadoPago",
                    transactionId: paymentId,
                    tenantId: fatura.TenantId,
                    criadoPor: alteradoPor));
            }

            var recibo = ReciboPagamento.Emitir(
                fatura: fatura,
                pagamentoFaturaId: pagamento.Id,
                valorPago: valorBruto,
                meioPagamento: "PIX",
                pagadorNome: cliente?.RazaoSocial,
                pagadorDocumento: cliente?.Cnpj,
                criadoPor: alteradoPor);
            _context.RecibosPagamento.Add(recibo);

            // 6e) Idempotência: marca o evento processado.
            await RegistrarEventoProcessadoAsync(paymentId, request.Action, cancellationToken, salvar: false);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Pagamento conciliado e fatura baixada via webhook Mercado Pago.", new
            {
                FaturaId = fatura.Id,
                PaymentId = paymentId,
                ValorBruto = valorBruto,
                Tarifa = tarifa,
                Liquido = liquido,
                ReciboNumero = recibo.Numero
            });
        }

        /// <summary>Baixa a fatura apurando o split de comissão (idêntico ao BaixarFaturaCommandHandler).</summary>
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

        /// <summary>Prefere a config global (plataforma) com segredo; senão qualquer config ativa.</summary>
        private async Task<ConfiguracaoGatewayPagamento?> ResolverConfigAtivaAsync(CancellationToken cancellationToken)
        {
            var global = await _context.ConfiguracoesGatewayPagamento
                .Where(c => c.Ativo && c.TenantAlvo == null)
                .OrderByDescending(c => c.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);
            if (global != null) return global;

            return await _context.ConfiguracoesGatewayPagamento
                .Where(c => c.Ativo)
                .OrderByDescending(c => c.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task RegistrarEventoProcessadoAsync(string eventoId, string acao, CancellationToken cancellationToken, bool salvar = true)
        {
            try
            {
                _context.WebhookEventosProcessados.Add(new WebhookEventoProcessado(ProvedorWebhook, eventoId, acao, "system"));
                if (salvar)
                    await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                _logger?.LogInformation("Registro de idempotência do webhook {EventoId} já existente (corrida de reentrega).", eventoId);
            }
        }
    }
}
