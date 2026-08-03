using System;
using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.RH.Domain.Folha.Calculo
{
    // ============================================================================================
    //  MOTOR DE RESCISÃO (RN-07) — verbas rescisórias por TIPO de desligamento.
    //  Skill: departamento-pessoal/folha (matriz de verbas) + CLT art. 477/487 + Lei 8.036/1990
    //  (multa FGTS) + Lei 12.506/2011 (aviso prévio proporcional).
    //  ⛔ Incidências finais (INSS/IRRF/FGTS de cada verba) seguem a tabela de rubricas do eSocial/RFB:
    //     // valida-contador — conferir natureza de cada rubrica no leiaute vigente antes do go-live.
    // ============================================================================================

    public enum TipoDesligamento
    {
        SemJustaCausaEmpregador,
        PedidoDemissao,
        JustaCausa,
        Acordo484A,
        FimContratoPrazoDeterminado
    }

    public sealed record VerbaRescisoria(string Codigo, string Descricao, decimal Valor, string Natureza);
    //  Natureza: "Salarial" (INSS/IRRF/FGTS), "Indenizatorio" (sem INSS/IRRF), "FGTS".

    public sealed record EntradaRescisao(
        TipoDesligamento Tipo,
        decimal SalarioMensal,
        DateTime DataAdmissao,
        DateTime DataDesligamento,
        int DiasTrabalhadosNoMes,
        decimal SaldoFgtsDepositado = 0m,
        bool TemFeriasVencidas = false,
        decimal RemuneracaoFeriasVencidas = 0m,
        decimal MediaVariaveis = 0m,
        int NumDependentes = 0,
        decimal PensaoAlimenticia = 0m);

    public sealed record ResultadoRescisao(
        IReadOnlyList<VerbaRescisoria> Verbas,
        int AnosCompletos,
        int DiasAvisoPrevio,
        int AvosDecimoTerceiro,
        int AvosFerias,
        decimal TotalProventos,
        decimal Inss,
        decimal Irrf,
        decimal MultaFgts,
        decimal TotalDescontos,
        decimal Liquido,
        bool TemDireitoSeguroDesemprego)
    {
        public decimal ValorVerba(string codigo) =>
            Verbas.Where(v => v.Codigo == codigo).Sum(v => v.Valor);
        public bool TemVerba(string codigo) => Verbas.Any(v => v.Codigo == codigo);
    }

    public static class MotorRescisao
    {
        public static ResultadoRescisao Calcular(EntradaRescisao e, TabelasFolha tabelas)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (tabelas == null) throw new ArgumentNullException(nameof(tabelas));
            if (e.SalarioMensal <= 0) throw new ArgumentException("Salário deve ser maior que zero.", nameof(e));
            if (e.DataDesligamento < e.DataAdmissao) throw new ArgumentException("Desligamento anterior à admissão.", nameof(e));

            var baseRemun = e.SalarioMensal + Math.Max(0m, e.MediaVariaveis);
            var diaria = baseRemun / 30m;

            int anos = AnosCompletos(e.DataAdmissao, e.DataDesligamento);

            // Matriz de direitos por tipo (skill folha, tabela RN-07).
            var (devidoAviso, avisoFator, devido13, devidoFeriasProp, multaFator, seguroDesemprego) = Matriz(e.Tipo);

            // Aviso prévio: 30 + 3 dias por ano completo, teto 90 (Lei 12.506/2011).
            int diasAviso = devidoAviso ? Math.Min(90, 30 + 3 * anos) : 0;

            // Aviso indenizado projeta a data → conta como tempo para 13º e férias.
            var dataProjetada = e.DataDesligamento.AddDays(diasAviso);

            int avos13 = devido13
                ? MotorDecimoTerceiro.AvosPorDias(InicioAnoDe(dataProjetada, e.DataAdmissao), dataProjetada)
                : 0;
            int avosFerias = devidoFeriasProp
                ? AvosFeriasProporcionais(e.DataAdmissao, dataProjetada)
                : 0;

            var verbas = new List<VerbaRescisoria>();

            // Saldo de salário (salarial).
            var saldoSalario = Arredondamento.Reais(diaria * e.DiasTrabalhadosNoMes);
            if (saldoSalario > 0)
                verbas.Add(new VerbaRescisoria("SALDO", "Saldo de salário", saldoSalario, "Salarial"));

            // Aviso prévio indenizado (indenizatório; FGTS incide, mas não INSS/IRRF).
            decimal avisoValor = 0m;
            if (diasAviso > 0)
            {
                avisoValor = Arredondamento.Reais(diaria * diasAviso * avisoFator);
                verbas.Add(new VerbaRescisoria("AVISO", "Aviso prévio indenizado", avisoValor, "Indenizatorio"));
            }

            // 13º proporcional (apuração própria de INSS/IRRF).
            decimal decimoBruto = 0m, inss13 = 0m, irrf13 = 0m;
            if (avos13 > 0)
            {
                var d = MotorDecimoTerceiro.Calcular(baseRemun, avos13, tabelas, numDependentes: e.NumDependentes);
                decimoBruto = d.Bruto; inss13 = d.Inss; irrf13 = d.Irrf;
                verbas.Add(new VerbaRescisoria("13PROP", "13º salário proporcional", decimoBruto, "Salarial"));
            }

            // Férias vencidas + 1/3 (indenizatório) — devidas em todos os tipos.
            if (e.TemFeriasVencidas)
            {
                var remun = e.RemuneracaoFeriasVencidas > 0 ? e.RemuneracaoFeriasVencidas : baseRemun;
                var fv = Arredondamento.Reais(remun + remun / 3m);
                verbas.Add(new VerbaRescisoria("FERVENC", "Férias vencidas + 1/3", fv, "Indenizatorio"));
            }

            // Férias proporcionais + 1/3 (indenizatório).
            if (avosFerias > 0)
            {
                var fp = Arredondamento.Reais(baseRemun / 12m * avosFerias);
                var fpTotal = Arredondamento.Reais(fp + fp / 3m);
                verbas.Add(new VerbaRescisoria("FERPROP", "Férias proporcionais + 1/3", fpTotal, "Indenizatorio"));
            }

            // Multa rescisória do FGTS (Lei 8.036/1990 art. 18): 40% (sem justa causa), 20% (acordo 484-A).
            decimal multaFgts = 0m;
            if (multaFator > 0m && e.SaldoFgtsDepositado > 0m)
            {
                multaFgts = Arredondamento.Reais(e.SaldoFgtsDepositado * multaFator);
                verbas.Add(new VerbaRescisoria("MULTAFGTS", $"Multa {multaFator * 100m:0}% FGTS", multaFgts, "FGTS"));
            }

            // Descontos: INSS/IRRF sobre saldo de salário (mensal) + INSS/IRRF do 13º (separado) + pensão.
            var inssSaldo = MotorInss.Calcular(saldoSalario, tabelas.Inss);
            var irrfSaldo = MotorIrrf.Calcular(saldoSalario, inssSaldo.Valor, e.NumDependentes, tabelas.Irrf,
                pensaoAlimenticia: e.PensaoAlimenticia);

            var totalInss = Arredondamento.Reais(inssSaldo.Valor + inss13);
            var totalIrrf = Arredondamento.Reais(irrfSaldo.Imposto + irrf13);
            var pensao = Arredondamento.Reais(Math.Max(0m, e.PensaoAlimenticia));

            // Total de proventos = verbas salariais + indenizatórias (multa FGTS entra no crédito ao empregado).
            var totalProventos = Arredondamento.Reais(
                verbas.Where(v => v.Natureza != "FGTS").Sum(v => v.Valor) + multaFgts);
            var totalDescontos = Arredondamento.Reais(totalInss + totalIrrf + pensao);
            var liquido = Arredondamento.Reais(totalProventos - totalDescontos);

            return new ResultadoRescisao(
                Verbas: verbas,
                AnosCompletos: anos,
                DiasAvisoPrevio: diasAviso,
                AvosDecimoTerceiro: avos13,
                AvosFerias: avosFerias,
                TotalProventos: totalProventos,
                Inss: totalInss,
                Irrf: totalIrrf,
                MultaFgts: multaFgts,
                TotalDescontos: totalDescontos,
                Liquido: liquido,
                TemDireitoSeguroDesemprego: seguroDesemprego);
        }

        private static (bool devidoAviso, decimal avisoFator, bool devido13, bool devidoFeriasProp, decimal multaFator, bool seguro)
            Matriz(TipoDesligamento tipo) => tipo switch
            {
                // Sem justa causa (empregador): aviso indenizado, 13 prop, férias prop, multa 40%, seguro.
                TipoDesligamento.SemJustaCausaEmpregador => (true, 1.0m, true, true, 0.40m, true),
                // Pedido de demissão: sem aviso indenizado (devido ao empregador se não cumprir), 13 e férias prop, sem multa/seguro.
                TipoDesligamento.PedidoDemissao => (false, 0m, true, true, 0m, false),
                // Justa causa: só saldo + férias vencidas. Férias proporcionais NÃO devidas (Súmula 171 TST). // valida-contador
                TipoDesligamento.JustaCausa => (false, 0m, false, false, 0m, false),
                // Acordo 484-A: aviso 50%, 13 e férias prop, multa 20%, sem seguro.
                TipoDesligamento.Acordo484A => (true, 0.5m, true, true, 0.20m, false),
                // Fim de contrato por prazo determinado: 13 e férias prop; aviso conforme cláusula (default sem aviso);
                // multa própria (não 40%). // valida-contador — cláusula do contrato a prazo.
                TipoDesligamento.FimContratoPrazoDeterminado => (false, 0m, true, true, 0m, false),
                _ => (false, 0m, false, false, 0m, false)
            };

        /// <summary>Anos completos de casa (para o aviso prévio proporcional).</summary>
        public static int AnosCompletos(DateTime admissao, DateTime desligamento)
        {
            int anos = desligamento.Year - admissao.Year;
            if (desligamento.Month < admissao.Month ||
                (desligamento.Month == admissao.Month && desligamento.Day < admissao.Day))
                anos--;
            return Math.Max(0, anos);
        }

        private static DateTime InicioAnoDe(DateTime data, DateTime admissao)
        {
            var jan1 = new DateTime(data.Year, 1, 1);
            return admissao > jan1 ? admissao : jan1;
        }

        /// <summary>Avos de férias proporcionais: meses (≥15 dias) desde o último período aquisitivo.</summary>
        public static int AvosFeriasProporcionais(DateTime admissao, DateTime desligamento)
        {
            // Último aniversário de admissão ≤ desligamento inicia o período aquisitivo corrente.
            var aniversario = new DateTime(desligamento.Year, admissao.Month, Math.Min(admissao.Day, DateTime.DaysInMonth(desligamento.Year, admissao.Month)));
            if (aniversario > desligamento) aniversario = aniversario.AddYears(-1);
            var inicio = aniversario < admissao ? admissao : aniversario;
            return MotorDecimoTerceiro.AvosPorDias(inicio, desligamento);
        }
    }
}
