using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>
    /// Item de devolucao com produto, quantidade, valor e tributacao (EF ECONOMIA_CIRCULAR 11.2).
    /// Fidelidade: 41 campos materiais preservados do legado.
    /// </summary>
    public class DevolucaoItemEco : EntidadeSaaSBase
    {
        public Guid DevolucaoId { get; private set; }
        public string? Codigo { get; private set; }
        public string? Nome { get; private set; }
        public string? Ncm { get; private set; }
        public string? Cfop { get; private set; }
        public decimal? ValorUnitario { get; private set; }
        public decimal? Quantidade { get; private set; }
        public bool ItemParcial { get; private set; }
        public string? CodigoBarras { get; private set; }
        public string? UnidadeMedida { get; private set; }
        public string? CstCsosn { get; private set; }
        public string? CstPis { get; private set; }
        public string? CstCofins { get; private set; }
        public string? CstIpi { get; private set; }
        public decimal? PercentualIcms { get; private set; }
        public decimal? PercentualPis { get; private set; }
        public decimal? PercentualCofins { get; private set; }
        public decimal? PercentualIpi { get; private set; }
        public decimal? PercentualReducaoBc { get; private set; }
        public decimal? ValorBaseCalculo { get; private set; }
        public decimal? ValorIcms { get; private set; }
        public decimal? PercentualGlp { get; private set; }
        public decimal? PercentualGnn { get; private set; }
        public decimal? PercentualGni { get; private set; }
        public string? CodigoAnp { get; private set; }
        public string? DescricaoAnp { get; private set; }
        public string? UfConsumo { get; private set; }
        public decimal? ValorPartida { get; private set; }
        public string? UnidadeTributavel { get; private set; }
        public decimal? QuantidadeTributavel { get; private set; }
        public decimal? ValorBcStRetido { get; private set; }
        public decimal? ValorFrete { get; private set; }
        public string? ModalidadeBcSt { get; private set; }
        public decimal? ValorBcSt { get; private set; }
        public decimal? PercentualIcmsSt { get; private set; }
        public decimal? ValorIcmsSt { get; private set; }
        public decimal? PercentualMvaSt { get; private set; }
        public string? Origem { get; private set; }
        public decimal? PercentualSt { get; private set; }
        public decimal? ValorIcmsSubstituto { get; private set; }
        public decimal? ValorIcmsStRetido { get; private set; }

        protected DevolucaoItemEco() { } // EF Core

        public DevolucaoItemEco(
            Guid devolucaoId,
            string? codigo,
            string? nome,
            string? ncm,
            string? cfop,
            decimal? valorUnitario,
            decimal? quantidade,
            bool itemParcial,
            string? unidadeMedida,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            DevolucaoId = devolucaoId;
            Codigo = codigo;
            Nome = nome;
            Ncm = ncm;
            Cfop = cfop;
            ValorUnitario = valorUnitario;
            Quantidade = quantidade;
            ItemParcial = itemParcial;
            UnidadeMedida = unidadeMedida;
            Validar();
        }

        public void PreencherTributacao(
            string? cstCsosn, string? cstPis, string? cstCofins, string? cstIpi,
            decimal? percentualIcms, decimal? percentualPis, decimal? percentualCofins, decimal? percentualIpi,
            decimal? percentualReducaoBc, decimal? valorBaseCalculo, decimal? valorIcms,
            string? origem, decimal? valorBcSt, decimal? percentualIcmsSt, decimal? valorIcmsSt,
            decimal? percentualMvaSt, decimal? percentualSt, decimal? valorIcmsSubstituto, decimal? valorIcmsStRetido,
            string? modalidadeBcSt, decimal? valorBcStRetido, decimal? valorFrete)
        {
            CstCsosn = cstCsosn;
            CstPis = cstPis;
            CstCofins = cstCofins;
            CstIpi = cstIpi;
            PercentualIcms = percentualIcms;
            PercentualPis = percentualPis;
            PercentualCofins = percentualCofins;
            PercentualIpi = percentualIpi;
            PercentualReducaoBc = percentualReducaoBc;
            ValorBaseCalculo = valorBaseCalculo;
            ValorIcms = valorIcms;
            Origem = origem;
            ValorBcSt = valorBcSt;
            PercentualIcmsSt = percentualIcmsSt;
            ValorIcmsSt = valorIcmsSt;
            PercentualMvaSt = percentualMvaSt;
            PercentualSt = percentualSt;
            ValorIcmsSubstituto = valorIcmsSubstituto;
            ValorIcmsStRetido = valorIcmsStRetido;
            ModalidadeBcSt = modalidadeBcSt;
            ValorBcStRetido = valorBcStRetido;
            ValorFrete = valorFrete;
        }

        public void PreencherCombustivel(
            decimal? percentualGlp, decimal? percentualGnn, decimal? percentualGni,
            string? codigoAnp, string? descricaoAnp, string? ufConsumo, decimal? valorPartida,
            string? unidadeTributavel, decimal? quantidadeTributavel, string? codigoBarras)
        {
            PercentualGlp = percentualGlp;
            PercentualGnn = percentualGnn;
            PercentualGni = percentualGni;
            CodigoAnp = codigoAnp;
            DescricaoAnp = descricaoAnp;
            UfConsumo = ufConsumo;
            ValorPartida = valorPartida;
            UnidadeTributavel = unidadeTributavel;
            QuantidadeTributavel = quantidadeTributavel;
            CodigoBarras = codigoBarras;
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DevolucaoItemEco>()
                .Requires()
                .AreNotEquals(DevolucaoId, Guid.Empty, nameof(DevolucaoId), "A devolucao e obrigatoria. [Origem: DevolucaoItemEco]"));
        }
    }
}
