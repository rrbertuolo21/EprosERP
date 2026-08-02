using System;

namespace Epros.Modules.RH.Domain.Folha.Calculo
{
    // ============================================================================================
    //  MOTORES DE FÉRIAS (RN-05) e 13º SALÁRIO (RN-06). Skill: departamento-pessoal/folha.
    //  ⚠️ Modelo de INSS/IRRF sobre férias e 13º é apuração SEPARADA da folha mensal (skill RN-02/06).
    //  // valida-contador — incidências finais conforme tabela de rubricas do eSocial/RFB no go-live.
    // ============================================================================================

    public sealed record ResultadoFerias(
        decimal BaseRemuneracao,
        decimal ValorFerias,
        decimal TercoConstitucional,
        decimal Abono,            // pecuniário (indenizatório): não sofre INSS/IRRF/FGTS
        decimal TercoAbono,
        decimal BaseTributavel,   // férias + 1/3 (tributável)
        decimal Inss,
        decimal Irrf,
        decimal Fgts,
        decimal TotalBruto,
        decimal Liquido);

    /// <summary>RN-05 · Férias = (remuneração ÷ 30) × dias + 1/3 constitucional. Abono pecuniário indenizatório.</summary>
    public static class MotorFerias
    {
        public static ResultadoFerias Calcular(
            decimal remuneracaoMensal,
            TabelasFolha tabelas,
            int diasFerias = 30,
            decimal mediaVariaveis = 0m,
            int diasAbono = 0,
            int numDependentes = 0)
        {
            if (tabelas == null) throw new ArgumentNullException(nameof(tabelas));
            if (remuneracaoMensal <= 0) throw new ArgumentException("Remuneração deve ser maior que zero.", nameof(remuneracaoMensal));
            if (diasFerias < 0 || diasFerias > 30) throw new ArgumentOutOfRangeException(nameof(diasFerias), "Dias de férias entre 0 e 30.");
            if (diasAbono < 0 || diasAbono > 10) throw new ArgumentOutOfRangeException(nameof(diasAbono), "Abono pecuniário: até 10 dias (CLT art. 143).");

            var baseRemun = remuneracaoMensal + Math.Max(0m, mediaVariaveis);
            var diaria = baseRemun / 30m;

            var valorFerias = Arredondamento.Reais(diaria * diasFerias);
            var terco = Arredondamento.Reais(valorFerias / 3m);

            var abono = Arredondamento.Reais(diaria * diasAbono);
            var tercoAbono = Arredondamento.Reais(abono / 3m);

            // Tributável = férias + 1/3 (abono é indenizatório, fora das bases).
            var tributavel = valorFerias + terco;
            var inss = MotorInss.Calcular(tributavel, tabelas.Inss);
            var irrf = MotorIrrf.Calcular(tributavel, inss.Valor, numDependentes, tabelas.Irrf);
            var fgts = MotorFgts.Calcular(tributavel, tabelas);

            var totalBruto = Arredondamento.Reais(valorFerias + terco + abono + tercoAbono);
            var liquido = Arredondamento.Reais(totalBruto - inss.Valor - irrf.Imposto);

            return new ResultadoFerias(
                BaseRemuneracao: Arredondamento.Reais(baseRemun),
                ValorFerias: valorFerias,
                TercoConstitucional: terco,
                Abono: abono,
                TercoAbono: tercoAbono,
                BaseTributavel: Arredondamento.Reais(tributavel),
                Inss: inss.Valor,
                Irrf: irrf.Imposto,
                Fgts: fgts,
                TotalBruto: totalBruto,
                Liquido: liquido);
        }
    }

    public sealed record ResultadoDecimoTerceiro(
        decimal Bruto,
        decimal PrimeiraParcela,   // adiantamento, até 30/nov: 50% sem descontos
        decimal Inss,              // INSS próprio do 13º (2ª parcela)
        decimal Irrf,              // IRRF exclusivo na fonte (2ª parcela), sem redutor 2026
        decimal SegundaParcela,    // saldo − INSS − IRRF
        decimal Fgts);

    /// <summary>RN-06 · 13º = (remuneração ÷ 12) × meses (fração ≥15 dias = mês). Duas parcelas.</summary>
    public static class MotorDecimoTerceiro
    {
        public static ResultadoDecimoTerceiro Calcular(
            decimal remuneracao,
            int mesesTrabalhados,
            TabelasFolha tabelas,
            decimal mediaVariaveis = 0m,
            int numDependentes = 0)
        {
            if (tabelas == null) throw new ArgumentNullException(nameof(tabelas));
            if (remuneracao <= 0) throw new ArgumentException("Remuneração deve ser maior que zero.", nameof(remuneracao));
            if (mesesTrabalhados < 0 || mesesTrabalhados > 12) throw new ArgumentOutOfRangeException(nameof(mesesTrabalhados), "Meses entre 0 e 12 (avos).");

            var baseRemun = remuneracao + Math.Max(0m, mediaVariaveis);
            var bruto = Arredondamento.Reais(baseRemun / 12m * mesesTrabalhados);

            var primeira = Arredondamento.Reais(bruto * 0.5m);

            var inss = MotorInss.Calcular(bruto, tabelas.Inss);
            // IRRF do 13º: exclusivo na fonte, SEM redutor 2026 (apuração separada da folha mensal).
            var irrf = MotorIrrf.Calcular(bruto, inss.Valor, numDependentes, tabelas.Irrf, aplicarRedutor2026: false);
            var fgts = MotorFgts.Calcular(bruto, tabelas);

            var segunda = Arredondamento.Reais(bruto - primeira - inss.Valor - irrf.Imposto);

            return new ResultadoDecimoTerceiro(
                Bruto: bruto,
                PrimeiraParcela: primeira,
                Inss: inss.Valor,
                Irrf: irrf.Imposto,
                SegundaParcela: segunda,
                Fgts: fgts);
        }

        /// <summary>Avos do 13º/férias proporcionais: mês com ≥15 dias trabalhados conta como mês inteiro.</summary>
        public static int AvosPorDias(DateTime admissao, DateTime referencia)
        {
            if (referencia < admissao) return 0;
            int meses = 0;
            var cursor = new DateTime(admissao.Year, admissao.Month, 1);
            while (cursor <= new DateTime(referencia.Year, referencia.Month, 1))
            {
                var inicioMes = cursor;
                var fimMes = cursor.AddMonths(1).AddDays(-1);
                var ini = admissao > inicioMes ? admissao : inicioMes;
                var fim = referencia < fimMes ? referencia : fimMes;
                var dias = (fim - ini).Days + 1;
                if (dias >= 15) meses++;
                cursor = cursor.AddMonths(1);
            }
            return Math.Min(12, meses);
        }
    }
}
