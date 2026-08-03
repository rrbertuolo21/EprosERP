using System;
using System.Collections.Generic;
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
    // T5 — CONSUMIDOR REAL do Outbox de PRODUÇÃO (MES), roteado pelo processador de
    // Outbox da Produção. Fecha a lacuna "MES não movimenta estoque de verdade":
    // a finalização da ordem MES passa a mover o estoque pelo MOTOR ÚNICO (kardex D1),
    // exatamente como o fluxo legado OrdemProducaoEncerrada — consolidando UMA porta
    // de entrada/saída de estoque para a produção.
    //
    // IDEMPOTENTE e ANTI-DUPLA-CONTAGEM: âncora determinística por ordem MES
    // (FatoGeradorEstoque.ReferenciaExterna == "MES {ordemId}"). Reprocessar a mesma
    // mensagem (retry do outbox) NÃO relança baixa/entrada.
    // EXPLÍCITO quanto ao tenant (usa message.TenantId + IgnoreQueryFilters), pois o
    // processador não manipula HttpContext.
    // ============================================================================

    /// <summary>
    /// Consome <c>prd.ordem.concluida</c> (Outbox da Produção/MES): baixa os insumos e dá entrada do
    /// produto acabado no Estoque pelo MOTOR ÚNICO. O custo unitário do acabado no kardex é a soma do
    /// custo médio dos insumos consumidos ÷ quantidade produzida — idêntico ao fluxo legado, mantendo
    /// coerência de custeio entre as duas portas de produção. Custo indireto/MOD/contábil: valida-contador.
    /// </summary>
    public class MesOrdemConcluidaEstoqueConsumer : IOutboxConsumer
    {
        private readonly ContextEstoque _context;
        public MesOrdemConcluidaEstoqueConsumer(ContextEstoque context) => _context = context;

        public string EventType => CatalogoEventosIntegracao.Producao.OrdemConcluida;

        public async Task ConsumeAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            var p = JsonSerializer.Deserialize<MesOrdemConcluidaPayload>(message.Payload, JsonOpts.Default);
            if (p == null || p.OrdemId == Guid.Empty) return;

            var tenantId = string.IsNullOrWhiteSpace(p.TenantId) ? message.TenantId : p.TenantId;
            var referencia = $"MES {p.OrdemId}";

            // ANTI-DUPLA-CONTAGEM (idempotência): fato gerador já criado para esta ordem MES.
            var jaProcessado = await _context.FatosGeradoresEstoque
                .IgnoreQueryFilters()
                .AnyAsync(f => f.TenantId == tenantId && f.ReferenciaExterna == referencia, cancellationToken);
            if (jaProcessado) return;

            var motor = new MotorMovimentacaoEstoque(_context, tenantId, "system_production");
            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.Producao, tenantId, "system_production", referenciaExterna: referencia);

            decimal custoTotalProducao = 0m;
            var moveu = false;

            // 1) Baixa dos insumos consumidos (saída) — acumula custo pelo custo médio vigente.
            if (p.Insumos != null)
            {
                foreach (var insumo in p.Insumos)
                {
                    if (insumo.ProdutoId == Guid.Empty || insumo.Quantidade <= 0m) continue;

                    var produto = await _context.Produtos
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == insumo.ProdutoId, cancellationToken);
                    if (produto == null) continue; // insumo não rastreado no Estoque: não inventa saldo.

                    var custoMedioInsumo = await _context.EstoqueProdutos
                        .IgnoreQueryFilters()
                        .Where(e => e.EmpresaId == MotorMovimentacaoEstoque.EmpresaPadrao && e.ProdutoId == produto.Id)
                        .Select(e => (decimal?)e.ValorCustoMedio)
                        .FirstOrDefaultAsync(cancellationToken) ?? produto.CustoMedio;

                    var res = await motor.AplicarSaidaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, produto.Id, insumo.Quantidade, fato.Id, null, cancellationToken);
                    if (!res.Sucesso) continue; // motor é a autoridade (D8): se bloquear a saída, não contabiliza o custo.

                    custoTotalProducao += custoMedioInsumo * insumo.Quantidade;
                    moveu = true;
                }
            }

            // 2) Entrada do produto acabado (entrada) — custo unitário = custo de produção ÷ quantidade.
            if (p.ProdutoAcabadoId != Guid.Empty && p.QuantidadeAcabado > 0m)
            {
                var acabado = await _context.Produtos
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == p.ProdutoAcabadoId, cancellationToken);
                if (acabado != null)
                {
                    var precoUnitario = custoTotalProducao > 0m ? custoTotalProducao / p.QuantidadeAcabado : 0m;
                    await motor.AplicarEntradaAsync(
                        MotorMovimentacaoEstoque.EmpresaPadrao, acabado.Id, ETipoEstoque.Geral, p.QuantidadeAcabado, precoUnitario,
                        fato.Id, null, string.IsNullOrWhiteSpace(p.Lote) ? null : p.Lote, p.Validade, ETipoCusteioEstoque.CustoMedio, cancellationToken);
                    moveu = true;
                }
            }

            if (!moveu) return; // nada rastreável a mover: não cria fato gerador vazio.

            _context.FatosGeradoresEstoque.Add(fato);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private class MesOrdemConcluidaPayload
        {
            public Guid OrdemId { get; set; }
            public string? TenantId { get; set; }
            public Guid ProdutoAcabadoId { get; set; }
            public decimal QuantidadeAcabado { get; set; }
            public string? Lote { get; set; }
            public DateTime? Validade { get; set; }
            public List<InsumoConsumidoItem> Insumos { get; set; } = new();
        }

        private class InsumoConsumidoItem
        {
            public Guid ProdutoId { get; set; }
            public decimal Quantidade { get; set; }
        }
    }
}
