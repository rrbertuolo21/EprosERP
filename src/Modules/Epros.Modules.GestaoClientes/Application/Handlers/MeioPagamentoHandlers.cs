using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Interfaces;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Resolve o cliente do tenant corrente (área do cliente) para operar seus meios de pagamento.</summary>
    internal static class ClienteDoTenant
    {
        public static Task<Cliente?> ResolverAsync(ContextGestaoClientes context, string tenantId, CancellationToken ct)
            => context.Clientes.IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.DeletadoEm == null, ct);
    }

    /// <summary>
    /// 1.08B — Adiciona um cartão-on-file. Cria o cartão no gateway a partir do TOKEN do front (PCI: sem
    /// PAN/CVV) e persiste só os identificadores opacos + metadados. O primeiro cartão vira padrão.
    /// </summary>
    public class AdicionarCartaoCommandHandler : ICommandHandler<AdicionarCartaoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IPaymentGateway _paymentGateway;

        public AdicionarCartaoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser, IPaymentGateway paymentGateway)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _paymentGateway = paymentGateway;
        }

        public async Task<CommandResult> Handle(AdicionarCartaoCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CardToken))
                return CommandResult.Falha("Token do cartão é obrigatório.");

            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cliente = await ClienteDoTenant.ResolverAsync(_context, tenantId, cancellationToken);
            if (cliente == null)
                return CommandResult.Falha("Cliente do tenant corrente não encontrado.");

            var config = await ResolverConfigAtivaAsync(tenantId, cancellationToken);
            if (config == null)
                return CommandResult.Falha("Gateway de pagamento não configurado.", "Gateway não configurado");

            var pagador = new DadosPagador(cliente.Email, cliente.RazaoSocial, cliente.Cnpj);
            var resultado = await _paymentGateway.CriarCartaoOnFileAsync(config, pagador, request.CardToken, cancellationToken);
            if (!resultado.Sucesso || resultado.Dados is not CartaoOnFileResultado card)
                return resultado;

            var jaTemCartao = await _context.MeiosPagamentoClientes.IgnoreQueryFilters()
                .AnyAsync(m => m.ClienteId == cliente.Id && m.Ativo && m.DeletadoEm == null, cancellationToken);

            var padrao = request.DefinirComoPadrao || !jaTemCartao;

            if (padrao)
            {
                // Zera o padrão anterior — só um padrão por cliente.
                var atuais = await _context.MeiosPagamentoClientes.IgnoreQueryFilters()
                    .Where(m => m.ClienteId == cliente.Id && m.Padrao && m.DeletadoEm == null)
                    .ToListAsync(cancellationToken);
                foreach (var m in atuais) m.DefinirPadrao(false, usuario);
            }

            var meio = new MeioPagamentoCliente(
                clienteId: cliente.Id,
                customerIdGateway: card.CustomerId,
                cardIdGateway: card.CardId,
                bandeira: card.Bandeira,
                ultimosQuatro: card.UltimosQuatro,
                validadeMes: card.ValidadeMes,
                validadeAno: card.ValidadeAno,
                padrao: padrao,
                tenantId: tenantId,
                criadoPor: usuario);

            if (!meio.IsValid)
                return CommandResult.Falha(meio.Notifications.Select(n => n.Message));

            _context.MeiosPagamentoClientes.Add(meio);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Cartão salvo com sucesso.", new
            {
                MeioPagamentoId = meio.Id,
                meio.Bandeira,
                meio.UltimosQuatro,
                meio.Padrao
            });
        }

        private async Task<ConfiguracaoGatewayPagamento?> ResolverConfigAtivaAsync(string tenantId, CancellationToken ct)
        {
            var porTenant = await _context.ConfiguracoesGatewayPagamento
                .Where(c => c.Ativo && c.TenantAlvo == tenantId).OrderByDescending(c => c.CriadoEm).FirstOrDefaultAsync(ct);
            if (porTenant != null) return porTenant;
            return await _context.ConfiguracoesGatewayPagamento
                .Where(c => c.Ativo && c.TenantAlvo == null).OrderByDescending(c => c.CriadoEm).FirstOrDefaultAsync(ct);
        }
    }

    public class RemoverMeioPagamentoCommandHandler : ICommandHandler<RemoverMeioPagamentoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RemoverMeioPagamentoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RemoverMeioPagamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cliente = await ClienteDoTenant.ResolverAsync(_context, tenantId, cancellationToken);
            if (cliente == null)
                return CommandResult.Falha("Cliente do tenant corrente não encontrado.");

            var meio = await _context.MeiosPagamentoClientes.IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == request.MeioPagamentoId && m.ClienteId == cliente.Id && m.DeletadoEm == null, cancellationToken);
            if (meio == null)
                return CommandResult.Falha("Meio de pagamento não encontrado.");

            meio.Desativar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Meio de pagamento removido.");
        }
    }

    public class DefinirMeioPagamentoPadraoCommandHandler : ICommandHandler<DefinirMeioPagamentoPadraoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirMeioPagamentoPadraoCommandHandler(ContextGestaoClientes context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirMeioPagamentoPadraoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cliente = await ClienteDoTenant.ResolverAsync(_context, tenantId, cancellationToken);
            if (cliente == null)
                return CommandResult.Falha("Cliente do tenant corrente não encontrado.");

            var meio = await _context.MeiosPagamentoClientes.IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == request.MeioPagamentoId && m.ClienteId == cliente.Id && m.Ativo && m.DeletadoEm == null, cancellationToken);
            if (meio == null)
                return CommandResult.Falha("Meio de pagamento não encontrado ou inativo.");

            var atuais = await _context.MeiosPagamentoClientes.IgnoreQueryFilters()
                .Where(m => m.ClienteId == cliente.Id && m.Padrao && m.DeletadoEm == null)
                .ToListAsync(cancellationToken);
            foreach (var m in atuais) m.DefinirPadrao(false, usuario);

            meio.DefinirPadrao(true, usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Meio de pagamento definido como padrão.");
        }
    }

    public class ListarMeiosPagamentoQueryHandler : IQueryHandler<ListarMeiosPagamentoQuery, List<MeioPagamentoClienteDto>>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ITenantProvider _tenantProvider;

        public ListarMeiosPagamentoQueryHandler(ContextGestaoClientes context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<List<MeioPagamentoClienteDto>> Handle(ListarMeiosPagamentoQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var cliente = await ClienteDoTenant.ResolverAsync(_context, tenantId, cancellationToken);
            if (cliente == null)
                return new List<MeioPagamentoClienteDto>();

            return await _context.MeiosPagamentoClientes.IgnoreQueryFilters()
                .Where(m => m.ClienteId == cliente.Id && m.Ativo && m.DeletadoEm == null)
                .OrderByDescending(m => m.Padrao).ThenByDescending(m => m.CriadoEm)
                .Select(m => new MeioPagamentoClienteDto
                {
                    Id = m.Id,
                    Tipo = m.Tipo,
                    Bandeira = m.Bandeira,
                    UltimosQuatro = m.UltimosQuatro,
                    ValidadeMes = m.ValidadeMes,
                    ValidadeAno = m.ValidadeAno,
                    Padrao = m.Padrao
                })
                .ToListAsync(cancellationToken);
        }
    }
}
