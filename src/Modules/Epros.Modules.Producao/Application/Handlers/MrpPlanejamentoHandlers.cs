using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Handlers
{
    /// <summary>PRD-MRP — Criação do ciclo MRP/IBP (MRP-EF §7.1).</summary>
    public class CriarMrpPlanejamentoCommandHandler : ICommandHandler<CriarMrpPlanejamentoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarMrpPlanejamentoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarMrpPlanejamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.MrpPlanejamentos.AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
                return CommandResult.Falha($"Código de planejamento '{request.Codigo}' já está em uso.");

            var planejamento = new MrpPlanejamento(request.Codigo, request.ResponsavelId, tenantId, usuario);
            if (!planejamento.IsValid)
                return CommandResult.Falha(planejamento.Notifications.Select(n => n.Message));

            _context.MrpPlanejamentos.Add(planejamento);
            _context.MrpHistoricos.Add(new MrpPlanejamentoHistorico(planejamento.Id, "Criacao", usuario, "{}", tenantId, usuario, null, EStatusWorkflowProducao.Rascunho));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Planejamento MRP/IBP criado em Rascunho.", new { planejamento.Id, planejamento.Codigo });
        }
    }

    public abstract class MrpTransicaoHandlerBase
    {
        protected readonly ContextProducao _context;
        protected readonly ITenantProvider _tenantProvider;
        protected readonly ICurrentUser _currentUser;

        protected MrpTransicaoHandlerBase(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        protected async Task<(MrpPlanejamento? planejamento, string usuario, string tenantId)> CarregarAsync(Guid id, CancellationToken ct)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();
            var planejamento = await _context.MrpPlanejamentos.FirstOrDefaultAsync(p => p.Id == id, ct);
            return (planejamento, usuario, tenantId);
        }

        protected async Task<CommandResult> FinalizarAsync(MrpPlanejamento p, string acao, string usuario, string tenantId, CancellationToken ct)
        {
            if (!p.IsValid)
                return CommandResult.Falha(p.Notifications.Select(n => n.Message));

            _context.MrpHistoricos.Add(new MrpPlanejamentoHistorico(p.Id, acao, usuario, "{}", tenantId, usuario, null, p.Status));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"Planejamento {acao} com sucesso.", new { p.Id, Status = p.Status.ToString() });
        }
    }

    public class SubmeterMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<SubmeterMrpPlanejamentoCommand>
    {
        public SubmeterMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(SubmeterMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.SubmeterParaAnalise(usuario);
            return await FinalizarAsync(p, "Submissao", usuario, tenantId, ct);
        }
    }

    public class AprovarMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<AprovarMrpPlanejamentoCommand>
    {
        public AprovarMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(AprovarMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.Aprovar(usuario);
            return await FinalizarAsync(p, "Aprovacao", usuario, tenantId, ct);
        }
    }

    public class RejeitarMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<RejeitarMrpPlanejamentoCommand>
    {
        public RejeitarMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(RejeitarMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.Rejeitar(request.Motivo, usuario);
            return await FinalizarAsync(p, "Rejeicao", usuario, tenantId, ct);
        }
    }

    public class InativarMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<InativarMrpPlanejamentoCommand>
    {
        public InativarMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(InativarMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.Inativar(usuario);
            return await FinalizarAsync(p, "Inativacao", usuario, tenantId, ct);
        }
    }

    public class ReativarMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<ReativarMrpPlanejamentoCommand>
    {
        public ReativarMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(ReativarMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.Reativar(usuario);
            return await FinalizarAsync(p, "Reativacao", usuario, tenantId, ct);
        }
    }

    public class EncerrarMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<EncerrarMrpPlanejamentoCommand>
    {
        public EncerrarMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(EncerrarMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.Encerrar(usuario);
            return await FinalizarAsync(p, "Encerramento", usuario, tenantId, ct);
        }
    }
}
