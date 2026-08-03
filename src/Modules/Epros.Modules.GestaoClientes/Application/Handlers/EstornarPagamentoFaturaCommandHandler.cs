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
using Epros.Modules.GestaoClientes.Application.Security;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>
    /// 1.08E — ESTORNO/refund de um <see cref="PagamentoFatura"/> liquidado. Operação LANDLORD: exige
    /// operador interno da Siser (<see cref="GuardaOperadorInterno"/>, defesa em profundidade da 1.11,
    /// além do AbacAuthorize no controller). Chama a API de refund do gateway (Mercado Pago
    /// <c>POST /v1/payments/{id}/refunds</c>) via <see cref="IPaymentGateway"/> e, no sucesso (ou no-op
    /// controlado sem credencial/ambiente), reverte localmente: pagamento → estornado, fatura → reaberta/
    /// estornada, pedido → Refunded, StatusSaaS do tenant volta a AguardandoPagamento, e enfileira
    /// <c>PagamentoEstornadoEvent</c> no Outbox (o processador 1.08C notifica). Idempotente: estornar um
    /// pagamento já estornado não duplica efeito nem evento.
    ///
    /// ⛔ Regra #0: NÃO trata o efeito CONTÁBIL/FISCAL do estorno (receita negativa, nota de crédito) —
    /// apenas o mecanismo de reverter pagamento/fatura. Efeito contábil/fiscal = valida-contador.
    /// </summary>
    public class EstornarPagamentoFaturaCommandHandler : ICommandHandler<EstornarPagamentoFaturaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ILogger<EstornarPagamentoFaturaCommandHandler>? _logger;

        public EstornarPagamentoFaturaCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IPaymentGateway paymentGateway,
            ILogger<EstornarPagamentoFaturaCommandHandler>? logger = null)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _paymentGateway = paymentGateway;
            _logger = logger;
        }

        public async Task<CommandResult> Handle(EstornarPagamentoFaturaCommand request, CancellationToken cancellationToken)
        {
            // 1.11 — estorno de fatura landlord exige operador interno (fail-closed no handler).
            var guarda = GuardaOperadorInterno.Exigir(_tenantProvider);
            if (guarda != null) return guarda;

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var pagamento = await _context.PagamentosFaturas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == request.PagamentoFaturaId && p.DeletadoEm == null, cancellationToken);
            if (pagamento == null)
                return CommandResult.Falha(new[] { "Pagamento da fatura não encontrado." }, "Erro");

            // IDEMPOTÊNCIA: já estornado → sucesso sem novo efeito/evento (estornar 2x não duplica).
            if (pagamento.EstaEstornado)
                return CommandResult.Ok("Pagamento já havia sido estornado anteriormente (idempotente).",
                    new { PagamentoFaturaId = pagamento.Id, Status = pagamento.Status.ToString() });

            if (pagamento.Status != PagamentoFaturaStatus.Paid)
                return CommandResult.Falha(new[] { "Somente pagamentos liquidados podem ser estornados." }, "Erro");

            var fatura = await _context.Faturas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.Id == pagamento.FaturaId && f.DeletadoEm == null, cancellationToken);
            if (fatura == null)
                return CommandResult.Falha(new[] { "Fatura do pagamento não encontrada." }, "Erro");

            // ===== Chamada REAL de refund ao gateway (quando aplicável) =====
            // Sem gateway configurado (ambiente/credencial) ou pagamento OFFLINE (manual) → no-op controlado + log.
            string? refundId = null;
            var podeChamarGateway = !pagamento.PagoManualmente && !string.IsNullOrWhiteSpace(pagamento.IdentificadorPagamento);
            if (podeChamarGateway)
            {
                var config = await ResolverConfigAtivaAsync(fatura.TenantId, cancellationToken);
                if (config != null)
                {
                    var gw = await _paymentGateway.EstornarPagamentoAsync(
                        pagamento.IdentificadorPagamento!, config, request.Valor, cancellationToken);
                    if (!gw.Sucesso)
                        return gw; // gateway recusou o estorno → não reverte localmente.

                    if (gw.Dados is EstornoResultado dto)
                        refundId = dto.RefundId;
                }
                else
                {
                    // Sem credencial/ambiente: não trava a operação — reverte localmente e loga o no-op.
                    _logger?.LogWarning(
                        "[Estorno] Gateway não configurado para o tenant {TenantId}; refund do pagamento {PagamentoId} tratado como no-op controlado (reversão apenas local).",
                        fatura.TenantId, pagamento.Id);
                }
            }
            else
            {
                _logger?.LogInformation(
                    "[Estorno] Pagamento {PagamentoId} sem chamada ao gateway (offline/manual ou sem identificador). Reversão apenas local.",
                    pagamento.Id);
            }

            // ===== Reversão local (fail-closed já garantido acima) =====
            pagamento.Estornar(refundId, request.Valor, alteradoPor);
            fatura.Estornar(alteradoPor);

            // Reverte a ativação do ciclo: pedido correspondente → Refunded (best-effort).
            var pedido = await _context.PedidosSaaS
                .IgnoreQueryFilters()
                .Where(p => p.ClienteId == fatura.ClienteId && p.Status == PedidoSaaSStatus.Succeeded && p.DeletadoEm == null)
                .OrderByDescending(p => p.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);
            pedido?.MarcarReembolsado(alteradoPor);

            // O StatusSaaS do tenant reflete a reversão do pagamento do ciclo (volta a aguardar pagamento).
            var cliente = await _context.Clientes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == fatura.ClienteId && c.DeletadoEm == null, cancellationToken);
            cliente?.AtualizarStatusSaaS(StatusSaaS.AguardandoPagamento, alteradoPor);

            // 1.08C — evento de estorno no Outbox (processador notifica o cliente).
            var payload = JsonSerializer.Serialize(new
            {
                ClienteId = fatura.ClienteId,
                TenantId = fatura.TenantId,
                FaturaId = fatura.Id,
                PagamentoFaturaId = pagamento.Id,
                PedidoId = pedido?.Id,
                Valor = pagamento.ValorEstornado ?? pagamento.ValorPago,
                RefundId = refundId,
                Motivo = request.Motivo,
                DataEstorno = pagamento.DataEstorno
            });
            _context.OutboxMessages.Add(new OutboxMessage(fatura.TenantId, "PagamentoEstornadoEvent", payload));

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Pagamento estornado com sucesso.", new
            {
                PagamentoFaturaId = pagamento.Id,
                FaturaId = fatura.Id,
                Status = pagamento.Status.ToString(),
                RefundId = refundId,
                ValorEstornado = pagamento.ValorEstornado,
                PedidoId = pedido?.Id
            });
        }

        /// <summary>Prefere a config ativa do tenant da fatura; se não houver, usa a global (TenantAlvo == null).</summary>
        private async Task<ConfiguracaoGatewayPagamento?> ResolverConfigAtivaAsync(string tenantId, CancellationToken cancellationToken)
        {
            var porTenant = await _context.ConfiguracoesGatewayPagamento
                .Where(c => c.Ativo && c.TenantAlvo == tenantId)
                .OrderByDescending(c => c.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);
            if (porTenant != null) return porTenant;

            return await _context.ConfiguracoesGatewayPagamento
                .Where(c => c.Ativo && c.TenantAlvo == null)
                .OrderByDescending(c => c.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
