using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    /// <summary>
    /// Item (linha) de uma devolução fiscal. Fiel à estrutura <c>item_devolucaos</c> comprovada no material
    /// (EF_DEVOLUCAO_FISCAL, seção 12.2): NCM, CFOP e CST por linha, além de produto e valores.
    /// </summary>
    public class DevolucaoFiscalItem : EntidadeSaaSBase
    {
        public Guid DevolucaoFiscalId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public string Sku { get; private set; } = string.Empty;
        public string NomeProduto { get; private set; } = string.Empty;
        public string Ncm { get; private set; } = string.Empty;
        public int Cfop { get; private set; }
        public string Cst { get; private set; } = string.Empty;
        public decimal Quantidade { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public decimal ValorTotal { get; private set; }
        public decimal AliquotaIcms { get; private set; }

        protected DevolucaoFiscalItem() { } // EF Core

        public DevolucaoFiscalItem(
            Guid devolucaoFiscalId,
            Guid? produtoId,
            string sku,
            string nomeProduto,
            string ncm,
            int cfop,
            string cst,
            decimal quantidade,
            decimal valorUnitario,
            decimal aliquotaIcms,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            DevolucaoFiscalId = devolucaoFiscalId;
            ProdutoId = produtoId;
            Sku = sku;
            NomeProduto = nomeProduto;
            Ncm = ncm;
            Cfop = cfop;
            Cst = cst;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            ValorTotal = quantidade * valorUnitario;
            AliquotaIcms = aliquotaIcms;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DevolucaoFiscalItem>()
                .Requires()
                .IsNotNullOrEmpty(Sku, nameof(Sku), "O SKU/código do produto devolvido é obrigatório [Origem: DevolucaoFiscalItem]")
                .IsNotNullOrEmpty(NomeProduto, nameof(NomeProduto), "O nome do produto devolvido é obrigatório [Origem: DevolucaoFiscalItem]")
                .IsNotNullOrEmpty(Ncm, nameof(Ncm), "O NCM da linha da devolução é obrigatório [Origem: DevolucaoFiscalItem]")
                .IsGreaterThan(Cfop, 0, nameof(Cfop), "O CFOP da linha da devolução deve ser informado [Origem: DevolucaoFiscalItem]")
                .IsNotNullOrEmpty(Cst, nameof(Cst), "O CST/CSOSN da linha da devolução é obrigatório [Origem: DevolucaoFiscalItem]")
                .IsGreaterThan(Quantidade, 0, nameof(Quantidade), "A quantidade devolvida deve ser maior que zero [Origem: DevolucaoFiscalItem]")
                .IsGreaterThan(ValorUnitario, -0.01m, nameof(ValorUnitario), "O valor unitário não pode ser negativo [Origem: DevolucaoFiscalItem]")
                .IsGreaterThan(AliquotaIcms, -0.01m, nameof(AliquotaIcms), "A alíquota de ICMS não pode ser negativa [Origem: DevolucaoFiscalItem]"));
        }
    }
}
