using System;
using Xunit;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Enums;

namespace Epros.Tests
{
    /// <summary>
    /// Testes de domínio das regras-chave dos submódulos de estoque EST-WMS (Gestão de Armazém),
    /// EST-TMS (Transporte e Frete), EST-GCC (Contratos de Compra) e EST-SUB (Subcontratação).
    /// Cobrem invariantes das entidades, máquinas de estado e a regra de saldo em poder de terceiros.
    /// </summary>
    public class EstoqueWmsTmsGccSubTests
    {
        private const string TenantId = "tenant-test-001";
        private const string UserId = "user-test-001";

        // ================= WMS =================

        [Fact(DisplayName = "WmsArmazem | Dados obrigatórios válidos deve nascer ativo (WMS-014/019)")]
        public void WmsArmazem_DadosValidos_DeveNascerAtivo()
        {
            var a = new WmsArmazem("Central", "Rua A, 100", "Curitiba", "80000-000", null, null, null, Guid.NewGuid(), TenantId, UserId);
            Assert.True(a.IsValid);
            Assert.True(a.Ativo); // WMS-014: novo armazém nasce ativo por padrão
        }

        [Fact(DisplayName = "WmsArmazem | Sem nome/endereço/cidade/CEP deve ser inválido (WMS-019)")]
        public void WmsArmazem_SemObrigatorios_DeveSerInvalido()
        {
            var a = new WmsArmazem("", "", "", "", null, null, true, Guid.NewGuid(), TenantId, UserId);
            Assert.False(a.IsValid);
        }

        [Fact(DisplayName = "WmsArmazem | E-mail inválido deve ser rejeitado (WMS-021)")]
        public void WmsArmazem_EmailInvalido_DeveSerInvalido()
        {
            var a = new WmsArmazem("Central", "Rua A", "Curitiba", "80000-000", null, "invalido", true, Guid.NewGuid(), TenantId, UserId);
            Assert.False(a.IsValid);
        }

        // ================= TMS =================

        [Fact(DisplayName = "CompraTransporte | Transporte nasce em Rascunho (TMS §8)")]
        public void CompraTransporte_Novo_DeveNascerRascunho()
        {
            var t = new CompraTransporte(Guid.NewGuid(), TenantId, UserId);
            Assert.Equal(ETmsStatusTransporte.Rascunho, t.Situacao);
        }

        [Fact(DisplayName = "CompraTransporte | Sem transportadora/veículo/volume deve ser inválido (TMS-001)")]
        public void CompraTransporte_SemNada_DeveSerInvalido()
        {
            var t = new CompraTransporte(Guid.NewGuid(), TenantId, UserId);
            t.Validar();
            Assert.False(t.IsValid); // TMS-001: ao menos transportadora, veículo ou volume
        }

        [Fact(DisplayName = "CompraTransporte | Com volume válido deve ser válido e confirmável (TMS-001/§8)")]
        public void CompraTransporte_ComVolume_DeveSerValidoEConfirmar()
        {
            var t = new CompraTransporte(Guid.NewGuid(), TenantId, UserId);
            t.IncluirVolume(2, "Caixa", "1-2", 10.5m, 12.0m, "MarcaX", UserId);
            Assert.True(t.IsValid);

            t.Confirmar(UserId);
            Assert.Equal(ETmsStatusTransporte.Confirmado, t.Situacao);
        }

        [Fact(DisplayName = "CompraTransporte | Veículo com placa acima de 8 caracteres deve ser inválido (TMS-002)")]
        public void CompraTransporte_PlacaAcimaLimite_DeveSerInvalido()
        {
            var t = new CompraTransporte(Guid.NewGuid(), TenantId, UserId);
            t.IncluirVeiculo(null, "ABC123456", EEstado.PR, null, UserId); // 9 caracteres
            Assert.False(t.IsValid);
        }

        // ================= GCC =================

        [Fact(DisplayName = "GccContratoCompra | Fornecedor e vigência válidos deve nascer em Rascunho (GCC-001/002)")]
        public void GccContrato_DadosValidos_DeveNascerRascunho()
        {
            var c = new GccContratoCompra(Guid.NewGuid(), "CT-1", DateTime.UtcNow, DateTime.UtcNow.AddMonths(6), 1000m, null, TenantId, UserId);
            Assert.True(c.IsValid);
            Assert.Equal(ESituacaoContratoCompra.Rascunho, c.Situacao);
            Assert.False(c.PodeConsumir()); // só aprovado consome
        }

