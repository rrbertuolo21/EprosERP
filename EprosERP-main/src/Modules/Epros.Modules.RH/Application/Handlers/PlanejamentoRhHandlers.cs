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
    public class CriarTurnoCommandHandler : ICommandHandler<CriarTurnoCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public CriarTurnoCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarTurnoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var turno = new PlnTurno(
                request.Nome, request.HoraInicio, request.HoraFim, request.IntervaloInicio, request.IntervaloFim,
                request.TurnoNoturno, request.CriadoPorId, request.OwnerId, true, tenantId, usuario);

            if (!turno.IsValid)
                return CommandResult.Falha(turno.Notifications.Select(n => n.Message));

            _context.PlnTurnos.Add(turno);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Turno criado.", new { TurnoId = turno.Id });
        }
    }

    public class CriarFeriadoCommandHandler : ICommandHandler<CriarFeriadoCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public CriarFeriadoCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarFeriadoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            // PLN secao 17.3: valida periodo.
            if (request.DataInicio.HasValue && request.DataFim.HasValue && request.DataFim.Value < request.DataInicio.Value)
                return CommandResult.Falha("A data final do feriado deve ser maior ou igual a data inicial (PLN secao 17.3).");

            var feriado = new PlnFeriado(
                request.Nome, request.DataInicio, request.DataFim, request.TipoFeriadoId, request.Descricao,
                request.Remunerado, null, null, request.CriadoPorId, request.OwnerId, tenantId, usuario);

            if (!feriado.IsValid)
                return CommandResult.Falha(feriado.Notifications.Select(n => n.Message));

            _context.PlnFeriados.Add(feriado);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Feriado criado.", new { FeriadoId = feriado.Id });
        }
    }

    public class DefinirHeadcountItemCommandHandler : ICommandHandler<DefinirHeadcountItemCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirHeadcountItemCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DefinirHeadcountItemCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var versaoExiste = await _context.PlnHeadcountVersaos.AnyAsync(v => v.Id == request.VersaoId, ct);
            if (!versaoExiste) return CommandResult.Falha("Versao de headcount nao encontrada.");

            var item = new PlnHeadcountItem(
                request.VersaoId, request.DepartamentoId, request.CargoId, request.QuantidadeAutorizada,
                request.CustoPrevisto, request.Observacao, tenantId, usuario);

            if (!item.IsValid)
                return CommandResult.Falha(item.Notifications.Select(n => n.Message));

            _context.PlnHeadcountItems.Add(item);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Item de headcount definido.", new { ItemId = item.Id });
        }
    }

    public class ListarTurnosQueryHandler : IQueryHandler<ListarTurnosQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarTurnosQueryHandler(ContextRH context) => _context = context;
        public async Task<CommandResult> Handle(ListarTurnosQuery request, CancellationToken ct)
            => CommandResult.Ok("Turnos listados.", await _context.PlnTurnos.OrderByDescending(t => t.CriadoEm).ToListAsync(ct));
    }

    public class ListarFeriadosQueryHandler : IQueryHandler<ListarFeriadosQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarFeriadosQueryHandler(ContextRH context) => _context = context;
        public async Task<CommandResult> Handle(ListarFeriadosQuery request, CancellationToken ct)
            => CommandResult.Ok("Feriados listados.", await _context.PlnFeriados.OrderByDescending(f => f.CriadoEm).ToListAsync(ct));
    }

    public class ListarHeadcountQueryHandler : IQueryHandler<ListarHeadcountQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarHeadcountQueryHandler(ContextRH context) => _context = context;
        public async Task<CommandResult> Handle(ListarHeadcountQuery request, CancellationToken ct)
            => CommandResult.Ok("Headcount listado.", await _context.PlnHeadcountItems.OrderByDescending(h => h.CriadoEm).ToListAsync(ct));
    }
}
