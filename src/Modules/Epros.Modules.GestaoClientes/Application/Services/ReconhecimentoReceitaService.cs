using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Services
{
    /// <summary>
    /// 1.08I — MECANISMO de reconhecimento de receita por competência (diferimento de plano anual).
    /// Gera o cronograma de <see cref="ReconhecimentoReceita"/> quando uma fatura é paga e apropria as
    /// parcelas mês a mês (rotina). NÃO chama SaveChanges (o orquestrador decide o commit) — igual ao
    /// <see cref="FaturaLiquidacaoService"/>.
    ///
    /// Base normativa (skill Negocio-acumulado/contabil, CPC 47 / IFRS 15 / Lei 6.404/76 art. 177):
    ///   • ANUAL à vista → 12 avos, passivo de receita diferida que baixa 1/12 por mês (RN05).
    ///   • MENSAL → reconhece integral no mês, sem diferir (RN04).
    ///   • VITALÍCIO → RN07 é [PARÂMETRO]; default do mecanismo = ponto único (1 parcela) — ⚠️ VALIDA CONTADOR.
    ///
    /// ⚠️ CONTAS CONTÁBEIS e POLÍTICA (pró-rata, vitalício, competência × caixa) = PARÂMETRO do
    /// cliente/contador. Este serviço só calcula 1/N por competência citando a norma — não inventa conta
    /// nem regime fiscal.
    /// </summary>
    public class ReconhecimentoReceitaService
    {
        /// <summary>Nº de avos do diferimento anual (CPC 47 RN05 — 12 competências mensais).</summary>
        public const int MesesAnual = 12;

        private readonly ContextGestaoClientes _context;

        public ReconhecimentoReceitaService(ContextGestaoClientes context)
        {
            _context = context;
        }

        /// <summary>
        /// Nº de parcelas do cronograma conforme a duração do plano (MECANISMO, não um número fiscal).
        /// </summary>
        public static int ParcelasPara(PlanoDuration duration) => duration switch
        {
            PlanoDuration.Anual => MesesAnual,     // RN05 — 12 avos
            PlanoDuration.Mensal => 1,             // RN04 — reconhece no mês
            PlanoDuration.Vitalicia => 1,          // RN07 — [PARÂMETRO] default ponto único, VALIDA CONTADOR
            _ => 1
        };

        /// <summary>
        /// Calcula (SEM persistir) o cronograma de reconhecimento: N parcelas de 1/N do valor, a partir da
        /// competência inicial, distribuindo o resíduo de centavos na ÚLTIMA parcela para que a soma feche
        /// exatamente o valor da fatura (invariante RN: soma das N = valor da fatura).
        /// </summary>
        public static List<ReconhecimentoReceita> CalcularCronograma(
            Guid faturaId,
            Guid clienteId,
            decimal valorTotal,
            PlanoDuration duration,
            DateTime competenciaInicial,
            string tenantId,
            string criadoPor)
        {
            var parcelas = ParcelasPara(duration);
            var baseComp = ReconhecimentoReceita.NormalizarCompetencia(competenciaInicial);
            var valorParcela = Math.Round(valorTotal / parcelas, 2, MidpointRounding.AwayFromZero);

            var itens = new List<ReconhecimentoReceita>(parcelas);
            decimal acumulado = 0m;
            for (int i = 0; i < parcelas; i++)
            {
                // Última parcela absorve o resíduo de arredondamento → soma exata.
                var valor = (i == parcelas - 1) ? (valorTotal - acumulado) : valorParcela;
                acumulado += valor;
                itens.Add(new ReconhecimentoReceita(
                    faturaId, clienteId, baseComp.AddMonths(i), i + 1, parcelas, valor, tenantId, criadoPor));
            }
            return itens;
        }

        /// <summary>
        /// Gera e ADICIONA ao contexto o cronograma da fatura (não persiste). Idempotente por fatura: se já
        /// houver cronograma para a fatura, não duplica. Retorna as parcelas geradas (vazio se já existiam).
        /// </summary>
        public async Task<IReadOnlyList<ReconhecimentoReceita>> GerarCronogramaAsync(
            Fatura fatura,
            PlanoDuration duration,
            DateTime competenciaInicial,
            string criadoPor,
            CancellationToken cancellationToken)
        {
            var jaExiste = await _context.ReconhecimentosReceita
                .IgnoreQueryFilters()
                .AnyAsync(r => r.FaturaId == fatura.Id && r.DeletadoEm == null, cancellationToken);
            if (jaExiste)
                return Array.Empty<ReconhecimentoReceita>();

            var itens = CalcularCronograma(
                fatura.Id, fatura.ClienteId, fatura.Valor, duration, competenciaInicial, fatura.TenantId, criadoPor);
            await _context.ReconhecimentosReceita.AddRangeAsync(itens, cancellationToken);
            return itens;
        }

        /// <summary>
        /// ROTINA MENSAL: apropria (reconhece) todas as parcelas ainda PENDENTES cuja competência já
        /// venceu (&lt;= mês de referência). Rodando mês a mês reconhece exatamente 1/N por mês; com atraso,
        /// faz catch-up das competências passadas. NÃO persiste. Retorna quantas parcelas reconheceu.
        /// [NORMA: CPC 47 item 35(a) — apropriação ao longo do tempo]
        /// </summary>
        public async Task<int> ReconhecerCompetenciaAsync(
            DateTime referencia,
            string alteradoPor,
            CancellationToken cancellationToken)
        {
            var alvo = ReconhecimentoReceita.NormalizarCompetencia(referencia);
            var pendentes = await _context.ReconhecimentosReceita
                .IgnoreQueryFilters()
                .Where(r => r.Status == ReconhecimentoReceitaStatus.Pendente
                            && r.Competencia <= alvo
                            && r.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            var reconhecidas = 0;
            foreach (var parcela in pendentes)
            {
                if (parcela.Reconhecer(referencia, alteradoPor))
                    reconhecidas++;
            }
            return reconhecidas;
        }
    }
}
