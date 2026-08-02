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
    /// <summary>Carrega o catálogo de estruturas ativas (com componentes) para os motores de BOM/custo.</summary>
    internal static class CatalogoBomLoader
    {
        public static Task<List<BomEstrutura>> CarregarAtivasAsync(ContextProducao ctx, CancellationToken ct) =>
            ctx.BomEstruturas
               .Include(e => e.Componentes)
               .Where(e => e.Status == EStatusWorkflowProducao.Ativo)
               .ToListAsync(ct);
    }

    /// <summary>PRD-BOM — dispara o BomExplosaoService (antes órfão) a partir do HTTP/MediatR.</summary>
    public class ExplodirBomCommandHandler : ICommandHandler<ExplodirBomCommand>
    {
        private readonly ContextProducao _context;
        public ExplodirBomCommandHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ExplodirBomCommand request, CancellationToken ct)
        {
            var catalogo = await CatalogoBomLoader.CarregarAtivasAsync(_context, ct);
            var raiz = catalogo.FirstOrDefault(e => e.Id == request.EstruturaRaizId);
            if (raiz == null)
                return CommandResult.Falha("Estrutura raiz não encontrada ou não está ativa.");

            var dataRef = request.DataReferencia ?? DateTime.UtcNow;
            var r = new BomExplosaoService().Explodir(raiz, request.QuantidadeSolicitada, catalogo, dataRef);

            if (r.PossuiCiclo)
                return CommandResult.Falha(r.MotivoIncompleto ?? "Estrutura contém ciclo (BOM-REG-019).");

            return CommandResult.Ok(
                r.CalculoIncompleto
                    ? $"Explosão concluída com pendências: {r.MotivoIncompleto}"
                    : "Explosão de BOM concluída.",
                new
                {
                    raiz.Id,
                    request.QuantidadeSolicitada,
                    r.CalculoIncompleto,
                    Itens = r.Itens.Select(i => new
                    {
                        i.ItemId,
                        i.Nivel,
                        i.QuantidadePorUnidade,
                        i.QuantidadeTotal,
                        i.EhFolha,
                        i.Tipo
                    })
                });
        }
    }

    /// <summary>PRD-CST — dispara o CustoRollupService (antes órfão). Taxas MOD/CIF: valida-contador.</summary>
    public class CalcularCustoRollupCommandHandler : ICommandHandler<CalcularCustoRollupCommand>
    {
        private readonly ContextProducao _context;
        public CalcularCustoRollupCommandHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(CalcularCustoRollupCommand request, CancellationToken ct)
        {
            var catalogo = await CatalogoBomLoader.CarregarAtivasAsync(_context, ct);
            var raiz = catalogo.FirstOrDefault(e => e.Id == request.EstruturaRaizId);
            if (raiz == null)
                return CommandResult.Falha("Estrutura raiz não encontrada ou não está ativa.");

            // Taxas de MOD/CIF vêm do comando (valida-contador — parametrizadas pelo contador, não inventadas).
            // Valoração de material de folha e horas por roteiro ficam como resíduo (integração Estoque/Roteiro).
            var parametros = new CustoRollupService.ParametrosCusteio
            {
                TaxaMaoDeObraPorHora = request.TaxaMaoDeObraPorHora,
                TaxaCifPorHoraMaquina = request.TaxaCifPorHoraMaquina
            };

            var dataRef = request.DataReferencia ?? DateTime.UtcNow;
            var c = new CustoRollupService().CalcularPrevisto(raiz, catalogo, parametros, dataRef);

            if (c.PossuiCiclo)
                return CommandResult.Falha("Estrutura contém ciclo (BOM-REG-019).");

            return CommandResult.Ok(
                "Custo previsto (rollup multinível) calculado.",
                new
                {
                    raiz.Id,
                    c.CustoMaterial,
                    c.CustoMaoDeObra,
                    c.CustoIndireto,
                    c.CustoExtra,
                    c.CustoTotal,
                    c.Rendimento,
                    c.CustoUnitario
                });
        }
    }

    /// <summary>PRD-PLN/CTR — dispara o CapacidadeService (antes órfão).</summary>
    public class AvaliarCapacidadeCommandHandler : ICommandHandler<AvaliarCapacidadeCommand>
    {
        private readonly ContextProducao _context;
        public AvaliarCapacidadeCommandHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(AvaliarCapacidadeCommand request, CancellationToken ct)
        {
            var centro = await _context.CentrosTrabalho.FirstOrDefaultAsync(c => c.Id == request.CentroTrabalhoId, ct);
            if (centro == null)
                return CommandResult.Falha("Centro de trabalho não encontrado.");

            var service = new CapacidadeService();
            var capacidade = service.CapacidadePeriodoMinutos(centro, request.DiasUteis);
            var avaliacao = service.Avaliar(capacidade, request.CargaMinutos);

            return CommandResult.Ok(
                avaliacao.Status == CapacidadeService.EStatusCapacidade.PendenteCapacidade
                    ? "Carga acima da capacidade: PENDENTE_CAPACIDADE (gargalo)."
                    : "Capacidade avaliada.",
                new
                {
                    centro.Id,
                    centro.Codigo,
                    request.DiasUteis,
                    avaliacao.Capacidade,
                    avaliacao.Carga,
                    avaliacao.Folga,
                    avaliacao.UtilizacaoPercentual,
                    Status = avaliacao.Status.ToString(),
                    DisponivelParaPrometer = service.DisponivelParaPrometer(capacidade, request.CargaMinutos)
                });
        }
    }

    /// <summary>PRD-ESC — dispara o SequenciamentoService (antes órfão). Detecta ciclo de precedência.</summary>
    public class SequenciarOperacoesCommandHandler : ICommandHandler<SequenciarOperacoesCommand>
    {
        public Task<CommandResult> Handle(SequenciarOperacoesCommand request, CancellationToken ct)
        {
            var ops = (request.Operacoes ?? new()).Select(o => new SequenciamentoService.OperacaoEntrada(
                o.Id,
                o.CentroTrabalhoId,
                o.Prioridade,
                o.DuracaoMinutos,
                o.SetupMinutos,
                (IReadOnlyCollection<Guid>)(o.Precedencias ?? new List<Guid>()),
                o.JanelaInicio));

            var inicio = request.InicioHorizonte ?? DateTime.UtcNow;

            try
            {
                var agenda = new SequenciamentoService().Sequenciar(ops, inicio);
                return Task.FromResult(CommandResult.Ok(
                    "Operações sequenciadas (capacidade finita por centro).",
                    new
                    {
                        Total = agenda.Count,
                        Operacoes = agenda.Select(a => new
                        {
                            a.Id,
                            a.CentroTrabalhoId,
                            a.Ordem,
                            a.Inicio,
                            a.Fim,
                            a.SetupMinutos
                        })
                    }));
            }
            catch (InvalidOperationException ex)
            {
                // Ciclo de precedência (indispachável) — não inventa uma ordem.
                return Task.FromResult(CommandResult.Falha(ex.Message));
            }
        }
    }
}
