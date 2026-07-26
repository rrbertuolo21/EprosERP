using System;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Domain.Enums;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes das regras-chave dos submódulos de evolução do Financeiro
    /// (FIN-CAM, FIN-AFX, FIN-CMG, FIN-GCF, FIN-SBF, FIN-CON, FIN-PO, FIN-TS).
    /// </summary>
    public class FinanceiroSubmodulosEvolucaoTests
    {
        private const string TenantId = "tenant-fin-evo-001";
        private const string UserId = "user-fin-evo-001";

        // ===== FIN-CAM: Câmbio e Risco =====
        [Fact(DisplayName = "Moeda | Sem código ISO deve ser inválida")]
        public void Moeda_SemCodigoIso_DeveSerInvalida()
        {
            var moeda = new Moeda("", "$", "Dólar", TenantId, UserId);
            Assert.False(moeda.IsValid);
        }

        [Fact(DisplayName = "TaxaCambio | Taxa zero deve ser inválida")]
        public void TaxaCambio_TaxaZero_DeveSerInvalida()
        {
            var taxa = new TaxaCambio(Guid.NewGuid(), DateTime.UtcNow, 0m, 0m, EOrigemTaxaCambio.Manual, null, TenantId, UserId);
            Assert.False(taxa.IsValid);
        }

        [Fact(DisplayName = "ExposicaoCambial | Encerrada não pode ser hedgeada")]
        public void ExposicaoCambial_Encerrada_NaoPodeHedgear()
        {
            var exp = new ExposicaoCambial(Guid.NewGuid(), 1000m, DateTime.UtcNow, null, null, null, null, null, TenantId, UserId);
            exp.Encerrar(UserId);
            exp.MarcarHedgeada(UserId);
            Assert.False(exp.IsValid);
        }

        [Fact(DisplayName = "ReavaliacaoTitulo | Totais somam itens e variação")]
        public void ReavaliacaoTitulo_AdicionarItens_TotalizaVariacao()
        {
            var reav = new ReavaliacaoTitulo(DateTime.UtcNow, "teste", TenantId, UserId);
            reav.AdicionarItem(Guid.NewGuid(), "contas a pagar", Guid.NewGuid(), Guid.NewGuid(), 100m, 130m, TenantId, UserId);
            reav.AdicionarItem(Guid.NewGuid(), "contas a receber", Guid.NewGuid(), Guid.NewGuid(), 200m, 180m, TenantId, UserId);
            Assert.Equal(300m, reav.TotalValorOriginal);
            Assert.Equal(310m, reav.TotalValorReavaliado);
            Assert.Equal(10m, reav.TotalVariacao);
        }

        [Fact(DisplayName = "ReavaliacaoTitulo | Contabilizada não pode ser cancelada")]
        public void ReavaliacaoTitulo_Contabilizada_NaoCancela()
        {
            var reav = new ReavaliacaoTitulo(DateTime.UtcNow, null, TenantId, UserId);
            reav.Aprovar(UserId);
            reav.Contabilizar(UserId);
            reav.Cancelar(UserId);
            Assert.False(reav.IsValid);
        }

        // ===== FIN-AFX: Ativos Fixos =====
        [Fact(DisplayName = "AtivoFixo | Depreciação até zerar marca TotalmenteDepreciado")]
        public void AtivoFixo_DepreciacaoAteZero_MarcaTotalmenteDepreciado()
        {
            var ativo = new AtivoFixo("Notebook", 1000m, DateTime.UtcNow, null, null, null, null, null, null,
                null, null, null, null, null, null, 1000m, null, true, ETipoDepreciacaoAtivo.Linear, 12m, 1m, TenantId, UserId);
            ativo.AplicarDepreciacao(1000m, DateTime.UtcNow, UserId);
            Assert.Equal(EStatusAtivoFixo.TotalmenteDepreciado, ativo.Status);
            Assert.Equal(0m, ativo.ValorAtualizado);
        }

        [Fact(DisplayName = "AtivoFixo | Baixado não pode ser depreciado")]
        public void AtivoFixo_Baixado_NaoDepreciavel()
        {
            var ativo = new AtivoFixo("Máquina", 5000m, DateTime.UtcNow, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, true, ETipoDepreciacaoAtivo.Linear, 10m, null, TenantId, UserId);
            ativo.Baixar(DateTime.UtcNow, 1000m, UserId);
            ativo.AplicarDepreciacao(100m, DateTime.UtcNow, UserId);
            Assert.False(ativo.IsValid);
        }

        // ===== FIN-CMG: Contabilidade Gerencial =====
        [Fact(DisplayName = "AlocacaoCentroCusto | Percentual acima de 100 é inválido")]
        public void AlocacaoCentroCusto_PercentualAcima100_Invalido()
        {
            var aloc = new AlocacaoCentroCusto(Guid.NewGuid(), ETipoTituloAlocacao.ContasAPagar, Guid.NewGuid(), 150m, null, TenantId, UserId);
            Assert.False(aloc.IsValid);
        }

        [Fact(DisplayName = "CentroCusto | Pai igual ao próprio Id é inválido")]
        public void CentroCusto_PaiIgualProprio_Invalido()
        {
            var cc = new CentroCusto("CC1", "Administrativo", null, TenantId, UserId);
            cc.Alterar("CC1", "Administrativo", cc.Id, UserId);
            Assert.False(cc.IsValid);
        }

        // ===== FIN-GCF: Contratos Financeiros =====
        [Fact(DisplayName = "ContratoFinanceiro | Ativo pode suspender e reativar")]
        public void ContratoFinanceiro_SuspenderReativar_VoltaAtivo()
        {
            var c = new ContratoFinanceiro(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, TenantId, UserId);
            c.Suspender(UserId);
            Assert.Equal(EStatusContratoFinanceiro.Suspenso, c.Status);
            c.Reativar(UserId);
            Assert.Equal(EStatusContratoFinanceiro.Ativo, c.Status);
        }

        [Fact(DisplayName = "PlanoContrato | Valor zero é inválido")]
        public void PlanoContrato_ValorZero_Invalido()
        {
            var p = new PlanoContrato("Mensal", 0m, EPeriodicidadeContrato.Mensal, TenantId, UserId);
            Assert.False(p.IsValid);
        }

        // ===== FIN-SBF: Subsídios e Fundos =====
        [Fact(DisplayName = "ProgramaSubsidio | Encerrado não reabre prestação de contas")]
        public void ProgramaSubsidio_Encerrado_NaoPrestacao()
        {
            var prog = new ProgramaSubsidio("SEFAZ", 10000m, DateTime.UtcNow, null, TenantId, UserId);
            prog.Encerrar(UserId);
            prog.IniciarPrestacaoContas(UserId);
            Assert.False(prog.IsValid);
        }

        [Fact(DisplayName = "UtilizacaoSubsidio | Valor elegível zero é inválido")]
        public void UtilizacaoSubsidio_ValorZero_Invalido()
        {
            var u = new UtilizacaoSubsidio(Guid.NewGuid(), Guid.NewGuid(), 0m, TenantId, UserId);
            Assert.False(u.IsValid);
        }

        // ===== FIN-CON: Consolidação =====
        [Fact(DisplayName = "Demonstrativo | Publicar altera estado para Publicado")]
        public void Demonstrativo_Publicar_AlteraEstado()
        {
            var demo = new Demonstrativo(Guid.NewGuid(), "2026-01", TenantId, UserId);
            demo.Publicar(Guid.NewGuid(), UserId);
            Assert.Equal(EEstadoConsolidacao.Publicado, demo.Estado);
            Assert.NotNull(demo.DataPublicacao);
        }

        [Fact(DisplayName = "BalanceteConsolidado | Sem período é inválido")]
        public void BalanceteConsolidado_SemPeriodo_Invalido()
        {
            var bal = new BalanceteConsolidado(Guid.NewGuid(), "", TenantId, UserId);
            Assert.False(bal.IsValid);
        }

        // ===== FIN-PO: Planejamento e Orçamento =====
        [Fact(DisplayName = "PeriodoOrcamentario | Data fim antes do início é inválido")]
        public void PeriodoOrcamentario_DataFimAntes_Invalido()
        {
            var p = new PeriodoOrcamentario(DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), TenantId, UserId);
            Assert.False(p.IsValid);
        }

        [Fact(DisplayName = "PeriodoOrcamentario | Ciclo rascunho→aprovado→ativo→encerrado")]
        public void PeriodoOrcamentario_Ciclo_Completo()
        {
            var p = new PeriodoOrcamentario(DateTime.UtcNow, DateTime.UtcNow.AddDays(30), TenantId, UserId);
            p.Aprovar(UserId); Assert.Equal(EStatusOrcamento.Aprovado, p.Status);
            p.Ativar(UserId); Assert.Equal(EStatusOrcamento.Ativo, p.Status);
            p.Encerrar(UserId); Assert.Equal(EStatusOrcamento.Encerrado, p.Status);
        }

        [Fact(DisplayName = "LinhaOrcamento | Realizado calcula variação valor e percentual")]
        public void LinhaOrcamento_Realizado_CalculaVariacao()
        {
            var linha = new LinhaOrcamento(Guid.NewGuid(), Guid.NewGuid(), null, "2026-01", 1000m, TenantId, UserId);
            linha.RegistrarRealizado(1200m, UserId);
            Assert.Equal(200m, linha.VariacaoValor);
            Assert.Equal(20m, linha.VariacaoPercentual);
        }

        [Fact(DisplayName = "MetaTracking | Percentual acima de 100 é inválido")]
        public void MetaTracking_PercentualAcima100_Invalido()
        {
            var t = new MetaTracking(Guid.NewGuid(), 120m, "andamento", null, TenantId, UserId);
            Assert.False(t.IsValid);
        }

        [Fact(DisplayName = "MetaContribuicao | Valor negativo é inválido")]
        public void MetaContribuicao_ValorNegativo_Invalido()
        {
            var c = new MetaContribuicao(Guid.NewGuid(), -10m, "aporte", null, TenantId, UserId);
            Assert.False(c.IsValid);
        }

        [Fact(DisplayName = "Budget | Editar fora de rascunho é bloqueado")]
        public void Budget_EditarForaRascunho_Bloqueado()
        {
            var b = new Budget(Guid.NewGuid(), "operacional", 5000m, TenantId, UserId);
            b.Aprovar(UserId);
            b.Alterar("operacional", 6000m, UserId);
            Assert.False(b.IsValid);
        }

        // ===== FIN-TS: Tesouraria =====
        [Fact(DisplayName = "ContaFinanceira | Sem nome é inválida")]
        public void ContaFinanceira_SemNome_Invalida()
        {
            var conta = new ContaFinanceira("", null, null, null, 0m, TenantId, UserId);
            Assert.False(conta.IsValid);
        }

        [Fact(DisplayName = "MovimentoFinanceiro | Sem crédito e sem débito é inválido")]
        public void MovimentoFinanceiro_SemValor_Invalido()
        {
            var mov = new MovimentoFinanceiro(null, DateTime.UtcNow, null, Guid.NewGuid(), 0m, 0m, null, null, null, null, false, TenantId, UserId);
            Assert.False(mov.IsValid);
        }

        [Fact(DisplayName = "CaixaOperacional | Fechar duas vezes é inválido")]
        public void CaixaOperacional_FecharDuasVezes_Invalido()
        {
            var caixa = new CaixaOperacional(null, Guid.NewGuid(), 100m, TenantId, UserId);
            caixa.Fechar(150m, 0m, 0m, "ok", UserId);
            caixa.Fechar(150m, 0m, 0m, "denovo", UserId);
            Assert.False(caixa.IsValid);
        }

        [Fact(DisplayName = "Cheque | Valor zero é inválido")]
        public void Cheque_ValorZero_Invalido()
        {
            var cheque = new Cheque(null, 1, 1, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 0m, null, null, TenantId, UserId);
            Assert.False(cheque.IsValid);
        }

        [Fact(DisplayName = "TransacaoContaFinanceira | Débito e crédito válidos são aceitos")]
        public void TransacaoContaFinanceira_Valida()
        {
            var tx = new TransacaoContaFinanceira(Guid.NewGuid(), 500m, ETipoTransacaoConta.Credito, "deposit",
                DateTime.UtcNow, "nota", null, null, null, null, TenantId, UserId);
            Assert.True(tx.IsValid);
        }
    }
}
