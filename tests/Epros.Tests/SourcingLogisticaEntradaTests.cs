using System;
using Xunit;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;

namespace Epros.Tests
{
    /// <summary>
    /// Testes de domínio do bloco de Sourcing (EST-SC-001 / COM-GC-001) e Logística de Entrada (EST-LDE).
    /// Cobrem invariantes das entidades e a máquina de estados da entrada.
    /// </summary>
    public class SourcingLogisticaEntradaTests
    {
        private const string TenantId = "tenant-test-001";
        private const string UserId = "user-test-001";

        // ============ SOURCING ============

        [Fact(DisplayName = "ScRequisicao | Tipo e colaborador válidos deve ser válida")]
        public void ScRequisicao_DadosValidos_DeveSerValida()
        {
            var req = new ScRequisicao(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, TenantId, UserId);
            Assert.True(req.IsValid);
        }

        [Fact(DisplayName = "ScRequisicao | Tipo vazio deve ser inválida")]
        public void ScRequisicao_TipoVazio_DeveSerInvalida()
        {
            var req = new ScRequisicao(Guid.Empty, Guid.NewGuid(), null, TenantId, UserId);
            Assert.False(req.IsValid);
        }

        [Fact(DisplayName = "ScRequisicaoItem | Produto vazio deve ser inválido")]
        public void ScRequisicaoItem_ProdutoVazio_DeveSerInvalido()
        {
            var item = new ScRequisicaoItem(Guid.NewGuid(), Guid.Empty, 1m, 0m, "N", TenantId, UserId);
            Assert.False(item.IsValid);
        }

        [Fact(DisplayName = "ScCotacao | Descrição e situação obrigatórias")]
        public void ScCotacao_SemDescricao_DeveSerInvalida()
        {
            var cot = new ScCotacao(DateTime.UtcNow, "", "", TenantId, UserId);
            Assert.False(cot.IsValid);
        }

        [Fact(DisplayName = "ScCotacaoFornecedor | Fornecedor obrigatório (SC-045)")]
        public void ScCotacaoFornecedor_SemFornecedor_DeveSerInvalido()
        {
            var f = new ScCotacaoFornecedor(Guid.NewGuid(), Guid.Empty, "5 dias", "30/60", 0m, 0m, 0m, TenantId, UserId);
            Assert.False(f.IsValid);
        }

        [Fact(DisplayName = "ScPedidoCompra | Dados válidos deve ser válido")]
        public void ScPedidoCompra_DadosValidos_DeveSerValido()
        {
            var pedido = new ScPedidoCompra(Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow, null, null,
                "Matriz", "Matriz", "Comprador", "Boleto", 3, 30, 30, TenantId, UserId);
            Assert.True(pedido.IsValid);
        }

        [Fact(DisplayName = "ScPedidoCompra | Fornecedor e locais obrigatórios")]
        public void ScPedidoCompra_SemFornecedorELocais_DeveSerInvalido()
        {
            var pedido = new ScPedidoCompra(Guid.NewGuid(), Guid.Empty, null, null, null, null,
                "", "", "", "", null, null, null, TenantId, UserId);
            Assert.False(pedido.IsValid);
        }

        [Fact(DisplayName = "ScPedidoCompraItem | Quantidade deve ser maior que zero")]
        public void ScPedidoCompraItem_QuantidadeZero_DeveSerInvalido()
        {
            var item = new ScPedidoCompraItem(Guid.NewGuid(), Guid.NewGuid(), 0m, 10m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, TenantId, UserId);
            Assert.False(item.IsValid);
        }

        // ============ LOGÍSTICA DE ENTRADA ============

