using System;
using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.Financeiro.Domain.Services
{
    /// <summary>Uma linha da tabela de amortização (k, prestação, juros, amortização, saldo devedor).</summary>
    public sealed record ParcelaAmortizacao(int Numero, decimal Prestacao, decimal Juros, decimal Amortizacao, decimal SaldoDevedor);

    /// <summary>Sistema de amortização.</summary>
    public enum ESistemaAmortizacao { Price = 0, Sac = 1 }

    /// <summary>
    /// Motor de crédito/dívida estruturada — fórmulas UNIVERSAIS de matemática financeira
    /// (cita Negocio-acumulado/financeiro/credito): tabela Price (prestação constante), tabela SAC
    /// (amortização constante), equivalência composta de taxas, CET (TIR do fluxo) e estrutura do IOF.
    ///
    /// Regra de fronteira (skill): aqui só o MECANISMO universal. TAXA de juros, TARIFAS, SEGUROS e
    /// as ALÍQUOTAS de IOF são parâmetro/convenção do contrato/cliente e chegam INFORMADOS.
    /// ⚠️ IOF e metodologia/divulgação do CET (Res. CMN 3.517/2007; Decreto 6.306/2007) são fato legal
    /// que MUDA por decreto (majoração 2025) → parametrizar por vigência; nunca hardcode. // valida-contador
    /// </summary>
    public static class CalculoAmortizacao
    {
        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        /// <summary>Equivalência composta: taxa mensal a partir da anual. i_mês = (1+i_ano)^(1/12) − 1 (nunca i_ano/12).</summary>
        public static decimal MensalDeAnual(decimal taxaAnual)
            => (decimal)Math.Pow(1d + (double)taxaAnual, 1d / 12d) - 1m;

        /// <summary>Anualiza uma taxa periódica composta. (1+i)^p − 1.</summary>
        public static decimal Anualizar(decimal taxaPeriodica, int periodosPorAno = 12)
            => (decimal)Math.Pow(1d + (double)taxaPeriodica, periodosPorAno) - 1m;

        /// <summary>Prestação da tabela Price: PMT = PV·i / (1 − (1+i)^−n). Com i=0 ⇒ PV/n.</summary>
        public static decimal PrestacaoPrice(decimal pv, decimal i, int n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
            if (i == 0m) return pv / n;
            var fator = 1m - (decimal)Math.Pow(1d + (double)i, -n);
            return pv * i / fator;
        }

        /// <summary>
        /// Monta a tabela de amortização. Juros_k = saldo_{k-1}·i em qualquer sistema; a última
        /// amortização é ajustada para zerar o saldo (absorve arredondamento).
        /// </summary>
        public static IReadOnlyList<ParcelaAmortizacao> Tabela(ESistemaAmortizacao sistema, decimal pv, decimal i, int n)
        {
            if (pv <= 0m) throw new ArgumentOutOfRangeException(nameof(pv));
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
            if (i < 0m) throw new ArgumentOutOfRangeException(nameof(i));

            var linhas = new List<ParcelaAmortizacao>(n);
            var saldo = pv;
            var pmtFixa = sistema == ESistemaAmortizacao.Price ? R2(PrestacaoPrice(pv, i, n)) : 0m;
            var amortFixa = sistema == ESistemaAmortizacao.Sac ? R2(pv / n) : 0m;

            for (var k = 1; k <= n; k++)
            {
                var juros = R2(saldo * i);
                decimal amortizacao, prestacao;

                if (k == n)
                {
                    // última parcela zera o saldo (plug do arredondamento)
                    amortizacao = saldo;
                    prestacao = R2(amortizacao + juros);
                }
                else if (sistema == ESistemaAmortizacao.Price)
                {
                    prestacao = pmtFixa;
                    amortizacao = R2(prestacao - juros);
                }
                else // SAC
                {
                    amortizacao = amortFixa;
                    prestacao = R2(amortizacao + juros);
                }

                saldo = R2(saldo - amortizacao);
                if (saldo < 0m) saldo = 0m;
                linhas.Add(new ParcelaAmortizacao(k, prestacao, juros, R2(amortizacao), saldo));
            }
            return linhas;
        }

        /// <summary>Total de juros da tabela.</summary>
        public static decimal TotalJuros(IEnumerable<ParcelaAmortizacao> tabela) => R2(tabela.Sum(p => p.Juros));

        /// <summary>Total pago (soma das prestações).</summary>
        public static decimal TotalPago(IEnumerable<ParcelaAmortizacao> tabela) => R2(tabela.Sum(p => p.Prestacao));

        /// <summary>
        /// IOF de crédito — ESTRUTURA (Decreto 6.306/2007, art. 7º): parcela diária (teto 365 dias)
        /// + adicional fixo. As ALÍQUOTAS são fato legal por vigência/tipo de mutuário (PF/PJ) e
        /// chegam INFORMADAS. // valida-contador — nunca inventar/hardcode a alíquota.
        /// </summary>
        public static decimal IofCredito(decimal pv, int prazoDias, decimal aliquotaDia, decimal aliquotaAdicional)
        {
            var dias = Math.Min(Math.Max(prazoDias, 0), 365);
            var diario = pv * aliquotaDia * dias;
            var adicional = pv * aliquotaAdicional;
            return R2(diario + adicional);
        }

        /// <summary>
        /// CET = TIR do fluxo de caixa do cliente, anualizada (Res. CMN 3.517/2007). Resolve por
        /// bisseção a taxa periódica que zera o VPL: PV_liquido = Σ parcela_k/(1+i)^k.
        /// Componentes (juros+IOF+tarifas+seguros) já devem estar embutidos em pvLiquido/parcelas.
        /// </summary>
        public static decimal CetAnual(decimal pvLiquido, IReadOnlyList<decimal> parcelas, int periodosPorAno = 12)
        {
            if (pvLiquido <= 0m) throw new ArgumentOutOfRangeException(nameof(pvLiquido));
            if (parcelas == null || parcelas.Count == 0) throw new ArgumentException("Fluxo de parcelas vazio.", nameof(parcelas));

            double lo = 0d, hi = 1d, i = 0d;
            for (var iter = 0; iter < 200; iter++)
            {
                i = (lo + hi) / 2d;
                var vpl = -(double)pvLiquido;
                for (var k = 0; k < parcelas.Count; k++)
                    vpl += (double)parcelas[k] / Math.Pow(1d + i, k + 1);
                if (vpl > 0d) lo = i; else hi = i; // VPL decresce com i
            }
            return (decimal)(Math.Pow(1d + i, periodosPorAno) - 1d);
        }
    }
}
