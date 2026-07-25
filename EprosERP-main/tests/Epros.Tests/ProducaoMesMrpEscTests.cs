using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Application.Handlers;
using Epros.Modules.Producao.Domain.Entities;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes das regras-chave dos submódulos de Produção pendentes portados:
    /// PRD-MES (Execução de Manufatura), PRD-MRP (MRP/IBP), PRD-ESC (Escalonamento/Programação).
    /// </summary>
    public class ProducaoMesMrpEscTests
    {
        private const string TenantId = "tenant-prd-002";
        private const string UserId = "user-prd-002";

        private static ContextProducao NovoContexto(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextProducao>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ContextProducao(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        // ===================== PRD-MES =====================

        [Fact(DisplayName = "MES | Ordem válida inicia em Rascunho")]
        public void Mes_OrdemValida_IniciaRascunho()
        {
            var ordem = new MesOrdem(Guid.NewGuid(), TenantId, UserId);
            Assert.True(ordem.IsValid);
            Assert.Equal(EStatusOrdemMes.Rascunho, ordem.Status);
        }

        [Fact(DisplayName = "MES | Ordem sem empresa é inválida (MES-REG-002)")]
        public void Mes_OrdemSemEmpresa_Invalida()
        {
            var ordem = new MesOrdem(Guid.Empty, TenantId, UserId);
            Assert.False(ordem.IsValid);
        }

        [Fact(DisplayName = "MES | Item exige produto e referência ao cabeçalho (MES-REG-004/005)")]
        public void Mes_Item_ExigeProdutoECabecalho()
        {
            var semProduto = new MesOrdemItem(Guid.NewGuid(), Guid.Empty, 10m, TenantId, UserId);
            Assert.False(semProduto.IsValid);

            var semOrdem = new MesOrdemItem(Guid.Empty, Guid.NewGuid(), 10m, TenantId, UserId);
            Assert.False(semOrdem.IsValid);
        }

        [Fact(DisplayName = "MES | Submeter sem item é bloqueado (MES-REG-003)")]
        public void Mes_SubmeterSemItem_Bloqueia()
        {
            var ordem = new MesOrdem(Guid.NewGuid(), TenantId, UserId);
            ordem.SubmeterParaAnalise(UserId);
            Assert.False(ordem.IsValid);
            Assert.Equal(EStatusOrdemMes.Rascunho, ordem.Status);
        }

        [Fact(DisplayName = "MES | Workflow Rascunho→EmAnalise→Ativo com item")]
        public void Mes_Workflow_AteAtivo()
        {
            var ordem = new MesOrdem(Guid.NewGuid(), TenantId, UserId);
            ordem.AdicionarItem(Guid.NewGuid(), 5m, UserId);
            ordem.SubmeterParaAnalise(UserId);
            Assert.Equal(EStatusOrdemMes.EmAnalise, ordem.Status);
            ordem.Aprovar(UserId);
            Assert.Equal(EStatusOrdemMes.Ativo, ordem.Status);
        }

        [Fact(DisplayName = "MES | Finalizar exige data, local e valor total final (MES-REG-009/010/011)")]
        public void Mes_Finalizar_ExigeObrigatorios()
        {
            var ordem = new MesOrdem(Guid.NewGuid(), TenantId, UserId);
            ordem.AdicionarItem(Guid.NewGuid(), 5m, UserId);
            ordem.SubmeterParaAnalise(UserId);
            ordem.Aprovar(UserId);

            ordem.Finalizar(default, Guid.Empty, 0m, UserId, exigirEstruturaAtiva: false);
            Assert.False(ordem.IsValid);
            Assert.NotEqual(EStatusOrdemMes.Finalizado, ordem.Status);
        }

        [Fact(DisplayName = "MES | Finalização válida bloqueia edição (MES-REG-017)")]
        public void Mes_FinalizacaoValida_BloqueiaEdicao()
        {
            var ordem = new MesOrdem(Guid.NewGuid(), TenantId, UserId);
            ordem.AdicionarItem(Guid.NewGuid(), 5m, UserId);
            ordem.SubmeterParaAnalise(UserId);
            ordem.Aprovar(UserId);

            ordem.Finalizar(DateTime.UtcNow, Guid.NewGuid(), 100m, UserId, exigirEstruturaAtiva: false);
            Assert.True(ordem.IsValid);
            Assert.True(ordem.Finalizada);
            Assert.Equal(EStatusOrdemMes.Finalizado, ordem.Status);

            // MES-REG-017: ordem finalizada bloqueia inclusão de item
            ordem.AdicionarItem(Guid.NewGuid(), 2m, UserId);
            Assert.False(ordem.IsValid);
        }

        [Fact(DisplayName = "MES | Estrutura ativa exigida bloqueia finalização sem estrutura (MES-REG-012)")]
        public void Mes_Finalizar_ExigeEstruturaAtiva()
        {
            var ordem = new MesOrdem(Guid.NewGuid(), TenantId, UserId);
            ordem.AdicionarItem(Guid.NewGuid(), 5m, UserId);
            ordem.SubmeterParaAnalise(UserId);
            ordem.Aprovar(UserId);

            ordem.Finalizar(DateTime.UtcNow, Guid.NewGuid(), 100m, UserId, exigirEstruturaAtiva: true);
            Assert.False(ordem.IsValid);
        }

        [Fact(DisplayName = "MES | Quantidade efetiva desconta desperdício (MES-REG-021)")]
        public void Mes_QuantidadeEfetiva_DescontaDesperdicio()
        {
            var ordem = new MesOrdem(Guid.NewGuid(), TenantId, UserId);
            ordem.AdicionarItem(Guid.NewGuid(), 10m, UserId);
            ordem.SubmeterParaAnalise(UserId);
            ordem.Aprovar(UserId);
            ordem.Finalizar(DateTime.UtcNow, Guid.NewGuid(), 100m, UserId, exigirEstruturaAtiva: false, desperdicioUnidades: 3m);
            Assert.Equal(7m, ordem.CalcularQuantidadeEfetiva(10m));
        }

        [Fact(DisplayName = "MES | Finalização gera entrada de produto acabado e consumo coordenado (MES-REG-014/015/016)")]
        public async Task Mes_Finalizar_GeraMovimentosCoordenados()
        {
            var ctx = NovoContexto(nameof(Mes_Finalizar_GeraMovimentosCoordenados));

            // estrutura BOM ativa com dois componentes
            var estrutura = new BomEstrutura(Guid.NewGuid(), Guid.NewGuid(), "BOM-MES-1", 0m, 0m, 0m, 10m, TenantId, UserId);
            estrutura.AdicionarComponente(Guid.NewGuid(), 2m, UserId);
            estrutura.AdicionarComponente(Guid.NewGuid(), 3m, UserId);
            ctx.BomEstruturas.Add(estrutura);

            var ordem = new MesOrdem(Guid.NewGuid(), TenantId, UserId, estruturaId: estrutura.Id, produtoAcabadoId: Guid.NewGuid());
            ordem.AdicionarItem(Guid.NewGuid(), 5m, UserId);
            ordem.SubmeterParaAnalise(UserId);
            ordem.Aprovar(UserId);
            ctx.MesOrdens.Add(ordem);
            await ctx.SaveChangesAsync();

            var handler = new FinalizarMesOrdemCommandHandler(ctx, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
            var result = await handler.Handle(new FinalizarMesOrdemCommand(ordem.Id, DateTime.UtcNow, Guid.NewGuid(), 200m), CancellationToken.None);

            Assert.True(result.Sucesso);
            var entradas = await ctx.MesMovimentos.Where(m => m.TipoMovimento == ETipoMovimentoMes.EntradaProdutoAcabado).ToListAsync();
            var consumos = await ctx.MesMovimentos.Where(m => m.TipoMovimento == ETipoMovimentoMes.ConsumoMaterial).ToListAsync();
            Assert.Single(entradas);
            Assert.Equal(2, consumos.Count);
            // consumos vinculados à entrada (movimento pai)
            Assert.All(consumos, c => Assert.Equal(entradas[0].Id, c.MovimentoPaiId));
            Assert.Equal(2, (await ctx.MesConsumos.ToListAsync()).Count);
        }

        // ===================== PRD-MRP =====================

        [Fact(DisplayName = "MRP | Planejamento exige código e responsável (RN-MRP-002/004)")]
        public void Mrp_ExigeCodigoEResponsavel()
        {
            var semCodigo = new MrpPlanejamento("", Guid.NewGuid(), TenantId, UserId);
            Assert.False(semCodigo.IsValid);

            var semResp = new MrpPlanejamento("MRP-1", Guid.Empty, TenantId, UserId);
            Assert.False(semResp.IsValid);
        }

        [Fact(DisplayName = "MRP | Workflow Rascunho→EmAnalise→Ativo")]
        public void Mrp_Workflow_AteAtivo()
        {
            var p = new MrpPlanejamento("MRP-2", Guid.NewGuid(), TenantId, UserId);
            Assert.Equal(EStatusWorkflowProducao.Rascunho, p.Status);
            p.SubmeterParaAnalise(UserId);
            Assert.Equal(EStatusWorkflowProducao.EmAnalise, p.Status);
            p.Aprovar(UserId);
            Assert.Equal(EStatusWorkflowProducao.Ativo, p.Status);
        }

        [Fact(DisplayName = "MRP | Rejeitar sem motivo é bloqueado (RN-MRP-007)")]
        public void Mrp_RejeitarSemMotivo_Bloqueia()
        {
            var p = new MrpPlanejamento("MRP-3", Guid.NewGuid(), TenantId, UserId);
            p.SubmeterParaAnalise(UserId);
            p.Rejeitar("", UserId);
            Assert.False(p.IsValid);
        }

        // ===================== PRD-ESC =====================

        [Fact(DisplayName = "ESC | Programação exige código e responsável (ESC-REG-002/004)")]
        public void Esc_ExigeCodigoEResponsavel()
        {
            var semCodigo = new EscProgramacao("", Guid.NewGuid(), TenantId, UserId);
            Assert.False(semCodigo.IsValid);

            var semResp = new EscProgramacao("ESC-1", Guid.Empty, TenantId, UserId);
            Assert.False(semResp.IsValid);
        }

        [Fact(DisplayName = "ESC | Workflow Rascunho→EmAnalise→Ativo→Inativo→Ativo→Encerrado")]
        public void Esc_Workflow_Completo()
        {
            var p = new EscProgramacao("ESC-2", Guid.NewGuid(), TenantId, UserId);
            p.SubmeterParaAnalise(UserId);
            Assert.Equal(EStatusWorkflowProducao.EmAnalise, p.Status);
            p.Aprovar(UserId);
            Assert.Equal(EStatusWorkflowProducao.Ativo, p.Status);
            p.Inativar(UserId);
            Assert.Equal(EStatusWorkflowProducao.Inativo, p.Status);
            p.Reativar(UserId);
            Assert.Equal(EStatusWorkflowProducao.Ativo, p.Status);
            p.Encerrar(UserId);
            Assert.Equal(EStatusWorkflowProducao.Encerrado, p.Status);
        }

        [Fact(DisplayName = "ESC | Operação com minutos fora de 0-59 é inválida (ESC-REG-018)")]
        public void Esc_Operacao_MinutosInvalidos()
        {
            var op = new EscOperacao(Guid.NewGuid(), TenantId, UserId, minutosPrevistos: 75);
            Assert.False(op.IsValid);
        }

        [Fact(DisplayName = "ESC | Anexo exige arquivo controlado (ESC-REG-022)")]
        public void Esc_Anexo_ExigeArquivo()
        {
            var anexo = new EscAnexo(Guid.NewGuid(), Guid.Empty, UserId, TenantId, UserId);
            Assert.False(anexo.IsValid);
        }

        // ===================== Helpers =====================

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => _userId;
            public string? GetUserEmail() => $"{_userId}@test.local";
        }
    }
}
