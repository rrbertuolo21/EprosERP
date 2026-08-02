using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands.Ncr;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.Handlers.Ncr
{
    public class AdicionarCausaRaizNcrCommandHandler : ICommandHandler<AdicionarCausaRaizNcrCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AdicionarCausaRaizNcrCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AdicionarCausaRaizNcrCommand r, CancellationToken ct)
        {
            var ncr = await _ctx.NcrRegistros.FirstOrDefaultAsync(n => n.Id == r.NcrId, ct);
            if (ncr is null) return CommandResult.Falha("NCR nao encontrada.", block: true);
            if (ncr.StatusRegistro == EStatusRegistroQualidade.Encerrado || ncr.StatusRegistro == EStatusRegistroQualidade.Inativo)
                return CommandResult.Falha("NCR encerrada/cancelada nao aceita investigacao.", block: true);

            var causa = new NcrCausaRaiz(r.NcrId, r.Metodo, r.DescricaoAnalise, r.CausaIdentificada, r.Conclusao,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!causa.IsValid) return CommandResult.Falha(causa.Notifications.Select(n => n.Message));

            _ctx.NcrCausasRaiz.Add(causa);
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Causa raiz registrada.", new { causa.Id });
        }
    }

    public class AdicionarAcaoCapaNcrCommandHandler : ICommandHandler<AdicionarAcaoCapaNcrCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AdicionarAcaoCapaNcrCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AdicionarAcaoCapaNcrCommand r, CancellationToken ct)
        {
            var ncr = await _ctx.NcrRegistros.FirstOrDefaultAsync(n => n.Id == r.NcrId, ct);
            if (ncr is null) return CommandResult.Falha("NCR nao encontrada.", block: true);
            // RN-NCR-008: causa raiz antes de aprovar/registrar CAPA.
            if (!await _ctx.NcrCausasRaiz.AnyAsync(c => c.NcrId == r.NcrId, ct))
                return CommandResult.Falha("Registre a causa raiz antes de definir a CAPA (RN-NCR-008).", block: true);

            var acao = new NcrAcaoCapa(r.NcrId, r.TipoAcao, r.Descricao, r.ResponsavelId, r.Prazo, r.EvidenciaObrigatoria,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!acao.IsValid) return CommandResult.Falha(acao.Notifications.Select(n => n.Message));

            _ctx.NcrAcoesCapa.Add(acao);
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Acao CAPA adicionada.", new { acao.Id, Status = acao.Status.ToString() });
        }
    }

    public class ConcluirAcaoCapaNcrCommandHandler : ICommandHandler<ConcluirAcaoCapaNcrCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ICurrentUser _user;
        public ConcluirAcaoCapaNcrCommandHandler(ContextQualidade ctx, ICurrentUser user) { _ctx = ctx; _user = user; }

        public async Task<CommandResult> Handle(ConcluirAcaoCapaNcrCommand r, CancellationToken ct)
        {
            var acao = await _ctx.NcrAcoesCapa.FirstOrDefaultAsync(a => a.Id == r.AcaoId, ct);
            if (acao is null) return CommandResult.Falha("Acao CAPA nao encontrada.", block: true);

            acao.Concluir(r.Resultado, _user.GetUserId() ?? "system");
            if (!acao.IsValid) return CommandResult.Falha(acao.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Acao CAPA concluida.", new { acao.Id, Status = acao.Status.ToString() });
        }
    }

    public class RegistrarVerificacaoEficaciaNcrCommandHandler : ICommandHandler<RegistrarVerificacaoEficaciaNcrCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarVerificacaoEficaciaNcrCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarVerificacaoEficaciaNcrCommand r, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var ncr = await _ctx.NcrRegistros.FirstOrDefaultAsync(n => n.Id == r.NcrId, ct);
            if (ncr is null) return CommandResult.Falha("NCR nao encontrada.", block: true);

            var verif = new NcrVerificacaoEficacia(r.NcrId, r.AcaoCapaId, r.Criterio, r.Resultado, r.DescricaoResultado,
                r.VerificadoPor, r.ProximaAcao, _tenant.GetTenantId(), usuario);
            if (!verif.IsValid) return CommandResult.Falha(verif.Notifications.Select(n => n.Message));
            _ctx.NcrVerificacoesEficacia.Add(verif);

            // RN-NCR-012: verificacao reprovada reabre a NCR para a etapa CAPA.
            if (r.Resultado == ENcrResultadoVerificacao.Reprovada)
                ncr.AvancarEtapa(ENcrEtapa.CAPA, usuario);
            else
                ncr.AvancarEtapa(ENcrEtapa.Verificacao, usuario);

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Verificacao de eficacia registrada.", new
            {
                verif.Id, Resultado = r.Resultado.ToString(), Etapa = ncr.EtapaNcr.ToString()
            });
        }
    }

    public class AvancarEtapaNcrCommandHandler : ICommandHandler<AvancarEtapaNcrCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ICurrentUser _user;
        public AvancarEtapaNcrCommandHandler(ContextQualidade ctx, ICurrentUser user) { _ctx = ctx; _user = user; }

        public async Task<CommandResult> Handle(AvancarEtapaNcrCommand r, CancellationToken ct)
        {
            var ncr = await _ctx.NcrRegistros.FirstOrDefaultAsync(n => n.Id == r.NcrId, ct);
            if (ncr is null) return CommandResult.Falha("NCR nao encontrada.", block: true);
            if (ncr.StatusRegistro == EStatusRegistroQualidade.Encerrado || ncr.StatusRegistro == EStatusRegistroQualidade.Inativo)
                return CommandResult.Falha("NCR encerrada/cancelada nao muda de etapa.", block: true);

            // Guardas de pre-requisito (D4).
            if (r.NovaEtapa == ENcrEtapa.CAPA && !await _ctx.NcrCausasRaiz.AnyAsync(c => c.NcrId == r.NcrId, ct))
                return CommandResult.Falha("Registre a causa raiz antes de avancar para CAPA (RN-NCR-008).", block: true);
            if (r.NovaEtapa == ENcrEtapa.Verificacao && !await _ctx.NcrAcoesCapa.AnyAsync(a => a.NcrId == r.NcrId, ct))
                return CommandResult.Falha("Defina ao menos uma acao CAPA antes da verificacao (RN-NCR-009).", block: true);
            // Encerrar/cancelar tem comandos proprios.
            if (r.NovaEtapa == ENcrEtapa.Encerrada || r.NovaEtapa == ENcrEtapa.Cancelada)
                return CommandResult.Falha("Use os comandos de encerrar/cancelar para essas etapas.", block: true);

            ncr.AvancarEtapa(r.NovaEtapa, _user.GetUserId() ?? "system");
            if (!ncr.IsValid) return CommandResult.Falha(ncr.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Etapa da NCR avancada.", new { ncr.Id, Etapa = ncr.EtapaNcr.ToString(), Status = ncr.StatusRegistro.ToString() });
        }
    }

    public class EncerrarNcrCommandHandler : ICommandHandler<EncerrarNcrCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public EncerrarNcrCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(EncerrarNcrCommand r, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var ncr = await _ctx.NcrRegistros.FirstOrDefaultAsync(n => n.Id == r.NcrId, ct);
            if (ncr is null) return CommandResult.Falha("NCR nao encontrada.", block: true);

            // RN-NCR-010: nao encerra com acao CAPA obrigatoria/aberta.
            if (await _ctx.NcrAcoesCapa.AnyAsync(a => a.NcrId == r.NcrId
                && (a.Status == ENcrStatusAcao.Pendente || a.Status == ENcrStatusAcao.EmExecucao), ct))
                return CommandResult.Falha("Nao e possivel encerrar: ha acao CAPA aberta (RN-NCR-010).", block: true);

            // RN-NCR-011: verificacao de eficacia obrigatoria quando ha CAPA; exige verificacao aprovada.
            var temCapa = await _ctx.NcrAcoesCapa.AnyAsync(a => a.NcrId == r.NcrId, ct);
            if (temCapa && !await _ctx.NcrVerificacoesEficacia.AnyAsync(v => v.NcrId == r.NcrId
                && v.Resultado == ENcrResultadoVerificacao.Aprovada, ct))
                return CommandResult.Falha("Nao e possivel encerrar: a verificacao de eficacia da CAPA precisa estar aprovada (RN-NCR-011).", block: true);

            ncr.Encerrar(r.Conclusao, usuario);
            if (!ncr.IsValid) return CommandResult.Falha(ncr.Notifications.Select(n => n.Message));

            _ctx.OutboxMessages.Add(new OutboxMessage(_tenant.GetTenantId(), CatalogoEventosIntegracao.Qualidade.NcrEncerrada,
                JsonSerializer.Serialize(new { ncrId = ncr.Id, ncr.Codigo, tenantId = _tenant.GetTenantId() })));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("NCR encerrada.", new { ncr.Id, Status = ncr.StatusRegistro.ToString() });
        }
    }

    public class CancelarNcrCommandHandler : ICommandHandler<CancelarNcrCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ICurrentUser _user;
        public CancelarNcrCommandHandler(ContextQualidade ctx, ICurrentUser user) { _ctx = ctx; _user = user; }

        public async Task<CommandResult> Handle(CancelarNcrCommand r, CancellationToken ct)
        {
            var ncr = await _ctx.NcrRegistros.FirstOrDefaultAsync(n => n.Id == r.NcrId, ct);
            if (ncr is null) return CommandResult.Falha("NCR nao encontrada.", block: true);

            ncr.Cancelar(r.Motivo, _user.GetUserId() ?? "system");
            if (!ncr.IsValid) return CommandResult.Falha(ncr.Notifications.Select(n => n.Message));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("NCR cancelada.", new { ncr.Id, Status = ncr.StatusRegistro.ToString() });
        }
    }
}
