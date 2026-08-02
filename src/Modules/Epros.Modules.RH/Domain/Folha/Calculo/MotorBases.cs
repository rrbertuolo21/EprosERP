using System;
using System.Collections.Generic;

namespace Epros.Modules.RH.Domain.Folha.Calculo
{
    // ============================================================================================
    //  MOTORES DE BASES LEGAIS — INSS, IRRF, FGTS.
    //  Skill: departamento-pessoal/folha — RN-01 (INSS), RN-02 (IRRF), RN-03 (FGTS).
    //  Puros e determinísticos: recebem valores + tabela vigente, devolvem resultado. Sem I/O, sem EF.
    // ============================================================================================

    public sealed record ResultadoInss(decimal BaseContribuicao, decimal Valor, decimal AliquotaEfetiva);

    public sealed record ResultadoIrrf(
        decimal BaseCalculo,
        decimal Imposto,
        decimal Aliquota,
        decimal ImpostoAntesRedutor,
        decimal Redutor,
        bool UsouSimplificado);

    /// <summary>RN-01 · INSS do empregado (desconto) — progressivo por faixas, limitado ao teto.</summary>
    public static class MotorInss
    {
        /// <summary>Calcula o desconto de INSS sobre o salário de contribuição, pela tabela vigente.</summary>
        public static ResultadoInss Calcular(decimal salarioContribuicao, TabelaInss tabela)
        {
            if (tabela == null) throw new ArgumentNullException(nameof(tabela));
            if (salarioContribuicao <= 0)
                return new ResultadoInss(0m, 0m, 0m);

            // Base limitada ao teto (acima do teto, desconto fixo do teto).
            var baseCap = Math.Min(salarioContribuicao, tabela.Teto);

            decimal anterior = 0m;
            decimal total = 0m;
            foreach (var faixa in tabela.Faixas)
            {
                var topo = Math.Min(baseCap, faixa.LimiteSuperior);
                if (topo > anterior)
                    total += (topo - anterior) * faixa.Aliquota;
                anterior = faixa.LimiteSuperior;
                if (baseCap <= faixa.LimiteSuperior) break;
            }

            var valor = Arredondamento.Reais(total);
            var aliqEfetiva = baseCap > 0 ? Arredondamento.Reais(valor / baseCap * 100m) : 0m;
            return new ResultadoInss(baseCap, valor, aliqEfetiva);
        }
    }

    /// <summary>RN-02 · IRRF (desconto) — tabela progressiva + dedução legal/simplificada + redutor 2026.</summary>
    public static class MotorIrrf
    {
        /// <summary>
        /// Cálculo completo do IRRF mensal do salário. Escolhe automaticamente entre dedução legal
        /// (INSS + dependentes + pensão + outras) e desconto simplificado, usando a MENOR carga.
        /// Aplica o redutor 2026 sobre o rendimento bruto tributável.
        /// </summary>
        /// <param name="rendimentoTributavel">Rendimento bruto tributável do mês (salário + adicionais habituais).</param>
        /// <param name="inssDescontado">INSS do mês (RN-01) — dedução legal.</param>
        /// <param name="numDependentes">Nº de dependentes para dedução.</param>
        /// <param name="pensaoAlimenticia">Pensão dedutível da base (conforme sentença).</param>
        /// <param name="outrasDeducoes">Previdência oficial/privada dedutível e afins.</param>
        public static ResultadoIrrf Calcular(
            decimal rendimentoTributavel,
            decimal inssDescontado,
            int numDependentes,
            TabelaIrrf tabela,
            decimal pensaoAlimenticia = 0m,
            decimal outrasDeducoes = 0m)
        {
            if (tabela == null) throw new ArgumentNullException(nameof(tabela));
            if (rendimentoTributavel <= 0)
                return new ResultadoIrrf(0m, 0m, 0m, 0m, 0m, false);

            // Via dedução legal (RN-02).
            var deducaoLegal = inssDescontado
                               + tabela.DeducaoPorDependente * Math.Max(0, numDependentes)
                               + Math.Max(0m, pensaoAlimenticia)
                               + Math.Max(0m, outrasDeducoes);
            var baseLegal = Math.Max(0m, rendimentoTributavel - deducaoLegal);

            // Via desconto simplificado (substitui as deduções legais).
            var baseSimplificada = Math.Max(0m, rendimentoTributavel - tabela.DescontoSimplificado);

            var (impLegal, aliqLegal) = AplicarTabela(baseLegal, tabela);
            var (impSimpl, aliqSimpl) = AplicarTabela(baseSimplificada, tabela);

            bool usouSimplificado = impSimpl < impLegal;
            var baseEscolhida = usouSimplificado ? baseSimplificada : baseLegal;
            var impostoTabela = usouSimplificado ? impSimpl : impLegal;
            var aliquota = usouSimplificado ? aliqSimpl : aliqLegal;

            // Redutor 2026 sobre o rendimento bruto.
            var redutor = CalcularRedutor(impostoTabela, rendimentoTributavel, tabela);
            var impostoFinal = Arredondamento.Reais(Math.Max(0m, impostoTabela - redutor));

            return new ResultadoIrrf(
                BaseCalculo: Arredondamento.Reais(baseEscolhida),
                Imposto: impostoFinal,
                Aliquota: aliquota,
                ImpostoAntesRedutor: Arredondamento.Reais(impostoTabela),
                Redutor: Arredondamento.Reais(redutor),
                UsouSimplificado: usouSimplificado);
        }

