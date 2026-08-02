using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Application.Outbox;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Outbox
{
    // ============================================================================
    // TRANSVERSAL T1 — CONSUMIDORES REAIS dos Outboxes de QUALIDADE e MANUTENÇÃO,
    // roteados pelo OutboxDispatcher central (substitui os jobs por-módulo que só
    // tratavam um evento e deixavam os demais "morrerem" na fila).
    //
    // Todos são IDEMPOTENTES e EXPLÍCITOS quanto ao tenant (usam message.TenantId +
    // IgnoreQueryFilters), pois o dispatcher não manipula HttpContext.
    //
    // ANTI-DUPLA-CONTAGEM: os eventos de bloqueio/quarentena NÃO recriam saldo nem
    // movimentam o kardex — apenas movem QuantidadeDisponivel -> QuantidadeBloqueada
    // do lote (contenção), coerente com quem já baixou o saldo (motor único).
    // ============================================================================

    // -------------------- QUALIDADE: inspeção reprovada -> baixa de estoque --------------------

    /// <summary>
    /// Consome <c>InspecaoReprovada</c> (Outbox de Qualidade): baixa o lote reprovado pelo MOTOR ÚNICO.
    /// Respeita o motor (se bloquear, não grava) e é idempotente por <c>InspecaoLoteId</c>.
    /// </summary>
    public class InspecaoReprovadaEstoqueConsumer : IOutboxConsumer
    {
        private readonly ContextEstoque _context;
        public InspecaoReprovadaEstoqueConsumer(ContextEstoque context) => _context = context;

        public string EventType => "InspecaoReprovada";

        public async Task ConsumeAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            var p = JsonSerializer.Deserialize<InspecaoReprovadaPayload>(message.Payload, JsonOpts.Default);
            if (p == null || string.IsNullOrWhiteSpace(p.Sku)) return;

            var tenantId = string.IsNullOrWhiteSpace(p.TenantId) ? message.TenantId : p.TenantId;
            var referencia = $"Inspeção reprovada {p.InspecaoLoteId}";

            // Idempotência: fato gerador já criado para esta inspeção.
            var jaProcessado = await _context.FatosGeradoresEstoque
                .IgnoreQueryFilters()
                .AnyAsync(f => f.TenantId == tenantId && f.ReferenciaExterna == referencia, cancellationToken);
            if (jaProcessado) return;

            var produto = await _context.Produtos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Sku == p.Sku, cancellationToken);
            if (produto == null) return; // Produto não rastreado no estoque: nada a baixar (não inventa saldo).

            var motor = new MotorMovimentacaoEstoque(_context, tenantId, "system_quality");
            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.Avaria, tenantId, "system_quality", referenciaExterna: referencia);
            var res = await motor.AplicarSaidaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, produto.Id, p.QuantidadeLote, fato.Id, null, cancellationToken);
            if (!res.Sucesso) return; // motor é a autoridade (D8)

            _context.FatosGeradoresEstoque.Add(fato);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private class InspecaoReprovadaPayload
        {
            public Guid InspecaoLoteId { get; set; }
            public string Sku { get; set; } = string.Empty;
            public string? NomeProduto { get; set; }
            public decimal QuantidadeLote { get; set; }
            public string? TenantId { get; set; }
        }
    }

    // -------------------- MANUTENÇÃO: OS concluída -> baixa das peças --------------------

    /// <summary>
    /// Consome <c>OrdemManutencaoConcluida</c> (Outbox de Manutenção): baixa as peças utilizadas pelo
    /// MOTOR ÚNICO. Respeita o motor e é idempotente por <c>OrdemManutencaoId</c>.
    /// </summary>
    public class OrdemManutencaoConcluidaEstoqueConsumer : IOutboxConsumer
    {
        private readonly ContextEstoque _context;
        public OrdemManutencaoConcluidaEstoqueConsumer(ContextEstoque context) => _context = context;

        public string EventType => "OrdemManutencaoConcluida";

        public async Task ConsumeAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            var p = JsonSerializer.Deserialize<OrdemManutencaoConcluidaPayload>(message.Payload, JsonOpts.Default);
            if (p == null || p.Pecas == null || p.Pecas.Count == 0) return;

            var tenantId = string.IsNullOrWhiteSpace(p.TenantId) ? message.TenantId : p.TenantId;
            var referencia = $"Baixa OS {p.OrdemManutencaoId}";

            var jaProcessado = await _context.MovimentosEstoque
                .IgnoreQueryFilters()
                .AnyAsync(m => m.TenantId == tenantId && m.Tipo == "Saida" && m.Historico.Contains(referencia), cancellationToken);
            if (jaProcessado) return;

            var motor = new MotorMovimentacaoEstoque(_context, tenantId, "system_maintenance");
            var gravou = false;

            foreach (var peca in p.Pecas)
            {
                var produto = await _context.Produtos
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == peca.ProdutoId, cancellationToken);
                if (produto == null) continue;

                var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.SaidaConsumidor, tenantId, "system_maintenance", referenciaExterna: referencia);
                var res = await motor.AplicarSaidaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, peca.ProdutoId, peca.Quantidade, fato.Id, null, cancellationToken);
                if (!res.Sucesso) continue; // motor é a autoridade

                _context.FatosGeradoresEstoque.Add(fato);
                _context.MovimentosEstoque.Add(new MovimentoEstoque(
                    produtoId: peca.ProdutoId, quantidade: peca.Quantidade, tipo: "Saida",
                    historico: $"{referencia} (Equipamento: {p.EquipamentoId})", tenantId: tenantId, criadoPor: "system_maintenance"));
                gravou = true;
            }

            if (gravou) await _context.SaveChangesAsync(cancellationToken);
        }

        private class OrdemManutencaoConcluidaPayload
        {
            public Guid OrdemManutencaoId { get; set; }
            public Guid EquipamentoId { get; set; }
            public List<PecaPayload> Pecas { get; set; } = new();
            public string? TenantId { get; set; }
        }
        private class PecaPayload
        {
            public Guid ProdutoId { get; set; }
            public decimal Quantidade { get; set; }
        }
    }

    // -------------------- QUALIDADE: bloqueio/quarentena/liberação de lote --------------------

    /// <summary>
    /// Base dos consumidores que refletem uma decisão de Qualidade sobre um LOTE no Estoque (contenção),
    /// resolvendo o lote pelo CÓDIGO e movendo saldo disponível -&gt; bloqueado (sem recriar saldo).
    /// </summary>
    public abstract class BloqueioLoteQualidadeConsumerBase : IOutboxConsumer
    {
        protected readonly ContextEstoque Context;
        protected BloqueioLoteQualidadeConsumerBase(ContextEstoque context) => Context = context;

        public abstract string EventType { get; }
        protected abstract ETipoBloqueioLote TipoBloqueio { get; }
        protected abstract string MotivoPrefixo { get; }

        public async Task ConsumeAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            var p = JsonSerializer.Deserialize<LoteEventoPayload>(message.Payload, JsonOpts.Default);
            if (p == null || string.IsNullOrWhiteSpace(p.Lote)) return; // sem lote rastreável (ex.: só serial): nada a conter.

            var tenantId = string.IsNullOrWhiteSpace(p.TenantId) ? message.TenantId : p.TenantId;

            var lote = await Context.LotesEstoque
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.CodigoLote == p.Lote && l.DeletadoEm == null, cancellationToken);
            if (lote == null) return; // lote não rastreado no Estoque: não inventa saldo.

            if (lote.QuantidadeDisponivel <= 0m) return; // nada disponível a bloquear (anti-dupla-contagem).

            // Idempotência: âncora determinística por mensagem de outbox.
            var marca = $"[outbox:{message.Id}]";
            var jaBloqueado = await Context.BloqueiosLote
                .IgnoreQueryFilters()
                .AnyAsync(b => b.LoteId == lote.Id && b.Motivo.Contains(marca), cancellationToken);
            if (jaBloqueado) return;

            decimal? qtd = p.Quantidade > 0m ? Math.Min(p.Quantidade, lote.QuantidadeDisponivel) : (decimal?)null;
            var motivo = $"{MotivoPrefixo} {marca}";

            lote.Bloquear(qtd, "system_quality");
            Context.BloqueiosLote.Add(new BloqueioLote(lote.Id, TipoBloqueio, motivo, tenantId, "system_quality"));
            Context.HistoricosEstoque.Add(new HistoricoEstoque(
                "LoteEstoque", lote.Id, "lote_bloqueado", null, EStatusLoteRastreabilidade.Bloqueado.ToString(),
                motivo, "system_quality", tenantId, "system_quality"));
            Context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.LoteBloqueado,
                JsonSerializer.Serialize(new { loteId = lote.Id, lote.ProdutoId, lote.CodigoLote, tipo = TipoBloqueio.ToString(), origem = EventType })));

            await Context.SaveChangesAsync(cancellationToken);
        }

        protected class LoteEventoPayload
        {
            public string? Lote { get; set; }
            public decimal Quantidade { get; set; }
            public string? TenantId { get; set; }
        }
    }

    /// <summary>qld.acr.lote_bloqueado -&gt; bloqueia o lote no Estoque.</summary>
    public class QualidadeLoteBloqueadoConsumer : BloqueioLoteQualidadeConsumerBase
    {
        public QualidadeLoteBloqueadoConsumer(ContextEstoque context) : base(context) { }
        public override string EventType => CatalogoEventosIntegracao.Qualidade.AcrLoteBloqueado;
        protected override ETipoBloqueioLote TipoBloqueio => ETipoBloqueioLote.Qualidade;
        protected override string MotivoPrefixo => "Bloqueio por decisão de Qualidade (ACR rejeitado)";
    }

    /// <summary>qld.acr.lote_quarentena -&gt; coloca o lote em contenção (bloqueia) no Estoque.</summary>
    public class QualidadeLoteQuarentenaConsumer : BloqueioLoteQualidadeConsumerBase
    {
        public QualidadeLoteQuarentenaConsumer(ContextEstoque context) : base(context) { }
        public override string EventType => CatalogoEventosIntegracao.Qualidade.AcrLoteQuarentena;
        protected override ETipoBloqueioLote TipoBloqueio => ETipoBloqueioLote.Qualidade;
        protected override string MotivoPrefixo => "Quarentena por decisão de Qualidade (ACR)";
    }

    /// <summary>qld.rst.bloqueio_solicitado -&gt; bloqueia o lote no Estoque (contenção de recall/RST).</summary>
    public class QualidadeRstBloqueioSolicitadoConsumer : BloqueioLoteQualidadeConsumerBase
    {
        public QualidadeRstBloqueioSolicitadoConsumer(ContextEstoque context) : base(context) { }
        public override string EventType => CatalogoEventosIntegracao.Qualidade.RstBloqueioSolicitado;
        protected override ETipoBloqueioLote TipoBloqueio => ETipoBloqueioLote.Recall;
        protected override string MotivoPrefixo => "Contenção solicitada por RST/Recall";
    }

    /// <summary>
    /// qld.acr.lote_liberado -&gt; DESBLOQUEIA no Estoque os bloqueios de Qualidade ativos do lote
    /// (reverte a contenção). Idempotente: no-op se o lote não tem bloqueio ativo.
    /// </summary>
    public class QualidadeLoteLiberadoConsumer : IOutboxConsumer
    {
        private readonly ContextEstoque _context;
        public QualidadeLoteLiberadoConsumer(ContextEstoque context) => _context = context;

        public string EventType => CatalogoEventosIntegracao.Qualidade.AcrLoteLiberado;

        public async Task ConsumeAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            var p = JsonSerializer.Deserialize<LoteLiberadoPayload>(message.Payload, JsonOpts.Default);
            if (p == null || string.IsNullOrWhiteSpace(p.Lote)) return;

            var tenantId = string.IsNullOrWhiteSpace(p.TenantId) ? message.TenantId : p.TenantId;

            var lote = await _context.LotesEstoque
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.CodigoLote == p.Lote && l.DeletadoEm == null, cancellationToken);
            if (lote == null || lote.QuantidadeBloqueada <= 0m) return;

            var bloqueiosAtivos = await _context.BloqueiosLote
                .IgnoreQueryFilters()
                .Where(b => b.TenantId == tenantId && b.LoteId == lote.Id && b.DataDesbloqueio == null && b.DeletadoEm == null)
                .ToListAsync(cancellationToken);
            if (bloqueiosAtivos.Count == 0) return;

            foreach (var b in bloqueiosAtivos)
                b.RegistrarDesbloqueio("Liberado por decisão de Qualidade (ACR aceito)", "system_quality");

            lote.Desbloquear("system_quality");
            _context.HistoricosEstoque.Add(new HistoricoEstoque(
                "LoteEstoque", lote.Id, "lote_desbloqueado", EStatusLoteRastreabilidade.Bloqueado.ToString(), lote.Status.ToString(),
                "Liberado por Qualidade", "system_quality", tenantId, "system_quality"));
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Estoque.LoteDesbloqueado,
                JsonSerializer.Serialize(new { loteId = lote.Id, lote.ProdutoId, lote.CodigoLote, origem = EventType })));

            await _context.SaveChangesAsync(cancellationToken);
        }

        private class LoteLiberadoPayload
        {
            public string? Lote { get; set; }
            public string? TenantId { get; set; }
        }
    }

    internal static class JsonOpts
    {
        public static readonly JsonSerializerOptions Default = new() { PropertyNameCaseInsensitive = true };
    }
}
