using System;
using System.Collections.Generic;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Modules.Producao.Domain.Services;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// PRD-CST — Testes do motor de custeio com rollup multinível (PD8). MOD/CIF são valida-contador
    /// (defaults 0), aqui exercitados com taxas explícitas para validar a mecânica do motor.
    /// </summary>
    public class ProducaoCustoRollupTests
    {
        private const string TenantId = "tenant-cst-eng";
        private const string UserId = "user-cst-eng";

        private static BomEstrutura EstruturaAtiva(
            Guid produtoId, decimal quantidadeTotal, decimal custoExtra,
            (Guid comp, decimal qtd, decimal? custoUnit, ETipoComponenteBom tipo)[] componentes)
        {
            var e = new BomEstrutura(produtoId, Guid.NewGuid(), null, 0m, 0m, custoExtra, quantidadeTotal, TenantId, UserId);
            foreach (var c in componentes)
                e.AdicionarComponente(c.comp, c.qtd, UserId, custoUnitarioComImpostos: c.custoUnit, tipoComponente: c.tipo);
            e.SubmeterParaAnalise(UserId);
            e.Aprovar(UserId);
            return e;
        }

        [Fact(DisplayName = "CST-ENG | Rollup de nível único soma material da folha")]
        public void Rollup_NivelUnico()
        {
            var produto = Guid.NewGuid();
            var r1 = Guid.NewGuid();
            var svc = new CustoRollupService();

            // rendimento 1, R1 qtd 2, custo unitário 10 => material 20
            var estrutura = EstruturaAtiva(produto, 1m, 0m,
                new[] { (r1, 2m, (decimal?)10m, ETipoComponenteBom.Normal) });

            var custo = svc.CalcularPrevisto(estrutura, new[] { estrutura }, new CustoRollupService.ParametrosCusteio(), DateTime.UtcNow);
            Assert.False(custo.PossuiCiclo);
            Assert.Equal(20m, custo.CustoMaterial);
            Assert.Equal(20m, custo.CustoTotal);
            Assert.Equal(20m, custo.CustoUnitario);
        }

        [Fact(DisplayName = "CST-ENG | Rollup multinível: custo da submontagem sobe para o pai")]
        public void Rollup_Multinivel()
        {
            var produto = Guid.NewGuid();
            var subMont = Guid.NewGuid();
            var r1 = Guid.NewGuid();
            var svc = new CustoRollupService();

            // submontagem: rendimento 1, R1 qtd 2 x 10 => custo unitário 20
            var filha = EstruturaAtiva(subMont, 1m, 0m,
                new[] { (r1, 2m, (decimal?)10m, ETipoComponenteBom.Normal) });
            // raiz: rendimento 1, submontagem qtd 3 => material 60
            var raiz = EstruturaAtiva(produto, 1m, 0m,
                new[] { (subMont, 3m, (decimal?)null, ETipoComponenteBom.Submontagem) });

            var custo = svc.CalcularPrevisto(raiz, new[] { raiz, filha }, new CustoRollupService.ParametrosCusteio(), DateTime.UtcNow);
            Assert.Equal(60m, custo.CustoMaterial);
        }

        [Fact(DisplayName = "CST-ENG | MOD e CIF entram por taxa configurada (valida-contador)")]
        public void Rollup_ModCif()
        {
            var produto = Guid.NewGuid();
            var r1 = Guid.NewGuid();
            var svc = new CustoRollupService();

            var estrutura = EstruturaAtiva(produto, 1m, 5m,
                new[] { (r1, 2m, (decimal?)10m, ETipoComponenteBom.Normal) });

            var p = new CustoRollupService.ParametrosCusteio
            {
                TaxaMaoDeObraPorHora = 15m,
                TaxaCifPorHoraMaquina = 5m,
                HorasPorEstrutura = _ => (2m, 4m) // 2h MOD, 4h máquina
            };

            var custo = svc.CalcularPrevisto(estrutura, new[] { estrutura }, p, DateTime.UtcNow);
            Assert.Equal(20m, custo.CustoMaterial);
            Assert.Equal(30m, custo.CustoMaoDeObra); // 2 * 15
            Assert.Equal(20m, custo.CustoIndireto);  // 4 * 5
            Assert.Equal(5m, custo.CustoExtra);
            Assert.Equal(75m, custo.CustoTotal);     // 20 + 30 + 20 + 5
        }

        [Fact(DisplayName = "CST-ENG | Desvio = realizado - previsto")]
        public void Desvio()
        {
            Assert.Equal(30m, CustoRollupService.CalcularDesvio(100m, 130m));
            Assert.Equal(-10m, CustoRollupService.CalcularDesvio(50m, 40m));
        }
    }
}