        /// <summary>Aplica só a tabela progressiva sobre uma base já apurada: base × alíquota − parcela.</summary>
        public static (decimal Imposto, decimal Aliquota) AplicarTabela(decimal baseCalculo, TabelaIrrf tabela)
        {
            if (baseCalculo <= 0) return (0m, 0m);
            foreach (var faixa in tabela.Faixas)
            {
                if (baseCalculo <= faixa.LimiteSuperior)
                {
                    var imp = baseCalculo * faixa.Aliquota - faixa.ParcelaDeduzir;
                    return (Math.Max(0m, imp), faixa.Aliquota);
                }
            }
            // Não deve ocorrer (última faixa é decimal.MaxValue), mas por segurança usa a última.
            var ultima = tabela.Faixas[tabela.Faixas.Count - 1];
            return (Math.Max(0m, baseCalculo * ultima.Aliquota - ultima.ParcelaDeduzir), ultima.Aliquota);
        }

        /// <summary>
        /// Redutor 2026 (Lei 15.270/2025). Modelo DEFAULT (o coeficiente exato da faixa de transição
        /// está [REQUER VALIDAÇÃO] na skill): rendimento ≤ piso → redução = 100% do imposto (IRRF zero);
        /// entre piso e teto → redução linear proporcional a (teto − rendimento)/(teto − piso);
        /// acima do teto → sem redução.
        /// // valida-contador — coeficiente da redução linear na faixa piso→teto (Lei 15.270/2025).
        /// </summary>
        public static decimal CalcularRedutor(decimal imposto, decimal rendimentoBruto, TabelaIrrf tabela)
        {
            if (imposto <= 0m) return 0m;
            if (rendimentoBruto <= tabela.PisoRedutor) return imposto;      // zera o IRRF
            if (rendimentoBruto >= tabela.TetoRedutor) return 0m;           // sem redutor

            var fator = (tabela.TetoRedutor - rendimentoBruto) / (tabela.TetoRedutor - tabela.PisoRedutor);
            if (fator < 0m) fator = 0m;
            if (fator > 1m) fator = 1m;
            return imposto * fator;
        }
    }

    /// <summary>RN-03 · FGTS (encargo do empregador, não desconta do empregado).</summary>
    public static class MotorFgts
    {
        /// <summary>8% da remuneração do mês (2% para aprendiz). Lei 8.036/1990 art. 15.</summary>
        public static decimal Calcular(decimal remuneracao, TabelasFolha tabelas, bool aprendiz = false)
        {
            if (tabelas == null) throw new ArgumentNullException(nameof(tabelas));
            if (remuneracao <= 0) return 0m;
            var aliquota = aprendiz ? tabelas.AliquotaFgtsAprendiz : tabelas.AliquotaFgts;
            return Arredondamento.Reais(remuneracao * aliquota);
        }
    }
}
