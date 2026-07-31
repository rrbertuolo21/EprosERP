using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Interfaces;
using Epros.Modules.GestaoClientes.Application.Services;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epros.Modules.GestaoClientes.Infrastructure.Gateways
{
    /// <summary>
    /// 1.08B — Implementação CONCRETA da cobrança recorrente por cartão-on-file (substitui o no-op da
    /// passada A). Se o cliente tem um cartão salvo padrão (<see cref="MeioPagamentoCliente"/>), cobra a
    /// fatura nele via <see cref="IPaymentGateway.CobrarCartaoAsync"/> (cartão já tokenizado no MP) e, quando
    /// aprovado, liquida a fatura pelo caminho único da passada A (<see cref="FaturaLiquidacaoService"/>).
    /// Falha na cobrança → deixa a fatura em aberto (cai no fluxo de inadimplência do
    /// <c>VerificarFaturasVencidasJob</c>).
    ///
    /// ⛔ PCI: nenhum dado de cartão cru (PAN/CVV) passa por aqui — só os identificadores opacos
    /// (customer_id/card_id) já salvos. Segredos do gateway seguem cifrados no cofre.
    /// </summary>
    public class CobrancaRecorrenteGatewayMercadoPago : ICobrancaRecorrenteGateway
    {
        private readonly ContextGestaoClientes _context;
        private readonly IPaymentGateway _paymentGateway;
        private readonly FaturaLiquidacaoService _liquidacao;
        private readonly ILogger<CobrancaRecorrenteGatewayMercadoPago>? _logger;

        public CobrancaRecorrenteGatewayMercadoPago(
            ContextGestaoClientes context,
            IPaymentGateway paymentGateway,
            FaturaLiquidacaoService liquidacao,
            ILogger<CobrancaRecorrenteGatewayMercadoPago>? logger = null)
        {
            _context = context;
            _paymentGateway = paymentGateway;
            _liquidacao = liquidacao;
            _logger = logger;
        }

        public async Task<bool> PossuiCartaoOnFileAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            return await _context.MeiosPagamentoClientes
                .IgnoreQueryFilters()
                .AnyAsync(m => m.ClienteId == clienteId && m.Ativo && m.DeletadoEm == null, cancellationToken);
        }

        public async Task<CommandResult> CobrarCartaoRecorrenteAsync(Fatura fatura, Guid clienteId, CancellationToken cancellationToken = default)
        {
            if (fatura.Status == FaturaStatus.Paga)
                return CommandResult.Ok("Fatura já está paga.");

            // Meio padrão do cliente; senão o mais recente ativo.
            var cartao = await _context.MeiosPagamentoClientes
                .IgnoreQueryFilters()
                .Where(m => m.ClienteId == clienteId && m.Ativo && m.DeletadoEm == null)
                .OrderByDescending(m => m.Padrao)
                .ThenByDescending(m => m.CriadoEm)
                .FirstOrDefaultAsync(cancellationToken);

            if (cartao == null)
                return CommandResult.Falha("Cliente não possui cartão-on-file.", "Cartão-on-file indisponível");

            var config = await ResolverConfigAtivaAsync(fatura.TenantId, cancellationToken);
            if (config == null)
                return CommandResult.Falha("Gateway de pagamento não configurado.", "Gateway não configurado");

            var cliente = await _context.Clientes.IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == clienteId && c.DeletadoEm == null, cancellationToken);
            var pagador = new DadosPagador(cliente?.Email ?? "sem-email@epros.com", cliente?.RazaoSocial, cliente?.Cnpj);

            var cobranca = await _paymentGateway.CobrarCartaoAsync(
                fatura, config, cartao.CustomerIdGateway, cartao.CardIdGateway, pagador, cancellationToken);

            if (!cobranca.Sucesso || cobranca.Dados is not ConsultaPagamentoResultado pg)
            {
                _logger?.LogWarning("[CobrancaRecorrente] Falha ao cobrar cartão da fatura {FaturaId}: {Msg}", fatura.Id, cobranca.Mensagem);
                return CommandResult.Falha(cobranca.Erros?.Any() == true ? cobranca.Erros : new[] { "Falha na cobrança do cartão." }, "Cobrança recusada");
            }

            var status = (pg.Status ?? string.Empty).ToLowerInvariant();
            if (status != "approved" && status != "paid" && status != "succeeded")
            {
                // Não aprovado → não liquida; deixa cair na inadimplência.
                _logger?.LogInformation("[CobrancaRecorrente] Cartão não aprovado (status {Status}) para fatura {FaturaId}.", pg.Status, fatura.Id);
                return CommandResult.Falha($"Pagamento no cartão não aprovado (status '{pg.Status}').", "Cobrança recusada");
            }

            var valorBruto = pg.ValorTransacao ?? fatura.Valor;
            var recibo = await _liquidacao.LiquidarAsync(
                fatura, pg.PaymentId, "Cartao", "MercadoPago", valorBruto, pg.ValorTarifa, pg.ValorLiquido, pg.DataAprovacao, "system-cobranca-recorrente", cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cobrança recorrente no cartão aprovada e fatura liquidada.", new
            {
                FaturaId = fatura.Id,
                PaymentId = pg.PaymentId,
                ReciboNumero = recibo?.Numero
            });
        }

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
