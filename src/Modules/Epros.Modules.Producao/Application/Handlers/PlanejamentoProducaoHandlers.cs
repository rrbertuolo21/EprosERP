using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Handlers
{
    public class CriarPlanejamentoProducaoCommandHandler : ICommandHandler<CriarPlanejamentoProducaoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPlanejamentoProducaoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPlanejamentoProducaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.Planejamentos.AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
                return CommandResult.Falha($"Código de planejamento '{request.Codigo}' já está em uso.");

            var plano = new PlanejamentoProducao(request.Codigo, request.ResponsavelId, tenantId, usuario);
            if (!plano.IsValid)
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            _context.Planejamentos.Add(plano);
            _context.PlanejamentoHistoricos.Add(new PlanejamentoHistorico(plano.Id, "Criacao", usuario, "{}", tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Planejamento de produção criado em Rascunho.", new { plano.Id, plano.Codigo });
        }
    }

    public class AdicionarSnapshotPlanejamentoCommandHandler : ICommandHandler<AdicionarSnapshotPlanejamentoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarSnapshotPlanejamentoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarSnapshotPlanejamentoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var plano = await _context.Planejamentos.Include(p => p.Snapshots).FirstOrDefaultAsync(p => p.Id == request.PlanejamentoId, cancellationToken);
            if (plano == null) return CommandResult.Falha("Planejamento não encontrado.");

            plano.AdicionarSnapshot(usuario, request.OrdemProducaoId, request.Inicio, request.PrevisaoEntrega,
                request.Termino, request.PorcentoVenda, request.PorcentoEstoque, request.CustoTotalPrevisto);

            if (!plano.IsValid)
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Snapshot planejado adicionado.", new { plano.Id });
        }
    }

    public abstract class PlanejamentoTransicaoHandlerBase
    {
        protected readonly ContextProducao _context;
        protected readonly ITenantProvider _tenantProvider;
        protected readonly ICurrentUser _currentUser;

        protected PlanejamentoTransicaoHandlerBase(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        protected async Task<CommandResult> ExecutarAsync(Guid id, string acao, Action<PlanejamentoProducao, string> transicao, CancellationToken ct)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();
            var plano = await _context.Planejamentos.Include(p => p.Snapshots).FirstOrDefaultAsync(p => p.Id == id, ct);
            if (plano == null) return CommandResult.Falha("Planejamento não encontrado.");

            transicao(plano, usuario);
            if (!plano.IsValid)
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            _context.PlanejamentoHistoricos.Add(new PlanejamentoHistorico(plano.Id, acao, usuario, "{}", tenantId, usuario));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"Planejamento {acao} com sucesso.", new { plano.Id, Status = plano.Status.ToString() });
        }
    }

    public class SubmeterPlanejamentoProducaoCommandHandler : PlanejamentoTransicaoHandlerBase, ICommandHandler<SubmeterPlanejamentoProducaoCommand>
    {
        public SubmeterPlanejamentoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(SubmeterPlanejamentoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Submissao", (p, u) => p.SubmeterParaAnalise(u), ct);
    }

    public class AprovarPlanejamentoProducaoCommandHandler : PlanejamentoTransicaoHandlerBase, ICommandHandler<AprovarPlanejamentoProducaoCommand>
    {
        public AprovarPlanejamentoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(AprovarPlanejamentoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Aprovacao", (p, u) => p.Aprovar(u), ct);
    }

    public class RejeitarPlanejamentoProducaoCommandHandler : PlanejamentoTransicaoHandlerBase, ICommandHandler<RejeitarPlanejamentoProducaoCommand>
    {
        public RejeitarPlanejamentoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(RejeitarPlanejamentoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Rejeicao", (p, u) => p.Rejeitar(r.Motivo, u), ct);
    }

    public class InativarPlanejamentoProducaoCommandHandler : PlanejamentoTransicaoHandlerBase, ICommandHandler<InativarPlanejamentoProducaoCommand>
    {
        public InativarPlanejamentoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(InativarPlanejamentoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Inativacao", (p, u) => p.Inativar(u), ct);
    }

    public class ReativarPlanejamentoProducaoCommandHandler : PlanejamentoTransicaoHandlerBase, ICommandHandler<ReativarPlanejamentoProducaoCommand>
    {
        public ReativarPlanejamentoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(ReativarPlanejamentoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Reativacao", (p, u) => p.Reativar(u), ct);
    }

    public class EncerrarPlanejamentoProducaoCommandHandler : PlanejamentoTransicaoHandlerBase, ICommandHandler<EncerrarPlanejamentoProducaoCommand>
    {
        public EncerrarPlanejamentoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(EncerrarPlanejamentoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Encerramento", (p, u) => p.Encerrar(u), ct);
    }
}
