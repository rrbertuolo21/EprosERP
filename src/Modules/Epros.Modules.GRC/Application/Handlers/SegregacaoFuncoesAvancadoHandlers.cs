using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    /// <summary>D-SOD-03 — bloqueio preventivo pré-gravação (GRC-CMD-002).</summary>
    public class AvaliarConcessaoSoDCommandHandler : ICommandHandler<AvaliarConcessaoSoDCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;

        public AvaliarConcessaoSoDCommandHandler(ContextGRC context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<CommandResult> Handle(AvaliarConcessaoSoDCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var novas = (request.FuncoesNovas ?? new List<Guid>()).ToHashSet();
            var efetivas = (request.FuncoesAtuais ?? new List<Guid>()).Concat(novas).ToHashSet();

            var regrasAtivas = await _context.RegrasSoD.Where(r => r.Status == "Ativo").ToListAsync(cancellationToken);

            var conflitos = regrasAtivas
                .Where(r => efetivas.Contains(r.FuncaoAId) && efetivas.Contains(r.FuncaoBId))
                .Where(r => novas.Count == 0 || novas.Contains(r.FuncaoAId) || novas.Contains(r.FuncaoBId))
                .ToList();

            var bloqueados = conflitos.Where(r => r.BloqueiaConcessao()).ToList();

            string decisao;
            string eventType;
            if (bloqueados.Any())
            {
                decisao = "Bloqueado";
                eventType = CatalogoEventosIntegracao.Grc.SodConcessaoBloqueada;
            }
            else if (conflitos.Any())
            {
                decisao = "PermitidoComExcecao";
                eventType = CatalogoEventosIntegracao.Grc.SodConcessaoAvaliada;
            }
            else
            {
                decisao = "Permitido";
                eventType = CatalogoEventosIntegracao.Grc.SodConcessaoAvaliada;
            }

            var payload = JsonSerializer.Serialize(new
            {
                request.PerfilId,
                request.UsuarioId,
                request.Alvo,
                Decisao = decisao,
                RegrasEmConflito = conflitos.Select(c => c.Id),
                RegrasBloqueantes = bloqueados.Select(c => c.Id)
            });
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, eventType, payload));
            await _context.SaveChangesAsync(cancellationToken);

            var dados = new { Decisao = decisao, QuantidadeConflitos = conflitos.Count, Bloqueia = bloqueados.Any() };
            if (bloqueados.Any())
            {
                return CommandResult.Falha(
                    $"Concessao bloqueada por SoD: {bloqueados.Count} regra(s) incompativel(is) com modo 'Bloqueia'.",
                    mensagem: "Concessao negada por segregacao de funcoes.", block: true);
            }

            return CommandResult.Ok(
                conflitos.Any()
                    ? "Concessao permitida mediante excecao aprovada (conflito de SoD com modo 'PermiteExcecao')."
                    : "Concessao permitida: sem conflito de SoD.",
                dados);
        }
    }

    /// <summary>D-SOD-02 — solicita exceção (EmAnalise, com controle compensatório obrigatório).</summary>
    public class SolicitarExcecaoSoDCommandHandler : ICommandHandler<SolicitarExcecaoSoDCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public SolicitarExcecaoSoDCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SolicitarExcecaoSoDCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var violacao = await _context.ViolacoesSoD.FirstOrDefaultAsync(v => v.Id == request.ViolacaoId, cancellationToken);
            if (violacao == null)
                return CommandResult.Falha("Violacao SoD nao encontrada.");

            var excecao = new ExcecaoSoD(
                request.ViolacaoId, request.Justificativa, request.SolicitanteId,
                request.DataInicio, request.DataFim,
                request.ControleCompensatorioId, request.ControleCompensatorio,
                tenantId, usuario);

            if (!excecao.IsValid)
                return CommandResult.Falha(excecao.Notifications.Select(n => n.Message));

            _context.ExcecoesSoD.Add(excecao);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Excecao SoD solicitada (em analise).", new { ExcecaoId = excecao.Id, excecao.Status });
        }
    }

    /// <summary>D-SOD-02 — aprova exceção (autoaprovação proibida) e mitiga a violação.</summary>
    public class AprovarExcecaoSoDCommandHandler : ICommandHandler<AprovarExcecaoSoDCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IRegistroAuditoriaService _auditoria;

        public AprovarExcecaoSoDCommandHandler(
            ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser, IRegistroAuditoriaService auditoria)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _auditoria = auditoria;
        }

        public async Task<CommandResult> Handle(AprovarExcecaoSoDCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var excecao = await _context.ExcecoesSoD.FirstOrDefaultAsync(e => e.Id == request.ExcecaoId, cancellationToken);
            if (excecao == null)
                return CommandResult.Falha("Excecao SoD nao encontrada.");

            excecao.Aprovar(request.AprovadorId, usuario);
            if (!excecao.IsValid)
                return CommandResult.Falha(excecao.Notifications.Select(n => n.Message));

            var violacao = await _context.ViolacoesSoD.FirstOrDefaultAsync(v => v.Id == excecao.ViolacaoId, cancellationToken);
            violacao?.Mitigar(usuario);

            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Grc.SodExcecaoAprovada,
                JsonSerializer.Serialize(new { excecao.Id, excecao.ViolacaoId, request.AprovadorId })));

            await _context.SaveChangesAsync(cancellationToken);

            await _auditoria.RegistrarAsync("ExcecaoSoD", excecao.Id.ToString(), "Aprovada",
                valoresDepois: JsonSerializer.Serialize(new { excecao.Status, excecao.AprovadorId, excecao.DataFim }),
                cancellationToken: cancellationToken);

            return CommandResult.Ok("Excecao SoD aprovada e violacao mitigada.", new { excecao.Id, excecao.Status });
        }
    }

    /// <summary>D-SOD-02 — renova exceção (respeita SOD_PRAZO_MAX_EXCECAO em dias, se parametrizado).</summary>
    public class RenovarExcecaoSoDCommandHandler : ICommandHandler<RenovarExcecaoSoDCommand>
    {
        private readonly ContextGRC _context;
        private readonly ICurrentUser _currentUser;

        public RenovarExcecaoSoDCommandHandler(ContextGRC context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RenovarExcecaoSoDCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var excecao = await _context.ExcecoesSoD.FirstOrDefaultAsync(e => e.Id == request.ExcecaoId, cancellationToken);
            if (excecao == null)
                return CommandResult.Falha("Excecao SoD nao encontrada.");

            // Prazo máximo por parâmetro (SOD_PRAZO_MAX_EXCECAO em dias a partir de DataInicio), se definido.
            var paramMax = await _context.SegregacaoParametros
                .FirstOrDefaultAsync(p => p.Chave == "SOD_PRAZO_MAX_EXCECAO" && p.Ativo, cancellationToken);
            if (paramMax != null && int.TryParse(paramMax.ValorJson.Trim('"'), out var maxDias))
            {
                var limite = excecao.DataInicio.AddDays(maxDias);
                if (request.NovaDataFim > limite)
                    return CommandResult.Falha($"A renovacao excede o prazo maximo de {maxDias} dias (SOD_PRAZO_MAX_EXCECAO).");
            }

            excecao.Renovar(request.NovaDataFim, usuario);
            if (!excecao.IsValid)
                return CommandResult.Falha(excecao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Excecao SoD renovada.", new { excecao.Id, excecao.DataFim, excecao.Renovacoes });
        }
    }

    /// <summary>RN-SOD-039 — expira exceções vencidas e reativa a violação mitigada.</summary>
    public class ExpirarExcecoesVencidasSoDCommandHandler : ICommandHandler<ExpirarExcecoesVencidasSoDCommand>
    {
        private readonly ContextGRC _context;
        private readonly ICurrentUser _currentUser;

        public ExpirarExcecoesVencidasSoDCommandHandler(ContextGRC context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ExpirarExcecoesVencidasSoDCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var agora = DateTime.UtcNow;

            var vencidas = await _context.ExcecoesSoD
                .Where(e => e.Status == "Aprovada" && e.DataFim < agora)
                .ToListAsync(cancellationToken);

            var reativadas = 0;
            foreach (var excecao in vencidas)
            {
                excecao.MarcarVencida(usuario);
                var violacao = await _context.ViolacoesSoD.FirstOrDefaultAsync(v => v.Id == excecao.ViolacaoId, cancellationToken);
                if (violacao != null && violacao.Status == "Mitigada")
                {
                    violacao.Reativar(usuario);
                    reativadas++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok($"{vencidas.Count} excecao(oes) expirada(s); {reativadas} violacao(oes) reativada(s).",
                new { Expiradas = vencidas.Count, Reativadas = reativadas });
        }
    }

    /// <summary>D-SOD-04 — registra bypass de SoD (nunca silencioso): persiste + alerta + trilha central.</summary>
    public class RegistrarBypassAdminSoDCommandHandler : ICommandHandler<RegistrarBypassAdminSoDCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IRegistroAuditoriaService _auditoria;

        public RegistrarBypassAdminSoDCommandHandler(
            ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser, IRegistroAuditoriaService auditoria)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _auditoria = auditoria;
        }

        public async Task<CommandResult> Handle(RegistrarBypassAdminSoDCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var bypass = new BypassSoD(
                request.RegraId, request.AtorId, request.AtorEhAdmin, request.Motivo,
                request.ControleCompensatorioId, request.ControleCompensatorio, tenantId, usuario);

            if (!bypass.IsValid)
                return CommandResult.Falha(bypass.Notifications.Select(n => n.Message));

            _context.BypassesSoD.Add(bypass);

            // Alerta (evento) — consumidores de compliance/segurança reagem ao bypass.
            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Grc.SodBypassAdmin,
                JsonSerializer.Serialize(new { bypass.Id, bypass.RegraId, bypass.AtorId, bypass.AtorEhAdmin, bypass.Motivo })));

            await _context.SaveChangesAsync(cancellationToken);

            // Trilha imutável central (T8).
            await _auditoria.RegistrarAsync("BypassSoD", bypass.Id.ToString(), "BypassRegistrado",
                valoresDepois: JsonSerializer.Serialize(new { bypass.RegraId, bypass.AtorId, bypass.AtorEhAdmin, bypass.Motivo }),
                cancellationToken: cancellationToken);

            return CommandResult.Ok("Bypass de SoD registrado com alerta e controle compensatorio.", new { bypass.Id });
        }
    }
}
