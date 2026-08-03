using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Entities;

namespace Epros.Modules.Financeiro.Domain.Services
{
    /// <summary>Resumo do que o motor de eliminação aplicou ao balancete consolidado.</summary>
    public sealed record ResultadoEliminacao(int EliminacoesAplicadas, decimal TotalEliminado, IReadOnlyList<BalanceteLinha> NovasLinhas);

    /// <summary>
    /// Motor de eliminação intercompany da consolidação (FIN-CON). Hoje "publicar" só troca estado e a
    /// eliminação era cadastro manual; este motor APLICA as eliminações intercompany registradas ao
    /// balancete consolidado, gerando as linhas de eliminação (partida balanceada) e reagregando os totais.
    ///
    /// ⛔ Regra #0 / valida-contador: a FÓRMULA de qual conta (a receber/a pagar intercompany) cada
    /// eliminação debita/credita é parametrização do contador — não é inventada aqui. O motor garante a
    /// ESTRUTURA (uma linha balanceada por eliminação) e a AGREGAÇÃO (recomputo dos totais). É idempotente:
    /// só aplica eliminações ainda pendentes.
    /// </summary>
    public static class MotorEliminacaoConsolidacao
    {
        /// <summary>
        /// Aplica as eliminações PENDENTES ao balancete (muta o balancete e marca cada eliminação como
        /// aplicada). Eliminações sem valor ou já aplicadas são ignoradas.
        /// </summary>
        public static ResultadoEliminacao Aplicar(
            BalanceteConsolidado balancete, IReadOnlyList<EliminacaoIntercompany> eliminacoes,
            string tenantId, string usuario)
        {
            if (balancete == null) throw new ArgumentNullException(nameof(balancete));

            var pendentes = eliminacoes
                .Where(e => !e.Aplicada && e.Valor.HasValue && e.Valor.Value != 0m)
                .ToList();

            decimal total = 0m;
            var novasLinhas = new List<BalanceteLinha>();
            foreach (var elim in pendentes)
            {
                var valor = Math.Abs(elim.Valor!.Value);
                novasLinhas.Add(balancete.AplicarLinhaEliminacao(valor, "ELIM", elim.Descricao ?? "Eliminação intercompany", tenantId, usuario));
                elim.MarcarAplicada(usuario);
                total += valor;
            }

            if (pendentes.Count > 0)
                balancete.MarcarEliminacoesAplicadas(usuario);

            return new ResultadoEliminacao(pendentes.Count, total, novasLinhas);
        }
    }
}
