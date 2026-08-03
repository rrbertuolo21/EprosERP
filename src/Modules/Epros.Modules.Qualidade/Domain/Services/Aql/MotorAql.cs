using System;
using System.Collections.Generic;

namespace Epros.Modules.Qualidade.Domain.Services.Aql
{
    /// <summary>
    /// Motor de amostragem por aceitacao por atributos (ISO 2859-1 / ANSI-ASQ Z1.4 / NBR 5426).
    /// Implementa: N + nivel -> letra-codigo (Tab. I); letra -> tamanho da amostra;
    /// (letra, AQL, severidade) -> Ac/Re pela forma fechada diagonal da skill canonica `qualidade`.
    ///
    /// Fonte: Negocio-acumulado/qualidade/SKILL.md (forma fechada `p = i + j - 18`,
    /// `AcSeq=[0,1,2,3,5,7,10,14,21,30,44]`, `Re=Ac+1`).
    ///
    /// ⚠️ As tabelas numericas (I, II-A/B/C) foram reconstruidas das normas na skill e NAO conferidas
    /// celula-a-celula contra o PDF oficial. Marcacoes `// valida (PDF ABNT NBR 5426)` sinalizam o que
    /// exige verificacao antes do go-live. A forma fechada NORMAL esta ancorada por casos conhecidos
    /// (L@1.0=5/6, H(50)@2.5=3/4, G@1.0=0/1) e coberta por testes.
    /// </summary>
    public sealed class MotorAql
    {
        // Letras do tamanho da amostra: nao existe "I" (pula H->J). Indice 1-based casa com i da skill.
        private static readonly char[] Letras = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R' };

        // Tamanho da amostra por letra-codigo (skill, Tab. I) — inspecao NORMAL/SEVERA.
        private static readonly int[] AmostraPorLetra = { 2, 3, 5, 8, 13, 20, 32, 50, 80, 125, 200, 315, 500, 800, 1250, 2000 };

        // Tamanho da amostra REDUZIDO (atenuada, Tab. II-C) por letra. // valida (PDF ABNT NBR 5426 — Tab. II-C)
        private static readonly int[] AmostraReduzidaPorLetra = { 2, 2, 2, 3, 5, 8, 13, 20, 32, 50, 80, 125, 200, 315, 500, 800 };

        // Sequencia de numeros de aceitacao (skill): note os pulos (nao ha 4, 6, 8, 9...).
        private static readonly int[] AcSeq = { 0, 1, 2, 3, 5, 7, 10, 14, 21, 30, 44 };

        // Sequencia de numeros de rejeicao para ATENUADA (com vao Ac<..<Re). O vao (Re > Ac+1) e
        // regra de negocio: d entre Ac e Re -> aceita mas volta a normal. // valida (PDF ABNT NBR 5426 — Tab. II-C)
        private static readonly int[] ReSeqAtenuada = { 1, 3, 4, 5, 7, 9, 12, 16, 23, 32, 46 };

        // Tabela I — letra-codigo por faixa de N x nivel. Colunas na ordem do enum ENivelInspecao
        // (S1,S2,S3,S4,I,II,III). // valida (PDF ABNT NBR 5426 — Tab. I / letra-codigo)
        private static readonly (long MaxN, char[] Letras)[] TabelaLetraCodigo =
        {
            (8,        new[] { 'A', 'A', 'A', 'A', 'A', 'A', 'B' }),
            (15,       new[] { 'A', 'A', 'A', 'A', 'A', 'B', 'C' }),
            (25,       new[] { 'A', 'A', 'B', 'B', 'B', 'C', 'D' }),
            (50,       new[] { 'A', 'B', 'B', 'C', 'C', 'D', 'E' }),
            (90,       new[] { 'B', 'B', 'C', 'C', 'C', 'E', 'F' }),
            (150,      new[] { 'B', 'B', 'C', 'D', 'D', 'F', 'G' }),
            (280,      new[] { 'B', 'C', 'D', 'E', 'E', 'G', 'H' }),
            (500,      new[] { 'B', 'C', 'D', 'E', 'F', 'H', 'J' }),
            (1200,     new[] { 'C', 'C', 'E', 'F', 'G', 'J', 'K' }),
            (3200,     new[] { 'C', 'D', 'E', 'G', 'H', 'K', 'L' }),
            (10000,    new[] { 'C', 'D', 'F', 'G', 'J', 'L', 'M' }),
            (35000,    new[] { 'C', 'D', 'F', 'H', 'K', 'M', 'N' }),
            (150000,   new[] { 'D', 'E', 'G', 'J', 'L', 'N', 'P' }),
            (500000,   new[] { 'D', 'E', 'G', 'J', 'M', 'P', 'Q' }),
            (long.MaxValue, new[] { 'D', 'E', 'H', 'K', 'N', 'Q', 'R' })
        };

        // Coluna AQL -> indice j (skill). Valores preferidos (numeros normalizados) da norma.
        private static readonly (decimal Aql, int J)[] IndiceAqlTabela =
        {
            (0.010m, 1), (0.015m, 2), (0.025m, 3), (0.040m, 4), (0.065m, 5), (0.10m, 6), (0.15m, 7),
            (0.25m, 8), (0.40m, 9), (0.65m, 10), (1.0m, 11), (1.5m, 12), (2.5m, 13), (4.0m, 14),
            (6.5m, 15), (10m, 16), (15m, 17), (25m, 18), (40m, 19), (65m, 20), (100m, 21)
        };