        [Fact(DisplayName = "LdeEntrada | Compra e fornecedor válidos deve nascer em Rascunho")]
        public void LdeEntrada_DadosValidos_DeveNascerRascunho()
        {
            var entrada = new LdeEntrada(Guid.NewGuid(), Guid.NewGuid(), null, null, TenantId, UserId);
            Assert.True(entrada.IsValid);
            Assert.Equal(ESituacaoEntradaLogistica.Rascunho, entrada.Situacao);
            Assert.True(entrada.PodeConfirmar());
        }

        [Fact(DisplayName = "LdeEntrada | Sem compra ou fornecedor deve ser inválida (LDE-001/012)")]
        public void LdeEntrada_SemCompraFornecedor_DeveSerInvalida()
        {
            var entrada = new LdeEntrada(Guid.Empty, Guid.Empty, null, null, TenantId, UserId);
            Assert.False(entrada.IsValid);
        }

        [Fact(DisplayName = "LdeEntrada | Confirmar muda situação e preenche data de confirmação")]
        public void LdeEntrada_Confirmar_MudaSituacao()
        {
            var entrada = new LdeEntrada(Guid.NewGuid(), Guid.NewGuid(), null, null, TenantId, UserId);
            entrada.Confirmar(UserId);
            Assert.Equal(ESituacaoEntradaLogistica.Confirmado, entrada.Situacao);
            Assert.NotNull(entrada.DataConfirmacao);
            Assert.False(entrada.PodeConfirmar());
        }

        [Fact(DisplayName = "LdeEntrada | Cancelar registra motivo e marca cancelada (LDE-018)")]
        public void LdeEntrada_Cancelar_RegistraMotivo()
        {
            var entrada = new LdeEntrada(Guid.NewGuid(), Guid.NewGuid(), null, null, TenantId, UserId);
            entrada.Cancelar("Divergência de nota", UserId);
            Assert.Equal(ESituacaoEntradaLogistica.Cancelado, entrada.Situacao);
            Assert.Equal("Divergência de nota", entrada.MotivoCancelamentoEstorno);
            Assert.True(entrada.EstaCancelada());
        }

        [Fact(DisplayName = "LdeEntrada | Estorno marca estornada e é considerado cancelado")]
        public void LdeEntrada_Estornar_MarcaEstornada()
        {
            var entrada = new LdeEntrada(Guid.NewGuid(), Guid.NewGuid(), null, null, TenantId, UserId);
            entrada.Estornar("Efeitos revertidos", UserId);
            Assert.Equal(ESituacaoEntradaLogistica.Estornado, entrada.Situacao);
            Assert.True(entrada.EstaCancelada());
        }

        [Fact(DisplayName = "LdeLocalEntregaCompra | Todos obrigatórios preenchidos deve ser válido (LDE-002..010)")]
        public void LdeLocalEntrega_DadosValidos_DeveSerValido()
        {
            var local = new LdeLocalEntregaCompra(Guid.NewGuid(), "Depósito Central", "4830001111", "recebimento@empresa.com",
                "ISENTO", "12345678000199", "SC", "Rua A", "100", null, "Centro",
                Guid.NewGuid(), "Joinville", "89200000", 1058, "Brasil", TenantId, UserId);
            Assert.True(local.IsValid);
        }

        [Fact(DisplayName = "LdeLocalEntregaCompra | Sem nome/documento/UF deve ser inválido")]
        public void LdeLocalEntrega_SemObrigatorios_DeveSerInvalido()
        {
            var local = new LdeLocalEntregaCompra(Guid.NewGuid(), "", "", "", "", "", "",
                null, null, null, null, Guid.Empty, null, null, 0, null, TenantId, UserId);
            Assert.False(local.IsValid);
        }

        [Fact(DisplayName = "LdeHistorico | Entrada e evento obrigatórios")]
        public void LdeHistorico_SemEvento_DeveSerInvalido()
        {
            var hist = new LdeHistorico(Guid.Empty, "", null, ESituacaoEntradaLogistica.Rascunho, null, UserId, TenantId, UserId);
            Assert.False(hist.IsValid);
        }
    }
}
