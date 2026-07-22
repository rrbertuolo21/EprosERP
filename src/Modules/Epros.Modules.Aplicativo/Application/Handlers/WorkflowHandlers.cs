using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Domain.Entities.Workflow;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    /// <summary>Handlers do motor de Workflow genérico (PLT-WF). Publicação de eventos via Outbox.</summary>
    public class WorkflowHandlers :
        ICommandHandler<CriarWfDefinicaoCommand>,
        ICommandHandler<AdicionarWfEstadoCommand>,
        ICommandHandler<AdicionarWfTransicaoCommand>,
        ICommandHandler<AtivarWfDefinicaoCommand>,
        ICommandHandler<CriarWfInstanciaCommand>,
        ICommandHandler<TransicionarWfInstanciaCommand>,
        ICommandHandler<CriarWfTarefaCommand>,
        ICommandHandler<ConcluirWfTarefaCommand>,
        ICommandHandler<CriarWfSolicitacaoCommand>,
        ICommandHandler<DecidirWfSolicitacaoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkflowHandlers(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
        }

        private string Tenant() => _tenantProvider.GetTenantId();
        private string User() => _currentUser.GetUserId() ?? "system";
        private string? Ip() => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public async Task<CommandResult> Handle(CriarWfDefinicaoCommand request, CancellationToken ct)
        {
            var def = new WfDefinicao(request.Modulo, request.Entidade, request.Nome, request.Versao, null, Tenant(), User());
            if (!def.IsValid) return CommandResult.Falha(def.Notifications.Select(n => n.Message));

            _context.WfDefinicoes.Add(def);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Definição de workflow criada com sucesso.", new { def.Id });
        }

        public async Task<CommandResult> Handle(AdicionarWfEstadoCommand request, CancellationToken ct)
        {
            var estado = new WfEstado(request.DefinicaoId, request.Codigo, request.Nome, request.Inicial, request.Final, request.Ativo, Tenant(), User());
            if (!estado.IsValid) return CommandResult.Falha(estado.Notifications.Select(n => n.Message));

            _context.WfEstados.Add(estado);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Estado adicionado à definição.", new { estado.Id });
        }

        public async Task<CommandResult> Handle(AdicionarWfTransicaoCommand request, CancellationToken ct)
        {
            var transicao = new WfTransicao(request.DefinicaoId, request.EstadoOrigemId, request.EstadoDestinoId, request.Evento, request.PermissaoRequerida, request.ExigeComentario, request.PublicaEvento, Tenant(), User());
            if (!transicao.IsValid) return CommandResult.Falha(transicao.Notifications.Select(n => n.Message));

            _context.WfTransicoes.Add(transicao);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Transição adicionada à definição.", new { transicao.Id });
        }

        public async Task<CommandResult> Handle(AtivarWfDefinicaoCommand request, CancellationToken ct)
        {
            var def = await _context.WfDefinicoes.FirstOrDefaultAsync(d => d.Id == request.DefinicaoId && d.DeletadoEm == null, ct);
            if (def == null) return CommandResult.Falha("Definição de workflow não encontrada.");

            def.Ativar(User());
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Definição de workflow ativada.");
        }

        public async Task<CommandResult> Handle(CriarWfInstanciaCommand request, CancellationToken ct)
        {
            var tenant = Tenant();
            var user = User();

            // Estado inicial da definição (inicial = true), quando existir.
            var estadoInicial = await _context.WfEstados.AsNoTracking()
                .Where(e => e.DefinicaoId == request.DefinicaoId && e.Inicial && e.DeletadoEm == null)
                .Select(e => (Guid?)e.Id)
                .FirstOrDefaultAsync(ct);

            var instancia = new WfInstancia(
                request.DefinicaoId, request.Modulo, request.EntidadeTipo, request.EntidadeId, request.Descricao,
                estadoInicial, request.ResponsavelUsuarioId, request.ValorReferencia, request.CommandType, request.Payload,
                tenant, user);

            if (!instancia.IsValid) return CommandResult.Falha(instancia.Notifications.Select(n => n.Message));

            _context.WfInstancias.Add(instancia);

            RegistrarHistorico(instancia.Id, request.EntidadeTipo, request.EntidadeId, "Criacao", null, EWfInstanciaStatus.Rascunho.ToString(), tenant, user);

            // Publica AprovacaoSolicitada via Outbox — contrato para qualquer módulo pedir aprovação por alçada.
            var evento = new AprovacaoSolicitadaEventNotification(
                InstanciaId: instancia.Id,
                DefinicaoId: request.DefinicaoId,
                Modulo: request.Modulo,
                EntidadeTipo: request.EntidadeTipo,
                EntidadeId: request.EntidadeId,
                Descricao: request.Descricao,
                ValorReferencia: request.ValorReferencia,
                CommandType: request.CommandType,
                Payload: request.Payload,
                SolicitadoPor: user,
                TenantId: tenant,
                UserId: user);

            _context.OutboxMessages.Add(new OutboxMessage(tenant, "AprovacaoSolicitada", JsonSerializer.Serialize(evento)));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Solicitação de aprovação registrada no workflow.", new { instancia.Id });
        }

        public async Task<CommandResult> Handle(TransicionarWfInstanciaCommand request, CancellationToken ct)
        {
            var tenant = Tenant();
            var user = User();

            var instancia = await _context.WfInstancias.FirstOrDefaultAsync(i => i.Id == request.InstanciaId && i.DeletadoEm == null, ct);
            if (instancia == null) return CommandResult.Falha("Instância de workflow não encontrada.");

            var usuarioGuid = Guid.TryParse(user, out var g) ? (Guid?)g : null;
            var estadoAnterior = instancia.Status.ToString();

            switch (request.Evento)
            {
                case EWfEvento.Submeter:
                    instancia.Submeter(request.EstadoDestinoId, user);
                    break;
                case EWfEvento.Aprovar:
                    instancia.Aprovar(request.EstadoDestinoId, usuarioGuid, user, request.Comentario, user);
                    break;
                case EWfEvento.Rejeitar:
                    instancia.Rejeitar(request.EstadoDestinoId, usuarioGuid, user, request.Comentario, user);
                    break;
                case EWfEvento.Inativar:
                    instancia.Inativar(request.EstadoDestinoId, user);
                    break;
                case EWfEvento.Encerrar:
                    instancia.Encerrar(request.EstadoDestinoId, user);
                    break;
                case EWfEvento.Reativar:
                    instancia.Reativar(request.EstadoDestinoId, user);
                    break;
                default:
                    return CommandResult.Falha("Evento de transição inválido.");
            }

            if (!instancia.IsValid) return CommandResult.Falha(instancia.Notifications.Select(n => n.Message));

            RegistrarHistorico(instancia.Id, instancia.EntidadeTipo, instancia.EntidadeIdReferencia, request.Evento.ToString(), estadoAnterior, instancia.Status.ToString(), tenant, user);

            // Ao concluir a decisão (aprovar/rejeitar), publica AprovacaoConcluida via Outbox.
            if (request.Evento == EWfEvento.Aprovar || request.Evento == EWfEvento.Rejeitar)
            {
                var aprovado = request.Evento == EWfEvento.Aprovar;
                var evento = new AprovacaoConcluidaEventNotification(
                    InstanciaId: instancia.Id,
                    DefinicaoId: instancia.DefinicaoId,
                    Modulo: instancia.Modulo,
                    EntidadeTipo: instancia.EntidadeTipo,
                    EntidadeId: instancia.EntidadeIdReferencia,
                    Aprovado: aprovado,
                    Comentario: request.Comentario,
                    CommandType: instancia.CommandType,
                    Payload: instancia.Payload,
                    AprovadoPor: user,
                    TenantId: tenant,
                    UserId: user);

                _context.OutboxMessages.Add(new OutboxMessage(tenant, "AprovacaoConcluida", JsonSerializer.Serialize(evento)));
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"Transição '{request.Evento}' aplicada com sucesso.", new { instancia.Id, Status = instancia.Status.ToString() });
        }

        public async Task<CommandResult> Handle(CriarWfTarefaCommand request, CancellationToken ct)
        {
            var tarefa = new WfTarefa(request.InstanciaId, request.Titulo, request.ResponsavelUsuarioId, request.ResponsavelPapel, request.PrazoEm, Tenant(), User());
            if (!tarefa.IsValid) return CommandResult.Falha(tarefa.Notifications.Select(n => n.Message));

            _context.WfTarefas.Add(tarefa);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Tarefa humana criada.", new { tarefa.Id });
        }

        public async Task<CommandResult> Handle(ConcluirWfTarefaCommand request, CancellationToken ct)
        {
            var tarefa = await _context.WfTarefas.FirstOrDefaultAsync(t => t.Id == request.TarefaId && t.DeletadoEm == null, ct);
            if (tarefa == null) return CommandResult.Falha("Tarefa não encontrada.");

            tarefa.Concluir(User());
            if (!tarefa.IsValid) return CommandResult.Falha(tarefa.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Tarefa concluída.");
        }

        public async Task<CommandResult> Handle(CriarWfSolicitacaoCommand request, CancellationToken ct)
        {
            var solicitacao = new WfSolicitacao(request.StartDate, request.EndDate, request.TotalDays, request.Reason, request.Attachment, request.EmployeeId, request.LeaveTypeId, request.CreatorId, Tenant(), User());
            if (!solicitacao.IsValid) return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message));

            _context.WfSolicitacoes.Add(solicitacao);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Solicitação registrada (pendente).", new { solicitacao.Id });
        }

        public async Task<CommandResult> Handle(DecidirWfSolicitacaoCommand request, CancellationToken ct)
        {
            var user = User();
            var solicitacao = await _context.WfSolicitacoes.FirstOrDefaultAsync(s => s.Id == request.SolicitacaoId && s.DeletadoEm == null, ct);
            if (solicitacao == null) return CommandResult.Falha("Solicitação não encontrada.");

            if (request.Aprovar) solicitacao.Aprovar(user, user);
            else solicitacao.Rejeitar(user, request.ApproverComment, user);

            if (!solicitacao.IsValid) return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok(request.Aprovar ? "Solicitação aprovada." : "Solicitação rejeitada.");
        }

        private void RegistrarHistorico(Guid instanciaId, string entidadeTipo, string entidadeId, string acao, string? estadoAnterior, string? estadoNovo, string tenant, string user)
        {
            var usuarioGuid = Guid.TryParse(user, out var g) ? (Guid?)g : null;
            var hist = new WfHistorico(instanciaId, entidadeTipo, entidadeId, acao, estadoAnterior, estadoNovo, usuarioGuid, Ip(), null, tenant, user);
            _context.WfHistoricos.Add(hist);
        }
    }
}
