using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class DocumentoFiscalItem : EntidadeSaaSBase
    {
        public Guid DocumentoFiscalId { get; private set; }
        public string Sku { get; private set; } = string.Empty;
        public string NomeProduto { get; private set; } = string.Empty;
        public string Cst { get; private set; } = string.Empty;
        public int Cfop { get; private set; }
        public string Ncm { get; private set; } = string.Empty;
        public decimal Quantidade { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public decimal ValorTotal { get; private set; }
        public decimal AliquotaIcms { get; private set; }
        public decimal ValorIcms { get; private set; }

        // ---- Classificação fiscal adicional (entradas do cálculo) ----
        // Preenchidos quando o item vem de um cálculo real; caem em defaults seguros caso contrário,
        // preservando o comportamento anterior (só ICMS informado).
        public string? Csosn { get; private set; }
        public string Origem { get; private set; } = "0";
        public string? CstIpi { get; private set; }
        public decimal AliquotaIpi { get; private set; }
        public string CstPisCofins { get; private set; } = "07";
        public decimal AliquotaPis { get; private set; }
        public decimal AliquotaCofins { get; private set; }
        public string? CstIbsCbs { get; private set; }
        public string? CClassTrib { get; private set; }

        // ---- Impostos calculados pelo motor (saída do ICalculoFiscalService) ----
        // ValorIcms acima é mantido; abaixo ficam os demais tributos apurados.
        public bool ImpostosCalculados { get; private set; }
        public decimal BaseCalculoIcms { get; private set; }
        public decimal ValorIpi { get; private set; }
        public decimal ValorPis { get; private set; }
        public decimal ValorCofins { get; private set; }
        public decimal ValorIcmsSt { get; private set; }
        public decimal ValorFcp { get; private set; }

        protected DocumentoFiscalItem() { } // EF Core

        public DocumentoFiscalItem(
            Guid documentoFiscalId,
            string sku,
            string nomeProduto,
            string cst,
            int cfop,
            string ncm,
            decimal quantidade,
            decimal valorUnitario,
            decimal aliquotaIcms,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DocumentoFiscalItem>()
                .Requires()
                .IsNotNullOrEmpty(sku, nameof(Sku), "O SKU do produto é obrigatório.")
                .IsNotNullOrEmpty(nomeProduto, nameof(NomeProduto), "O Nome do produto é obrigatório.")
                .IsNotNullOrEmpty(cst, nameof(Cst), "O CST do ICMS é obrigatório.")
                .IsGreaterThan(cfop, 0, nameof(Cfop), "O CFOP deve ser informado.")
                .IsNotNullOrEmpty(ncm, nameof(Ncm), "O NCM é obrigatório.")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A quantidade deve ser maior que zero.")
                .IsGreaterThan(valorUnitario, -0.01m, nameof(ValorUnitario), "O valor unitário não pode ser negativo.")
                .IsGreaterThan(aliquotaIcms, -0.01m, nameof(AliquotaIcms), "A alíquota de ICMS não pode ser negativa.")
            );

            DocumentoFiscalId = documentoFiscalId;
            Sku = sku;
            NomeProduto = nomeProduto;
            Cst = cst;
            Cfop = cfop;
            Ncm = ncm;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            ValorTotal = quantidade * valorUnitario;
            AliquotaIcms = aliquotaIcms;
            ValorIcms = Math.Round(ValorTotal * (aliquotaIcms / 100m), 2);
            BaseCalculoIcms = ValorTotal;
        }

        /// <summary>
        /// Define a classificação fiscal completa (CSOSN/origem/IPI/PIS/COFINS/IBS-CBS) usada como
        /// entrada do motor de cálculo. Mantém o CST/CFOP/NCM informados no construtor.
        /// </summary>
        public void DefinirClassificacaoFiscal(
            string? csosn,
            string origem,
            string? cstIpi,
            decimal aliquotaIpi,
            string cstPisCofins,
            decimal aliquotaPis,
            decimal aliquotaCofins,
            string? cstIbsCbs,
            string? cClassTrib)
        {
            Csosn = csosn;
            Origem = string.IsNullOrWhiteSpace(origem) ? "0" : origem;
            CstIpi = cstIpi;
            AliquotaIpi = aliquotaIpi;
            CstPisCofins = string.IsNullOrWhiteSpace(cstPisCofins) ? "07" : cstPisCofins;
            AliquotaPis = aliquotaPis;
            AliquotaCofins = aliquotaCofins;
            CstIbsCbs = cstIbsCbs;
            CClassTrib = cClassTrib;
        }

        /// <summary>
        /// Aplica os valores de imposto apurados pelo ICalculoFiscalService, para que o XML transmitido
        /// carregue os impostos reais (não os defaults zero). Não altera o valor comercial do item.
        /// </summary>
        public void AplicarCalculoFiscal(
            decimal baseCalculoIcms,
            decimal aliquotaIcms,
            decimal valorIcms,
            decimal valorIcmsSt,
            decimal valorFcp,
            decimal valorIpi,
            decimal valorPis,
            decimal valorCofins)
        {
            BaseCalculoIcms = baseCalculoIcms;
            AliquotaIcms = aliquotaIcms;
            ValorIcms = valorIcms;
            ValorIcmsSt = valorIcmsSt;
            ValorFcp = valorFcp;
            ValorIpi = valorIpi;
            ValorPis = valorPis;
            ValorCofins = valorCofins;
            ImpostosCalculados = true;
        }
    }
}
