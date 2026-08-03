using System;
using System.Collections.Generic;

namespace Epros.Modules.DMS.Domain.Financiamento
{
    /// <summary>Custos além do juro puro que entram no CET (skill Receita D). Todos opcionais.</summary>
    public sealed record CustosOperacao(
        decimal TarifasFinanciadas = 0m,   // embutidas nas parcelas (não reduzem o t0)
        decimal TarifasDescontadas = 0m,   // retidas na liberação (reduzem o t0 do CET)
        decimal SeguroPorParcela = 0m,     // prestamista mensal embutido na prestação
        bool IofFinanciado = true);        // true: IOF embutido; false: descontado na liberação

    /// <summary>
    /// MOTOR DE CÁLCULO F&amp;I — Price/SAC + IOF + CET (TIR).
    ///
    /// Aterra a skill de negócio AGNÓSTICA <c>Negocio-acumulado/financeiro/credito</c> (Receitas
    /// A/B/C/D). Aqui vive só a MATEMÁTICA universal (juros compostos, amortização, TIR) e a
    /// ESTRUTURA do IOF; os PARÂMETROS do cliente (taxa praticada, tarifas, seguros, tabela de IOF
    /// vigente) entram por argumento / <see cref="TabelaIof"/> e são // valida-contador.
    ///
    /// Contrato (skill "O motor em uma frase"):
    ///   Entrada: principal (PV) · taxa periódica (i) · prazo (n) · sistema (PRICE|SAC) + custos
    ///   Saída:   tabela [k, prestação, juros, amortização, saldo] + total pago/juros + IOF + CET anual
    /// </summary>
    public static class MotorFinanciamento
    {
        private const int CasasMonetarias = 2;

        /// <summary>
        /// Converte taxa anual efetiva em taxa mensal por EQUIVALÊNCIA COMPOSTA (skill Princípio 1):
        /// i_mês = (1 + i_ano)^(1/12) − 1. NUNCA i_ano/12 (taxa nominal linear subestima).
        /// </summary>
        public static decimal TaxaAnualParaMensal(decimal taxaAnual)
        {
            var mensal = Math.Pow(1.0 + (double)taxaAnual, 1.0 / 12.0) - 1.0;
            return (decimal)mensal;
        }

        /// <summary>Anualiza uma taxa periódica: (1 + i)^p − 1 (skill Receita D, passo 4).</summary>
        public static decimal Anualizar(decimal taxaPeriodica, int periodosPorAno = 12)
        {
            var anual = Math.Pow(1.0 + (double)taxaPeriodica, periodosPorAno) - 1.0;
            return (decimal)anual;
        }

        /// <summary>
        /// Simula o financiamento. <paramref name="taxaPeriodica"/> deve bater com o período da parcela
        /// (mensal → parcela mensal). <paramref name="prazo"/> = nº de parcelas.
        /// </summary>
        public static ResultadoSimulacao Simular(
            decimal principal,
            decimal taxaPeriodica,
            int prazo,
            SistemaAmortizacao sistema,
            CustosOperacao? custos = null,
            TipoMutuario mutuario = TipoMutuario.PessoaFisica,
            DateTime? dataOperacao = null,
            int diasPorPeriodo = 30,
            int periodosPorAno = 12)
        {
            if (principal <= 0m) throw new ArgumentOutOfRangeException(nameof(principal), "Principal deve ser > 0.");
            if (prazo <= 0) throw new ArgumentOutOfRangeException(nameof(prazo), "Prazo deve ser > 0.");
            if (taxaPeriodica < 0m) throw new ArgumentOutOfRangeException(nameof(taxaPeriodica), "Taxa não pode ser negativa.");

            custos ??= new CustosOperacao();
            var data = dataOperacao ?? DateTime.UtcNow;

            var parcelas = sistema == SistemaAmortizacao.Price
                ? MontarPrice(principal, taxaPeriodica, prazo, custos.SeguroPorParcela)
                : MontarSac(principal, taxaPeriodica, prazo, custos.SeguroPorParcela);

            decimal totalPago = 0m, totalJuros = 0m;
            foreach (var p in parcelas)
            {
                totalPago += p.Prestacao;
                totalJuros += p.Juros;
            }
            totalPago = Round(totalPago);
            totalJuros = Round(totalJuros);

            var prazoDias = prazo * diasPorPeriodo;
            var iof = TabelaIof.Calcular(principal, prazoDias, mutuario, data);

            // Fluxo de caixa do cliente para o CET (skill Receita D):
            //  - t0 líquido = principal recebido − (tarifas/IOF DESCONTADOS na liberação);
            //  - parcelas t1..tn = prestações já com seguro/tarifas mensais embutidos.
            var iofDescontado = custos.IofFinanciado ? 0m : iof;
            var pvLiquido = principal - custos.TarifasDescontadas - iofDescontado;

            var fluxo = new decimal[parcelas.Count];
            for (var k = 0; k < parcelas.Count; k++)
                fluxo[k] = parcelas[k].Prestacao;

            var cetPeriodico = CalcularTir(pvLiquido, fluxo);
            var cetAnual = Anualizar(cetPeriodico, periodosPorAno);
            var taxaJurosAnual = Anualizar(taxaPeriodica, periodosPorAno);

            // Sanidade (skill Receita D): CET nunca é inferior ao juro contratual. Se der abaixo por
            // ruído numérico quando não há custo extra, ancora no piso do juro anual.
            if (cetAnual < taxaJurosAnual)
                cetAnual = taxaJurosAnual;

            return new ResultadoSimulacao
            {
                Principal = principal,
                TaxaPeriodica = taxaPeriodica,
                Prazo = prazo,
                Sistema = sistema,
                PrimeiraPrestacao = parcelas.Count > 0 ? parcelas[0].Prestacao : 0m,
                TotalPago = totalPago,
                TotalJuros = totalJuros,
                Iof = iof,
                CetAnual = Math.Round(cetAnual, 6, MidpointRounding.AwayFromZero),
                TaxaJurosAnual = Math.Round(taxaJurosAnual, 6, MidpointRounding.AwayFromZero),
                Parcelas = parcelas
            };
        }

