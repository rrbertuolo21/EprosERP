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
    /// 1.08B — Emite o BOLETO de uma fatura no gateway ativo e persiste o PagamentoFatura pendente com os
    /// dados do boleto. Config resolvida por tenant, com fallback para a global da plataforma.
    /// </summary>
    public class GerarBoletoCommandHandler : ICommandHandler<GerarBoletoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IPaymentGateway _paymentGateway;

        public GerarBoletoCommandHandler(
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

        public async Task<CommandResult> Handle(GerarBoletoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuarioId = _currentUser.GetUserId() ?? "system";

            var fatura = await _context.Faturas.FirstOrDefaultAsync(f => f.Id == request.FaturaId, cancellationToken);
            if (fatura == null)
                return CommandResult.Falha(new[] { "Fatura não encontrada." }, "Erro");
            if (fatura.Status == FaturaStatus.Paga)
                return CommandResult.Falha(new[] { "Fatura já está paga; não é possível gerar boleto." }, "Erro");
            if (fatura.Status == FaturaStatus.Cancelada)
                return CommandResult.Falha(new[] { "Fatura cancelada; não é possível gerar boleto." }, "Erro");

            var config = await ResolverConfigAtivaAsync(tenantId, cancellationToken);
            if (config == null)
                return CommandResult.Falha("Gateway não configurado", "Nenhum gateway de pagamento ativo foi encontrado.");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == fatura.ClienteId, cancellationToken);
            if (cliente == null)
                return CommandResult.Falha(new[] { "Cliente da fatura não encontrado." }, "Erro");

            var pagador = new DadosPagador(cliente.Email, cliente.RazaoSocial, cliente.Cnpj);
            var vencimento = fatura.DataVencimento < DateTime.UtcNow ? DateTime.UtcNow.AddDays(3) : fatura.DataVencimento;

            var gatewayResult = await _paymentGateway.GerarBoletoAsync(fatura, config, pagador, vencimento, cancellationToken);
            if (!gatewayResult.Sucesso || gatewayResult.Dados is not CobrancaBoletoResultado dto)
                return gatewayResult;

            var pagamento = await _context.PagamentosFaturas
                .FirstOrDefaultAsync(p => p.FaturaId == fatura.Id
                                          && p.TipoPagamento == "Boleto"
                                          && p.Status == PagamentoFaturaStatus.Pending, cancellationToken);

            if (pagamento == null)
            {
                pagamento = new PagamentoFatura(
                    fatura.Id, "Boleto", PagamentoFaturaStatus.Pending, fatura.Valor,
                    null, dto.PaymentId, false, null, tenantId, usuarioId);
                if (!pagamento.IsValid)
                    return CommandResult.Falha(pagamento.Notifications.Select(n => n.Message), "Falha ao registrar o pagamento");
                pagamento.RegistrarCobrancaBoleto(dto.PaymentId, dto.LinhaDigitavel, dto.CodigoBarras, dto.UrlBoleto, dto.DataVencimento, usuarioId);
                _context.PagamentosFaturas.Add(pagamento);
            }
            else
            {
                pagamento.RegistrarCobrancaBoleto(dto.PaymentId, dto.LinhaDigitavel, dto.CodigoBarras, dto.UrlBoleto, dto.DataVencimento, usuarioId);
                _context.PagamentosFaturas.Update(pagamento);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Boleto gerado com sucesso.", new
            {
                paymentId = dto.PaymentId,
                linhaDigitavel = dto.LinhaDigitavel,
                codigoBarras = dto.CodigoBarras,
                urlBoleto = dto.UrlBoleto,
                dataVencimento = dto.DataVencimento
            });
        }

        private async Task<ConfiguracaoGatewayPagamento?> ResolverConfigAtivaAsync(string tenantId, CancellationToken cancellationToken)
        {
            var porTenant = await _context.ConfiguracoesGatewayPagamento
                .Where(c => c.Ativo && c.TenantAlvo == tenantId).OrderByDescending(c => c.CriadoEm).FirstOrDefaultAsync(cancellationToken);
            if (porTenant != null) return porTenant;
            return await _context.ConfiguracoesGatewayPagamento
                .Where(c => c.Ativo && c.TenantAlvo == null).OrderByDescending(c => c.CriadoEm).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
