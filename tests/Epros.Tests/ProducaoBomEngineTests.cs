using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Modules.Producao.Domain.Services;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// PRD-BOM — Testes do motor de explosão multinível, validação circular e vigência (PD6).
    /// </summary>
    public class ProducaoBomEngineTests
    {
        private const string TenantId = "tenant-bom-eng";
        private const string UserId = "user-bom-eng";

        private static BomEstrutura EstruturaAtiva(
            Guid produtoId,
            Guid variacaoId,
            decimal quantidadeTotal,
            (Guid comp, decimal qtd, decimal? desperdicio, ETipoComponenteBom tipo)[] componentes,
            DateTime? inicio = null,
            DateTime? fim = null)
        {
            var e = new BomEstrutura(produtoId, variacaoId, null, 0m, 0m, 0m, quantidadeTotal, TenantId, UserId,
                inicioVigencia: inicio, fimVigencia: fim);
            foreach (var c in componentes)
                e.AdicionarComponente(c.comp, c.qtd, UserId, percentualDesperdicioLinha: c.desperdicio, tipoComponente: c.tipo);
            e.SubmeterParaAnalise(UserId);
            e.Aprovar(UserId);
            return e;
        }

        [Fact(DisplayName = "BOM-ENG | Vigência seleciona a estrutura ativa no período (CHK-BOM-001)")]
        public void Vigencia_SelecionaAtivaNoPeriodo()
        {
            var produto = Guid.NewGuid();
            var variacao = Guid.NewGuid();
            var svc = new BomExplosaoService();

            var vigente = EstruturaAtiva(produto, variacao, 1m,
                new[] { (Guid.NewGuid(), 1m, (decimal?)null, ETipoComponenteBom.Normal) },
                inicio: new DateTime(2026, 1, 1), fim: new DateTime(2026, 12, 31));
            var expirada = EstruturaAtiva(produto, variacao, 1m,
                new[] { (Guid.NewGuid(), 1m, (decimal?)null, ETipoComponenteBom.Normal) },
                inicio: new DateTime(2025, 1, 1), fim: new DateTime(2025, 12, 31));

            var selecionada = svc.SelecionarVigente(new[] { vigente, expirada }, produto, variacao, new DateTime(2026, 6, 1));
            Assert.NotNull(selecionada);
            Assert.Equal(vigente.Id, selecionada!.Id);
        }

        [Fact(DisplayName = "BOM-ENG | Duas estruturas vigentes sobrepostas violam ativa-única")]
        public void Vigencia_DuasSobrepostas_Lanca()
        {
            var produto = Guid.NewGuid();
            var variacao = Guid.NewGuid();
            var svc = new BomExplosaoService();

            var a = EstruturaAtiva(produto, variacao, 1m, new[] { (Guid.NewGuid(), 1m, (decimal?)null, ETipoComponenteBom.Normal) });
            var b = EstruturaAtiva(produto, variacao, 1m, new[] { (Guid.NewGuid(), 1m, (decimal?)null, ETipoComponenteBom.Normal) });

            Assert.Throws<InvalidOperationException>(() =>
                svc.SelecionarVigente(new[] { a, b }, produto, variacao, DateTime.UtcNow));
        }

        [Fact(DisplayName = "BOM-ENG | Detecta ciclo A→B→A (BOM-REG-019)")]
        public void Circular_Detecta()
        {
            var pa = Guid.NewGuid();
            var pb = Guid.NewGuid();
            var svc = new BomExplosaoService();

            var a = EstruturaAtiva(pa, Guid.NewGuid(), 1m, new[] { (pb, 1m, (decimal?)null, ETipoComponenteBom.Normal) });
            var b = EstruturaAtiva(pb, Guid.NewGuid(), 1m, new[] { (pa, 1m, (decimal?)null, ETipoComponenteBom.Normal) });

            Assert.True(svc.PossuiCiclo(a, new[] { a, b }, DateTime.UtcNow));

            var res = svc.Explodir(a, 1m, new[] { a, b }, DateTime.UtcNow);
            Assert.True(res.PossuiCiclo);
        }

        [Fact(DisplayName = "BOM-ENG | Explosão de nível único aplica rendimento e desperdício")]
        public void Explosao_NivelUnico()
        {
            var produto = Guid.NewGuid();
            var r1 = Guid.NewGuid();
            var svc = new BomExplosaoService();

            // rendimento 1, componente 10 com 10% de perda => QuantidadeFinal = 9 por unidade
            var estrutura = EstruturaAtiva(produto, Guid.Empty, 1m,
                new[] { (r1, 10m, (decimal?)10m, ETipoComponenteBom.Normal) });

            var res = svc.Explodir(estrutura, 5m, new[] { estrutura }, DateTime.UtcNow);
            Assert.False(res.PossuiCiclo);
            Assert.False(res.CalculoIncompleto);
            Assert.Equal(45m, res.NecessidadesFolha[r1]); // 9 * 5
        }

        [Fact(DisplayName = "BOM-ENG | Explosão multinível desce por submontagem")]
        public void Explosao_Multinivel()
        {
            var produto = Guid.NewGuid();
            var subMont = Guid.NewGuid();
            var r1 = Guid.NewGuid();
            var svc = new BomExplosaoService();

            // raiz: 1 produto = 2 x submontagem
            var raiz = EstruturaAtiva(produto, Guid.Empty, 1m,
                new[] { (subMont, 2m, (decimal?)null, ETipoComponenteBom.Submontagem) });
            // submontagem: 1 unidade = 3 x matéria-prima R1
            var filha = EstruturaAtiva(subMont, Guid.Empty, 1m,
                new[] { (r1, 3m, (decimal?)null, ETipoComponenteBom.Normal) });

            var res = svc.Explodir(raiz, 5m, new[] { raiz, filha }, DateTime.UtcNow);

            Assert.Equal(30m, res.NecessidadesFolha[r1]); // 2 * 3 * 5
            Assert.DoesNotContain(res.Itens.Where(i => i.EhFolha), i => i.ItemId == subMont); // submontagem não é folha
        }

        [Fact(DisplayName = "BOM-ENG | Item fantasma é atravessado sem gerar necessidade própria (DP-BOM-013)")]
        public void Explosao_Fantasma_Atravessa()
        {
            var produto = Guid.NewGuid();
            var fantasma = Guid.NewGuid();
            var r2 = Guid.NewGuid();
            var svc = new BomExplosaoService();

            var raiz = EstruturaAtiva(produto, Guid.Empty, 1m,
                new[] { (fantasma, 2m, (decimal?)null, ETipoComponenteBom.Fantasma) });
            var subFantasma = EstruturaAtiva(fantasma, Guid.Empty, 1m,
                new[] { (r2, 4m, (decimal?)null, ETipoComponenteBom.Normal) });

            var res = svc.Explodir(raiz, 1m, new[] { raiz, subFantasma }, DateTime.UtcNow);

            Assert.Equal(8m, res.NecessidadesFolha[r2]); // 2 * 4
            Assert.DoesNotContain(res.Itens, i => i.ItemId == fantasma); // fantasma não vira item
        }
    }
}
