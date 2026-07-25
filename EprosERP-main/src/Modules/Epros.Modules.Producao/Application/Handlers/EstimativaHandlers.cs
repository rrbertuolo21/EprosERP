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
    public class CriarEstimativaCommandHandler : ICommandHandler<CriarEstimativaCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarEstimativaCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarEstimativaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.Estimativas.AnyAsync(e => e.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
                return CommandResult.Falha($"Código de estimativa '{request.Codigo}' já está em uso.");

            var estimativa = new Estimativa(
                request.Codigo, request.ResponsavelId, tenantId, usuario,
                request.PropostaReferenciaId, request.EstruturaRascunhoId, request.CustoPrevistoTotal);

            if (request.Componentes != null)
            {
                foreach (var c in request.Componentes)
                {
                    estimativa.AdicionarComponente(c.TipoComponente, usuario, c.ReferenciaId, c.Quantidade,
                        c.TempoEstimado, c.TaxaEstimada, c.CustoPrevisto, c.Observacao);
                }
            }

            if (!estimativa.IsValid)
                return CommandResult.Falha(estimativa.Notifications.Select(n => n.Message));

            _context.Estimativas.Add(estimativa);
            _context.EstimativaHistoricos.Add(new EstimativaHistorico(estimativa.Id, "Criacao", usuario, "{}", tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Estimativa criada em Rascunho.", new { estimativa.Id, estimativa.Codigo });
        }
    }

    public abstract class EstimativaTransicaoHandlerBase
    {
        protected readonly ContextProducao _context;
        protected readonly ITenantProvider _tenantProvider;
        protected readonly ICurrentUser _currentUser;

        protected EstimativaTransicaoHandlerBase(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        protected async Task<CommandResult> ExecutarAsync(Guid id, string acao, Action<Estimativa, string> transicao, CancellationToken ct)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();
            var estimativa = await _context.Estimativas.Include(e => e.Componentes).FirstOrDefaultAsync(e => e.Id == id, ct);
            if (estimativa == null) return CommandResult.Falha("Estimativa não encontrada.");

            transicao(estimativa, usuario);
            if (!estimativa.IsValid)
                return CommandResult.Falha(estimativa.Notifications.Select(n => n.Message));

            _context.EstimativaHistoricos.Add(new EstimativaHistorico(estimativa.Id, acao, usuario, "{}", tenantId, usuario));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"Estimativa {acao} com sucesso.", new { estimativa.Id, Status = estimativa.Status.ToString() });
        }
    }

    public class SubmeterEstimativaCommandHandler : EstimativaTransicaoHandlerBase, ICommandHandler<SubmeterEstimativaCommand>
    {
        public SubmeterEstimativaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(SubmeterEstimativaCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Submissao", (e, u) => e.SubmeterParaAnalise(u), ct);
    }

    public class AprovarEstimativaCommandHandler : EstimativaTransicaoHandlerBase, ICommandHandler<AprovarEstimativaCommand>
    {
        public AprovarEstimativaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(AprovarEstimativaCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Aprovacao", (e, u) => e.Aprovar(u), ct);
    }

    public class RejeitarEstimativaCommandHandler : EstimativaTransicaoHandlerBase, ICommandHandler<RejeitarEstimativaCommand>
    {
        public RejeitarEstimativaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(RejeitarEstimativaCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Rejeicao", (e, u) => e.Rejeitar(r.Motivo, u), ct);
    }

    public class ConverterEstimativaCommandHandler : EstimativaTransicaoHandlerBase, ICommandHandler<ConverterEstimativaCommand>
    {
        public ConverterEstimativaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(ConverterEstimativaCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Conversao", (e, u) => e.ConverterParaPlanejamento(r.PlanejamentoOrigemId, u), ct);
    }

    public class InativarEstimativaCommandHandler : EstimativaTransicaoHandlerBase, ICommandHandler<InativarEstimativaCommand>
    {
        public InativarEstimativaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(InativarEstimativaCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Inativacao", (e, u) => e.Inativar(u), ct);
    }

    public class ReativarEstimativaCommandHandler : EstimativaTransicaoHandlerBase, ICommandHandler<ReativarEstimativaCommand>
    {
        public ReativarEstimativaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(ReativarEstimativaCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Reativacao", (e, u) => e.Reativar(u), ct);
    }

    public class EncerrarEstimativaCommandHandler : EstimativaTransicaoHandlerBase, ICommandHandler<EncerrarEstimativaCommand>
    {
        public EncerrarEstimativaCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(EncerrarEstimativaCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Encerramento", (e, u) => e.Encerrar(u), ct);
    }
}