        [Fact(DisplayName = "GccContratoCompra | Sem fornecedor deve ser inválido (GCC-001)")]
        public void GccContrato_SemFornecedor_DeveSerInvalido()
        {
            var c = new GccContratoCompra(Guid.Empty, null, DateTime.UtcNow, null, null, null, TenantId, UserId);
            Assert.False(c.IsValid);
        }

        [Fact(DisplayName = "GccContratoCompra | Sem vigência inicial deve ser inválido (GCC-002)")]
        public void GccContrato_SemVigencia_DeveSerInvalido()
        {
            var c = new GccContratoCompra(Guid.NewGuid(), null, null, null, null, null, TenantId, UserId);
            Assert.False(c.IsValid);
        }

        [Fact(DisplayName = "GccContratoCompra | Vigência final anterior à inicial deve ser inválida (CA-002)")]
        public void GccContrato_VigenciaInvertida_DeveSerInvalido()
        {
            var c = new GccContratoCompra(Guid.NewGuid(), null, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), null, null, TenantId, UserId);
            Assert.False(c.IsValid);
        }

        [Fact(DisplayName = "GccContratoCompra | Aprovar habilita consumo (GCC-007)")]
        public void GccContrato_Aprovado_PodeConsumir()
        {
            var c = new GccContratoCompra(Guid.NewGuid(), null, DateTime.UtcNow, null, null, null, TenantId, UserId);
            c.Aprovar(UserId);
            Assert.True(c.PodeConsumir());
        }

        [Fact(DisplayName = "GccContratoCompraItem | Consumo reduz saldo de quantidade (GCC-008)")]
        public void GccItem_Consumo_ReduzSaldo()
        {
            var item = new GccContratoCompraItem(Guid.NewGuid(), Guid.NewGuid(), 10m, 100m, TenantId, UserId);
            Assert.Equal(100m, item.SaldoQuantidade);

            item.RegistrarConsumo(30m, 300m, UserId);
            Assert.Equal(70m, item.SaldoQuantidade);
            Assert.Equal(700m, item.SaldoValor); // 10*100 - 300
        }

        [Fact(DisplayName = "GccContratoCompraItem | Sem produto/preço/quantidade deve ser inválido (GCC-004/005/006)")]
        public void GccItem_SemObrigatorios_DeveSerInvalido()
        {
            var item = new GccContratoCompraItem(Guid.Empty, Guid.Empty, 0m, 0m, TenantId, UserId);
            Assert.False(item.IsValid);
        }

        // ================= SUB =================

        [Fact(DisplayName = "SubOrdem | Com fornecedor deve ser válida e nascer Aberta (SUB-001)")]
        public void SubOrdem_ComFornecedor_DeveSerValida()
        {
            var o = new SubOrdem(null, "OS-1", null, Guid.NewGuid(), null, null, null, TenantId, UserId);
            Assert.True(o.IsValid);
            Assert.Equal(EStatusSubOrdem.Aberta, o.Status);
        }

        [Fact(DisplayName = "SubOrdem | Sem fornecedor deve ser inválida (SUB-001)")]
        public void SubOrdem_SemFornecedor_DeveSerInvalida()
        {
            var o = new SubOrdem(null, null, null, Guid.Empty, null, null, null, TenantId, UserId);
            Assert.False(o.IsValid);
        }

        [Fact(DisplayName = "SubSaldoTerceiro | Envio soma e retorno/perda reduzem o saldo (SUB-005)")]
        public void SubSaldoTerceiro_EnvioRetorno_RecalculaSaldo()
        {
            var s = new SubSaldoTerceiro(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TenantId, UserId);
            s.RegistrarEnvio(100m, UserId);
            Assert.Equal(100m, s.QuantidadeEmPoderTerceiro);

            s.RegistrarRetorno(60m, 5m, UserId); // retorno 60 + perda 5
            Assert.Equal(35m, s.QuantidadeEmPoderTerceiro); // 100 - 60 - 5
        }

        [Fact(DisplayName = "SubEnvioItem | Sem produto ou quantidade deve ser inválido (SUB-003)")]
        public void SubEnvioItem_SemObrigatorios_DeveSerInvalido()
        {
            var item = new SubEnvioItem(Guid.NewGuid(), Guid.Empty, 0m, null, null, TenantId, UserId);
            Assert.False(item.IsValid);
        }

        [Fact(DisplayName = "SubRetornoItem | Sem produto ou quantidade deve ser inválido (SUB-004)")]
        public void SubRetornoItem_SemObrigatorios_DeveSerInvalido()
        {
            var item = new SubRetornoItem(Guid.NewGuid(), Guid.Empty, 0m, null, null, null, null, TenantId, UserId);
            Assert.False(item.IsValid);
        }
    }
}
