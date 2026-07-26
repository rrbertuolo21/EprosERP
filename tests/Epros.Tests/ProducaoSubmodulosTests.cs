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
    /// Testes das regras-chave dos submódulos de Produção portados: PRD-BOM, PRD-CST, PRD-PLN, PRD-GOS, PRD-EST.
    /// </summary>
    public class ProducaoSubmodulosTests
    {
        private const string TenantId = "tenant-prd-001";
        private const string UserId = "user-prd-001";

        private static ContextProducao NovoContexto(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextProducao>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ContextProducao(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        // ===================== PRD-BOM =====================

        [Fact(DisplayName = "BOM | Estrutura válida inicia em Rascunho")]
        public void Bom_EstruturaValida_IniciaRascunho()
        {
            var estrutura = new BomEstrutura(Guid.NewGuid(), Guid.NewGuid(), "BOM-1", 0m, 0m, 0m, 10m, TenantId, UserId);
            Assert.True(estrutura.IsValid);
            Assert.Equal(EStatusWorkflowProducao.Rascunho, estrutura.Status);
        }

        [Fact(DisplayName = "BOM | Submeter sem componente é bloqueado (BOM-REG-005)")]
        public void Bom_SubmeterSemComponente_Bloqueia()
        {
            var estrutura = new BomEstrutura(Guid.NewGuid(), Guid.NewGuid(), null, 0m, 0m, 0m, 10m, TenantId, UserId);
            estrutura.SubmeterParaAnalise(UserId);
            Assert.False(estrutura.IsValid);
            Assert.Equal(EStatusWorkflowProducao.Rascunho, estrutura.Status);
        }

        [Fact(DisplayName = "BOM | Workflow Rascunho→EmAnalise→Ativo com componente")]
        public void Bom_Workflow_AteAtivo()
        {
            var estrutura = new BomEstrutura(Guid.NewGuid(), Guid.NewGuid(), null, 0m, 0m, 0m, 10m, TenantId, UserId);
            estrutura.AdicionarComponente(Guid.NewGuid(), 2m, UserId);
            estrutura.SubmeterParaAnalise(UserId);
            Assert.True(estrutura.IsValid);
            Assert.Equal(EStatusWorkflowProducao.EmAnalise, estrutura.Status);
            estrutura.Aprovar(UserId);
            Assert.Equal(EStatusWorkflowProducao.Ativo, estrutura.Status);
        }

        [Fact(DisplayName = "BOM | Rejeitar sem motivo é bloqueado (BOM-REG-027)")]
        public void Bom_RejeitarSemMotivo_Bloqueia()
        {
            var estrutura = new BomEstrutura(Guid.NewGuid(), Guid.NewGuid(), null, 0m, 0m, 0m, 10m, TenantId, UserId);
            estrutura.AdicionarComponente(Guid.NewGuid(), 2m, UserId);
            estrutura.SubmeterParaAnalise(UserId);
            estrutura.Rejeitar("", UserId);
            Assert.False(estrutura.IsValid);
        }

        [Fact(DisplayName = "BOM | Quantidade de componente <= 0 é bloqueada (BOM-REG-007)")]
        public void Bom_ComponenteQuantidadeInvalida_Bloqueia()
        {
            var componente = new BomComponente(Guid.NewGuid(), Guid.NewGuid(), 0m, TenantId, UserId);
            Assert.False(componente.IsValid);
        }

        [Fact(DisplayName = "BOM | Desperdício não pode consumir toda a quantidade (BOM-REG-008)")]
        public void Bom_DesperdicioTotal_Bloqueia()
        {
            var componente = new BomComponente(Guid.NewGuid(), Guid.NewGuid(), 5m, TenantId, UserId, percentualDesperdicio: 100m);
            Assert.False(componente.IsValid);
        }

        [Fact(DisplayName = "BOM | Custo da linha = custo unitário * qtd * multiplicador (BOM-REG-012)")]
        public void Bom_CustoLinha_Calculado()
        {
            var componente = new BomComponente(Guid.NewGuid(), Guid.NewGuid(), 3m, TenantId, UserId,
                multiplicadorUnidade: 2m, custoUnitarioComImpostos: 10m);
            Assert.True(componente.IsValid);
            Assert.Equal(60m, componente.CustoLinha);
        }

        [Fact(DisplayName = "BOM | Instrução com código duplicado é bloqueada (BOM-REG-020)")]
        public async Task Bom_InstrucaoCodigoDuplicado_Bloqueia()
        {
            using var context = NovoContexto("db_bom_instrucao_dup");
            var tp = new TestTenantProvider(TenantId);
            var cu = new TestCurrentUser(UserId);
            var handler = new CriarBomInstrucaoCommandHandler(context, tp, cu);

            var r1 = await handler.Handle(new CriarBomInstrucaoCommand("INS-1", "Instrução 1"), CancellationToken.None);
            var r2 = await handler.Handle(new CriarBomInstrucaoCommand("INS-1", "Instrução 1 dup"), CancellationToken.None);

            Assert.True(r1.Sucesso);
            Assert.False(r2.Sucesso);
        }

        // ===================== PRD-CST =====================

        [Fact(DisplayName = "CST | Desvio total = realizado - previsto")]
        public void Cst_DesvioTotal_Calculado()
        {
            var custo = new CustoProducao("CST-1", Guid.NewGuid(), TenantId, UserId,
                custoTotalPrevisto: 100m, custoTotalRealizado: 130m);
            Assert.True(custo.IsValid);
            Assert.Equal(30m, custo.DesvioTotal);
        }

        [Fact(DisplayName = "CST | Referência calcula desvio próprio")]
        public void Cst_ReferenciaDesvio_Calculado()
        {
            var custo = new CustoProducao("CST-2", Guid.NewGuid(), TenantId, UserId);
            custo.AdicionarReferencia("Material", UserId, custoPrevisto: 50m, custoRealizado: 40m);
            Assert.True(custo.IsValid);
            Assert.Single(custo.Referencias);
            Assert.Equal(-10m, custo.Referencias.First().Desvio);
        }

        [Fact(DisplayName = "CST | Aprovar exige estar EmAnalise")]
        public void Cst_AprovarForaDeAnalise_Bloqueia()
        {
            var custo = new CustoProducao("CST-3", Guid.NewGuid(), TenantId, UserId);
            custo.Aprovar(UserId); // ainda Rascunho
            Assert.False(custo.IsValid);
            Assert.Equal(EStatusWorkflowProducao.Rascunho, custo.Status);
        }

        // ===================== PRD-PLN =====================

        [Fact(DisplayName = "PLN | Planejamento válido inicia em Rascunho e sobe workflow")]
        public void Pln_Workflow_Ok()
        {
            var plano = new PlanejamentoProducao("PLN-1", Guid.NewGuid(), TenantId, UserId);
            Assert.True(plano.IsValid);
            Assert.Equal(EStatusWorkflowProducao.Rascunho, plano.Status);
            plano.SubmeterParaAnalise(UserId);
            plano.Aprovar(UserId);
            Assert.Equal(EStatusWorkflowProducao.Ativo, plano.Status);
        }

        [Fact(DisplayName = "PLN | Código obrigatório")]
        public void Pln_SemCodigo_Bloqueia()
        {
            var plano = new PlanejamentoProducao("", Guid.NewGuid(), TenantId, UserId);
            Assert.False(plano.IsValid);
        }

        // ===================== PRD-GOS =====================

        [Fact(DisplayName = "GOS | Ficha inicia Aguardando pagamento e transita até Concluído")]
        public void Gos_Situacao_Transicoes()
        {
            var ficha = new FichaProducao(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                ELogomarcaFichaProducao.Bordada, 1, 1, TenantId, UserId);
            Assert.True(ficha.IsValid);
            Assert.Equal(ESituacaoFichaProducao.AguardandoPagamento, ficha.Situacao);

            ficha.IniciarProducao(UserId);
            Assert.Equal(ESituacaoFichaProducao.EmProducao, ficha.Situacao);

            ficha.Concluir(UserId);
            Assert.Equal(ESituacaoFichaProducao.Concluido, ficha.Situacao);
            Assert.NotNull(ficha.Saida);
        }

        [Fact(DisplayName = "GOS | Concluir antes de iniciar é bloqueado")]
        public void Gos_ConcluirSemIniciar_Bloqueia()
        {
            var ficha = new FichaProducao(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                ELogomarcaFichaProducao.SemLogo, 0, 0, TenantId, UserId);
            ficha.Concluir(UserId);
            Assert.False(ficha.IsValid);
            Assert.Equal(ESituacaoFichaProducao.AguardandoPagamento, ficha.Situacao);
        }

        [Fact(DisplayName = "GOS | Venda obrigatória")]
        public void Gos_SemVenda_Bloqueia()
        {
            var ficha = new FichaProducao(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(),
                ELogomarcaFichaProducao.SemLogo, 0, 0, TenantId, UserId);
            Assert.False(ficha.IsValid);
        }

        // ===================== PRD-EST =====================

        [Fact(DisplayName = "EST | Converter exige estimativa Ativa (EST §7.4)")]
        public void Est_ConverterForaDeAtivo_Bloqueia()
        {
            var estimativa = new Estimativa("EST-1", Guid.NewGuid(), TenantId, UserId);
            estimativa.ConverterParaPlanejamento(Guid.NewGuid(), UserId); // ainda Rascunho
            Assert.False(estimativa.IsValid);
            Assert.Null(estimativa.PlanejamentoOrigemId);
        }

        [Fact(DisplayName = "EST | Fluxo aprovar e converter para planejamento")]
        public void Est_AprovarEConverter_Ok()
        {
            var estimativa = new Estimativa("EST-2", Guid.NewGuid(), TenantId, UserId);
            estimativa.AdicionarComponente("Material", UserId, custoPrevisto: 100m);
            estimativa.SubmeterParaAnalise(UserId);
            estimativa.Aprovar(UserId);
            Assert.Equal(EStatusWorkflowProducao.Ativo, estimativa.Status);

            var planId = Guid.NewGuid();
            estimativa.ConverterParaPlanejamento(planId, UserId);
            Assert.True(estimativa.IsValid);
            Assert.Equal(planId, estimativa.PlanejamentoOrigemId);
        }

        private sealed class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        private sealed class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
