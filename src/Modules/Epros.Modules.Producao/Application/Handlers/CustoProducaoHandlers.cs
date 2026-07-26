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
    public class CriarCustoProducaoCommandHandler : ICommandHandler<CriarCustoProducaoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCustoProducaoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCustoProducaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.CustosProducao.AnyAsync(c => c.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
                return CommandResult.Falha($"Código de custo '{request.Codigo}' já está em uso.");

            var custo = new CustoProducao(
                request.Codigo, request.ResponsavelId, tenantId, usuario,
                request.ReferenciaOrigem, request.ReferenciaId,
                request.CustoTotalPrevisto, request.CustoTotalRealizado);

            if (request.Referencias != null)
            {
                foreach (var r in request.Referencias)
                {
                    custo.AdicionarReferencia(r.TipoReferencia, usuario, r.ReferenciaId, r.CustoPrevisto,
                        r.CustoRealizado, r.CustoExtra, r.TipoCustoProducao, r.PercentualCustoProducao);
                }
            }

            if (!custo.IsValid)
                return CommandResult.Falha(custo.Notifications.Select(n => n.Message));

            _context.CustosProducao.Add(custo);
            _context.CustosHistorico.Add(new CustoHistorico(custo.Id, "Criacao", usuario, "{}", tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Registro de custo de produção criado em Rascunho.", new { custo.Id, custo.Codigo });
        }
    }

    public abstract class CustoProducaoTransicaoHandlerBase
    {
        protected readonly ContextProducao _context;
        protected readonly ITenantProvider _tenantProvider;
        protected readonly ICurrentUser _currentUser;

        protected CustoProducaoTransicaoHandlerBase(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        protected async Task<CommandResult> ExecutarAsync(Guid id, string acao, Action<CustoProducao, string> transicao, CancellationToken ct)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();
            var custo = await _context.CustosProducao.Include(c => c.Referencias).FirstOrDefaultAsync(c => c.Id == id, ct);
            if (custo == null) return CommandResult.Falha("Registro de custo não encontrado.");

            transicao(custo, usuario);
            if (!custo.IsValid)
                return CommandResult.Falha(custo.Notifications.Select(n => n.Message));

            _context.CustosHistorico.Add(new CustoHistorico(custo.Id, acao, usuario, "{}", tenantId, usuario));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"Custo {acao} com sucesso.", new { custo.Id, Status = custo.Status.ToString() });
        }
    }

    public class SubmeterCustoProducaoCommandHandler : CustoProducaoTransicaoHandlerBase, ICommandHandler<SubmeterCustoProducaoCommand>
    {
        public SubmeterCustoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(SubmeterCustoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Submissao", (c, u) => c.SubmeterParaAnalise(u), ct);
    }

    public class AprovarCustoProducaoCommandHandler : CustoProducaoTransicaoHandlerBase, ICommandHandler<AprovarCustoProducaoCommand>
    {
        public AprovarCustoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(AprovarCustoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Aprovacao", (c, u) => c.Aprovar(u), ct);
    }

    public class RejeitarCustoProducaoCommandHandler : CustoProducaoTransicaoHandlerBase, ICommandHandler<RejeitarCustoProducaoCommand>
    {
        public RejeitarCustoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(RejeitarCustoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Rejeicao", (c, u) => c.Rejeitar(r.Motivo, u), ct);
    }

    public class InativarCustoProducaoCommandHandler : CustoProducaoTransicaoHandlerBase, ICommandHandler<InativarCustoProducaoCommand>
    {
        public InativarCustoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(InativarCustoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Inativacao", (c, u) => c.Inativar(u), ct);
    }

    public class ReativarCustoProducaoCommandHandler : CustoProducaoTransicaoHandlerBase, ICommandHandler<ReativarCustoProducaoCommand>
    {
        public ReativarCustoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(ReativarCustoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Reativacao", (c, u) => c.Reativar(u), ct);
    }

    public class EncerrarCustoProducaoCommandHandler : CustoProducaoTransicaoHandlerBase, ICommandHandler<EncerrarCustoProducaoCommand>
    {
        public EncerrarCustoProducaoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public Task<CommandResult> Handle(EncerrarCustoProducaoCommand r, CancellationToken ct) =>
            ExecutarAsync(r.Id, "Encerramento", (c, u) => c.Encerrar(u), ct);
    }
}
