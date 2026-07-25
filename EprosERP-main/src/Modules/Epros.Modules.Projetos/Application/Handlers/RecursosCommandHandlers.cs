using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Domain.Entities.Recursos;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    public class RegistrarApontamentoCommandHandler : ICommandHandler<RegistrarApontamentoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarApontamentoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarApontamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var apontamento = new RecursoTimesheet(
                request.UsuarioId,
                request.ProjetoId,
                request.TarefaId,
                request.Data,
                request.Horas,
                request.Minutos,
                request.Notas,
                request.Tipo,
                tenantId,
                usuario);

            if (!apontamento.IsValid)
                return CommandResult.Falha(apontamento.Notifications.Select(n => n.Message));

            _context.RecursoTimesheets.Add(apontamento);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Apontamento registrado com sucesso!", new { apontamento.Id });
        }
    }

    public class SubmeterApontamentoCommandHandler : ICommandHandler<SubmeterApontamentoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public SubmeterApontamentoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SubmeterApontamentoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var apontamento = await _context.RecursoTimesheets.FirstOrDefaultAsync(t => t.Id == request.TimesheetId, cancellationToken);
            if (apontamento == null)
                return CommandResult.Falha("Apontamento nao encontrado.");

            apontamento.Submeter(usuario);
            if (!apontamento.IsValid)
                return CommandResult.Falha(apontamento.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Apontamento submetido para aprovacao.", new { apontamento.Id });
        }
    }

    public class AprovarApontamentoCommandHandler : ICommandHandler<AprovarApontamentoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AprovarApontamentoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarApontamentoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var apontamento = await _context.RecursoTimesheets.FirstOrDefaultAsync(t => t.Id == request.TimesheetId, cancellationToken);
            if (apontamento == null)
                return CommandResult.Falha("Apontamento nao encontrado.");

            apontamento.Aprovar(usuario);
            if (!apontamento.IsValid)
                return CommandResult.Falha(apontamento.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Apontamento aprovado com sucesso!", new { apontamento.Id });
        }
    }

    public class CriarAlocacaoRecursoCommandHandler : ICommandHandler<CriarAlocacaoRecursoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarAlocacaoRecursoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarAlocacaoRecursoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var alocacao = new RecursoAlocacao(
                request.RecursoId,
                request.ProjetoId,
                request.TarefaId,
                request.PapelNoProjeto,
                request.DataInicio,
                request.DataFim,
                request.CargaPlanejadaHoras,
                tenantId,
                usuario);

            if (!alocacao.IsValid)
                return CommandResult.Falha(alocacao.Notifications.Select(n => n.Message));

            _context.RecursoAlocacoes.Add(alocacao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Alocacao de recurso criada com sucesso!", new { alocacao.Id });
        }
    }
}
