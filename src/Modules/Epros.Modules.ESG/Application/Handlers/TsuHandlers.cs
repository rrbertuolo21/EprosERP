using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Application.Commands;
using Epros.Modules.ESG.Application.EventHandlers;
using Epros.Modules.ESG.Domain.Entities;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class CriarRegistroTsuCommandHandler : ICommandHandler<CriarRegistroTsuCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarRegistroTsuCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }

        public async Task<CommandResult> Handle(CriarRegistroTsuCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";
            if (await _context.RegistrosTsu.AnyAsync(r => r.Codigo == request.Codigo, ct))
                return CommandResult.Falha($"Ja existe registro de transporte com codigo {request.Codigo}.");

            var reg = new TsuRegistro(request.Codigo, request.Descricao, request.ResponsavelId, tenantId, usuario);
            if (!reg.IsValid) return CommandResult.Falha(reg.Notifications.Select(n => n.Message));
            _context.RegistrosTsu.Add(reg);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Registro de transporte criado!", new { reg.Id });
        }
    }

    public class SubmeterRegistroTsuCommandHandler : ICommandHandler<SubmeterRegistroTsuCommand>
    {
        private readonly ContextESG _context;
        private readonly ICurrentUser _user;
        public SubmeterRegistroTsuCommandHandler(ContextESG c, ICurrentUser u) { _context = c; _user = u; }
        public async Task<CommandResult> Handle(SubmeterRegistroTsuCommand request, CancellationToken ct)
        {
            var reg = await _context.RegistrosTsu.FirstOrDefaultAsync(r => r.Id == request.RegistroId, ct);
            if (reg == null) return CommandResult.Falha("Registro nao encontrado.");
            reg.Submeter(_user.GetUserId() ?? "system");
            if (!reg.IsValid) return CommandResult.Falha(reg.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Registro submetido.", new { reg.Id, Status = reg.Status.ToString() });
        }
    }

    public class AprovarRegistroTsuCommandHandler : ICommandHandler<AprovarRegistroTsuCommand>
    {
        private readonly ContextESG _context;
        private readonly ICurrentUser _user;
        public AprovarRegistroTsuCommandHandler(ContextESG c, ICurrentUser u) { _context = c; _user = u; }
        public async Task<CommandResult> Handle(AprovarRegistroTsuCommand request, CancellationToken ct)
        {
            var reg = await _context.RegistrosTsu.FirstOrDefaultAsync(r => r.Id == request.RegistroId, ct);
            if (reg == null) return CommandResult.Falha("Registro nao encontrado.");
            reg.Aprovar(_user.GetUserId() ?? "system");
            if (!reg.IsValid) return CommandResult.Falha(reg.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Registro aprovado.", new { reg.Id, Status = reg.Status.ToString() });
        }
    }

    public class AdicionarOperacaoTsuCommandHandler : ICommandHandler<AdicionarOperacaoTsuCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AdicionarOperacaoTsuCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }
        public async Task<CommandResult> Handle(AdicionarOperacaoTsuCommand request, CancellationToken ct)
        {
            if (!await _context.RegistrosTsu.AnyAsync(r => r.Id == request.RegistroTsuId, ct))
                return CommandResult.Falha("Registro de transporte nao encontrado.");
            var op = new TsuOperacao(request.RegistroTsuId, request.Referencia, request.DataOperacao, request.Modal, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!op.IsValid) return CommandResult.Falha(op.Notifications.Select(n => n.Message));
            _context.OperacoesTsu.Add(op);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Operacao adicionada!", new { op.Id });
        }
    }

    public class AdicionarTrechoTsuCommandHandler : ICommandHandler<AdicionarTrechoTsuCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AdicionarTrechoTsuCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }
        public async Task<CommandResult> Handle(AdicionarTrechoTsuCommand request, CancellationToken ct)
        {
            if (!await _context.OperacoesTsu.AnyAsync(o => o.Id == request.OperacaoId, ct))
                return CommandResult.Falha("Operacao nao encontrada.");
            var trecho = new TsuTrecho(request.OperacaoId, request.Modal, request.Distancia, request.UnidadeDistancia,
                request.Massa, request.UnidadeMassa, request.EnergiaCombustivel, request.UnidadeEnergia, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!trecho.IsValid) return CommandResult.Falha(trecho.Notifications.Select(n => n.Message));
            _context.TrechosTsu.Add(trecho);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Trecho adicionado!", new { trecho.Id, Tkm = trecho.CalcularTkm() });
        }
    }

    public class CalcularEmissaoTsuCommandHandler : ICommandHandler<CalcularEmissaoTsuCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CalcularEmissaoTsuCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }

        public async Task<CommandResult> Handle(CalcularEmissaoTsuCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var trecho = await _context.TrechosTsu.FirstOrDefaultAsync(t => t.Id == request.TrechoId, ct);
            if (trecho == null) return CommandResult.Falha("Trecho nao encontrado.");
            if (!trecho.IsValid) return CommandResult.Falha(trecho.Notifications.Select(n => n.Message)); // RN-TSU-013

            var operacao = await _context.OperacoesTsu.FirstOrDefaultAsync(o => o.Id == trecho.OperacaoId, ct);
            var data = operacao?.DataOperacao ?? DateTime.UtcNow;
            var tkm = trecho.CalcularTkm();

            // NF-07/NF-04: fator COMPARTILHADO com o GHG (mesmo catalogo versionado). // valida-humano.
            var fator = await ResolvedorFatorEmissaoGee.ResolverVigenteAsync(
                _context, tenantId, GhgFatorCodigos.TransportePorTkm, data, ct);

            var calculo = fator != null
                ? TsuCalculo.CalculadoComFator(trecho.Id, tkm, request.FormulaVersao, fator, tenantId, usuario)
                // Regra #0: sem fator homologado, calculo pendente — NAO emite numero inventado.
                : TsuCalculo.PendenteDeFator(trecho.Id, tkm, request.FormulaVersao, GhgFatorCodigos.TransportePorTkm, tenantId, usuario);

            if (!calculo.IsValid) return CommandResult.Falha(calculo.Notifications.Select(n => n.Message));
            _context.CalculosTsu.Add(calculo);
            await _context.SaveChangesAsync(ct);

            var msg = calculo.FatorPendente
                ? "Calculo registrado como PENDENTE DE FATOR (NF-07): tkm apurado, CO2e aguarda fator oficial homologado."
                : "Emissao de transporte calculada!";
            return CommandResult.Ok(msg, new { calculo.Id, calculo.Tkm, calculo.ResultadoCO2e, calculo.Intensidade, calculo.FatorPendente });
        }
    }

    public class DefinirMetaModalTsuCommandHandler : ICommandHandler<DefinirMetaModalTsuCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public DefinirMetaModalTsuCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }
        public async Task<CommandResult> Handle(DefinirMetaModalTsuCommand request, CancellationToken ct)
        {
            if (!await _context.RegistrosTsu.AnyAsync(r => r.Id == request.RegistroTsuId, ct))
                return CommandResult.Falha("Registro de transporte nao encontrado.");
            var meta = new TsuMetaModal(request.RegistroTsuId, request.Modal, request.ParticipacaoAlvo, request.PeriodoInicio, request.PeriodoFim, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!meta.IsValid) return CommandResult.Falha(meta.Notifications.Select(n => n.Message));
            _context.MetasModalTsu.Add(meta);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Meta modal definida!", new { meta.Id });
        }
    }

    public class RegistrarMedicaoModalTsuCommandHandler : ICommandHandler<RegistrarMedicaoModalTsuCommand>
    {
        private readonly ContextESG _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarMedicaoModalTsuCommandHandler(ContextESG c, ITenantProvider t, ICurrentUser u) { _context = c; _tenant = t; _user = u; }
        public async Task<CommandResult> Handle(RegistrarMedicaoModalTsuCommand request, CancellationToken ct)
        {
            if (!await _context.MetasModalTsu.AnyAsync(m => m.Id == request.MetaModalId, ct))
                return CommandResult.Falha("Meta modal nao encontrada.");
            var med = new TsuMedicao(request.MetaModalId, request.Periodo, request.ParticipacaoRealizada, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!med.IsValid) return CommandResult.Falha(med.Notifications.Select(n => n.Message));
            _context.MedicoesModalTsu.Add(med);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Medicao modal registrada!", new { med.Id });
        }
    }
}
