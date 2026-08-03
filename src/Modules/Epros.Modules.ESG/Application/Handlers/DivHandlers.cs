using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class CriarProgramaDivCommandHandler : ICommandHandler<CriarProgramaDivCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarProgramaDivCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }

        public async Task<CommandResult> Handle(CriarProgramaDivCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (await _context.ProgramasDiv.AnyAsync(p => p.Codigo == request.Codigo, ct))
                return CommandResult.Falha($"Ja existe programa social com codigo {request.Codigo}.");

            var programa = new DivPrograma(request.Codigo, request.Descricao, request.ResponsavelId, request.Observacao, tenantId, usuario);
            if (!programa.IsValid) return CommandResult.Falha(programa.Notifications.Select(n => n.Message));

            _context.ProgramasDiv.Add(programa);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Programa social criado!", new { programa.Id });
        }
    }

    public class SubmeterProgramaDivCommandHandler : ICommandHandler<SubmeterProgramaDivCommand>
    {
        private readonly ContextESG _context;
        private readonly ICurrentUser _user;
        public SubmeterProgramaDivCommandHandler(ContextESG c, ICurrentUser u) { _context = c; _user = u; }

        public async Task<CommandResult> Handle(SubmeterProgramaDivCommand request, CancellationToken ct)
        {
            var programa = await _context.ProgramasDiv.FirstOrDefaultAsync(p => p.Id == request.ProgramaId, ct);
            if (programa == null) return CommandResult.Falha("Programa social nao encontrado.");
            programa.Submeter(_user.GetUserId() ?? "system");
            if (!programa.IsValid) return CommandResult.Falha(programa.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Programa submetido.", new { programa.Id, Status = programa.Status.ToString() });
        }
    }

    public class AprovarProgramaDivCommandHandler : ICommandHandler<AprovarProgramaDivCommand>
    {
        private readonly ContextESG _context;
        private readonly ICurrentUser _user;
        public AprovarProgramaDivCommandHandler(ContextESG c, ICurrentUser u) { _context = c; _user = u; }

        public async Task<CommandResult> Handle(AprovarProgramaDivCommand request, CancellationToken ct)
        {
            var programa = await _context.ProgramasDiv.FirstOrDefaultAsync(p => p.Id == request.ProgramaId, ct);
            if (programa == null) return CommandResult.Falha("Programa social nao encontrado.");
            programa.Aprovar(_user.GetUserId() ?? "system");
            if (!programa.IsValid) return CommandResult.Falha(programa.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Programa aprovado.", new { programa.Id, Status = programa.Status.ToString() });
        }
    }

    public class CriarIndicadorDivCommandHandler : ICommandHandler<CriarIndicadorDivCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarIndicadorDivCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }

        public async Task<CommandResult> Handle(CriarIndicadorDivCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (await _context.IndicadoresDiv.AnyAsync(i => i.Codigo == request.Codigo && i.Versao == request.Versao, ct))
                return CommandResult.Falha($"Ja existe indicador {request.Codigo} versao {request.Versao}.");

            if (request.ProgramaId.HasValue && !await _context.ProgramasDiv.AnyAsync(p => p.Id == request.ProgramaId.Value, ct))
                return CommandResult.Falha("Programa vinculado nao encontrado.");

            var indicador = new DivIndicador(request.ProgramaId, request.Codigo, request.Versao, request.Descricao, request.Unidade, request.Fonte, request.Formula, tenantId, usuario);
            if (!indicador.IsValid) return CommandResult.Falha(indicador.Notifications.Select(n => n.Message));

            _context.IndicadoresDiv.Add(indicador);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Indicador social criado!", new { indicador.Id });
        }
    }

    public class DefinirParametroDivCommandHandler : ICommandHandler<DefinirParametroDivCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public DefinirParametroDivCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }

        public async Task<CommandResult> Handle(DefinirParametroDivCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var existente = await _context.ParametrosDiv.FirstOrDefaultAsync(p => p.Chave == request.Chave, ct);
            if (existente != null)
                return CommandResult.Falha($"Parametro {request.Chave} ja definido. Use uma nova versao/valor governado.");

            var parametro = new DivParametro(request.Chave, request.ValorJson, tenantId, usuario);
            if (!parametro.IsValid) return CommandResult.Falha(parametro.Notifications.Select(n => n.Message));

            _context.ParametrosDiv.Add(parametro);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Parametro DIV definido!", new { parametro.Id });
        }
    }

    public class RegistrarMedicaoDivCommandHandler : ICommandHandler<RegistrarMedicaoDivCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarMedicaoDivCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }

        public async Task<CommandResult> Handle(RegistrarMedicaoDivCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (!await _context.IndicadoresDiv.AnyAsync(i => i.Id == request.IndicadorId, ct))
                return CommandResult.Falha("Indicador nao encontrado.");

            // NF-09/T1: limiar de supressao vem do parametro do tenant, NUNCA constante inventada.
            // valida-humano (privacidade/LGPD). Sem parametro definido -> supressao desligada (0),
            // e a medicao NAO e publicada ate o limiar ser homologado (fica visivel como pendencia de privacidade).
            var limite = 0;
            var paramLimite = await _context.ParametrosDiv.FirstOrDefaultAsync(p => p.Chave == DivParametro.ChaveLimiteSupressaoGrupo, ct);
            if (paramLimite != null && int.TryParse(paramLimite.ValorJson.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                limite = parsed;

            var medicao = new DivMedicao(
                request.IndicadorId, request.PeriodoInicio, request.PeriodoFim, request.Dimensao,
                request.QtdIndividuos, request.ValorAgregado, limite, request.Origem, tenantId, usuario);
            if (!medicao.IsValid) return CommandResult.Falha(medicao.Notifications.Select(n => n.Message));

            var duplicada = await _context.MedicoesDiv.AnyAsync(
                m => m.IndicadorId == request.IndicadorId && m.DimensoesHash == medicao.DimensoesHash, ct);
            if (duplicada) return CommandResult.Falha("Ja existe medicao para este indicador/dimensao/periodo.");

            _context.MedicoesDiv.Add(medicao);
            await _context.SaveChangesAsync(ct);

            var aviso = paramLimite == null
                ? "Medicao registrada, mas o limiar de supressao (NF-09) nao esta parametrizado: pendente de homologacao de privacidade antes da publicacao."
                : "Medicao registrada.";
            return CommandResult.Ok(aviso, new { medicao.Id, medicao.Suprimido, medicao.ValorAgregado, LimiteAplicado = limite });
        }
    }

    public class DefinirMetaDivCommandHandler : ICommandHandler<DefinirMetaDivCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public DefinirMetaDivCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }

        public async Task<CommandResult> Handle(DefinirMetaDivCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (!await _context.IndicadoresDiv.AnyAsync(i => i.Id == request.IndicadorId, ct))
                return CommandResult.Falha("Indicador nao encontrado.");

            var meta = new DivMeta(request.IndicadorId, request.PeriodoInicio, request.PeriodoFim, request.ValorEsperado, request.Unidade, tenantId, usuario);
            if (!meta.IsValid) return CommandResult.Falha(meta.Notifications.Select(n => n.Message));

            _context.MetasDiv.Add(meta);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Meta social definida!", new { meta.Id });
        }
    }

    public class CriarAcaoDivCommandHandler : ICommandHandler<CriarAcaoDivCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarAcaoDivCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }

        public async Task<CommandResult> Handle(CriarAcaoDivCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            if (!await _context.ProgramasDiv.AnyAsync(p => p.Id == request.ProgramaId, ct))
                return CommandResult.Falha("Programa nao encontrado.");

            var acao = new DivAcao(request.ProgramaId, request.Descricao, request.ResponsavelId, request.Prazo, tenantId, usuario);
            if (!acao.IsValid) return CommandResult.Falha(acao.Notifications.Select(n => n.Message));

            _context.AcoesDiv.Add(acao);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Acao social criada!", new { acao.Id });
        }
    }
}
