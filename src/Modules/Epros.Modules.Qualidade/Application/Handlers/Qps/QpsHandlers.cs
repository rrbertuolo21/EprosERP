using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands.Qps;
using Epros.Modules.Qualidade.Application.Queries.Qps;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Domain.Services.Qps;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.Handlers.Qps
{
    public class CriarQpsRegistroCommandHandler : ICommandHandler<CriarQpsRegistroCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarQpsRegistroCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarQpsRegistroCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";
            if (await _ctx.QpsRegistros.AnyAsync(x => x.Codigo == r.Codigo, ct))
                return CommandResult.Falha($"Ja existe um registro com o codigo '{r.Codigo}' neste tenant.", block: true);

            var reg = new QpsRegistro(r.Codigo, r.ParceiroId, r.ResponsavelId, r.NomeParceiro, tenantId, usuario);
            if (!reg.IsValid) return CommandResult.Falha(reg.Notifications.Select(n => n.Message));

            _ctx.QpsRegistros.Add(reg);
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Registro de qualidade de fornecedor criado.", new { reg.Id, reg.Codigo, StatusHomologacao = reg.StatusHomologacao.ToString() });
        }
    }

    public class HomologarFornecedorCommandHandler : ICommandHandler<HomologarFornecedorCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ICurrentUser _user;
        public HomologarFornecedorCommandHandler(ContextQualidade ctx, ICurrentUser user) { _ctx = ctx; _user = user; }

        public async Task<CommandResult> Handle(HomologarFornecedorCommand r, CancellationToken ct)
        {
            var reg = await _ctx.QpsRegistros.FirstOrDefaultAsync(x => x.Id == r.RegistroId, ct);
            if (reg is null) return CommandResult.Falha("Registro nao encontrado.", block: true);

            reg.Homologar(r.DataValidade, _user.GetUserId() ?? "system");
            if (!reg.IsValid) return CommandResult.Falha(reg.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Fornecedor homologado.", new { reg.Id, StatusHomologacao = reg.StatusHomologacao.ToString(), reg.DataValidadeHomologacao });
        }
    }

    public class BloquearFornecedorCommandHandler : ICommandHandler<BloquearFornecedorCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public BloquearFornecedorCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(BloquearFornecedorCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";
            var reg = await _ctx.QpsRegistros.FirstOrDefaultAsync(x => x.Id == r.RegistroId, ct);
            if (reg is null) return CommandResult.Falha("Registro nao encontrado.", block: true);

            reg.Bloquear(r.Motivo, usuario);
            if (!reg.IsValid) return CommandResult.Falha(reg.Notifications.Select(n => n.Message));

            var bloqueio = new QpsBloqueio(r.RegistroId, r.TipoBloqueio, r.Motivo, r.AlcadaId, tenantId, usuario);
            if (!bloqueio.IsValid) return CommandResult.Falha(bloqueio.Notifications.Select(n => n.Message));
            _ctx.QpsBloqueios.Add(bloqueio);

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Fornecedor bloqueado.", new { reg.Id, StatusHomologacao = reg.StatusHomologacao.ToString(), BloqueioId = bloqueio.Id });
        }
    }

    public class DesbloquearFornecedorCommandHandler : ICommandHandler<DesbloquearFornecedorCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ICurrentUser _user;
        public DesbloquearFornecedorCommandHandler(ContextQualidade ctx, ICurrentUser user) { _ctx = ctx; _user = user; }

        public async Task<CommandResult> Handle(DesbloquearFornecedorCommand r, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var reg = await _ctx.QpsRegistros.FirstOrDefaultAsync(x => x.Id == r.RegistroId, ct);
            if (reg is null) return CommandResult.Falha("Registro nao encontrado.", block: true);

            var bloqueios = await _ctx.QpsBloqueios.Where(b => b.RegistroId == r.RegistroId && b.Ativo).ToListAsync(ct);
            foreach (var b in bloqueios) b.Desbloquear(usuario);
            reg.SolicitarReHomologacao(usuario);

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Fornecedor desbloqueado (re-homologacao).", new { reg.Id, StatusHomologacao = reg.StatusHomologacao.ToString() });
        }
    }

    public class CalcularScoreFornecedorCommandHandler : ICommandHandler<CalcularScoreFornecedorCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        private readonly MotorScoreFornecedor _motor;
        public CalcularScoreFornecedorCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user, MotorScoreFornecedor motor)
        { _ctx = ctx; _tenant = tenant; _user = user; _motor = motor; }

        public async Task<CommandResult> Handle(CalcularScoreFornecedorCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";
            var reg = await _ctx.QpsRegistros.FirstOrDefaultAsync(x => x.Id == r.RegistroId, ct);
            if (reg is null) return CommandResult.Falha("Registro nao encontrado.", block: true);

            var indicadores = (r.Indicadores ?? new System.Collections.Generic.List<IndicadorScoreDto>())
                .Select(i => new IndicadorScore(i.Codigo, i.Valor, i.Peso));
            var resultado = _motor.Calcular(indicadores, r.LimiteBloqueio);

            var scorecard = new QpsScorecard(r.RegistroId, r.Periodo, resultado.Score, resultado.AbaixoLimite,
                resultado.AbaixoLimite ? $"Score abaixo do limite de bloqueio ({resultado.LimiteBloqueio})." : null,
                tenantId, usuario);
            if (!scorecard.IsValid) return CommandResult.Falha(scorecard.Notifications.Select(n => n.Message));
            _ctx.QpsScorecards.Add(scorecard);

            foreach (var i in r.Indicadores ?? new System.Collections.Generic.List<IndicadorScoreDto>())
            {
                var ind = new QpsIndicador(scorecard.Id, i.Codigo, i.Valor, i.Peso, i.Fonte, tenantId, usuario);
                if (ind.IsValid) _ctx.QpsIndicadores.Add(ind);
            }

            reg.AtualizarScore(resultado.Score, usuario);
            await _ctx.SaveChangesAsync(ct);

            return CommandResult.Ok("Score do fornecedor calculado.", new
            {
                reg.Id,
                ScorecardId = scorecard.Id,
                resultado.Score,
                resultado.AbaixoLimite,
                resultado.LimiteBloqueio
            });
        }
    }

    public class AdicionarDocumentoQpsCommandHandler : ICommandHandler<AdicionarDocumentoQpsCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AdicionarDocumentoQpsCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AdicionarDocumentoQpsCommand r, CancellationToken ct)
        {
            var reg = await _ctx.QpsRegistros.FirstOrDefaultAsync(x => x.Id == r.RegistroId, ct);
            if (reg is null) return CommandResult.Falha("Registro nao encontrado.", block: true);

            var doc = new QpsDocumento(r.RegistroId, r.TipoDocumento, r.Titulo, r.Numero, r.DataValidade, r.ArquivoId,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!doc.IsValid) return CommandResult.Falha(doc.Notifications.Select(n => n.Message));

            _ctx.QpsDocumentos.Add(doc);
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Documento adicionado.", new { doc.Id });
        }
    }

    // ================= Query =================
    public class ListarQpsRegistrosQueryHandler : IQueryHandler<ListarQpsRegistrosQuery, CommandResult>
    {
        private readonly ContextQualidade _ctx;
        public ListarQpsRegistrosQueryHandler(ContextQualidade ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(ListarQpsRegistrosQuery request, CancellationToken ct)
        {
            var query = _ctx.QpsRegistros.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.StatusHomologacao)
                && Enum.TryParse<EQpsStatusHomologacao>(request.StatusHomologacao, true, out var st))
                query = query.Where(x => x.StatusHomologacao == st);

            var total = await query.CountAsync(ct);
            var itens = await query
                .OrderByDescending(x => x.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(ct);

            return CommandResult.Ok("Registros de qualidade de fornecedor listados.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }
}
