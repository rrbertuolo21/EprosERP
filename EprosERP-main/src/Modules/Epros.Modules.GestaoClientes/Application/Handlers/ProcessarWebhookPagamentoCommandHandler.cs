using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Processa Webhook Pagamento.</summary>
    public class ProcessarWebhookPagamentoCommandHandler : ICommandHandler<ProcessarWebhookPagamentoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly IMediator _mediator;
        private readonly ILogger<ProcessarWebhookPagamentoCommandHandler>? _logger;

        public ProcessarWebhookPagamentoCommandHandler(ContextGestaoClientes context, IMediator mediator, ILogger<ProcessarWebhookPagamentoCommandHandler>? logger = null)
        {
            _context = context;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<CommandResult> Handle(ProcessarWebhookPagamentoCommand request, CancellationToken cancellationToken)
        {
            // O webhook nos avisa sobre ações de pagamento
            if (request.Action != "payment.created" && request.Action != "payment.updated")
            {
                return CommandResult.Falha("Ação não suportada pelo webhook.");
            }

            if (request.Data == null || string.IsNullOrEmpty(request.Data.Id))
            {
                return CommandResult.Falha("Identificador do pagamento não fornecido no webhook.");
            }

            // 1. Validar assinatura HMAC-SHA256 para garantir autenticidade (REG-035)
            var configSecret = await _context.ConfiguracoesGlobais
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Chave == "webhook_secret", cancellationToken);

            var secretKey = configSecret?.Valor;

            // Fail-closed: sem segredo configurado NÃO validamos HMAC com segredo default — rejeitamos o webhook.
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger?.LogWarning("Webhook rejeitado: segredo do webhook (webhook_secret) não configurado.");
                return CommandResult.Falha("Segredo do webhook não configurado. Webhook rejeitado.");
            }

            if (string.IsNullOrEmpty(request.Signature))
            {
                return CommandResult.Falha("Assinatura do webhook não fornecida (X-Signature ou Signature).");
            }

            var payloadToHash = $"{request.Action}:{request.Data.Id}";

            using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secretKey)))
            {
                var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payloadToHash));
                var computedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                if (!string.Equals(computedSignature, request.Signature.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return CommandResult.Falha("Assinatura do webhook inválida. Acesso não autorizado.");
                }
            }

            // Tenta converter o id do pagamento do Mercado Pago para GUID (que no nosso fluxo representa o Id da Fatura)
            if (!Guid.TryParse(request.Data.Id, out var faturaId))
            {
                return CommandResult.Falha("ID do pagamento inválido ou não corresponde ao formato GUID da Fatura.");
            }

            // Busca a fatura (ignoramos os filtros de tenant porque webhooks chegam sem contexto de tenant HTTP)
            var fatura = await _context.Faturas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.Id == faturaId && f.DeletadoEm == null, cancellationToken);

            if (fatura == null)
            {
                return CommandResult.Falha($"Fatura correspondente ao pagamento {faturaId} não foi encontrada.");
            }

            // Se a fatura já estiver paga, apenas retorna Ok para evitar processamento duplicado
            if (fatura.Status == FaturaStatus.Paga)
            {
                return CommandResult.Ok("Fatura já se encontra baixada/paga.");
            }

            // Executa a baixa da fatura invocando o comando correspondente
            var result = await _mediator.Send(new BaixarFaturaCommand(faturaId), cancellationToken);

            if (!result.Sucesso)
            {
                return CommandResult.Falha(result.Erros, $"Erro ao processar a baixa da fatura {faturaId} via webhook.");
            }

            return CommandResult.Ok("Baixa da fatura processada com sucesso via webhook!");
        }
    }
}
