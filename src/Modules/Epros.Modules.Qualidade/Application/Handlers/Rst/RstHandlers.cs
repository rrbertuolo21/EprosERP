using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Application.Commands.Rst;
using Epros.Modules.Qualidade.Application.Queries.Rst;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Domain.Services.Rst;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Qualidade.Application.Handlers.Rst
{
    public class CriarCampanhaRecallCommandHandler : ICommandHandler<CriarCampanhaRecallCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarCampanhaRecallCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarCampanhaRecallCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";
            if (await _ctx.RstCampanhas.AnyAsync(x => x.Codigo == r.Codigo, ct))
                return CommandResult.Falha($"Ja existe uma campanha com o codigo '{r.Codigo}' neste tenant.", block: true);

            var camp = new RstCampanha(r.Codigo, r.Titulo, r.Gravidade, r.ResponsavelId, r.Descricao, r.ProdutoId, r.NcrId, tenantId, usuario);
            if (!camp.IsValid) return CommandResult.Falha(camp.Notifications.Select(n => n.Message));
            _ctx.RstCampanhas.Add(camp);

            _ctx.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Qualidade.RstRecallAberto,
                JsonSerializer.Serialize(new { campanhaId = camp.Id, camp.Codigo, gravidade = camp.Gravidade.ToString(), ncrId = camp.NcrId, tenantId })));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Campanha de recall aberta.", new { camp.Id, camp.Codigo, Etapa = camp.Etapa.ToString() });
        }
    }

    public class AdicionarItemAfetadoRecallCommandHandler : ICommandHandler<AdicionarItemAfetadoRecallCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AdicionarItemAfetadoRecallCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AdicionarItemAfetadoRecallCommand r, CancellationToken ct)
        {
            if (!await _ctx.RstCampanhas.AnyAsync(c => c.Id == r.CampanhaId, ct))
                return CommandResult.Falha("Campanha nao encontrada.", block: true);
            var item = new RstItemAfetado(r.CampanhaId, r.Quantidade, r.ProdutoId, r.Lote, r.Serial, r.Localizacao,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!item.IsValid) return CommandResult.Falha(item.Notifications.Select(n => n.Message));
            _ctx.RstItensAfetados.Add(item);
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Item afetado adicionado.", new { item.Id });
        }
    }

    public class RegistrarGenealogiaNoCommandHandler : ICommandHandler<RegistrarGenealogiaNoCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarGenealogiaNoCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarGenealogiaNoCommand r, CancellationToken ct)
        {
            if (!await _ctx.RstCampanhas.AnyAsync(c => c.Id == r.CampanhaId, ct))
                return CommandResult.Falha("Campanha nao encontrada.", block: true);
            var no = new RstGenealogiaNo(r.CampanhaId, r.TipoNo, r.Nivel, r.PaiId, r.ProdutoId, r.Lote, r.Serial,
                r.Lacuna, r.Justificativa, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!no.IsValid) return CommandResult.Falha(no.Notifications.Select(n => n.Message));
            _ctx.RstGenealogiaNos.Add(no);
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("No de genealogia registrado.", new { no.Id, no.Lacuna });
        }
    }

    public class SolicitarBloqueioRecallCommandHandler : ICommandHandler<SolicitarBloqueioRecallCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public SolicitarBloqueioRecallCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(SolicitarBloqueioRecallCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";
            var camp = await _ctx.RstCampanhas.FirstOrDefaultAsync(c => c.Id == r.CampanhaId, ct);
            if (camp is null) return CommandResult.Falha("Campanha nao encontrada.", block: true);

            var bloqueio = new RstBloqueio(r.CampanhaId, r.Quantidade, r.Lote, r.Serial, r.Motivo, tenantId, usuario);
            if (!bloqueio.IsValid) return CommandResult.Falha(bloqueio.Notifications.Select(n => n.Message));
            _ctx.RstBloqueios.Add(bloqueio);

            // Contencao: solicita bloqueio ao Estoque (D6/D24) — nao movimenta saldo.
            _ctx.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Qualidade.RstBloqueioSolicitado,
                JsonSerializer.Serialize(new { campanhaId = r.CampanhaId, bloqueioId = bloqueio.Id, r.Lote, r.Serial, quantidade = r.Quantidade, tenantId })));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Bloqueio de contencao solicitado ao Estoque.", new { bloqueio.Id });
        }
    }

    public class RegistrarComunicacaoRecallCommandHandler : ICommandHandler<RegistrarComunicacaoRecallCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarComunicacaoRecallCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarComunicacaoRecallCommand r, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            if (!await _ctx.RstCampanhas.AnyAsync(c => c.Id == r.CampanhaId, ct))
                return CommandResult.Falha("Campanha nao encontrada.", block: true);

            var com = new RstComunicacao(r.CampanhaId, r.Canal, r.Conteudo, _tenant.GetTenantId(), usuario);
            if (!com.IsValid) return CommandResult.Falha(com.Notifications.Select(n => n.Message));
            if (r.Aprovar) com.Aprovar(r.AprovadoPor ?? Guid.Empty, usuario);

            _ctx.RstComunicacoes.Add(com);
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Comunicacao registrada.", new { com.Id, Status = com.Status.ToString() });
        }
    }

    public class RegistrarRecolhimentoRecallCommandHandler : ICommandHandler<RegistrarRecolhimentoRecallCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarRecolhimentoRecallCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarRecolhimentoRecallCommand r, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            if (!await _ctx.RstCampanhas.AnyAsync(c => c.Id == r.CampanhaId, ct))
                return CommandResult.Falha("Campanha nao encontrada.", block: true);

            var rec = new RstRecolhimento(r.CampanhaId, r.QuantidadePrevista, _tenant.GetTenantId(), usuario);
            if (!rec.IsValid) return CommandResult.Falha(rec.Notifications.Select(n => n.Message));
            if (r.QuantidadeRecolhida > 0) rec.RegistrarRecolhimento(r.QuantidadeRecolhida, usuario);

            _ctx.RstRecolhimentos.Add(rec);
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Recolhimento registrado.", new { rec.Id, Status = rec.Status.ToString(), rec.QuantidadeRecolhida });
        }
    }

    public class RegistrarDisposicaoRecallCommandHandler : ICommandHandler<RegistrarDisposicaoRecallCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarDisposicaoRecallCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarDisposicaoRecallCommand r, CancellationToken ct)
        {
            if (!await _ctx.RstCampanhas.AnyAsync(c => c.Id == r.CampanhaId, ct))
                return CommandResult.Falha("Campanha nao encontrada.", block: true);
            var disp = new RstDisposicao(r.CampanhaId, r.TipoDisposicao, r.Quantidade, r.Observacao,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!disp.IsValid) return CommandResult.Falha(disp.Notifications.Select(n => n.Message));
            _ctx.RstDisposicoes.Add(disp);
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Disposicao registrada.", new { disp.Id });
        }
    }

    public class AvancarEtapaRecallCommandHandler : ICommandHandler<AvancarEtapaRecallCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ICurrentUser _user;
        public AvancarEtapaRecallCommandHandler(ContextQualidade ctx, ICurrentUser user) { _ctx = ctx; _user = user; }

        public async Task<CommandResult> Handle(AvancarEtapaRecallCommand r, CancellationToken ct)
        {
            var camp = await _ctx.RstCampanhas.FirstOrDefaultAsync(c => c.Id == r.CampanhaId, ct);
            if (camp is null) return CommandResult.Falha("Campanha nao encontrada.", block: true);
            if (r.NovaEtapa == ERstEtapaCampanha.Encerramento || r.NovaEtapa == ERstEtapaCampanha.Cancelada)
                return CommandResult.Falha("Use os comandos de encerrar/cancelar para essas etapas.", block: true);

            camp.AvancarEtapa(r.NovaEtapa, _user.GetUserId() ?? "system");
            if (!camp.IsValid) return CommandResult.Falha(camp.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Etapa da campanha avancada.", new { camp.Id, Etapa = camp.Etapa.ToString() });
        }
    }

    public class EncerrarRecallCommandHandler : ICommandHandler<EncerrarRecallCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public EncerrarRecallCommandHandler(ContextQualidade ctx, ITenantProvider tenant, ICurrentUser user)
        { _ctx = ctx; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(EncerrarRecallCommand r, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var camp = await _ctx.RstCampanhas.FirstOrDefaultAsync(c => c.Id == r.CampanhaId, ct);
            if (camp is null) return CommandResult.Falha("Campanha nao encontrada.", block: true);

            camp.Encerrar(r.Conclusao, _user.GetUserId() ?? "system");
            if (!camp.IsValid) return CommandResult.Falha(camp.Notifications.Select(n => n.Message));

            _ctx.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Qualidade.RstRecallEncerrado,
                JsonSerializer.Serialize(new { campanhaId = camp.Id, camp.Codigo, tenantId })));

            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Campanha de recall encerrada.", new { camp.Id, Status = camp.Status.ToString() });
        }
    }

    public class CancelarRecallCommandHandler : ICommandHandler<CancelarRecallCommand>
    {
        private readonly ContextQualidade _ctx;
        private readonly ICurrentUser _user;
        public CancelarRecallCommandHandler(ContextQualidade ctx, ICurrentUser user) { _ctx = ctx; _user = user; }

        public async Task<CommandResult> Handle(CancelarRecallCommand r, CancellationToken ct)
        {
            var camp = await _ctx.RstCampanhas.FirstOrDefaultAsync(c => c.Id == r.CampanhaId, ct);
            if (camp is null) return CommandResult.Falha("Campanha nao encontrada.", block: true);
            camp.Cancelar(r.Motivo, _user.GetUserId() ?? "system");
            if (!camp.IsValid) return CommandResult.Falha(camp.Notifications.Select(n => n.Message));
            await _ctx.SaveChangesAsync(ct);
            return CommandResult.Ok("Campanha cancelada.", new { camp.Id, Status = camp.Status.ToString() });
        }
    }

    // ================= Queries =================
    public class ListarCampanhasRecallQueryHandler : IQueryHandler<ListarCampanhasRecallQuery, CommandResult>
    {
        private readonly ContextQualidade _ctx;
        public ListarCampanhasRecallQueryHandler(ContextQualidade ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(ListarCampanhasRecallQuery request, CancellationToken ct)
        {
            var query = _ctx.RstCampanhas.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Etapa) && Enum.TryParse<ERstEtapaCampanha>(request.Etapa, true, out var et))
                query = query.Where(x => x.Etapa == et);

            var total = await query.CountAsync(ct);
            var itens = await query
                .OrderByDescending(x => x.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(ct);

            return CommandResult.Ok("Campanhas de recall listadas.", new { total, request.Pagina, request.TamanhoPagina, itens });
        }
    }

    public class ObterGenealogiaRecallQueryHandler : IQueryHandler<ObterGenealogiaRecallQuery, CommandResult>
    {
        private readonly ContextQualidade _ctx;
        private readonly MotorGenealogia _motor;
        public ObterGenealogiaRecallQueryHandler(ContextQualidade ctx, MotorGenealogia motor) { _ctx = ctx; _motor = motor; }

        public async Task<CommandResult> Handle(ObterGenealogiaRecallQuery request, CancellationToken ct)
        {
            var nos = await _ctx.RstGenealogiaNos.AsNoTracking()
                .Where(n => n.CampanhaId == request.CampanhaId).ToListAsync(ct);

            var entrada = nos.Select(n => new NoGenealogia(n.Id, n.PaiId,
                RotuloNo(n), n.Lacuna));
            var arvore = _motor.MontarArvore(entrada);

            return CommandResult.Ok("Arvore de genealogia montada.", new
            {
                campanhaId = request.CampanhaId,
                arvore.TemLacuna,
                arvore.TotalNos,
                raizes = arvore.Raizes.Select(Projetar).ToList()
            });
        }

        private static string RotuloNo(RstGenealogiaNo n)
            => $"{n.TipoNo}:{n.Lote ?? n.Serial ?? n.ProdutoId?.ToString() ?? n.Id.ToString()}";

        private static object Projetar(NoArvoreGenealogia no) => new
        {
            no.Id,
            no.Rotulo,
            no.Lacuna,
            no.Nivel,
            filhos = no.Filhos.Select(Projetar).ToList()
        };
    }
}
