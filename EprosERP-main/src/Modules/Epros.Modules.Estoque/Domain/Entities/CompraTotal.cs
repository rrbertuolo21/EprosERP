using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Totais de uma compra (bases e valores de ICMS/ST/IPI/PIS/COFINS, frete, seguro, desconto, nota).
    /// Porte fiel do legado Epros.ERP.Domain.Entities.Compras.CompraTotal.
    /// </summary>
    public class CompraTotal : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public decimal ValorBaseDeCalculoIcms { get; private set; }
        public decimal ValorIcms { get; private set; }
        public decimal ValorIcmsDesonerado { get; private set; }
        public decimal ValorFcp { get; private set; }
        public decimal ValorBaseDeCalculoSt { get; private set; }
        public decimal ValorSt { get; private set; }
        public decimal ValorFcpSt { get; private set; }
        public decimal ValorFcpRetido { get; private set; }
        public decimal ValorProduto { get; private set; }
        public decimal ValorFrete { get; private set; }
        public decimal ValorSeguro { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorImpostoImportacao { get; private set; }
        public decimal ValorIpi { get; private set; }
        public decimal ValorIpiDevolucao { get; private set; }
        public decimal ValorPis { get; private set; }
        public decimal ValorCofins { get; private set; }
        public decimal ValorOutro { get; private set; }
        public decimal ValorNotaFiscal { get; private set; }

        // Navegação intra-módulo
        public Compra? Compra { get; private set; }

        protected CompraTotal() { } // EF Core

        public CompraTotal(Guid compraId, decimal valorBaseDeCalculoIcms, decimal valorIcms, decimal valorIcmsDesonerado, decimal valorFcp, decimal valorBaseDeCalculoSt, decimal valorSt, decimal valorFcpSt, decimal valorFcpRetido, decimal valorProduto, decimal valorFrete, decimal valorSeguro, decimal valorDesconto, decimal valorImpostoImportacao, decimal valorIpi, decimal valorIpiDevolucao, decimal valorPis, decimal valorCofins, decimal valorOutro, decimal valorNotaFiscal, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            ValorBaseDeCalculoIcms = valorBaseDeCalculoIcms;
            ValorIcms = valorIcms;
            ValorIcmsDesonerado = valorIcmsDesonerado;
            ValorFcp = valorFcp;
            ValorBaseDeCalculoSt = valorBaseDeCalculoSt;
            ValorSt = valorSt;
            ValorFcpSt = valorFcpSt;
            ValorFcpRetido = valorFcpRetido;
            ValorProduto = valorProduto;
            ValorFrete = valorFrete;
            ValorSeguro = valorSeguro;
            ValorDesconto = valorDesconto;
            ValorImpostoImportacao = valorImpostoImportacao;
            ValorIpi = valorIpi;
            ValorIpiDevolucao = valorIpiDevolucao;
            ValorPis = valorPis;
            ValorCofins = valorCofins;
            ValorOutro = valorOutro;
            ValorNotaFiscal = valorNotaFiscal;
        }

        public void Alterar(Guid compraId, decimal valorBaseDeCalculoIcms, decimal valorIcms, decimal valorIcmsDesonerado, decimal valorFcp, decimal valorBaseDeCalculoSt, decimal valorSt, decimal valorFcpSt, decimal valorFcpRetido, decimal valorProduto, decimal valorFrete, decimal valorSeguro, decimal valorDesconto, decimal valorImpostoImportacao, decimal valorIpi, decimal valorIpiDevolucao, decimal valorPis, decimal valorCofins, decimal valorOutro, decimal valorNotaFiscal, string usuario)
        {
            CompraId = compraId;
            ValorBaseDeCalculoIcms = valorBaseDeCalculoIcms;
            ValorIcms = valorIcms;
            ValorIcmsDesonerado = valorIcmsDesonerado;
            ValorFcp = valorFcp;
            ValorBaseDeCalculoSt = valorBaseDeCalculoSt;
            ValorSt = valorSt;
            ValorFcpSt = valorFcpSt;
            ValorFcpRetido = valorFcpRetido;
            ValorProduto = valorProduto;
            ValorFrete = valorFrete;
            ValorSeguro = valorSeguro;
            ValorDesconto = valorDesconto;
            ValorImpostoImportacao = valorImpostoImportacao;
            ValorIpi = valorIpi;
            ValorIpiDevolucao = valorIpiDevolucao;
            ValorPis = valorPis;
            ValorCofins = valorCofins;
            ValorOutro = valorOutro;
            ValorNotaFiscal = valorNotaFiscal;
            MarcarAlterado(usuario);
        }
    }
}
