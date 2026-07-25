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
    public class RegistrarMarcacaoCommandHandler : ICommandHandler<RegistrarMarcacaoCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public RegistrarMarcacaoCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context; _tenant = tenant; _user = user;
        }

        public async Task<CommandResult> Handle(RegistrarMarcacaoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var m = new PntMarcacao(request.ColaboradorId, request.RelogioId, request.Nsr, request.DataMarcacao,
                request.HoraMarcacao, request.TipoMarcacao, request.TipoRegistro, request.ParEntradaSaida,
                request.Justificativa, request.Origem ?? "Manual", "Importada", tenantId, usuario);

            if (!m.IsValid)
                return CommandResult.Falha(m.Notifications.Select(n => n.Message));

            _context.PntMarcacaos.Add(m);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Marcacao registrada.", new { MarcacaoId = m.Id });
        }
    }

    public class AbrirPeriodoApuracaoCommandHandler : ICommandHandler<AbrirPeriodoApuracaoCommand>
    {
        private readonly ContextRH _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public AbrirPeriodoApuracaoCommandHandler(ContextRH context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context; _tenant = tenant; _user = user;
        }

        public async Task<CommandResult> Handle(AbrirPeriodoApuracaoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var jaExiste = await _context.PntPeriodoApuracaos
                .AnyAsync(p => p.EmpresaId == request.EmpresaId && p.Competencia == request.Competencia, ct);
            if (jaExiste)
                return CommandResult.Falha($"Ja existe periodo de apuracao {request.Competencia} para a empresa.");

            var periodo = new PntPeriodoApuracao(request.EmpresaId, request.Competencia, request.DataInicio,
                request.DataFim, PntPeriodoApuracao.StAberto, null, null, tenantId, usuario);

            if (!periodo.IsValid)
                return CommandResult.Falha(periodo.Notifications.Select(n => n.Message));

            _context.PntPeriodoApuracaos.Add(periodo);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Periodo de apuracao aberto.", new { PeriodoId = periodo.Id });
        }
    }

    public class FecharPeriodoApuracaoCommandHandler : ICommandHandler<FecharPeriodoApuracaoCommand>
    {
        private readonly ContextRH _context;
        private readonly ICurrentUser _user;

        public FecharPeriodoApuracaoCommandHandler(ContextRH context, ICurrentUser user)
        {
            _context = context; _user = user;
        }

        public async Task<CommandResult> Handle(FecharPeriodoApuracaoCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var periodo = await _context.PntPeriodoApuracaos.FirstOrDefaultAsync(p => p.Id == request.PeriodoId, ct);
            if (periodo == null) return CommandResult.Falha("Periodo nao encontrado.");

            periodo.Fechar(usuario);
            if (!periodo.IsValid)
                return CommandResult.Falha(periodo.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Periodo fechado.", new { periodo.Id, periodo.Status });
        }
    }

    public class ListarMarcacoesQueryHandler : IQueryHandler<ListarMarcacoesQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarMarcacoesQueryHandler(ContextRH context) => _context = context;

        public async Task<CommandResult> Handle(ListarMarcacoesQuery request, CancellationToken ct)
        {
            var itens = await _context.PntMarcacaos.OrderByDescending(m => m.DataMarcacao).ToListAsync(ct);
            return CommandResult.Ok("Marcacoes listadas.", itens);
        }
    }

    public class ListarPeriodosApuracaoQueryHandler : IQueryHandler<ListarPeriodosApuracaoQuery, CommandResult>
    {
        private readonly ContextRH _context;
        public ListarPeriodosApuracaoQueryHandler(ContextRH context) => _context = context;

        public async Task<CommandResult> Handle(ListarPeriodosApuracaoQuery request, CancellationToken ct)
        {
            var itens = await _context.PntPeriodoApuracaos.OrderByDescending(p => p.Competencia).ToListAsync(ct);
            return CommandResult.Ok("Periodos listados.", itens);
        }
    }
}
