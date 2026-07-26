using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Domain.Entities.Rastreamento;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    public class CriarEstagioTarefaCommandHandler : ICommandHandler<CriarEstagioTarefaCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEstagioTarefaCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEstagioTarefaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var estagio = new EstagioTarefa(request.Nome, request.Cor, request.IndicadorConclusao, request.Ordem, tenantId, usuario);
            if (!estagio.IsValid)
                return CommandResult.Falha(estagio.Notifications.Select(n => n.Message));

            _context.EstagiosTarefa.Add(estagio);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Estagio criado com sucesso!", new { estagio.Id });
        }
    }

    public class CriarTarefaProjetoCommandHandler : ICommandHandler<CriarTarefaProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarTarefaProjetoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarTarefaProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var projetoExiste = await _context.Projetos.AnyAsync(p => p.Id == request.ProjetoId, cancellationToken);
            if (!projetoExiste)
                return CommandResult.Falha("Projeto nao encontrado.");

            var tarefa = new TarefaProjeto(
                request.ProjetoId,
                request.Titulo,
                request.Descricao,
                request.EstagioId,
                request.MarcoId,
                request.Prioridade,
                request.DataInicio,
                request.DataTermino,
                request.Duracao,
                request.EsforcoEstimado,
                request.TarefaSuperiorId,
                request.IndicadorMarco,
                request.Visibilidade,
                request.Ordem,
                tenantId,
                usuario);

            if (!tarefa.IsValid)
                return CommandResult.Falha(tarefa.Notifications.Select(n => n.Message));

            _context.TarefasProjeto.Add(tarefa);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Tarefa criada com sucesso!", new { tarefa.Id });
        }
    }

    public class MoverTarefaQuadroCommandHandler : ICommandHandler<MoverTarefaQuadroCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public MoverTarefaQuadroCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(MoverTarefaQuadroCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var tarefa = await _context.TarefasProjeto.FirstOrDefaultAsync(t => t.Id == request.TarefaId, cancellationToken);
            if (tarefa == null)
                return CommandResult.Falha("Tarefa nao encontrada.");

            var estagio = await _context.EstagiosTarefa.FirstOrDefaultAsync(e => e.Id == request.EstagioId, cancellationToken);
            if (estagio == null)
                return CommandResult.Falha("Estagio de destino nao encontrado.");

            tarefa.MoverNoQuadro(request.EstagioId, request.NovaOrdem, estagio.IndicadorConclusao, usuario);
            if (!tarefa.IsValid)
                return CommandResult.Falha(tarefa.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Tarefa movida no quadro com sucesso!", new { tarefa.Id, tarefa.EstagioId, tarefa.Ordem });
        }
    }

    public class AtualizarProgressoTarefaProjetoCommandHandler : ICommandHandler<AtualizarProgressoTarefaProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarProgressoTarefaProjetoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarProgressoTarefaProjetoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tarefa = await _context.TarefasProjeto.FirstOrDefaultAsync(t => t.Id == request.TarefaId, cancellationToken);
            if (tarefa == null)
                return CommandResult.Falha("Tarefa nao encontrada.");

            tarefa.AtualizarProgresso(request.PercentualConcluido, usuario);
            if (!tarefa.IsValid)
                return CommandResult.Falha(tarefa.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Progresso da tarefa atualizado com sucesso!", new { tarefa.Id, tarefa.PercentualConcluido });
        }
    }

    public class ConcluirTarefaProjetoCommandHandler : ICommandHandler<ConcluirTarefaProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public ConcluirTarefaProjetoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConcluirTarefaProjetoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tarefa = await _context.TarefasProjeto.FirstOrDefaultAsync(t => t.Id == request.TarefaId, cancellationToken);
            if (tarefa == null)
                return CommandResult.Falha("Tarefa nao encontrada.");

            // PRJ-RST-RN-013: bloqueada por dependencia pendente nao pode concluir.
            var possuiDependenciaAberta = await _context.DependenciasTarefa
                .AnyAsync(d => d.TarefaDependenteId == request.TarefaId && d.Bloqueada, cancellationToken);

            tarefa.Concluir(possuiDependenciaAberta, usuario);
            if (!tarefa.IsValid)
                return CommandResult.Falha(tarefa.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Tarefa concluida com sucesso!", new { tarefa.Id });
        }
    }

    public class CriarDependenciaTarefaCommandHandler : ICommandHandler<CriarDependenciaTarefaCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarDependenciaTarefaCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarDependenciaTarefaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var dependencia = new DependenciaTarefa(
                request.TarefaDependenteId,
                request.TarefaPredecessoraId,
                request.TipoDependencia,
                request.Observacao,
                tenantId,
                usuario);

            if (!dependencia.IsValid)
                return CommandResult.Falha(dependencia.Notifications.Select(n => n.Message));

            _context.DependenciasTarefa.Add(dependencia);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Dependencia criada com sucesso!", new { dependencia.Id });
        }
    }
}
