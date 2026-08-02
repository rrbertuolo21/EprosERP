using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Queries;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    public record ListarRegrasContabilizacaoQuery() : IRequest<CommandResult>;

    // Cria/atualiza (upsert por tipo de evento) a regra de de-para evento→conta.
    public class DefinirRegraContabilizacaoCommandHandler : IRequestHandler<DefinirRegraContabilizacaoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ITenantProvider _tenant; private readonly ICurrentUser _user;
        public DefinirRegraContabilizacaoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DefinirRegraContabilizacaoCommand r, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var existente = await _context.RegrasContabilizacao.FirstOrDefaultAsync(x => x.TipoEvento == r.TipoEvento, ct);
            if (existente == null)
            {
                var regra = new RegraContabilizacao(r.TipoEvento, r.ContaDebitoId, r.ContaCreditoId, r.Historico, _tenant.GetTenantId(), userId);
                if (!regra.IsValid) return CommandResult.Falha(regra.Notifications.Select(n => n.Message));
                _context.RegrasContabilizacao.Add(regra);
                await _context.SaveChangesAsync(ct);
                return CommandResult.Ok("Regra de contabilização criada.", new { regra.Id, regra.Completa });
            }
            existente.Alterar(r.ContaDebitoId, r.ContaCreditoId, r.Historico, userId);
            if (!existente.IsValid) return CommandResult.Falha(existente.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Regra de contabilização atualizada.", new { existente.Id, existente.Completa });
        }
    }

    public class RemoverRegraContabilizacaoCommandHandler : IRequestHandler<RemoverRegraContabilizacaoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context; private readonly ICurrentUser _user;
        public RemoverRegraContabilizacaoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(RemoverRegraContabilizacaoCommand r, CancellationToken ct)
        {
            var regra = await _context.RegrasContabilizacao.FirstOrDefaultAsync(x => x.Id == r.Id, ct);
            if (regra == null) return CommandResult.Falha("Regra de contabilização não encontrada.");
            regra.Deletar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Regra de contabilização removida.", new { regra.Id });
        }
    }

    public class RegraContabilizacaoQueryHandler : IRequestHandler<ListarRegrasContabilizacaoQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public RegraContabilizacaoQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarRegrasContabilizacaoQuery request, CancellationToken ct)
        {
            var itens = await _context.RegrasContabilizacao.AsNoTracking()
                .OrderBy(r => r.TipoEvento)
                .Select(r => new { r.Id, r.TipoEvento, r.ContaDebitoId, r.ContaCreditoId, r.Historico, r.Ativo, Completa = r.ContaDebitoId != null && r.ContaCreditoId != null })
                .ToListAsync(ct);
            return CommandResult.Ok("Regras de contabilização listadas.", new { itens });
        }
    }
}
