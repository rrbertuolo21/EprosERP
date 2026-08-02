using System;
using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.Financeiro.Domain.Services
{
    /// <summary>Uma linha do cronograma de arrendamento IFRS-16 / CPC 06 (R2).</summary>
    /// <param name="Numero">Ordem da contraprestação (1..n).</param>
    /// <param name="Pagamento">Contraprestação paga no período.</param>
    /// <param name="Juros">Juros sobre o passivo (saldo anterior × taxa incremental).</param>
    /// <param name="AmortizacaoPrincipal">Redução do passivo de arrendamento (pagamento − juros).</param>
    /// <param name="SaldoPassivo">Saldo do passivo de arrendamento após o período.</param>
    /// <param name="DepreciacaoDireitoUso">Depreciação linear do ativo de direito de uso no período.</param>
    /// <param name="SaldoDireitoUso">Saldo do ativo de direito de uso após a depreciação.</param>
    public sealed record LinhaCronogramaArrendamento(
        int Numero, decimal Pagamento, decimal Juros, decimal AmortizacaoPrincipal,
        decimal SaldoPassivo, decimal DepreciacaoDireitoUso, decimal SaldoDireitoUso);

    /// <summary>Reconhecimento inicial de um arrendamento IFRS-16 / CPC 06 (R2).</summary>
    /// <param name="PassivoArrendamento">Passivo = valor presente das contraprestações à taxa incremental.</param>
    /// <param name="DireitoDeUso">Ativo = passivo + pagamentos antecipados/custos diretos iniciais − incentivos.</param>
    public sealed record ReconhecimentoArrendamento(decimal PassivoArrendamento, decimal DireitoDeUso);

    /// <summary>
    /// Motor de arrendamento mercantil (leasing) — IFRS-16 / CPC 06 (R2). A regra UNIVERSAL do modelo
    /// é: um arrendamento (que não seja de curto prazo/baixo valor) é reconhecido como um PASSIVO de
    /// arrendamento igual ao VALOR PRESENTE das contraprestações, descontado pela TAXA INCREMENTAL de
    /// captação do arrendatário, e um ATIVO de DIREITO DE USO correspondente (cita
    /// Negocio-acumulado/financeiro/credito: "lease = valor presente das parcelas"; matemática de VP
    /// idêntica à usada em <see cref="CalculoAmortizacao"/>).
    ///
    /// Fronteira (Regra #0): aqui só o MECANISMO universal. A TAXA incremental, o prazo, o tratamento
    /// como curto-prazo/baixo-valor, custos diretos iniciais, incentivos e a classificação
    /// (financeiro × operacional para o arrendador) são convenção do contrato/cliente e chegam
    /// INFORMADOS. A contabilização (conta-débito × conta-crédito do direito de uso, do passivo, da
    /// despesa de juros e da depreciação) = // valida-contador — nunca inventar conta nem taxa.
    /// </summary>
    public static class CalculoLeasing
    {
        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Valor presente de uma série uniforme de contraprestações. Postecipado (fim do período):
        /// PV = PMT·(1−(1+i)^−n)/i. Antecipado (início): multiplica por (1+i). Com i=0 ⇒ PMT·n.
        /// </summary>
        public static decimal ValorPresenteContraprestacoes(decimal contraprestacao, decimal taxaPeriodo, int n, bool pagamentoAntecipado = false)
        {
            if (contraprestacao <= 0m) throw new ArgumentOutOfRangeException(nameof(contraprestacao));
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
            if (taxaPeriodo < 0m) throw new ArgumentOutOfRangeException(nameof(taxaPeriodo));

            decimal pv;
            if (taxaPeriodo == 0m)
                pv = contraprestacao * n;
            else
            {
                var fator = (1m - (decimal)Math.Pow(1d + (double)taxaPeriodo, -n)) / taxaPeriodo;
                pv = contraprestacao * fator;
            }
            if (pagamentoAntecipado) pv *= (1m + taxaPeriodo);
            return R2(pv);
        }

        /// <summary>
        /// Reconhecimento inicial (IFRS-16 §22–24): passivo = VP das contraprestações; direito de uso =
        /// passivo + custos diretos iniciais + pagamentos antecipados − incentivos recebidos. Custos e
        /// incentivos chegam INFORMADOS (parâmetro do contrato) — default zero.
        /// </summary>
        public static ReconhecimentoArrendamento ReconhecerInicial(
            decimal contraprestacao, decimal taxaPeriodo, int n, bool pagamentoAntecipado = false,
            decimal custosDiretosIniciais = 0m, decimal pagamentosAntecipadosAdicionais = 0m, decimal incentivosRecebidos = 0m)
        {
            var passivo = ValorPresenteContraprestacoes(contraprestacao, taxaPeriodo, n, pagamentoAntecipado);
            var direitoUso = R2(passivo + custosDiretosIniciais + pagamentosAntecipadosAdicionais - incentivosRecebidos);
            if (direitoUso < 0m) direitoUso = 0m;
            return new ReconhecimentoArrendamento(passivo, direitoUso);
        }

        /// <summary>
        /// Monta o cronograma completo: amortização do PASSIVO pelo método do custo amortizado
        /// (juros = saldo × taxa; principal = contraprestação − juros; a última parcela zera o saldo,
        /// absorvendo arredondamento) e DEPRECIAÇÃO LINEAR do direito de uso ao longo do prazo do
        /// arrendamento (IFRS-16 §31; base = direito de uso inicial / n).
        /// </summary>
        public static IReadOnlyList<LinhaCronogramaArrendamento> Cronograma(
            decimal contraprestacao, decimal taxaPeriodo, int n, bool pagamentoAntecipado = false,
            decimal custosDiretosIniciais = 0m, decimal pagamentosAntecipadosAdicionais = 0m, decimal incentivosRecebidos = 0m)
        {
            var rec = ReconhecerInicial(contraprestacao, taxaPeriodo, n, pagamentoAntecipado,
                custosDiretosIniciais, pagamentosAntecipadosAdicionais, incentivosRecebidos);

            var linhas = new List<LinhaCronogramaArrendamento>(n);
            var saldoPassivo = rec.PassivoArrendamento;
            var saldoDireito = rec.DireitoDeUso;
            var deprecFixa = R2(rec.DireitoDeUso / n);

            for (var k = 1; k <= n; k++)
            {
                decimal juros, principal;
                // No arrendamento antecipado, a 1ª contraprestação (paga no início) não rende juros.
                var incideJuros = !(pagamentoAntecipado && k == 1);
                juros = incideJuros ? R2(saldoPassivo * taxaPeriodo) : 0m;

                if (k == n)
                {
                    principal = saldoPassivo;            // última parcela zera o passivo
                }
                else
                {
                    principal = R2(contraprestacao - juros);
                    if (principal > saldoPassivo) principal = saldoPassivo;
                }
                saldoPassivo = R2(saldoPassivo - principal);
                if (saldoPassivo < 0m) saldoPassivo = 0m;

                var deprec = k == n ? saldoDireito : deprecFixa; // última absorve o resíduo de depreciação
                saldoDireito = R2(saldoDireito - deprec);
                if (saldoDireito < 0m) saldoDireito = 0m;

                linhas.Add(new LinhaCronogramaArrendamento(
                    k, R2(contraprestacao), juros, R2(principal), saldoPassivo, R2(deprec), saldoDireito));
            }
            return linhas;
        }

        /// <summary>Total de juros (despesa financeira) do arrendamento ao longo do prazo.</summary>
        public static decimal TotalJuros(IEnumerable<LinhaCronogramaArrendamento> cronograma) => R2(cronograma.Sum(l => l.Juros));
    }
}
