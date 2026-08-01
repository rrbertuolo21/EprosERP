using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Interfaces;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Application.Security;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Utilitários de máscara de segredos para exposição segura em queries.</summary>
    internal static class SegredoMascaraExtensions
    {
        /// <summary>Descriptografa e devolve apenas os últimos 4 caracteres mascarados; nunca lança.</summary>
        public static async Task<string> MascararAsync(this ISegredoCofreService cofre, string? ciphertext)
        {
            if (string.IsNullOrWhiteSpace(ciphertext)) return string.Empty;
            try
            {
                var plain = await cofre.DescriptografarAsync(ciphertext);
                if (string.IsNullOrEmpty(plain)) return "••••";
                var ultimos = plain.Length <= 4 ? plain : plain.Substring(plain.Length - 4);
                return $"••••{ultimos}";
            }
            catch
            {
                return "••••";
            }
        }
    }

    // ===================== Commands =====================

    /// <summary>Cria configuração de gateway (cifra os segredos antes de persistir).</summary>
    public class CriarGatewayPagamentoCommandHandler : ICommandHandler<CriarGatewayPagamentoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        private readonly ISegredoCofreService _cofreService;
        private readonly ITenantProvider _tenantProvider;

        public CriarGatewayPagamentoCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser, ISegredoCofreService cofreService, ITenantProvider tenantProvider)
        {
            _context = context;
            _currentUser = currentUser;
            _cofreService = cofreService;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(CriarGatewayPagamentoCommand request, CancellationToken cancellationToken)
        {
            // 1.11 fix #2 — configuração de gateway (chaves/segredos) é landlord (só operador interno).
            var guarda = GuardaOperadorInterno.Exigir(_tenantProvider);
            if (guarda != null) return guarda;

            var criadoPor = _currentUser.GetUserId() ?? "system";

            var accessTokenCifrado = await _cofreService.CriptografarAsync(request.AccessToken);
            var webhookSecretCifrado = string.IsNullOrWhiteSpace(request.WebhookSecret)
                ? null
                : await _cofreService.CriptografarAsync(request.WebhookSecret!);

            var config = new ConfiguracaoGatewayPagamento(
                request.Provedor,
                request.Ambiente,
                accessTokenCifrado,
                request.PublicKey,
                webhookSecretCifrado,
                request.Moeda,
                request.NotificationUrl,
                request.TenantId,
                request.Ativo,
                criadoPor);

            if (!config.IsValid)
                return CommandResult.Falha(config.Notifications.Select(n => n.Message), "Falha na validação do gateway");

            _context.ConfiguracoesGatewayPagamento.Add(config);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Gateway de pagamento cadastrado com sucesso!", new { GatewayId = config.Id });
        }
    }

    /// <summary>Atualiza configuração de gateway (recifra segredos apenas quando informados).</summary>
    public class AtualizarGatewayPagamentoCommandHandler : ICommandHandler<AtualizarGatewayPagamentoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        private readonly ISegredoCofreService _cofreService;
        private readonly ITenantProvider _tenantProvider;

        public AtualizarGatewayPagamentoCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser, ISegredoCofreService cofreService, ITenantProvider tenantProvider)
        {
            _context = context;
            _currentUser = currentUser;
            _cofreService = cofreService;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(AtualizarGatewayPagamentoCommand request, CancellationToken cancellationToken)
        {
            // 1.11 fix #2 — configuração de gateway (chaves/segredos) é landlord (só operador interno).
            var guarda = GuardaOperadorInterno.Exigir(_tenantProvider);
            if (guarda != null) return guarda;

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var config = await _context.ConfiguracoesGatewayPagamento
                .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
            if (config == null)
                return CommandResult.Falha(new[] { "Gateway não encontrado." }, "Erro");

            var accessTokenCifrado = string.IsNullOrWhiteSpace(request.AccessToken)
                ? null
                : await _cofreService.CriptografarAsync(request.AccessToken!);
            var webhookSecretCifrado = string.IsNullOrWhiteSpace(request.WebhookSecret)
                ? null
                : await _cofreService.CriptografarAsync(request.WebhookSecret!);

            config.Atualizar(
                request.Provedor,
                request.Ambiente,
                accessTokenCifrado,
                request.PublicKey,
                webhookSecretCifrado,
                request.Moeda,
                request.NotificationUrl,
                request.TenantId,
                request.Ativo,
                alteradoPor);

            if (!config.IsValid)
                return CommandResult.Falha(config.Notifications.Select(n => n.Message), "Falha na validação do gateway");

            _context.ConfiguracoesGatewayPagamento.Update(config);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Gateway de pagamento atualizado com sucesso!", new { GatewayId = config.Id });
        }
    }

    /// <summary>Exclui (soft-delete) configuração de gateway.</summary>
    public class ExcluirGatewayPagamentoCommandHandler : ICommandHandler<ExcluirGatewayPagamentoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        private readonly ITenantProvider _tenantProvider;

        public ExcluirGatewayPagamentoCommandHandler(ContextGestaoClientes context, ICurrentUser currentUser, ITenantProvider tenantProvider)
        {
            _context = context;
            _currentUser = currentUser;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(ExcluirGatewayPagamentoCommand request, CancellationToken cancellationToken)
        {
            // 1.11 fix #2 — excluir gateway é landlord (só operador interno).
            var guarda = GuardaOperadorInterno.Exigir(_tenantProvider);
            if (guarda != null) return guarda;

            var deletadoPor = _currentUser.GetUserId() ?? "system";

            var config = await _context.ConfiguracoesGatewayPagamento
                .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
            if (config == null)
                return CommandResult.Falha(new[] { "Gateway não encontrado." }, "Erro");

            config.Deletar(deletadoPor);
            _context.ConfiguracoesGatewayPagamento.Update(config);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Gateway de pagamento excluído com sucesso!");
        }
    }

    /// <summary>Testa a conexão do gateway chamando o provedor (GET leve).</summary>
    public class TestarConexaoGatewayPagamentoCommandHandler : ICommandHandler<TestarConexaoGatewayPagamentoCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ITenantProvider _tenantProvider;

        public TestarConexaoGatewayPagamentoCommandHandler(ContextGestaoClientes context, IPaymentGateway paymentGateway, ITenantProvider tenantProvider)
        {
            _context = context;
            _paymentGateway = paymentGateway;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(TestarConexaoGatewayPagamentoCommand request, CancellationToken cancellationToken)
        {
            // 1.11 fix #2 — testar gateway (usa segredos) é landlord (só operador interno).
            var guarda = GuardaOperadorInterno.Exigir(_tenantProvider);
            if (guarda != null) return guarda;

            var config = await _context.ConfiguracoesGatewayPagamento
                .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
            if (config == null)
                return CommandResult.Falha(new[] { "Gateway não encontrado." }, "Erro");

            return await _paymentGateway.TestarConexaoAsync(config, cancellationToken);
        }
    }

    // ===================== Queries =====================

    /// <summary>Lista gateways (paginado), mascarando os segredos.</summary>
    public class ListarGatewaysPagamentoQueryHandler : IQueryHandler<ListarGatewaysPagamentoQuery, PagedQueryResult<GatewayPagamentoListaDto>>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ISegredoCofreService _cofreService;

        public ListarGatewaysPagamentoQueryHandler(ContextGestaoClientes context, ISegredoCofreService cofreService)
        {
            _context = context;
            _cofreService = cofreService;
        }

        public async Task<PagedQueryResult<GatewayPagamentoListaDto>> Handle(ListarGatewaysPagamentoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ConfiguracoesGatewayPagamento.AsNoTracking();

            var totalRegistros = await query.CountAsync(cancellationToken);
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)request.TamanhoPagina);

            var configs = await query
                .OrderByDescending(g => g.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            var items = new System.Collections.Generic.List<GatewayPagamentoListaDto>();
            foreach (var g in configs)
            {
                items.Add(new GatewayPagamentoListaDto
                {
                    Id = g.Id,
                    Provedor = g.Provedor.ToString(),
                    Ambiente = g.Ambiente.ToString(),
                    PublicKey = g.PublicKey,
                    Moeda = g.Moeda,
                    NotificationUrl = g.NotificationUrl,
                    TenantId = g.TenantAlvo,
                    Ativo = g.Ativo,
                    AccessTokenMascarado = await _cofreService.MascararAsync(g.AccessToken),
                    PossuiWebhookSecret = !string.IsNullOrWhiteSpace(g.WebhookSecret),
                    CriadoEm = g.CriadoEm
                });
            }

            return new PagedQueryResult<GatewayPagamentoListaDto>(items, totalRegistros, totalPaginas);
        }
    }

    /// <summary>Obtém gateway por Id, mascarando os segredos.</summary>
    public class ObterGatewayPagamentoPorIdQueryHandler : IQueryHandler<ObterGatewayPagamentoPorIdQuery, GatewayPagamentoDetalhadoDto>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ISegredoCofreService _cofreService;

        public ObterGatewayPagamentoPorIdQueryHandler(ContextGestaoClientes context, ISegredoCofreService cofreService)
        {
            _context = context;
            _cofreService = cofreService;
        }

        public async Task<GatewayPagamentoDetalhadoDto> Handle(ObterGatewayPagamentoPorIdQuery request, CancellationToken cancellationToken)
        {
            var g = await _context.ConfiguracoesGatewayPagamento
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (g == null) return null!;

            return new GatewayPagamentoDetalhadoDto
            {
                Id = g.Id,
                Provedor = g.Provedor.ToString(),
                Ambiente = g.Ambiente.ToString(),
                PublicKey = g.PublicKey,
                Moeda = g.Moeda,
                NotificationUrl = g.NotificationUrl,
                TenantId = g.TenantAlvo,
                Ativo = g.Ativo,
                AccessTokenMascarado = await _cofreService.MascararAsync(g.AccessToken),
                PossuiWebhookSecret = !string.IsNullOrWhiteSpace(g.WebhookSecret),
                CriadoEm = g.CriadoEm
            };
        }
    }
}
