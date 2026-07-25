using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Domain.Entities.Risco;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    // ===== PRJ-RSK (Gestao de Riscos de Projeto) =====

    public class CriarEstagioRiscoCommandHandler : ICommandHandler<CriarEstagioRiscoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEstagioRiscoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEstagioRiscoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var estagio = new EstagioRisco(request.Nome, request.Cor, request.Completo, request.Ordem, request.CriadorId, tenantId, usuario);
            if (!estagio.IsValid)
                return CommandResult.Falha(estagio.Notifications.Select(n => n.Message));

            _context.EstagiosRisco.Add(estagio);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Estagio de risco criado com sucesso!", new { estagio.Id });
        }
    }

    public class CriarRiscoProjetoCommandHandler : ICommandHandler<CriarRiscoProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRiscoProjetoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRiscoProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN-RSK-005: estagio deve existir.
            var estagioExiste = await _context.EstagiosRisco.AnyAsync(e => e.Id == request.EstagioId, cancellationToken);
            if (!estagioExiste)
                return CommandResult.Falha("Estagio invalido.");

            var risco = new RiscoProjeto(request.ProjetoId, request.Titulo, request.Prioridade, request.Descricao,
                request.EstagioId, request.Responsaveis, request.CriadorId, tenantId, usuario);
            if (!risco.IsValid)
                return CommandResult.Falha(risco.Notifications.Select(n => n.Message));

            _context.Riscos.Add(risco);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Risco de projeto criado com sucesso!", new { risco.Id });
        }
    }

    public class MoverRiscoCommandHandler : ICommandHandler<MoverRiscoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public MoverRiscoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(MoverRiscoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var risco = await _context.Riscos.Include(r => r.Historicos).FirstOrDefaultAsync(r => r.Id == request.RiscoId, cancellationToken);
            if (risco == null) return CommandResult.Falha("Risco nao encontrado.");

            // RN-RSK-007: estagio destino deve existir.
            var estagioExiste = await _context.EstagiosRisco.AnyAsync(e => e.Id == request.EstagioDestinoId, cancellationToken);
            if (!estagioExiste) return CommandResult.Falha("Estagio invalido.");

            risco.Mover(request.EstagioDestinoId, request.UsuarioId, usuario);
            if (!risco.IsValid) return CommandResult.Falha(risco.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Risco movido com sucesso!", new { risco.Id, risco.EstagioId });
        }
    }

    public class ComentarRiscoCommandHandler : ICommandHandler<ComentarRiscoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public ComentarRiscoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ComentarRiscoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var risco = await _context.Riscos
                .Include(r => r.Comentarios)
                .Include(r => r.Historicos)
                .FirstOrDefaultAsync(r => r.Id == request.RiscoId, cancellationToken);
            if (risco == null) return CommandResult.Falha("Risco nao encontrado.");

            risco.Comentar(request.UsuarioId, request.Comentario, usuario);
            if (!risco.IsValid) return CommandResult.Falha(risco.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Comentario registrado com sucesso!", new { risco.Id });
        }
    }

    public abstract class RiscoWorkflowHandlerBase
    {
        protected readonly ContextProjetos Context;
        protected readonly ICurrentUser CurrentUser;

        protected RiscoWorkflowHandlerBase(ContextProjetos context, ICurrentUser currentUser)
        {
            Context = context;
            CurrentUser = currentUser;
        }

        protected async Task<(RiscoProjeto? risco, string usuario)> CarregarAsync(Guid id, CancellationToken ct)
        {
            var usuario = CurrentUser.GetUserId() ?? "system";
            var risco = await Context.Riscos.Include(r => r.Historicos).FirstOrDefaultAsync(r => r.Id == id, ct);
            return (risco, usuario);
        }
    }

    public class AlterarPrioridadeRiscoCommandHandler : RiscoWorkflowHandlerBase, ICommandHandler<AlterarPrioridadeRiscoCommand>
    {
        public AlterarPrioridadeRiscoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(AlterarPrioridadeRiscoCommand request, CancellationToken cancellationToken)
        {
            var (risco, usuario) = await CarregarAsync(request.RiscoId, cancellationToken);
            if (risco == null) return CommandResult.Falha("Risco nao encontrado.");
            risco.AlterarPrioridade(request.Prioridade, request.UsuarioId, usuario);
            if (!risco.IsValid) return CommandResult.Falha(risco.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Prioridade alterada com sucesso!", new { risco.Id, risco.Prioridade });
        }
    }

    public class SubmeterRiscoCommandHandler : RiscoWorkflowHandlerBase, ICommandHandler<SubmeterRiscoCommand>
    {
        public SubmeterRiscoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(SubmeterRiscoCommand request, CancellationToken cancellationToken)
        {
            var (risco, usuario) = await CarregarAsync(request.RiscoId, cancellationToken);
            if (risco == null) return CommandResult.Falha("Risco nao encontrado.");
            risco.Submeter(request.UsuarioId, usuario);
            if (!risco.IsValid) return CommandResult.Falha(risco.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Risco submetido para analise.", new { risco.Id });
        }
    }

    public class AprovarRiscoCommandHandler : RiscoWorkflowHandlerBase, ICommandHandler<AprovarRiscoCommand>
    {
        public AprovarRiscoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(AprovarRiscoCommand request, CancellationToken cancellationToken)
        {
            var (risco, usuario) = await CarregarAsync(request.RiscoId, cancellationToken);
            if (risco == null) return CommandResult.Falha("Risco nao encontrado.");
            risco.Aprovar(request.UsuarioId, usuario);
            if (!risco.IsValid) return CommandResult.Falha(risco.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Risco ativado com sucesso!", new { risco.Id });
        }
    }

    public class RejeitarRiscoCommandHandler : RiscoWorkflowHandlerBase, ICommandHandler<RejeitarRiscoCommand>
    {
        public RejeitarRiscoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(RejeitarRiscoCommand request, CancellationToken cancellationToken)
        {
            var (risco, usuario) = await CarregarAsync(request.RiscoId, cancellationToken);
            if (risco == null) return CommandResult.Falha("Risco nao encontrado.");
            risco.Rejeitar(request.Motivo, request.UsuarioId, usuario);
            if (!risco.IsValid) return CommandResult.Falha(risco.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Risco rejeitado.", new { risco.Id });
        }
    }

    public class EscalonarRiscoCommandHandler : RiscoWorkflowHandlerBase, ICommandHandler<EscalonarRiscoCommand>
    {
        public EscalonarRiscoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(EscalonarRiscoCommand request, CancellationToken cancellationToken)
        {
            var (risco, usuario) = await CarregarAsync(request.RiscoId, cancellationToken);
            if (risco == null) return CommandResult.Falha("Risco nao encontrado.");
            risco.Escalonar(request.UsuarioId, usuario);
            if (!risco.IsValid) return CommandResult.Falha(risco.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Risco escalonado.", new { risco.Id });
        }
    }

    public class EncerrarRiscoCommandHandler : RiscoWorkflowHandlerBase, ICommandHandler<EncerrarRiscoCommand>
    {
        public EncerrarRiscoCommandHandler(ContextProjetos context, ICurrentUser currentUser) : base(context, currentUser) { }

        public async Task<CommandResult> Handle(EncerrarRiscoCommand request, CancellationToken cancellationToken)
        {
            var (risco, usuario) = await CarregarAsync(request.RiscoId, cancellationToken);
            if (risco == null) return CommandResult.Falha("Risco nao encontrado.");
            risco.Encerrar(request.Motivo, request.UsuarioId, usuario);
            if (!risco.IsValid) return CommandResult.Falha(risco.Notifications.Select(n => n.Message));
            await Context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Risco encerrado.", new { risco.Id });
        }
    }
}
