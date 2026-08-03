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
        private readonly Epros.Modules.GestaoClientes.Application.Services.FaturaLiquidacaoService _liquidacao;
        private readonly ILogger<ProcessarWebhookPagamentoCommandHandler>? _logger;

        public ProcessarWebhookPagamentoCommandHandler(
            ContextGestaoClientes context,
            IPaymentGateway paymentGateway,
            ISegredoCofreService cofreService,
            Epros.Modules.GestaoClientes.Application.Services.FaturaLiquidacaoService? liquidacao = null,
            ILogger<ProcessarWebhookPagamentoCommandHandler>? logger = null)
        {
            _context = context;
            _paymentGateway = paymentGateway;
            _cofreService = cofreService;
            // Fallback para o serviço sem estado — mantém compatibilidade dos call sites (testes) 3-arg.
            _liquidacao = liquidacao ?? new Epros.Modules.GestaoClientes.Application.Services.FaturaLiquidacaoService(context);
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

            // Descobre o meio pelo PagamentoFatura pendente (PIX/Boleto); default PIX.
            var meio = await _context.PagamentosFaturas.IgnoreQueryFilters()
                .Where(p => p.FaturaId == fatura.Id && (p.IdentificadorPagamento == paymentId || p.Status == PagamentoFaturaStatus.Pending) && p.DeletadoEm == null)
                .OrderByDescending(p => p.CriadoEm)
                .Select(p => p.TipoPagamento)
                .FirstOrDefaultAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(meio)) meio = "PIX";

            // 6a-6d) Liquidação (caminho único da passada A, agora compartilhado com cartão/boleto/checkout).
            var recibo = await _liquidacao.LiquidarAsync(
                fatura, paymentId, meio, "MercadoPago", valorBruto, tarifa, liquido, pg.DataAprovacao, alteradoPor, cancellationToken);

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
                ReciboNumero = recibo?.Numero
            });
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
