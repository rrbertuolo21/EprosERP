using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Interfaces;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>
    /// Gera a cobrança PIX de uma fatura no gateway ativo e persiste o PagamentoFatura
    /// (PaymentId + QR + ticket). Config resolvida por tenant, com fallback para a global da plataforma.
    /// </summary>
    public class GerarCobrancaPixCommandHandler : ICommandHandler<GerarCobrancaPixCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IPaymentGateway _paymentGateway;

        public GerarCobrancaPixCommandHandler(
            ContextGestaoClientes context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IPaymentGateway paymentGateway)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _paymentGateway = paymentGateway;
        }

        public async Task<CommandResult> Handle(GerarCobrancaPixCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuarioId = _currentUser.GetUserId() ?? "system";

            var fatura = await _context.Faturas.FirstOrDefaultAsync(f => f.Id == request.FaturaId, cancellationToken);
            if (fatura == null)
                return CommandResult.Falha(new[] { "Fatura não encontrada." }, "Erro");

            if (fatura.Status == FaturaStatus.Paga)
                return CommandResult.Falha(new[] { "Fatura já está paga; não é possível gerar nova cobrança." }, "Erro");

            if (fatura.Status == FaturaStatus.Cancelada)
                return CommandResult.Falha(new[] { "Fatura cancelada; não é possível gerar cobrança." }, "Erro");

            // Resolução da configuração: override por tenant, senão global (TenantAlvo == null).
            var config = await ResolverConfigAtivaAsync(tenantId, cancellationToken);
            if (config == null)
                return CommandResult.Falha("Gateway não configurado", "Nenhum gateway de pagamento ativo foi encontrado (nem por tenant, nem global).");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == fatura.ClienteId, cancellationToken);
            if (cliente == null)
                return CommandResult.Falha(new[] { "Cliente da fatura não encontrado." }, "Erro");

            var pagador = new DadosPagador(cliente.Email, cliente.RazaoSocial, cliente.Cnpj);

            var gatewayResult = await _paymentGateway.GerarCobrancaPixAsync(fatura, config, pagador, cancellationToken);
            if (!gatewayResult.Sucesso || gatewayResult.Dados is not CobrancaPixResultado dto)
                return gatewayResult;

            // Cria ou atualiza o PagamentoFatura PIX pendente da fatura.
            var pagamento = await _context.PagamentosFaturas
                .FirstOrDefaultAsync(p => p.FaturaId == fatura.Id
                                          && p.TipoPagamento == "PIX"
                                          && p.Status == PagamentoFaturaStatus.Pending, cancellationToken);

            if (pagamento == null)
            {
                pagamento = new PagamentoFatura(
                    fatura.Id,
                    "PIX",
                    PagamentoFaturaStatus.Pending,
                    fatura.Valor,
                    null,
                    dto.PaymentId,
                    false,
                    null,
                    tenantId,
                    usuarioId);

                if (!pagamento.IsValid)
                    return CommandResult.Falha(pagamento.Notifications.Select(n => n.Message), "Falha ao registrar o pagamento");

                pagamento.RegistrarCobrancaPix(dto.PaymentId, dto.QrCode, dto.QrCodeBase64, dto.TicketUrl, usuarioId);
                _context.PagamentosFaturas.Add(pagamento);
            }
            else
            {
                pagamento.RegistrarCobrancaPix(dto.PaymentId, dto.QrCode, dto.QrCodeBase64, dto.TicketUrl, usuarioId);
                _context.PagamentosFaturas.Update(pagamento);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cobrança PIX gerada com sucesso.", new
            {
                paymentId = dto.PaymentId,
                qrCode = dto.QrCode,
                qrCodeBase64 = dto.QrCodeBase64,
                ticketUrl = dto.TicketUrl,
                dataExpiracao = dto.DataExpiracao
            });
        }

        /// <summary>Prefere a config ativa do tenant; se não houver, usa a config global (TenantAlvo == null).</summary>
        private async Task<ConfiguracaoGatewayPagamento?> ResolverConfigAtivaAsync(string tenantId, CancellationToken cancellationToken)
        {
            // ConfiguracaoGatewayPagamento é IGlobalEntity: não sofre filtro automático por tenant.
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