        /// <summary>Receita A — Tabela Price (prestação constante). PMT = PV·i / (1 − (1+i)^−n).</summary>
        private static List<ParcelaFinanciamento> MontarPrice(decimal pv, decimal i, int n, decimal seguro)
        {
            var parcelas = new List<ParcelaFinanciamento>(n);

            decimal pmt;
            if (i == 0m)
            {
                pmt = Round(pv / n);
            }
            else
            {
                var fator = 1.0 - Math.Pow(1.0 + (double)i, -n);
                pmt = Round((decimal)((double)(pv * i) / fator));
            }

            var saldo = pv;
            for (var k = 1; k <= n; k++)
            {
                var juros = Round(saldo * i);
                var amortizacao = pmt - juros;
                var prestacao = pmt;

                if (k == n)
                {
                    // Última parcela absorve o arredondamento → saldo final = 0 (skill Receita A / armadilha "saldo que não zera").
                    amortizacao = saldo;
                    prestacao = Round(juros + amortizacao);
                    saldo = 0m;
                }
                else
                {
                    saldo = Round(saldo - amortizacao);
                }

                parcelas.Add(new ParcelaFinanciamento(k, Round(prestacao + seguro), juros, Round(amortizacao), saldo));
            }

            return parcelas;
        }

        /// <summary>Receita B — Tabela SAC (amortização constante = PV/n; prestação decrescente).</summary>
        private static List<ParcelaFinanciamento> MontarSac(decimal pv, decimal i, int n, decimal seguro)
        {
            var parcelas = new List<ParcelaFinanciamento>(n);
            var amortizacaoBase = Round(pv / n);

            var saldo = pv;
            for (var k = 1; k <= n; k++)
            {
                var juros = Round(saldo * i);
                var amortizacao = amortizacaoBase;

                if (k == n)
                {
                    amortizacao = saldo; // zera o saldo na última
                    saldo = 0m;
                }
                else
                {
                    saldo = Round(saldo - amortizacao);
                }

                var prestacao = Round(juros + amortizacao);
                parcelas.Add(new ParcelaFinanciamento(k, Round(prestacao + seguro), juros, Round(amortizacao), saldo));
            }

            return parcelas;
        }

        /// <summary>
        /// Receita D — CET = TIR do fluxo do cliente, por BISSEÇÃO (skill pseudocódigo).
        /// Acha a taxa periódica i que zera o VPL: PV_liquido = Σ parcela_k / (1+i)^k.
        /// VPL decresce com i, logo a bisseção converge.
        /// </summary>
        public static decimal CalcularTir(decimal pvLiquido, decimal[] fluxo)
        {
            if (pvLiquido <= 0m || fluxo.Length == 0)
                return 0m;

            double lo = 0.0, hi = 1.0;
            var pv = (double)pvLiquido;

            double Vpl(double i)
            {
                var soma = -pv;
                var fator = 1.0 + i;
                var acum = fator;
                for (var k = 0; k < fluxo.Length; k++)
                {
                    soma += (double)fluxo[k] / acum;
                    acum *= fator;
                }
                return soma;
            }

            // Expande o teto se com hi=100% ainda houver VPL positivo (fluxo muito caro).
            var vplHi = Vpl(hi);
            var guarda = 0;
            while (vplHi > 0 && guarda++ < 20)
            {
                hi *= 2.0;
                vplHi = Vpl(hi);
            }

            var i = (lo + hi) / 2.0;
            for (var iter = 0; iter < 200; iter++)
            {
                i = (lo + hi) / 2.0;
                var vpl = Vpl(i);
                if (Math.Abs(vpl) < 1e-9) break;
                if (vpl > 0) lo = i; else hi = i;
            }

            return (decimal)i;
        }

        private static decimal Round(decimal v) => Math.Round(v, CasasMonetarias, MidpointRounding.AwayFromZero);
    }
}
