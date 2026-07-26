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
    public class CriarPppCommandHandler : ICommandHandler<CriarPppCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public CriarPppCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context; _tenant = tenant; _user = user;
        }

        public async Task<CommandResult> Handle(CriarPppCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var ppp = new SsoPpp(request.ColaboradorId, request.Observacao, SsoPpp.StElaboracao, tenantId, usuario);
            if (!ppp.IsValid)
                return CommandResult.Falha(ppp.Notifications.Select(n => n.Message));

            _context.SsoPpps.Add(ppp);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("PPP criado.", new { PppId = ppp.Id });
        }
    }

    public class RegistrarExameMedicoCommandHandler : ICommandHandler<RegistrarExameMedicoCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarExameMedicoCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context; _tenant = tenant; _user = user;
        }

        public async Task<CommandResult> Handle(RegistrarExameMedicoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var exame = new SsoExameMedico(request.PppId, request.ColaboradorId, request.DataUltimo, request.Tipo,
                request.Natureza, request.Exame, request.IndicacaoResultados, SsoExameMedico.StRegistrado, tenantId, usuario);

            if (!exame.IsValid)
                return CommandResult.Falha(exame.Notifications.Select(n => n.Message));

            _context.SsoExameMedicos.Add(exame);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Exame medico registrado.", new { ExameId = exame.Id });
        }
    }

    public class ListarPppsQueryHandler : IQueryHandler<ListarPppsQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarPppsQueryHandler(ContextRH context) => _context = context;

        public async Task<CommandResult> Handle(ListarPppsQuery request, CancellationToken ct)
        {
            var itens = await _context.SsoPpps.OrderByDescending(p => p.CriadoEm).ToListAsync(ct);
            return CommandResult.Ok("PPPs listados.", itens);
        }
    }
}
