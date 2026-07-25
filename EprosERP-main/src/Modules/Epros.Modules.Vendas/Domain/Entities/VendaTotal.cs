using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaTotal (totais fiscais da venda). FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaTotal : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
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
        public Venda Venda { get; private set; } = null!;

        protected VendaTotal() { } // EF Core

        public VendaTotal(Guid vendaId, decimal valorBaseDeCalculoIcms, decimal valorIcms, decimal valorIcmsDesonerado, decimal valorFcp, decimal valorBaseDeCalculoSt, decimal valorSt, decimal valorFcpSt, decimal valorFcpRetido, decimal valorProduto, decimal valorFrete, decimal valorSeguro, decimal valorDesconto, decimal valorImpostoImportacao, decimal valorIpi, decimal valorIpiDevolucao, decimal valorPis, decimal valorCofins, decimal valorOutro, decimal valorNotaFiscal,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
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

        public void Alterar(decimal valorBaseDeCalculoIcms, decimal valorIcms, decimal valorIcmsDesonerado, decimal valorFcp, decimal valorBaseDeCalculoSt, decimal valorSt, decimal valorFcpSt, decimal valorFcpRetido, decimal valorProduto, decimal valorFrete, decimal valorSeguro, decimal valorDesconto, decimal valorImpostoImportacao, decimal valorIpi, decimal valorIpiDevolucao, decimal valorPis, decimal valorCofins, decimal valorOutro, decimal valorNotaFiscal, string alteradoPor)
        {
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
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaTotal.Duplicar (novo Id/FK).</summary>
        public VendaTotal Duplicar(Guid novaVendaId, string criadoPor)
            => new(novaVendaId, ValorBaseDeCalculoIcms, ValorIcms, ValorIcmsDesonerado, ValorFcp, ValorBaseDeCalculoSt, ValorSt,
                   ValorFcpSt, ValorFcpRetido, ValorProduto, ValorFrete, ValorSeguro, ValorDesconto, ValorImpostoImportacao,
                   ValorIpi, ValorIpiDevolucao, ValorPis, ValorCofins, ValorOutro, ValorNotaFiscal, TenantId, criadoPor);
    }
}
