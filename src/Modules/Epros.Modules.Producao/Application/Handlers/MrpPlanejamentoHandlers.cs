using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Modules.Producao.Domain.Services;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Handlers
{
    /// <summary>PRD-MRP — Criação do ciclo MRP/IBP (MRP-EF §7.1).</summary>
    public class CriarMrpPlanejamentoCommandHandler : ICommandHandler<CriarMrpPlanejamentoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarMrpPlanejamentoCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarMrpPlanejamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var duplicado = await _context.MrpPlanejamentos.AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);
            if (duplicado)
                return CommandResult.Falha($"Código de planejamento '{request.Codigo}' já está em uso.");

            var planejamento = new MrpPlanejamento(request.Codigo, request.ResponsavelId, tenantId, usuario);
            if (!planejamento.IsValid)
                return CommandResult.Falha(planejamento.Notifications.Select(n => n.Message));

            _context.MrpPlanejamentos.Add(planejamento);
            _context.MrpHistoricos.Add(new MrpPlanejamentoHistorico(planejamento.Id, "Criacao", usuario, "{}", tenantId, usuario, null, EStatusWorkflowProducao.Rascunho));
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Planejamento MRP/IBP criado em Rascunho.", new { planejamento.Id, planejamento.Codigo });
        }
    }

    public abstract class MrpTransicaoHandlerBase
    {
        protected readonly ContextProducao _context;
        protected readonly ITenantProvider _tenantProvider;
        protected readonly ICurrentUser _currentUser;

        protected MrpTransicaoHandlerBase(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        protected async Task<(MrpPlanejamento? planejamento, string usuario, string tenantId)> CarregarAsync(Guid id, CancellationToken ct)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var tenantId = _tenantProvider.GetTenantId();
            var planejamento = await _context.MrpPlanejamentos.FirstOrDefaultAsync(p => p.Id == id, ct);
            return (planejamento, usuario, tenantId);
        }

        protected async Task<CommandResult> FinalizarAsync(MrpPlanejamento p, string acao, string usuario, string tenantId, CancellationToken ct)
        {
            if (!p.IsValid)
                return CommandResult.Falha(p.Notifications.Select(n => n.Message));

            _context.MrpHistoricos.Add(new MrpPlanejamentoHistorico(p.Id, acao, usuario, "{}", tenantId, usuario, null, p.Status));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"Planejamento {acao} com sucesso.", new { p.Id, Status = p.Status.ToString() });
        }
    }

    public class SubmeterMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<SubmeterMrpPlanejamentoCommand>
    {
        public SubmeterMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(SubmeterMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.SubmeterParaAnalise(usuario);
            return await FinalizarAsync(p, "Submissao", usuario, tenantId, ct);
        }
    }

    public class AprovarMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<AprovarMrpPlanejamentoCommand>
    {
        public AprovarMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(AprovarMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.Aprovar(usuario);
            return await FinalizarAsync(p, "Aprovacao", usuario, tenantId, ct);
        }
    }

    public class RejeitarMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<RejeitarMrpPlanejamentoCommand>
    {
        public RejeitarMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(RejeitarMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.Rejeitar(request.Motivo, usuario);
            return await FinalizarAsync(p, "Rejeicao", usuario, tenantId, ct);
        }
    }

    public class InativarMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<InativarMrpPlanejamentoCommand>
    {
        public InativarMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(InativarMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.Inativar(usuario);
            return await FinalizarAsync(p, "Inativacao", usuario, tenantId, ct);
        }
    }

    public class ReativarMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<ReativarMrpPlanejamentoCommand>
    {
        public ReativarMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(ReativarMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.Reativar(usuario);
            return await FinalizarAsync(p, "Reativacao", usuario, tenantId, ct);
        }
    }

    public class EncerrarMrpPlanejamentoCommandHandler : MrpTransicaoHandlerBase, ICommandHandler<EncerrarMrpPlanejamentoCommand>
    {
        public EncerrarMrpPlanejamentoCommandHandler(ContextProducao c, ITenantProvider t, ICurrentUser u) : base(c, t, u) { }
        public async Task<CommandResult> Handle(EncerrarMrpPlanejamentoCommand request, CancellationToken ct)
        {
            var (p, usuario, tenantId) = await CarregarAsync(request.Id, ct);
            if (p == null) return CommandResult.Falha("Planejamento não encontrado.");
            p.Encerrar(usuario);
            return await FinalizarAsync(p, "Encerramento", usuario, tenantId, ct);
        }
    }

    /// <summary>
    /// PD3/PD21 — Motor MRP: explode a BOM vigente multinível, faz o netting e persiste
    /// necessidades (bruta/líquida) e sugestões (compra/produção). Recalcula preservando sugestões
    /// já convertidas (idempotência PRD-INT-CA-002). BOM incompleta → resultado sinaliza cálculo incompleto.
    /// </summary>
    public class CalcularMrpCommandHandler : ICommandHandler<CalcularMrpCommand>
    {
        private readonly ContextProducao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CalcularMrpCommandHandler(ContextProducao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CalcularMrpCommand request, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var planejamento = await _context.MrpPlanejamentos.FirstOrDefaultAsync(p => p.Id == request.PlanejamentoId, ct);
            if (planejamento == null) return CommandResult.Falha("Planejamento MRP não encontrado.");
            if (request.Demandas == null || request.Demandas.Count == 0)
                return CommandResult.Falha("Informe ao menos uma demanda para o cálculo do MRP.");

            var catalogo = await _context.BomEstruturas
                .Include(e => e.Componentes)
                .Where(e => e.Status == EStatusWorkflowProducao.Ativo)
                .ToListAsync(ct);

            var parametrosPorItem = (request.Parametros ?? new()).ToDictionary(p => p.ItemId);
            decimal Lookup(Guid item, Func<MrpParametroItemDto, decimal> sel) =>
                parametrosPorItem.TryGetValue(item, out var p) ? sel(p) : 0m;

            var parametros = new MrpService.ParametrosMrp
            {
                Disponibilidade = i => Lookup(i, p => p.Disponibilidade),
                RecebimentosProgramados = i => Lookup(i, p => p.RecebimentosProgramados),
                EstoqueSeguranca = i => Lookup(i, p => p.EstoqueSeguranca),
                LoteMinimo = i => Lookup(i, p => p.LoteMinimo),
                LoteMultiplo = i => Lookup(i, p => p.LoteMultiplo)
            };

            var dataRef = request.Demandas[0].DataReferencia ?? DateTime.UtcNow;
            var demandas = request.Demandas.Select(d =>
                new MrpService.Demanda(d.ItemId, d.Quantidade, d.DataReferencia ?? DateTime.UtcNow, d.VariacaoId));

            var resultado = new MrpService().Planejar(demandas, catalogo, parametros, dataRef);

            // recálculo idempotente: limpa necessidades e sugestões ainda não convertidas do planejamento
            var necessidadesAntigas = await _context.MrpNecessidades
                .Where(n => n.PlanejamentoId == planejamento.Id).ToListAsync(ct);
            _context.MrpNecessidades.RemoveRange(necessidadesAntigas);

            var sugestoesAntigas = await _context.MrpSugestoes
                .Where(s => s.PlanejamentoId == planejamento.Id && s.Estado != EEstadoSugestaoMrp.Convertida)
                .ToListAsync(ct);
            _context.MrpSugestoes.RemoveRange(sugestoesAntigas);

            foreach (var n in resultado.Necessidades)
            {
                _context.MrpNecessidades.Add(new MrpNecessidade(
                    planejamento.Id, n.ItemId, n.Nivel, dataRef,
                    n.Bruta, n.Disponibilidade, n.Recebimentos, n.EstoqueSeguranca, tenantId, usuario));
            }

            foreach (var s in resultado.Sugestoes)
            {
                _context.MrpSugestoes.Add(new MrpSugestao(
                    planejamento.Id, s.ItemId, s.Tipo, s.Quantidade, dataRef, tenantId, usuario));
            }

            _context.MrpHistoricos.Add(new MrpPlanejamentoHistorico(
                planejamento.Id, "CalculoMrp", usuario, "{}", tenantId, usuario, null, planejamento.Status));
            await _context.SaveChangesAsync(ct);

            return CommandResult.Ok(
                resultado.CalculoIncompleto
                    ? $"MRP calculado com pendências: {resultado.MotivoIncompleto}"
                    : "MRP calculado: necessidades e sugestões geradas.",
                new
                {
                    planejamento.Id,
                    Necessidades = resultado.Necessidades.Count,
                    Sugestoes = resultado.Sugestoes.Count,
                    resultado.CalculoIncompleto
                });
        }
    }

    public abstract class MrpSugestaoHandlerBase
    {
        protected readonly ContextProducao _context;
        protected readonly ICurrentUser _currentUser;
        protected MrpSugestaoHandlerBase(ContextProducao context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        protected async Task<CommandResult> AplicarAsync(Guid sugestaoId, Action<MrpSugestao, string> acao, string ok, CancellationToken ct)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var sugestao = await _context.MrpSugestoes.FirstOrDefaultAsync(s => s.Id == sugestaoId, ct);
            if (sugestao == null) return CommandResult.Falha("Sugestão MRP não encontrada.");
            acao(sugestao, usuario);
            if (!sugestao.IsValid) return CommandResult.Falha(sugestao.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok(ok, new { sugestao.Id, Estado = sugestao.Estado.ToString(), sugestao.DocumentoGeradoId });
        }
    }

    public class SubmeterSugestaoMrpCommandHandler : MrpSugestaoHandlerBase, ICommandHandler<SubmeterSugestaoMrpCommand>
    {
        public SubmeterSugestaoMrpCommandHandler(ContextProducao c, ICurrentUser u) : base(c, u) { }
        public Task<CommandResult> Handle(SubmeterSugestaoMrpCommand r, CancellationToken ct)
            => AplicarAsync(r.SugestaoId, (s, u) => s.SubmeterAprovacao(u), "Sugestão submetida para aprovação.", ct);
    }

    public class AprovarSugestaoMrpCommandHandler : MrpSugestaoHandlerBase, ICommandHandler<AprovarSugestaoMrpCommand>
    {
        public AprovarSugestaoMrpCommandHandler(ContextProducao c, ICurrentUser u) : base(c, u) { }
        public Task<CommandResult> Handle(AprovarSugestaoMrpCommand r, CancellationToken ct)
            => AplicarAsync(r.SugestaoId, (s, u) => s.Aprovar(u), "Sugestão aprovada.", ct);
    }

    public class ConverterSugestaoMrpCommandHandler : MrpSugestaoHandlerBase, ICommandHandler<ConverterSugestaoMrpCommand>
    {
        public ConverterSugestaoMrpCommandHandler(ContextProducao c, ICurrentUser u) : base(c, u) { }
        public Task<CommandResult> Handle(ConverterSugestaoMrpCommand r, CancellationToken ct)
            => AplicarAsync(r.SugestaoId, (s, u) => s.Converter(r.DocumentoGeradoId, u), "Sugestão convertida em documento.", ct);
    }

    public class CancelarSugestaoMrpCommandHandler : MrpSugestaoHandlerBase, ICommandHandler<CancelarSugestaoMrpCommand>
    {
        public CancelarSugestaoMrpCommandHandler(ContextProducao c, ICurrentUser u) : base(c, u) { }
        public Task<CommandResult> Handle(CancelarSugestaoMrpCommand r, CancellationToken ct)
            => AplicarAsync(r.SugestaoId, (s, u) => s.Cancelar(r.Motivo, u), "Sugestão cancelada.", ct);
    }
}
