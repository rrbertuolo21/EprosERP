using System;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes de domínio dos submódulos pendentes de Vendas (VEN-GAR, VEN-PDM, VEN-GCV, VEN-LDS,
    /// VEN-PCL, VEN-ECM). Fonte: EFs V1 de cada submódulo. Cobrem as regras-chave de cada agregado.
    /// </summary>
    public class VendasSubmodulosPendentesTests
    {
        private const string TenantId = "tenant-ven-001";
        private const string UserId = "user-ven-001";

        // ==================== Garantias (VEN-GAR) ====================

        [Fact(DisplayName = "GarantiaPolitica | Duração > 0 é válida (GAR-002/GAR-006)")]
        public void Garantia_DuracaoValida_DeveSerValida()
        {
            var p = new GarantiaPolitica("Garantia 12m", null, 12, EGarantiaTipoDuracao.Meses, TenantId, UserId);
            Assert.True(p.IsValid);
        }

        [Fact(DisplayName = "GarantiaPolitica | Duração zero é inválida (GAR-006)")]
        public void Garantia_DuracaoZero_DeveSerInvalida()
        {
            var p = new GarantiaPolitica("X", null, 0, EGarantiaTipoDuracao.Meses, TenantId, UserId);
            Assert.False(p.IsValid);
        }

        [Fact(DisplayName = "GarantiaCobertura | Com data de origem e duração calcula vencimento vigente (GAR-016)")]
        public void Cobertura_ComDados_CalculaVencimentoVigente()
        {
            var origem = DateTime.UtcNow.Date.AddDays(-10);
            var c = new GarantiaCobertura(Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, origem, null, 12, EGarantiaTipoDuracao.Meses, TenantId, UserId);
            Assert.True(c.IsValid);
            Assert.Equal(EGarantiaSituacaoCobertura.Vigente, c.Situacao);
            Assert.Equal(origem.AddMonths(12), c.DataVencimento);
        }

        [Fact(DisplayName = "GarantiaCobertura | Sem data de origem fica Indeterminada (GAR-017)")]
        public void Cobertura_SemDataOrigem_FicaIndeterminada()
        {
            var c = new GarantiaCobertura(Guid.NewGuid(), null, null, null, null, null, null, null, 12, EGarantiaTipoDuracao.Meses, TenantId, UserId);
            Assert.Equal(EGarantiaSituacaoCobertura.Indeterminada, c.Situacao);
            Assert.Null(c.DataVencimento);
        }

        [Fact(DisplayName = "GarantiaCobertura | Data de origem antiga fica Vencida (GAR-016)")]
        public void Cobertura_OrigemAntiga_FicaVencida()
        {
            var origem = DateTime.UtcNow.Date.AddYears(-3);
            var c = new GarantiaCobertura(Guid.NewGuid(), null, null, null, null, null, origem, null, 12, EGarantiaTipoDuracao.Meses, TenantId, UserId);
            Assert.Equal(EGarantiaSituacaoCobertura.Vencida, c.Situacao);
        }

        // ---- Correção INV-01: duas dimensões (tempo + uso), vence o que ocorrer primeiro ----

        [Fact(DisplayName = "GarantiaPolitica | Política só por uso (sem tempo) é válida (GAR-003)")]
        public void Garantia_SoUso_DeveSerValida()
        {
            var p = new GarantiaPolitica("100 mil km", null, 0, EGarantiaTipoDuracao.Meses, TenantId, UserId, 100000m, EGarantiaUnidadeUso.Km);
            Assert.True(p.IsValid);
            Assert.False(p.TemDimensaoTempo);
            Assert.True(p.TemDimensaoUso);
        }

        [Fact(DisplayName = "GarantiaPolitica | Sem nenhuma dimensão (tempo=0, sem uso) é inválida (GAR-003)")]
        public void Garantia_SemDimensao_DeveSerInvalida()
        {
            var p = new GarantiaPolitica("X", null, 0, EGarantiaTipoDuracao.Meses, TenantId, UserId, null, EGarantiaUnidadeUso.Nenhuma);
            Assert.False(p.IsValid);
        }

        [Fact(DisplayName = "GarantiaCobertura | Uso na origem >= limite vence por uso mesmo com tempo vigente (GAR-016)")]
        public void Cobertura_UsoAtingido_VencePorUso()
        {
            var origem = DateTime.UtcNow.Date.AddDays(-1); // tempo vigente
            var c = new GarantiaCobertura(Guid.NewGuid(), null, null, null, null, null, origem, null, 12, EGarantiaTipoDuracao.Meses, TenantId, UserId,
                usoOrigem: 90000m, limiteUsoPolitica: 10000m, unidadeUsoPolitica: EGarantiaUnidadeUso.Km);
            Assert.Equal(100000m, c.UsoVencimento); // 90000 + 10000
            c.RegistrarUso(100000m, UserId); // atingiu o limite
            Assert.Equal(EGarantiaSituacaoCobertura.Vencida, c.Situacao);
        }

        [Fact(DisplayName = "GarantiaCobertura | Uso abaixo do limite e tempo vigente permanece Vigente (GAR-016)")]
        public void Cobertura_UsoAbaixo_PermaneceVigente()
        {
            var origem = DateTime.UtcNow.Date.AddDays(-1);
            var c = new GarantiaCobertura(Guid.NewGuid(), null, null, null, null, null, origem, null, 12, EGarantiaTipoDuracao.Meses, TenantId, UserId,
                usoOrigem: 0m, limiteUsoPolitica: 100000m, unidadeUsoPolitica: EGarantiaUnidadeUso.Km);
            c.RegistrarUso(5000m, UserId);
            Assert.Equal(EGarantiaSituacaoCobertura.Vigente, c.Situacao);
        }

        [Fact(DisplayName = "GarantiaCobertura | Tempo vencido vence mesmo com uso disponível (o que vier primeiro) (GAR-016)")]
        public void Cobertura_TempoVencido_VencePorTempo()
        {
            var origem = DateTime.UtcNow.Date.AddYears(-3); // tempo já vencido
            var c = new GarantiaCobertura(Guid.NewGuid(), null, null, null, null, null, origem, null, 12, EGarantiaTipoDuracao.Meses, TenantId, UserId,
                usoOrigem: 0m, limiteUsoPolitica: 100000m, unidadeUsoPolitica: EGarantiaUnidadeUso.Km);
            c.RegistrarUso(10m, UserId); // uso baixíssimo, mas tempo venceu
            Assert.Equal(EGarantiaSituacaoCobertura.Vencida, c.Situacao);
        }

        // ==================== Planejamento de Demanda (VEN-PDM) ====================

        [Fact(DisplayName = "DemandaPrevisao | Início antes do fim é válida (§16)")]
        public void Previsao_PeriodoValido_DeveSerValida()
        {
            var p = new DemandaPrevisao(null, "P-2026", "Previsão 2026", new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), null, TenantId, UserId);
            Assert.True(p.IsValid);
            Assert.Equal(EDemandaStatus.Rascunho, p.Status);
        }

        [Fact(DisplayName = "DemandaPrevisao | Fim antes do início é inválida (§16)")]
        public void Previsao_PeriodoInvertido_DeveSerInvalida()
        {
            var p = new DemandaPrevisao(null, null, "X", new DateTime(2026, 12, 31), new DateTime(2026, 1, 1), null, TenantId, UserId);
            Assert.False(p.IsValid);
        }

        [Fact(DisplayName = "DemandaPrevisao | Aprovar muda status para Aprovado (§8)")]
        public void Previsao_Aprovar_MudaStatus()
        {
            var p = new DemandaPrevisao(null, null, "X", new DateTime(2026, 1, 1), new DateTime(2026, 6, 1), null, TenantId, UserId);
            p.Aprovar(null, null, UserId);
            Assert.Equal(EDemandaStatus.Aprovado, p.Status);
        }

        [Fact(DisplayName = "DemandaItem | Produto e quantidade prevista obrigatórios (§16)")]
        public void DemandaItem_SemProduto_DeveSerInvalido()
        {
            var item = new DemandaItem(Guid.NewGuid(), null, null, Guid.Empty, "2026-01", null, 10, null, null, TenantId, UserId);
            Assert.False(item.IsValid);
        }

        // ==================== Contratos de Venda (VEN-GCV) ====================

        [Fact(DisplayName = "Contrato | Data fim posterior à início é válido (GCV-006)")]
        public void Contrato_DatasValidas_DeveSerValido()
        {
            var c = NovoContrato(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
            Assert.True(c.IsValid);
            Assert.Equal(EContratoTipoOrigem.Contrato, c.TipoOrigem); // GCV-008
            Assert.False(string.IsNullOrEmpty(c.IdentificadorPublico)); // GCV-039
        }

        [Fact(DisplayName = "Contrato | Data fim anterior à início é inválido (GCV-006)")]
        public void Contrato_DatasInvertidas_DeveSerInvalido()
        {
            var c = NovoContrato(new DateTime(2026, 12, 31), new DateTime(2026, 1, 1));
            Assert.False(c.IsValid);
        }

        [Fact(DisplayName = "Contrato | Assinaturas de empresa e cliente ativam o contrato (GCV-034)")]
        public void Contrato_AmbasAssinaturas_AtivaContrato()
        {
            var c = NovoContrato(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
            c.RegistrarAssinatura(EContratoParteAssinatura.Empresa, UserId);
            Assert.NotEqual(EContratoStatus.Ativo, c.Status);
            c.RegistrarAssinatura(EContratoParteAssinatura.Cliente, UserId);
            Assert.Equal(EContratoStatus.Ativo, c.Status);
        }

        [Fact(DisplayName = "Contrato | Publicação agendada no passado é rejeitada (GCV-038)")]
        public void Contrato_PublicacaoAgendadaPassado_Rejeita()
        {
            var c = NovoContrato(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));
            c.Publicar(DateTime.UtcNow.AddDays(-1), false, UserId);
            Assert.False(c.IsValid);
        }

        [Fact(DisplayName = "ContratoAssinatura | Conteúdo menor que 10 caracteres é inválido (GCV-032)")]
        public void Assinatura_ConteudoCurto_DeveSerInvalida()
        {
            var a = new ContratoAssinatura(Guid.NewGuid(), Guid.NewGuid(), EContratoParteAssinatura.Cliente, EContratoTipoAssinatura.Desenhada, "abc", null, TenantId, UserId);
            Assert.False(a.IsValid);
        }

        [Fact(DisplayName = "ContratoRenovacao | Data inicial no passado é inválida (GCV-026)")]
        public void Renovacao_DataInicialPassado_DeveSerInvalida()
        {
            var r = new ContratoRenovacao(Guid.NewGuid(), DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddMonths(6), 100m, null, EContratoRenovacaoStatus.Pendente, Guid.NewGuid(), TenantId, UserId);
            Assert.False(r.IsValid);
        }

        [Fact(DisplayName = "ContratoConfiguracao | Prefixo até 10 caracteres é válido (GCV-010)")]
        public void ConfigContrato_PrefixoValido_DeveSerValida()
        {
            var cfg = new ContratoConfiguracao("CT", null, null, TenantId, UserId);
            Assert.True(cfg.IsValid);
        }

        private static Contrato NovoContrato(DateTime inicio, DateTime fim) =>
            new Contrato("Assunto", null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1000m, inicio, fim,
                null, null, null, null, null, null, null, false, null, Guid.NewGuid(), Guid.NewGuid(), TenantId, UserId);

        // ==================== Logística de Saída (VEN-LDS) ====================

        [Fact(DisplayName = "Expedicao | Pedido obrigatório (§19.1)")]
        public void Expedicao_SemPedido_DeveSerInvalida()
        {
            var e = new Expedicao(Guid.NewGuid(), Guid.Empty, null, null, null, null, null, TenantId, UserId);
            Assert.False(e.IsValid);
        }

        [Fact(DisplayName = "Expedicao | Confirmar muda status e grava data (§19)")]
        public void Expedicao_Confirmar_MudaStatus()
        {
            var e = new Expedicao(Guid.NewGuid(), Guid.NewGuid(), null, null, null, null, null, TenantId, UserId);
            e.Confirmar(UserId);
            Assert.Equal(EExpedicaoStatus.Confirmado, e.Status);
            Assert.NotNull(e.DataConfirmacao);
        }

        [Fact(DisplayName = "ExpedicaoItemEntrega | Entregue não pode ultrapassar vendida (§14.5)")]
        public void ItemEntrega_EntregueMaiorQueVendida_DeveSerInvalido()
        {
            var i = new ExpedicaoItemEntrega(Guid.NewGuid(), Guid.NewGuid(), null, 10, 15, null, TenantId, UserId);
            Assert.False(i.IsValid);
        }

        [Fact(DisplayName = "ExpedicaoItemEntrega | Entrega parcial calcula saldo (§14.2)")]
        public void ItemEntrega_Parcial_CalculaSaldo()
        {
            var i = new ExpedicaoItemEntrega(Guid.NewGuid(), Guid.NewGuid(), null, 10, 4, null, TenantId, UserId);
            Assert.True(i.IsValid);
            Assert.Equal(6, i.SaldoEntrega);
        }

        // ==================== Portal do Cliente (VEN-PCL) ====================

        [Fact(DisplayName = "PortalUsuarioCliente | Cliente e e-mail válidos criam usuário ativo (§18)")]
        public void PortalUsuario_Valido_DeveSerAtivo()
        {
            var u = new PortalUsuarioCliente(Guid.NewGuid(), "Cliente A", "a@cliente.com", null, false, null, TenantId, UserId);
            Assert.True(u.IsValid);
            Assert.Equal(EPortalUsuarioStatus.Ativo, u.Status);
            Assert.True(u.PodeAcessar());
        }

        [Fact(DisplayName = "PortalUsuarioCliente | Bloqueado não pode acessar (§18)")]
        public void PortalUsuario_Bloqueado_NaoAcessa()
        {
            var u = new PortalUsuarioCliente(Guid.NewGuid(), "Cliente A", "a@cliente.com", null, false, null, TenantId, UserId);
            u.Bloquear(UserId);
            Assert.False(u.PodeAcessar());
        }

        [Fact(DisplayName = "PortalSolicitacao | Sem origem (cliente/usuário) é inválida (§18)")]
        public void PortalSolicitacao_SemOrigem_DeveSerInvalida()
        {
            var s = new PortalSolicitacao(null, null, null, "Assunto", "Descrição", null, TenantId, UserId);
            Assert.False(s.IsValid);
        }

        [Fact(DisplayName = "PortalSolicitacao | Com cliente e assunto é válida e inicia Aberta (§16.4)")]
        public void PortalSolicitacao_Valida_IniciaAberta()
        {
            var s = new PortalSolicitacao(Guid.NewGuid(), null, null, "Assunto", null, null, TenantId, UserId);
            Assert.True(s.IsValid);
            Assert.Equal(EPortalSolicitacaoStatus.Aberta, s.Status);
        }

        // ==================== Comércio Eletrônico (VEN-ECM) ====================

        [Fact(DisplayName = "EcoPedido | Total = itens + frete - desconto (ECO-017)")]
        public void EcoPedido_Total_CalculadoCorretamente()
        {
            var p = new EcoPedido(Guid.NewGuid(), Guid.NewGuid(), 100m, 20m, 15m, "PAC", null, null, TenantId, UserId);
            Assert.True(p.IsValid);
            Assert.Equal(105m, p.ValorTotal);
            Assert.Equal(EEcoStatusPagamento.CriadoSemPagamento, p.StatusPagamentoCodigo); // ECO-016
        }

        [Fact(DisplayName = "EcoPedido | Confirmar pagamento só após início muda para Confirmado (ECO-024)")]
        public void EcoPedido_ConfirmarPagamento_MudaStatus()
        {
            var p = new EcoPedido(Guid.NewGuid(), Guid.NewGuid(), 100m, 0m, 0m, null, null, null, TenantId, UserId);
            p.IniciarPagamentoPix("tx1", "qr", "qr64", UserId);
            Assert.Equal(EEcoStatusPagamento.PagamentoIniciado, p.StatusPagamentoCodigo);
            p.ConfirmarPagamento("approved", null, UserId);
            Assert.Equal(EEcoStatusPagamento.PagamentoConfirmado, p.StatusPagamentoCodigo);
            Assert.Equal(EEcoStatusPreparacao.Aprovado, p.StatusPreparacaoCodigo);
        }

        [Fact(DisplayName = "EcoCupom | Percentual limita desconto ao total (ECO-029/ECO-030)")]
        public void EcoCupom_Percentual_LimitaAoTotal()
        {
            var cupom = new EcoCupom("PROMO1", 10m, EEcoTipoCupom.Percentual, TenantId, UserId);
            Assert.Equal(10m, cupom.CalcularDesconto(100m)); // 10% de 100
            var cupomFixo = new EcoCupom("PROMO2", 500m, EEcoTipoCupom.Fixo, TenantId, UserId);
            Assert.Equal(100m, cupomFixo.CalcularDesconto(100m)); // ECO-029: desconto não maior que o total
        }

        [Fact(DisplayName = "EcoPedidoItem | Quantidade zero é inválida (ECO-018)")]
        public void EcoPedidoItem_QuantidadeZero_DeveSerInvalido()
        {
            var item = new EcoPedidoItem(Guid.NewGuid(), Guid.NewGuid(), 0, null, 50m, TenantId, UserId);
            Assert.False(item.IsValid);
        }

        [Fact(DisplayName = "EcoCliente | E-mail inválido reprova cadastro (ECO-014)")]
        public void EcoCliente_EmailInvalido_DeveSerInvalido()
        {
            var c = new EcoCliente("Fulano", null, null, null, "email-invalido", null, "hash123", TenantId, UserId);
            Assert.False(c.IsValid);
        }
    }
}
