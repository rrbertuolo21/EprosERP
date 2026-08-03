using System;
using System.Linq;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Modules.Producao.Domain.Services;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// PRD-MRP — Testes do motor MRP: explosão + netting + geração de sugestões e estados (PD3/PD21).
    /// </summary>
    public class ProducaoMrpEngineTests
    {
        private const string TenantId = "tenant-mrp-eng";
        private const string UserId = "user-mrp-eng";

        private static BomEstrutura EstruturaAtiva(Guid produtoId, decimal rendimento, (Guid comp, decimal qtd)[] componentes)
        {
            var e = new BomEstrutura(produtoId, Guid.NewGuid(), null, 0m, 0m, 0m, rendimento, TenantId, UserId);
            foreach (var c in componentes)
                e.AdicionarComponente(c.comp, c.qtd, UserId);
            e.SubmeterParaAnalise(UserId);
            e.Aprovar(UserId);
            return e;
        }

        [Fact(DisplayName = "MRP-ENG | Netting: líquida = bruta - disponibilidade - recebimentos")]
        public void Netting_Basico()
        {
            var produto = Guid.NewGuid();
            var comp = Guid.NewGuid();
            var svc = new MrpService();

            var estrutura = EstruturaAtiva(produto, 1m, new[] { (comp, 2m) }); // 2 do comp por unidade
            var parametros = new MrpService.ParametrosMrp
            {
                Disponibilidade = i => i == comp ? 50m : 0m,
                RecebimentosProgramados = i => i == comp ? 30m : 0m
            };

            var res = svc.Planejar(
                new[] { new MrpService.Demanda(produto, 100m, DateTime.UtcNow) },
                new[] { estrutura }, parametros, DateTime.UtcNow);

            Assert.False(res.CalculoIncompleto);
            var necComp = res.Necessidades.Single(n => n.ItemId == comp);
            Assert.Equal(200m, necComp.Bruta);   // 100 * 2
            Assert.Equal(120m, necComp.Liquida);  // 200 - 50 - 30
        }

        [Fact(DisplayName = "MRP-ENG | Sugestão: compra p/ item sem BOM, produção p/ item com BOM")]
        public void Sugestao_TipoPorEstrutura()
        {
            var produto = Guid.NewGuid();
            var comp = Guid.NewGuid();
            var svc = new MrpService();
            var estrutura = EstruturaAtiva(produto, 1m, new[] { (comp, 1m) });

            var res = svc.Planejar(
                new[] { new MrpService.Demanda(produto, 10m, DateTime.UtcNow) },
                new[] { estrutura }, new MrpService.ParametrosMrp(), DateTime.UtcNow);

            var sugProduto = res.Sugestoes.Single(s => s.ItemId == produto);
            var sugComp = res.Sugestoes.Single(s => s.ItemId == comp);
            Assert.Equal(ETipoSugestaoMrp.Producao, sugProduto.Tipo); // tem BOM
            Assert.Equal(ETipoSugestaoMrp.Compra, sugComp.Tipo);      // sem BOM
        }

        [Fact(DisplayName = "MRP-ENG | Estoque de segurança e lote mínimo elevam a líquida")]
        public void EstoqueSeguranca_LoteMinimo()
        {
            var comp = Guid.NewGuid();
            var svc = new MrpService();
            var parametros = new MrpService.ParametrosMrp
            {
                EstoqueSeguranca = _ => 5m,
                LoteMinimo = _ => 100m
            };

            // demanda direta do componente (sem BOM): bruta 10 + seg 5 = 15, elevado ao lote mínimo 100
            var res = svc.Planejar(
                new[] { new MrpService.Demanda(comp, 10m, DateTime.UtcNow) },
                Array.Empty<BomEstrutura>(), parametros, DateTime.UtcNow);

            var nec = res.Necessidades.Single();
            Assert.Equal(100m, nec.Liquida);
        }

        [Fact(DisplayName = "MRP-ENG | Necessidade líquida nunca é negativa")]
        public void Netting_NuncaNegativa()
        {
            var comp = Guid.NewGuid();
            var svc = new MrpService();
            var parametros = new MrpService.ParametrosMrp { Disponibilidade = _ => 1000m };

            var res = svc.Planejar(
                new[] { new MrpService.Demanda(comp, 10m, DateTime.UtcNow) },
                Array.Empty<BomEstrutura>(), parametros, DateTime.UtcNow);

            Assert.Equal(0m, res.Necessidades.Single().Liquida);
            Assert.Empty(res.Sugestoes); // nada a suprir
        }

        // ---------- Estados da sugestão (entidade) ----------

        [Fact(DisplayName = "MRP-SUG | Ciclo Calculada→Pendente→Aprovada→Convertida")]
        public void Sugestao_CicloEstados()
        {
            var s = new MrpSugestao(Guid.NewGuid(), Guid.NewGuid(), ETipoSugestaoMrp.Compra, 10m, DateTime.UtcNow, TenantId, UserId);
            Assert.Equal(EEstadoSugestaoMrp.Calculada, s.Estado);
            s.SubmeterAprovacao(UserId);
            Assert.Equal(EEstadoSugestaoMrp.PendenteAprovacao, s.Estado);
            s.Aprovar(UserId);
            Assert.Equal(EEstadoSugestaoMrp.Aprovada, s.Estado);

            var doc = Guid.NewGuid();
            s.Converter(doc, UserId);
            Assert.Equal(EEstadoSugestaoMrp.Convertida, s.Estado);
            Assert.Equal(doc, s.DocumentoGeradoId);
        }

        [Fact(DisplayName = "MRP-SUG | Conversão é idempotente e não duplica documento (PRD-INT-CA-002)")]
        public void Sugestao_ConversaoIdempotente()
        {
            var s = new MrpSugestao(Guid.NewGuid(), Guid.NewGuid(), ETipoSugestaoMrp.Compra, 10m, DateTime.UtcNow, TenantId, UserId, impactoFinanceiro: false);
            s.Aprovar(UserId); // sem impacto: aprova direto
            var doc = Guid.NewGuid();
            s.Converter(doc, UserId);
            s.Converter(doc, UserId); // mesmo doc: no-op idempotente
            Assert.True(s.IsValid);
            Assert.Equal(EEstadoSugestaoMrp.Convertida, s.Estado);

            s.Converter(Guid.NewGuid(), UserId); // outro doc: bloqueia
            Assert.False(s.IsValid);
        }
    }
}
