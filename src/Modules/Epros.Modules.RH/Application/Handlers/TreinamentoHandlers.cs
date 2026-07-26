using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.RH.Application.Commands;
using Epros.Modules.RH.Application.Queries;
using Epros.Modules.RH.Domain.Entities;
using Epros.Modules.RH.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Application.Handlers
{
    public class CriarTreinamentoCommandHandler : ICommandHandler<CriarTreinamentoCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public CriarTreinamentoCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarTreinamentoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var treinamento = new LmsTreinamento(
                request.Titulo, request.Descricao, request.TipoTreinamentoId, request.TreinadorId,
                request.FilialId, request.DepartamentoId, request.DataInicio, request.DataFim,
                request.HoraInicio, request.HoraFim, request.Local, request.CapacidadeMaxima, request.Custo,
                LmsTreinamento.StScheduled, request.CriadoPorUsuarioId, request.DonoFuncionalId, tenantId, usuario);

            treinamento.ValidarRegras();
            if (!treinamento.IsValid)
                return CommandResult.Falha(treinamento.Notifications.Select(n => n.Message));

            _context.LmsTreinamentos.Add(treinamento);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Treinamento criado.", new { TreinamentoId = treinamento.Id, treinamento.Status });
        }
    }

    public class ConcluirTarefaTreinamentoCommandHandler : ICommandHandler<ConcluirTarefaTreinamentoCommand>
    {
        private readonly ContextRH _context;
        private readonly ICurrentUser _user;

        public ConcluirTarefaTreinamentoCommandHandler(ContextRH context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(ConcluirTarefaTreinamentoCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var tarefa = await _context.LmsTarefas.FirstOrDefaultAsync(t => t.Id == request.TarefaId, ct);
            if (tarefa == null) return CommandResult.Falha("Tarefa nao encontrada.");

            tarefa.Concluir(usuario);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Tarefa concluida.", new { tarefa.Id, tarefa.Status });
        }
    }

    public class RegistrarFeedbackTarefaCommandHandler : ICommandHandler<RegistrarFeedbackTarefaCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarFeedbackTarefaCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarFeedbackTarefaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var tarefa = await _context.LmsTarefas.FirstOrDefaultAsync(t => t.Id == request.TarefaId, ct);
            if (tarefa == null) return CommandResult.Falha("Tarefa nao encontrada.");

            var feedback = new LmsFeedback(
                request.TarefaId, request.UsuarioAlvoId, request.Nota, request.Comentarios,
                request.CriadoPorUsuarioId, request.DonoFuncionalId, tenantId, usuario);

            feedback.ValidarNota();
            if (!feedback.IsValid)
                return CommandResult.Falha(feedback.Notifications.Select(n => n.Message));

            _context.LmsFeedbacks.Add(feedback);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Feedback registrado.", new { FeedbackId = feedback.Id });
        }
    }

    public class ListarTreinamentosQueryHandler : IQueryHandler<ListarTreinamentosQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarTreinamentosQueryHandler(ContextRH context) => _context = context;
        public async Task<CommandResult> Handle(ListarTreinamentosQuery request, CancellationToken ct)
            => CommandResult.Ok("Treinamentos listados.", await _context.LmsTreinamentos.OrderByDescending(t => t.CriadoEm).ToListAsync(ct));
    }

    public class ListarCertificacoesQueryHandler : IQueryHandler<ListarCertificacoesQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarCertificacoesQueryHandler(ContextRH context) => _context = context;
        public async Task<CommandResult> Handle(ListarCertificacoesQuery request, CancellationToken ct)
            => CommandResult.Ok("Certificacoes listadas.", await _context.LmsCertificacaos.OrderByDescending(c => c.CriadoEm).ToListAsync(ct));
    }
}
