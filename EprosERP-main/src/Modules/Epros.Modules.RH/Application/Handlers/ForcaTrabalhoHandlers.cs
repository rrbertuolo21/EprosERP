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
    public class AdmitirWfmColaboradorCommandHandler : ICommandHandler<AdmitirWfmColaboradorCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public AdmitirWfmColaboradorCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AdmitirWfmColaboradorCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            // WFM secao 20.2: matricula unica por tenant.
            var matriculaExiste = await _context.WfmColaboradors.AnyAsync(c => c.Matricula == request.Matricula, ct);
            if (matriculaExiste)
                return CommandResult.Falha("Ja existe colaborador com esta matricula (WFM secao 20.2).");

            var colaborador = new WfmColaborador(
                request.PessoaId, null, request.Matricula, null, null, DateTime.UtcNow, request.DataAdmissao, null,
                null, null, WfmColaborador.StRascunho, request.CargoId, request.DepartamentoId, request.FilialId,
                request.TurnoId, null, request.SalarioBase, request.TipoRemuneracao, null, null, null, null, true,
                tenantId, usuario);

            if (!colaborador.IsValid)
                return CommandResult.Falha(colaborador.Notifications.Select(n => n.Message));

            _context.WfmColaboradors.Add(colaborador);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Colaborador admitido (rascunho).", new { ColaboradorId = colaborador.Id, colaborador.Matricula, colaborador.Status });
        }
    }

    public class DemitirWfmColaboradorCommandHandler : ICommandHandler<DemitirWfmColaboradorCommand>
    {
        private readonly ContextRH _context;
        private readonly ICurrentUser _user;

        public DemitirWfmColaboradorCommandHandler(ContextRH context, ICurrentUser user)
        { _context = context; _user = user; }

        public async Task<CommandResult> Handle(DemitirWfmColaboradorCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var colaborador = await _context.WfmColaboradors.FirstOrDefaultAsync(c => c.Id == request.ColaboradorId, ct);
            if (colaborador == null) return CommandResult.Falha("Colaborador nao encontrado.");

            // WFM secao 20.9: demissao bloqueia folha normal e aciona rescisao.
            colaborador.Demitir(usuario);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Colaborador demitido. Folha normal bloqueada; rescisao deve ser acionada.",
                new { colaborador.Id, colaborador.Status, BloqueiaFolhaNormal = colaborador.BloqueiaFolhaNormal() });
        }
    }

    public class DefinirComissaoColaboradorCommandHandler : ICommandHandler<DefinirComissaoColaboradorCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public DefinirComissaoColaboradorCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(DefinirComissaoColaboradorCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var comissao = new WfmComissaoColaborador(
                request.ColaboradorId, request.TipoCargo, request.ValorPercentualComissao, true, tenantId, usuario);

            // WFM secao 20.7/20.8: percentual maximo 100; tipo de cargo de dominio fechado.
            comissao.ValidarRegras();
            if (!comissao.IsValid)
                return CommandResult.Falha(comissao.Notifications.Select(n => n.Message));

            _context.WfmComissaoColaboradors.Add(comissao);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Comissao definida.", new { ComissaoId = comissao.Id });
        }
    }

    public class ListarWfmColaboradoresQueryHandler : IQueryHandler<ListarWfmColaboradoresQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarWfmColaboradoresQueryHandler(ContextRH context) => _context = context;
        public async Task<CommandResult> Handle(ListarWfmColaboradoresQuery request, CancellationToken ct)
            => CommandResult.Ok("Colaboradores listados.", await _context.WfmColaboradors.OrderByDescending(c => c.CriadoEm).ToListAsync(ct));
    }
}
