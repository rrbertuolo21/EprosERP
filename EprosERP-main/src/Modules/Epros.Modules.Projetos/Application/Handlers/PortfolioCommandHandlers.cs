using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Domain.Entities.Portfolio;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    // ===== PRJ-PRT (Portfolio e Priorizacao) =====

    public class CriarPortfolioCommandHandler : ICommandHandler<CriarPortfolioCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPortfolioCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPortfolioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // PRJ-PRT-RN-002: codigo unico por tenant.
            var codigoExiste = await _context.Portfolios.AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);
            if (codigoExiste)
                return CommandResult.Falha("Ja existe portfolio com este codigo para o tenant.");

            var portfolio = new Portfolio(request.Codigo, request.Descricao, request.ResponsavelId, request.TipoPortfolio, request.Justificativa, tenantId, usuario);
            if (!portfolio.IsValid)
                return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));

            _context.Portfolios.Add(portfolio);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Portfolio criado com sucesso!", new { portfolio.Id });
        }
    }

    public class AdicionarItemPortfolioCommandHandler : ICommandHandler<AdicionarItemPortfolioCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarItemPortfolioCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarItemPortfolioCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var portfolio = await _context.Portfolios.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == request.PortfolioId, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");

            portfolio.AdicionarItem(request.Sequencia, request.TipoItem, request.ProjetoId, request.ProgramaId, request.Titulo,
                request.ValorEstimado, request.EsforcoEstimado, request.CapacidadeRequerida, request.Npv, request.Payback,
                request.AlinhamentoEstrategico, request.Risco, request.Score, request.JustificativaPrioridade, request.Observacao, usuario);
            if (!portfolio.IsValid) return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item de portfolio adicionado com sucesso!", new { portfolio.Id });
        }
    }

    public class PriorizarPortfolioManualCommandHandler : ICommandHandler<PriorizarPortfolioManualCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public PriorizarPortfolioManualCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(PriorizarPortfolioManualCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var portfolio = await _context.Portfolios.Include(p => p.Historicos).FirstOrDefaultAsync(p => p.Id == request.PortfolioId, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");

            portfolio.PriorizarManual(request.ScoreTotal, request.Justificativa, request.UsuarioId, usuario);
            if (!portfolio.IsValid) return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Priorizacao manual registrada com sucesso!", new { portfolio.Id, portfolio.ScoreTotal });
        }
    }

    public class AnexarDocumentoPortfolioCommandHandler : ICommandHandler<AnexarDocumentoPortfolioCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AnexarDocumentoPortfolioCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AnexarDocumentoPortfolioCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var portfolio = await _context.Portfolios.Include(p => p.Anexos).FirstOrDefaultAsync(p => p.Id == request.PortfolioId, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");

            portfolio.AnexarDocumento(request.ArquivoId, request.ItemId, request.TipoAnexo, usuario);
            if (!portfolio.IsValid) return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Documento anexado com sucesso!", new { portfolio.Id });
        }
    }

    public abstract class PortfolioWorkflowHandlerBase
    {
        protected readonly ContextProjetos Context;
        protected readonly ICurrentUser CurrentUser;

        protected PortfolioWorkflowHandlerBase(ContextProjetos context, ICurrentUser currentUser)
        {
            Context = context;
            CurrentUser = currentUser;
        }

        protected async Task<(Portfolio? portfolio, string usuario)> CarregarAsync(Guid id, CancellationToken ct)
        {
            var usuario = CurrentUser.GetUserId() ?? "system";
            var portfolio = await Context.Portfolios.Include(p => p.Historicos).FirstOrDefaultAsync(p => p.Id == id, ct);
            return (portfolio, usuario);
        }
    }

    public class SubmeterPortfolioCommandHandler : PortfolioWorkflowHandlerBase, ICommandHandler<SubmeterPortfolioCommand>
    {
        public SubmeterPortfolioCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(SubmeterPortfolioCommand request, CancellationToken cancellationToken)
        {
            var (portfolio, usuario) = await CarregarAsync(request.PortfolioId, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");
            portfolio.Submeter(request.UsuarioId, usuario);
            if (!portfolio.IsValid) return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Portfolio submetido para analise.", new { portfolio.Id });
        }
    }

    public class AprovarPortfolioCommandHandler : PortfolioWorkflowHandlerBase, ICommandHandler<AprovarPortfolioCommand>
    {
        public AprovarPortfolioCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(AprovarPortfolioCommand request, CancellationToken cancellationToken)
        {
            var (portfolio, usuario) = await CarregarAsync(request.PortfolioId, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");
            portfolio.Aprovar(request.UsuarioId, usuario);
            if (!portfolio.IsValid) return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Portfolio aprovado com sucesso!", new { portfolio.Id });
        }
    }

    public class RejeitarPortfolioCommandHandler : PortfolioWorkflowHandlerBase, ICommandHandler<RejeitarPortfolioCommand>
    {
        public RejeitarPortfolioCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(RejeitarPortfolioCommand request, CancellationToken cancellationToken)
        {
            var (portfolio, usuario) = await CarregarAsync(request.PortfolioId, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");
            portfolio.Rejeitar(request.Motivo, request.UsuarioId, usuario);
            if (!portfolio.IsValid) return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Portfolio rejeitado.", new { portfolio.Id });
        }
    }

    public class SuspenderPortfolioCommandHandler : PortfolioWorkflowHandlerBase, ICommandHandler<SuspenderPortfolioCommand>
    {
        public SuspenderPortfolioCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(SuspenderPortfolioCommand request, CancellationToken cancellationToken)
        {
            var (portfolio, usuario) = await CarregarAsync(request.PortfolioId, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");
            portfolio.Suspender(request.UsuarioId, usuario);
            if (!portfolio.IsValid) return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Portfolio suspenso.", new { portfolio.Id });
        }
    }

    public class RetomarPortfolioCommandHandler : PortfolioWorkflowHandlerBase, ICommandHandler<RetomarPortfolioCommand>
    {
        public RetomarPortfolioCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(RetomarPortfolioCommand request, CancellationToken cancellationToken)
        {
            var (portfolio, usuario) = await CarregarAsync(request.PortfolioId, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");
            portfolio.Retomar(request.UsuarioId, usuario);
            if (!portfolio.IsValid) return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Portfolio retomado.", new { portfolio.Id });
        }
    }

    public class EncerrarPortfolioCommandHandler : PortfolioWorkflowHandlerBase, ICommandHandler<EncerrarPortfolioCommand>
    {
        public EncerrarPortfolioCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(EncerrarPortfolioCommand request, CancellationToken cancellationToken)
        {
            var (portfolio, usuario) = await CarregarAsync(request.PortfolioId, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");
            portfolio.Encerrar(request.UsuarioId, usuario);
            if (!portfolio.IsValid) return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Portfolio encerrado.", new { portfolio.Id });
        }
    }

    public class InativarPortfolioCommandHandler : PortfolioWorkflowHandlerBase, ICommandHandler<InativarPortfolioCommand>
    {
        public InativarPortfolioCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(InativarPortfolioCommand request, CancellationToken cancellationToken)
        {
            var (portfolio, usuario) = await CarregarAsync(request.PortfolioId, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");
            portfolio.Inativar(request.UsuarioId, usuario);
            if (!portfolio.IsValid) return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Portfolio inativado.", new { portfolio.Id });
        }
    }

    public class ReativarPortfolioCommandHandler : PortfolioWorkflowHandlerBase, ICommandHandler<ReativarPortfolioCommand>
    {
        public ReativarPortfolioCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(ReativarPortfolioCommand request, CancellationToken cancellationToken)
        {
            var (portfolio, usuario) = await CarregarAsync(request.PortfolioId, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");
            portfolio.Reativar(request.UsuarioId, usuario);
            if (!portfolio.IsValid) return CommandResult.Falha(portfolio.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Portfolio reativado.", new { portfolio.Id });
        }
    }
}
