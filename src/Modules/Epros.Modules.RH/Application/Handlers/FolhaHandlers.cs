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
    public class CriarRubricaCommandHandler : ICommandHandler<CriarRubricaCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public CriarRubricaCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context; _tenant = tenant; _user = user;
        }

        public async Task<CommandResult> Handle(CriarRubricaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (!FolRubrica.TipoValido(request.Tipo))
                return CommandResult.Falha("A rubrica deve ser Provento, Desconto ou Informativo.");

            var jaExiste = await _context.FolRubricas
                .AnyAsync(r => r.EmpresaId == request.EmpresaId && r.Codigo == request.Codigo, ct);
            if (jaExiste)
                return CommandResult.Falha("Ja existe rubrica com este codigo para a empresa.");

            var rubrica = new FolRubrica(request.EmpresaId, request.Codigo, request.Nome, request.Descricao,
                request.Tipo, request.Unidade, request.BaseCalculo, request.Taxa, request.Ativo, tenantId, usuario);

            if (!rubrica.IsValid)
                return CommandResult.Falha(rubrica.Notifications.Select(n => n.Message));

            _context.FolRubricas.Add(rubrica);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Rubrica criada com sucesso!", new { RubricaId = rubrica.Id });
        }
    }

    public class AbrirCompetenciaCommandHandler : ICommandHandler<AbrirCompetenciaCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public AbrirCompetenciaCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context; _tenant = tenant; _user = user;
        }

        public async Task<CommandResult> Handle(AbrirCompetenciaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var jaExiste = await _context.FolCompetencias
                .AnyAsync(c => c.EmpresaId == request.EmpresaId && c.Competencia == request.Competencia, ct);
            if (jaExiste)
                return CommandResult.Falha($"Ja existe competencia {request.Competencia} para a empresa.");

            var comp = new FolCompetencia(request.EmpresaId, request.Competencia, request.Tipo,
                request.PeriodoInicio, request.PeriodoFim, null, null, null, request.Descricao,
                FolCompetencia.StRascunho, null, null, tenantId, usuario);

            if (!comp.IsValid)
                return CommandResult.Falha(comp.Notifications.Select(n => n.Message));

            _context.FolCompetencias.Add(comp);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Competencia aberta com sucesso!", new { CompetenciaId = comp.Id });
        }
    }

    public class FecharCompetenciaCommandHandler : ICommandHandler<FecharCompetenciaCommand>
    {
        private readonly ContextRH _context;
        private readonly ICurrentUser _user;

        public FecharCompetenciaCommandHandler(ContextRH context, ICurrentUser user)
        {
            _context = context; _user = user;
        }

        public async Task<CommandResult> Handle(FecharCompetenciaCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var comp = await _context.FolCompetencias.FirstOrDefaultAsync(c => c.Id == request.CompetenciaId, ct);
            if (comp == null) return CommandResult.Falha("Competencia nao encontrada.");

            comp.Fechar(usuario);
            if (!comp.IsValid)
                return CommandResult.Falha(comp.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Competencia fechada.", new { comp.Id, comp.Status });
        }
    }

    public class ListarRubricasQueryHandler : IQueryHandler<ListarRubricasQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarRubricasQueryHandler(ContextRH context) => _context = context;

        public async Task<CommandResult> Handle(ListarRubricasQuery request, CancellationToken ct)
        {
            var itens = await _context.FolRubricas.OrderBy(r => r.Codigo).ToListAsync(ct);
            return CommandResult.Ok("Rubricas listadas.", itens);
        }
    }

    public class ListarCompetenciasQueryHandler : IQueryHandler<ListarCompetenciasQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarCompetenciasQueryHandler(ContextRH context) => _context = context;

        public async Task<CommandResult> Handle(ListarCompetenciasQuery request, CancellationToken ct)
        {
            var itens = await _context.FolCompetencias.OrderByDescending(c => c.Competencia).ToListAsync(ct);
            return CommandResult.Ok("Competencias listadas.", itens);
        }
    }
}
