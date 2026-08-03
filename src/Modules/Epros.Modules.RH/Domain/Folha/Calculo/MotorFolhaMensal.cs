using System;
using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.RH.Domain.Folha.Calculo
{
    // ============================================================================================
    //  MOTOR DA FOLHA MENSAL — orquestra a ordem de apuração (skill folha, "Ordem de apuração").
    //  1) proventos  2) descontos que reduzem base  3) INSS  4) IRRF  5) FGTS (encargo, fora do líquido)
    //  6) demais descontos (VT ≤ 6%, adiantamento, pensão, faltas)  7) líquido = Σprov − Σdesc.
    //  Puro: recebe as verbas já apuradas (HE/adicionais/DSR vêm dos motores de jornada) e devolve o holerite.
    // ============================================================================================

    public sealed record ItemProvento(
        string Codigo,
        string Descricao,
        decimal Valor,
        bool IncideInss = true,
        bool IncideIrrf = true,
        bool IncideFgts = true);

    public sealed record ItemDesconto(string Codigo, string Descricao, decimal Valor);

    public sealed record EntradaFolhaMensal(
        decimal SalarioBase,
        IReadOnlyList<ItemProvento>? ProventosAdicionais = null,
        IReadOnlyList<ItemDesconto>? DescontosDiversos = null,
        int NumDependentes = 0,
        decimal PensaoAlimenticiaBaseIrrf = 0m,
        decimal DescontoValeTransporte = 0m,
        bool Aprendiz = false);

    public sealed record HoleriteRubrica(string Codigo, string Descricao, string Tipo, decimal Valor);

    public sealed record ResultadoFolhaMensal(
        IReadOnlyList<HoleriteRubrica> Rubricas,
        decimal TotalProventos,
        decimal BaseInss,
        decimal Inss,
        decimal BaseIrrf,
        decimal Irrf,
        decimal BaseFgts,
        decimal Fgts,
        decimal TotalDescontos,
        decimal Liquido);

    public static class MotorFolhaMensal
    {
        public const string LimiteVtPercentual = "0.06"; // VT: desconto ≤ 6% do salário-base (skill, edge cases)

        public static ResultadoFolhaMensal Calcular(EntradaFolhaMensal e, TabelasFolha tabelas)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (tabelas == null) throw new ArgumentNullException(nameof(tabelas));
            if (e.SalarioBase <= 0) throw new ArgumentException("Salário-base deve ser maior que zero.", nameof(e));

            var rubricas = new List<HoleriteRubrica>();

            // 1) Proventos: salário-base + adicionais (HE, adicional noturno, DSR, comissões, abonos…).
            var proventos = new List<ItemProvento>
            {
                new("001", "Salário base", e.SalarioBase, IncideInss: true, IncideIrrf: true, IncideFgts: true)
            };
            if (e.ProventosAdicionais != null) proventos.AddRange(e.ProventosAdicionais);

            foreach (var p in proventos)
                rubricas.Add(new HoleriteRubrica(p.Codigo, p.Descricao, "Provento", Arredondamento.Reais(p.Valor)));

            var totalProventos = Arredondamento.Reais(proventos.Sum(p => p.Valor));

            // 3) INSS sobre a base salarial (só o que incide) → gera a base para o IRRF.
            var baseInss = proventos.Where(p => p.IncideInss).Sum(p => p.Valor);
            var inss = MotorInss.Calcular(baseInss, tabelas.Inss);

            // 4) IRRF sobre o rendimento tributável, após INSS e deduções.
            var rendTributavel = proventos.Where(p => p.IncideIrrf).Sum(p => p.Valor);
            var irrf = MotorIrrf.Calcular(
                rendTributavel, inss.Valor, e.NumDependentes, tabelas.Irrf,
                pensaoAlimenticia: e.PensaoAlimenticiaBaseIrrf);

            // 5) FGTS — encargo do empregador, FORA do líquido do empregado.
            var baseFgts = proventos.Where(p => p.IncideFgts).Sum(p => p.Valor);
            var fgts = MotorFgts.Calcular(baseFgts, tabelas, e.Aprendiz);

            // 6) Descontos.
            if (inss.Valor > 0)
                rubricas.Add(new HoleriteRubrica("998", "INSS", "Desconto", inss.Valor));
            if (irrf.Imposto > 0)
                rubricas.Add(new HoleriteRubrica("999", "IRRF", "Desconto", irrf.Imposto));

            // Vale-transporte limitado a 6% do salário-base (excedente é custo do empregador).
            decimal vt = 0m;
            if (e.DescontoValeTransporte > 0)
            {
                var teto = Arredondamento.Reais(e.SalarioBase * decimal.Parse(LimiteVtPercentual, System.Globalization.CultureInfo.InvariantCulture));
                vt = Math.Min(Arredondamento.Reais(e.DescontoValeTransporte), teto);
                rubricas.Add(new HoleriteRubrica("930", "Vale-transporte", "Desconto", vt));
            }

            decimal diversos = 0m;
            if (e.DescontosDiversos != null)
            {
                foreach (var d in e.DescontosDiversos)
                {
                    var v = Arredondamento.Reais(d.Valor);
                    diversos += v;
                    rubricas.Add(new HoleriteRubrica(d.Codigo, d.Descricao, "Desconto", v));
                }
            }

            // FGTS informado como base/encargo (não entra no líquido).
            rubricas.Add(new HoleriteRubrica("905", "FGTS do mês (encargo)", "Encargo", fgts));

            var totalDescontos = Arredondamento.Reais(inss.Valor + irrf.Imposto + vt + diversos);

            // 7) Líquido.
            var liquido = Arredondamento.Reais(totalProventos - totalDescontos);

            return new ResultadoFolhaMensal(
                Rubricas: rubricas,
                TotalProventos: totalProventos,
                BaseInss: inss.BaseContribuicao,
                Inss: inss.Valor,
                BaseIrrf: irrf.BaseCalculo,
                Irrf: irrf.Imposto,
                BaseFgts: Arredondamento.Reais(baseFgts),
                Fgts: fgts,
                TotalDescontos: totalDescontos,
                Liquido: liquido);
        }
    }

    /// <summary>RN-04 · DSR sobre verbas variáveis = (Σ variáveis ÷ dias úteis) × dias de repouso.</summary>
    public static class MotorDsr
    {
        public static decimal Calcular(decimal somaVariaveis, int diasUteis, int diasRepouso)
        {
            if (somaVariaveis <= 0 || diasUteis <= 0 || diasRepouso <= 0) return 0m;
            return Arredondamento.Reais(somaVariaveis / diasUteis * diasRepouso);
        }
    }
}
