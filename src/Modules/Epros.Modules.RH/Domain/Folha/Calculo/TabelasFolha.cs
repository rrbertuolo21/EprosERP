using System;
using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.RH.Domain.Folha.Calculo
{
    // ============================================================================================
    //  PARÂMETROS LEGAIS DA FOLHA — versionados por VIGÊNCIA (ano-base).
    //
    //  Skill de negócio: Negocio-acumulado/departamento-pessoal/folha (SKILL.md).
    //  ⚠️ Ano-base 2026. As TABELAS numéricas (INSS, IRRF, salário mínimo, salário-família, teto)
    //     mudam a cada ano/portaria. A LÓGICA de cálculo (motores) é estável; os NÚMEROS não.
    //  ⛔ Regra #0: nenhuma alíquota/base é inventada. Todo valor abaixo carrega `// valida-contador`
    //     e cita a fonte oficial. Ano sem tabela confirmada => TabelasFolha.Vigente lança (nunca inventa).
    // ============================================================================================

    /// <summary>Faixa progressiva do INSS: alíquota que incide só sobre a parcela dentro da faixa.</summary>
    public sealed record FaixaInss(decimal LimiteSuperior, decimal Aliquota);

    /// <summary>Faixa da tabela progressiva mensal do IRRF (base × alíquota − parcela a deduzir).</summary>
    public sealed record FaixaIrrf(decimal LimiteSuperior, decimal Aliquota, decimal ParcelaDeduzir);

    /// <summary>Tabela do INSS do empregado (progressiva por faixas, com teto).</summary>
    public sealed class TabelaInss
    {
        public IReadOnlyList<FaixaInss> Faixas { get; }
        public decimal Teto { get; }

        public TabelaInss(IReadOnlyList<FaixaInss> faixas, decimal teto)
        {
            if (faixas == null || faixas.Count == 0)
                throw new ArgumentException("Tabela de INSS sem faixas.", nameof(faixas));
            Faixas = faixas.OrderBy(f => f.LimiteSuperior).ToList();
            Teto = teto;
        }
    }

    /// <summary>Tabela do IRRF: faixas progressivas + deduções e o redutor 2026.</summary>
    public sealed class TabelaIrrf
    {
        public IReadOnlyList<FaixaIrrf> Faixas { get; }
        public decimal DeducaoPorDependente { get; }
        public decimal DescontoSimplificado { get; }

        // Redutor 2026 (Lei 15.270/2025): rendimento mensal até PisoRedutor => IRRF efetivo zero;
        // entre PisoRedutor e TetoRedutor => redução linear decrescente; acima => sem redutor.
        public decimal PisoRedutor { get; }
        public decimal TetoRedutor { get; }

        public TabelaIrrf(
            IReadOnlyList<FaixaIrrf> faixas,
            decimal deducaoPorDependente,
            decimal descontoSimplificado,
            decimal pisoRedutor,
            decimal tetoRedutor)
        {
            if (faixas == null || faixas.Count == 0)
                throw new ArgumentException("Tabela de IRRF sem faixas.", nameof(faixas));
            Faixas = faixas.OrderBy(f => f.LimiteSuperior).ToList();
            DeducaoPorDependente = deducaoPorDependente;
            DescontoSimplificado = descontoSimplificado;
            PisoRedutor = pisoRedutor;
            TetoRedutor = tetoRedutor;
        }
    }

    /// <summary>Conjunto de parâmetros legais vigentes num ano-base. Imutável.</summary>
    public sealed class TabelasFolha
    {
        public int Ano { get; }
        public TabelaInss Inss { get; }
        public TabelaIrrf Irrf { get; }
        public decimal SalarioMinimo { get; }
        public decimal AliquotaFgts { get; }
        public decimal AliquotaFgtsAprendiz { get; }

        /// <summary>Cota do salário-família por dependente. null => não confirmada (valida-contador).</summary>
        public decimal? SalarioFamiliaCota { get; }
        /// <summary>Teto de renda para direito ao salário-família. null => não confirmado.</summary>
        public decimal? SalarioFamiliaTetoRenda { get; }

        private TabelasFolha(
            int ano,
            TabelaInss inss,
            TabelaIrrf irrf,
            decimal salarioMinimo,
            decimal aliquotaFgts,
            decimal aliquotaFgtsAprendiz,
            decimal? salarioFamiliaCota,
            decimal? salarioFamiliaTetoRenda)
        {
            Ano = ano;
            Inss = inss;
            Irrf = irrf;
            SalarioMinimo = salarioMinimo;
            AliquotaFgts = aliquotaFgts;
            AliquotaFgtsAprendiz = aliquotaFgtsAprendiz;
            SalarioFamiliaCota = salarioFamiliaCota;
            SalarioFamiliaTetoRenda = salarioFamiliaTetoRenda;
        }

        // ----------------------------------------------------------------------------------------
        //  2026 — fontes oficiais citadas na skill folha/SKILL.md ("Tabelas vigentes — ano-base 2026").
        //  Cada número é `// valida-contador` (revalidar todo janeiro / por portaria).
        // ----------------------------------------------------------------------------------------
        private static readonly Lazy<TabelasFolha> _2026 = new(() =>
        {
            // INSS — Portaria Interministerial MPS/MF nº 13, de 09/01/2026 (progressiva por faixas).
            var inss = new TabelaInss(
                new List<FaixaInss>
                {
                    new(1621.00m, 0.075m),  // valida-contador — até R$ 1.621,00 → 7,5%
                    new(2902.84m, 0.09m),   // valida-contador — 1.621,01 a 2.902,84 → 9%
                    new(4354.27m, 0.12m),   // valida-contador — 2.902,85 a 4.354,27 → 12%
                    new(8475.55m, 0.14m),   // valida-contador — 4.354,28 a 8.475,55 → 14%
                },
                teto: 8475.55m);            // valida-contador — teto do salário de contribuição 2026

            // IRRF — Lei 15.191/2025 (tabela) + Lei 15.270/2025 (redutor/isenção ampliada) + RIR/2018.
            var irrf = new TabelaIrrf(
                new List<FaixaIrrf>
                {
                    new(2428.80m, 0.00m,   0.00m),    // valida-contador — até 2.428,80 → isento
                    new(2826.65m, 0.075m,  182.16m),  // valida-contador — 2.428,81 a 2.826,65 → 7,5%
                    new(3751.05m, 0.15m,   394.16m),  // valida-contador — 2.826,66 a 3.751,05 → 15%
                    new(4664.68m, 0.225m,  675.49m),  // valida-contador — 3.751,06 a 4.664,68 → 22,5%
                    new(decimal.MaxValue, 0.275m, 908.73m), // valida-contador — acima de 4.664,68 → 27,5%
                },
                deducaoPorDependente: 189.59m,        // valida-contador — dedução por dependente/mês
                descontoSimplificado: 607.20m,        // valida-contador — desconto simplificado/mês
                // Redutor 2026 (Lei 15.270/2025). Faixas confirmadas na skill; o COEFICIENTE exato da
                // redução linear entre piso e teto está marcado [REQUER VALIDAÇÃO] na skill → default
                // adotado: redução linear proporcional que zera o IRRF no piso e some no teto.
                pisoRedutor: 5000.00m,                // valida-contador — rendimento até 5.000 → IRRF zero
                tetoRedutor: 7350.00m);               // valida-contador — acima de 7.350 → sem redutor

            return new TabelasFolha(
                ano: 2026,
                inss: inss,
                irrf: irrf,
                salarioMinimo: 1621.00m,              // valida-contador — SM nacional 2026 (confirmar decreto)
                aliquotaFgts: 0.08m,                  // Lei 8.036/1990 art. 15 — 8% (estável)
                aliquotaFgtsAprendiz: 0.02m,          // Lei 8.036/1990 — aprendiz 2% (estável)
                salarioFamiliaCota: null,             // valida-contador — cota 2026 não confirmada na skill
                salarioFamiliaTetoRenda: null);       // valida-contador — teto de renda 2026 não confirmado
        });

        /// <summary>
        /// Parâmetros legais vigentes no ano informado. ⛔ Ano sem tabela oficial confirmada
        /// dispara exceção (regra #0: a fábrica não inventa alíquota/base — abrir ingestão + valida-contador).
        /// </summary>
        public static TabelasFolha Vigente(int ano) => ano switch
        {
            2026 => _2026.Value,
            _ => throw new InvalidOperationException(
                $"Sem tabela de folha confirmada para o ano {ano}. Regra #0: não inventar alíquota/tabela. " +
                "Carregar a portaria/lei do ano na skill departamento-pessoal/folha e validar com o contador.")
        };
    }

    /// <summary>Arredondamento monetário padrão da folha (2 casas, meio para cima / longe do zero).</summary>
    public static class Arredondamento
    {
        public static decimal Reais(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
    }
}