        /// <summary>N + nivel -> letra-codigo (Tab. I). N &lt; 2 usa a menor faixa.</summary>
        public char ResolverLetraCodigo(long tamanhoLote, ENivelInspecao nivel)
        {
            if (tamanhoLote < 2) tamanhoLote = 2;
            foreach (var faixa in TabelaLetraCodigo)
                if (tamanhoLote <= faixa.MaxN)
                    return faixa.Letras[(int)nivel];
            return 'R';
        }

        /// <summary>Tamanho da amostra (n) por letra-codigo, no regime informado.</summary>
        public int TamanhoAmostraPorLetra(char letra, ESeveridadeAql severidade = ESeveridadeAql.Normal)
        {
            var idx = IndiceLetra(letra);
            if (idx < 0) throw new ArgumentException($"Letra-codigo invalida: '{letra}'.", nameof(letra));
            return severidade == ESeveridadeAql.Atenuada
                ? AmostraReduzidaPorLetra[idx]
                : AmostraPorLetra[idx];
        }

        /// <summary>
        /// Calcula o plano de amostragem simples completo dado N, nivel, AQL e severidade corrente.
        /// Aplica a forma fechada diagonal e as substituicoes de seta (↓ = 100% se n&gt;=N; ↑ = plano de cima).
        /// </summary>
        public PlanoAmostragemResultado CalcularPlano(
            long tamanhoLote, ENivelInspecao nivel, decimal aql, ESeveridadeAql severidade = ESeveridadeAql.Normal)
        {
            if (tamanhoLote < 1)
                throw new ArgumentOutOfRangeException(nameof(tamanhoLote), "O tamanho do lote deve ser >= 1.");

            var letra = ResolverLetraCodigo(tamanhoLote, nivel);
            var j = IndiceAql(aql);
            var i = IndiceLetra(letra) + 1; // 1-based (skill: A=1..R=16)

            // Deslocamento do regime: severa = 1 casa mais rigorosa (Ac menor); atenuada = 1 mais frouxa (Ac maior).
            // Constante base 18 (skill). // valida (PDF ABNT NBR 5426 — Tab. II-B severa / II-C atenuada)
            int baseK = severidade switch
            {
                ESeveridadeAql.Severa => 19,
                ESeveridadeAql.Atenuada => 17,
                _ => 18
            };

            int p = i + j - baseK;

            if (p >= 0 && p <= 10)
                return MontarResultado(letra, severidade, p, tamanhoLote, nivel, aql);

            if (p < 0)
            {
                // Seta ↓: desce ate a letra onde Ac aparece (p=0), i = baseK - j.
                int iAlvo = baseK - j;
                if (iAlvo > 16 || iAlvo < 1)
                    return Inspecao100(tamanhoLote, severidade, nivel, aql); // fora da tabela util -> 100%
                char letraAlvo = Letras[iAlvo - 1];
                int nAlvo = TamanhoAmostraPorLetra(letraAlvo, severidade);
                if (nAlvo >= tamanhoLote)
                    return Inspecao100(tamanhoLote, severidade, nivel, aql);
                return MontarResultado(letraAlvo, severidade, 0, tamanhoLote, nivel, aql);
            }

            // p > 10 -> Seta ↑: sobe ate a letra onde Ac=44 (p=10), i = baseK + 10 - j.
            int iSup = baseK + 10 - j;
            if (iSup < 1) iSup = 1;
            if (iSup > 16) iSup = 16;
            char letraSup = Letras[iSup - 1];
            return MontarResultado(letraSup, severidade, 10, tamanhoLote, nivel, aql);
        }

        private PlanoAmostragemResultado MontarResultado(
            char letra, ESeveridadeAql severidade, int p, long tamanhoLote, ENivelInspecao nivel, decimal aql)
        {
            int ac = AcSeq[p];
            int re = severidade == ESeveridadeAql.Atenuada ? ReSeqAtenuada[p] : ac + 1;
            int n = TamanhoAmostraPorLetra(letra, severidade);
            return new PlanoAmostragemResultado(letra, n, ac, re, false, severidade, nivel, tamanhoLote, aql);
        }

        private static PlanoAmostragemResultado Inspecao100(
            long tamanhoLote, ESeveridadeAql severidade, ENivelInspecao nivel, decimal aql)
        {
            // Inspecao 100%: amostra = lote inteiro; aceita com zero defeito.
            int n = tamanhoLote > int.MaxValue ? int.MaxValue : (int)tamanhoLote;
            return new PlanoAmostragemResultado('-', n, 0, 1, true, severidade, nivel, tamanhoLote, aql);
        }

        private static int IndiceLetra(char letra)
        {
            for (int k = 0; k < Letras.Length; k++)
                if (Letras[k] == char.ToUpperInvariant(letra)) return k;
            return -1;
        }

        private static int IndiceAql(decimal aql)
        {
            foreach (var (valor, jj) in IndiceAqlTabela)
                if (valor == aql) return jj;
            // AQL nao listado: arredonda para o valor preferido mais proximo (nunca interpola Ac/Re).
            decimal melhorDist = decimal.MaxValue;
            int melhorJ = 11; // default 1.0
            foreach (var (valor, jj) in IndiceAqlTabela)
            {
                var dist = Math.Abs(valor - aql);
                if (dist < melhorDist) { melhorDist = dist; melhorJ = jj; }
            }
            return melhorJ;
        }

        /// <summary>Lista dos AQL preferidos aceitos pelo motor (para validacao de entrada/UI).</summary>
        public static IReadOnlyList<decimal> AqlsPreferidos()
        {
            var lista = new List<decimal>(IndiceAqlTabela.Length);
            foreach (var (valor, _) in IndiceAqlTabela) lista.Add(valor);
            return lista;
        }
    }
}
