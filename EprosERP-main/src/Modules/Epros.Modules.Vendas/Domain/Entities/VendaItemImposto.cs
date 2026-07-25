using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaItemImposto (legado). ICMS/CST/CSOSN/ST/FCP/IPI/PIS/COFINS/DIFAL,
    /// redução de BC, diferimento, desoneração. FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaItemImposto : EntidadeSaaSBase
    {
        public Guid VendaItemId { get; private set; }
        public EOrigemMercadoria Origem { get; private set; }
        public ECodigoSituacaoTributariaIcms CstIcms { get; private set; }
        public ECodigoSituacaoOperacaoSimplesNacional Csosn { get; private set; }
        public EModalidadeBaseDeCalculosIcms ModalidadeDeterminacaoBaseCalculoIcms { get; private set; }
        public decimal ValorBaseDeCalculoIcms { get; private set; }
        public decimal PercentualReducaoBaseDeCalculoIcms { get; private set; }
        public decimal AliquotaIcms { get; private set; }
        public decimal ValorImpostoIcms { get; private set; }
        public EModalidadeBaseDeCalculosST ModalidadeBaseDeCalculosST { get; private set; }
        public decimal PercentualMvaBaseDeCalculoST { get; private set; }
        public decimal PercentualReducaoBaseDeCalculoST { get; private set; }
        public decimal ValorBaseDeCalculoSt { get; private set; }
        public decimal AliquotaSt { get; private set; }
        public decimal ValorImpostoSt { get; private set; }
        public EMotivoDesoneracaoIcms MotivoDesoneracaoIcms { get; private set; }
        public decimal ValorBaseDeCalculoStRetido { get; private set; }
        public decimal ValorImpostoStRetido { get; private set; }
        public decimal PercentualCreditoSimplesNacionalIcms { get; private set; }
        public decimal ValorImpostoCreditoSimplesNacionalIcms { get; private set; }
        public decimal ValorBaseDeCalculoFcp { get; private set; }
        public decimal PercentualFcp { get; private set; }
        public decimal ValorImpostoFcp { get; private set; }
        public decimal ValorOperacaoDiferimentoIcms { get; private set; }
        public decimal PercentualDiferimentoIcms { get; private set; }
        public decimal ValorImpostoDiferimentoIcms { get; private set; }
        public ECodigoSituacaoTributariaIpi CstIpiSaida { get; private set; }
        public decimal ValorBaseDeCalculoIpi { get; private set; }
        public decimal AliquotaIpi { get; private set; }
        public decimal ValorImpostoDiferimentoIpi { get; set; }
        public decimal ValorQuantidadeTotalParaTributacaoIpi { get; set; }
        public decimal ValorPorUnidadeTributavelIpi { get; private set; }
        public ECodigoSituacaoTributariaPisCofins CstPis { get; private set; }
        public decimal ValorBaseDeCalculoPis { get; private set; }
        public decimal AliquotaPis { get; private set; }
        public decimal ValorQuantidadeVendidaProdutoPis { get; private set; }
        public decimal AliquotaPorUnidadeVendidaPis { get; private set; }
        public decimal ValorImpostoDiferimentoPis { get; private set; }
        public ECodigoSituacaoTributariaPisCofins CstCofins { get; private set; }
        public decimal ValorBaseDeCalculoCofins { get; private set; }
        public decimal AliquotaCofins { get; private set; }
        public decimal ValorQuantidadeVendidaProdutoCofins { get; private set; }
        public decimal AliquotaPorUnidadeVendidaCofins { get; private set; }
        public decimal ValorImpostoDiferimentoCofins { get; private set; }
        public ETipoReducaoBaseDeCalculo TipoReducaoIcms { get; private set; }
        public ETipoReducaoBaseDeCalculo TipoReducaoIcmsSt { get; private set; }
        public decimal ValorBaseDeCalculoFcpSt { get; private set; }
        public decimal PercentualFcpSt { get; private set; }
        public decimal ValorImpostoFcpSt { get; private set; }
        public decimal ValorIcmsProprioSubistituto { get; private set; }
        public decimal ValorAliquotaIcmsInterna { get; private set; }
        public decimal ValorAliquotaIcmsInternaEstadual { get; private set; }
        public int EnquadramentoIpi { get; private set; }
        public decimal ValorReducaoIpiPercentual { get; private set; }
        public bool IpiEmbutido { get; private set; }
        public bool DifalTipoCalculoPorDentro { get; private set; }
        public ETipoReducaoBaseDeCalculo TipoReducaoIpi { get; private set; }
        public EDeterminacaoBaseIcmsSt TipoCalculoBaseIcmsSt { get; private set; }
        public decimal ValorUnitFixadoIcmsSt { get; private set; }
        public decimal ValorBaseDeCalculoDifal { get; private set; }
        public decimal ValorImpostoDevidoDifal { get; private set; }
        public decimal ValorImpostoDevidoRecolherSt { get; private set; }
        public decimal ValorImpostoDevidoFcp { get; private set; }
        public decimal ValorIcmsIsento { get; private set; }
        public decimal ValorIcmsOutros { get; private set; }
        public string? IcmsObservacao { get; private set; }
        public decimal ValorIpiIsento { get; private set; }
        public decimal ValorIpiOutros { get; private set; }
        public string? IpiObservacao { get; private set; }

        // Navegação intra-módulo
        public VendaItem VendaItem { get; private set; } = null!;

        protected VendaItemImposto() { } // EF Core

        public VendaItemImposto(Guid vendaItemId, EOrigemMercadoria origem, ECodigoSituacaoTributariaIcms cstIcms, ECodigoSituacaoOperacaoSimplesNacional csosn, EModalidadeBaseDeCalculosIcms modalidadeDeterminacaoBaseCalculoIcms, decimal valorBaseDeCalculoIcms, decimal percentualReducaoBaseDeCalculoIcms, decimal aliquotaIcms, decimal valorImpostoIcms, EModalidadeBaseDeCalculosST modalidadeBaseDeCalculosST, decimal percentualMvaBaseDeCalculoST, decimal percentualReducaoBaseDeCalculoST, decimal valorBaseDeCalculoSt, decimal aliquotaSt, decimal valorImpostoSt, EMotivoDesoneracaoIcms motivoDesoneracaoIcms, decimal valorBaseDeCalculoStRetido, decimal valorImpostoStRetido, decimal percentualCreditoSimplesNacionalIcms, decimal valorImpostoCreditoSimplesNacionalIcms, decimal valorBaseDeCalculoFcp, decimal percentualFcp, decimal valorImpostoFcp, decimal valorOperacaoDiferimentoIcms, decimal percentualDiferimentoIcms, decimal valorImpostoDiferimentoIcms, ECodigoSituacaoTributariaIpi cstIpiSaida, decimal valorBaseDeCalculoIpi, decimal aliquotaIpi, decimal valorImpostoDiferimentoIpi, decimal valorQuantidadeTotalParaTributacaoIpi, decimal valorPorUnidadeTributavelIpi, ECodigoSituacaoTributariaPisCofins cstPis, decimal valorBaseDeCalculoPis, decimal aliquotaPis, decimal valorQuantidadeVendidaProdutoPis, decimal aliquotaPorUnidadeVendidaPis, decimal valorImpostoDiferimentoPis, ECodigoSituacaoTributariaPisCofins cstCofins, decimal valorBaseDeCalculoCofins, decimal aliquotaCofins, decimal valorQuantidadeVendidaProdutoCofins, decimal aliquotaPorUnidadeVendidaCofins, decimal valorImpostoDiferimentoCofins, ETipoReducaoBaseDeCalculo tipoReducaoIcms, ETipoReducaoBaseDeCalculo tipoReducaoIcmsSt, decimal valorBaseDeCalculoFcpSt, decimal percentualFcpSt, decimal valorImpostoFcpSt, decimal valorIcmsProprioSubistituto, decimal valorAliquotaIcmsInterna, decimal valorAliquotaIcmsInternaEstadual, int enquadramentoIpi, decimal valorReducaoIpiPercentual, bool ipiEmbutido, bool difalTipoCalculoPorDentro, ETipoReducaoBaseDeCalculo tipoReducaoIpi, EDeterminacaoBaseIcmsSt tipoCalculoBaseIcmsSt, decimal valorUnitFixadoIcmsSt, decimal valorBaseDeCalculoDifal, decimal valorImpostoDevidoDifal, decimal valorImpostoDevidoRecolherSt, decimal valorImpostoDevidoFcp, decimal valorIcmsIsento, decimal valorIcmsOutros, string? icmsObservacao, decimal valorIpiIsento, decimal valorIpiOutros, string? ipiObservacao,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaItemId = vendaItemId;
            Origem = origem;
            CstIcms = cstIcms;
            Csosn = csosn;
            ModalidadeDeterminacaoBaseCalculoIcms = modalidadeDeterminacaoBaseCalculoIcms;
            ValorBaseDeCalculoIcms = valorBaseDeCalculoIcms;
            PercentualReducaoBaseDeCalculoIcms = percentualReducaoBaseDeCalculoIcms;
            AliquotaIcms = aliquotaIcms;
            ValorImpostoIcms = valorImpostoIcms;
            ModalidadeBaseDeCalculosST = modalidadeBaseDeCalculosST;
            PercentualMvaBaseDeCalculoST = percentualMvaBaseDeCalculoST;
            PercentualReducaoBaseDeCalculoST = percentualReducaoBaseDeCalculoST;
            ValorBaseDeCalculoSt = valorBaseDeCalculoSt;
            AliquotaSt = aliquotaSt;
            ValorImpostoSt = valorImpostoSt;
            MotivoDesoneracaoIcms = motivoDesoneracaoIcms;
            ValorBaseDeCalculoStRetido = valorBaseDeCalculoStRetido;
            ValorImpostoStRetido = valorImpostoStRetido;
            PercentualCreditoSimplesNacionalIcms = percentualCreditoSimplesNacionalIcms;
            ValorImpostoCreditoSimplesNacionalIcms = valorImpostoCreditoSimplesNacionalIcms;
            ValorBaseDeCalculoFcp = valorBaseDeCalculoFcp;
            PercentualFcp = percentualFcp;
            ValorImpostoFcp = valorImpostoFcp;
            ValorOperacaoDiferimentoIcms = valorOperacaoDiferimentoIcms;
            PercentualDiferimentoIcms = percentualDiferimentoIcms;
            ValorImpostoDiferimentoIcms = valorImpostoDiferimentoIcms;
            CstIpiSaida = cstIpiSaida;
            ValorBaseDeCalculoIpi = valorBaseDeCalculoIpi;
            AliquotaIpi = aliquotaIpi;
            ValorImpostoDiferimentoIpi = valorImpostoDiferimentoIpi;
            ValorQuantidadeTotalParaTributacaoIpi = valorQuantidadeTotalParaTributacaoIpi;
            ValorPorUnidadeTributavelIpi = valorPorUnidadeTributavelIpi;
            CstPis = cstPis;
            ValorBaseDeCalculoPis = valorBaseDeCalculoPis;
            AliquotaPis = aliquotaPis;
            ValorQuantidadeVendidaProdutoPis = valorQuantidadeVendidaProdutoPis;
            AliquotaPorUnidadeVendidaPis = aliquotaPorUnidadeVendidaPis;
            ValorImpostoDiferimentoPis = valorImpostoDiferimentoPis;
            CstCofins = cstCofins;
            ValorBaseDeCalculoCofins = valorBaseDeCalculoCofins;
            AliquotaCofins = aliquotaCofins;
            ValorQuantidadeVendidaProdutoCofins = valorQuantidadeVendidaProdutoCofins;
            AliquotaPorUnidadeVendidaCofins = aliquotaPorUnidadeVendidaCofins;
            ValorImpostoDiferimentoCofins = valorImpostoDiferimentoCofins;
            TipoReducaoIcms = tipoReducaoIcms;
            TipoReducaoIcmsSt = tipoReducaoIcmsSt;
            ValorBaseDeCalculoFcpSt = valorBaseDeCalculoFcpSt;
            PercentualFcpSt = percentualFcpSt;
            ValorImpostoFcpSt = valorImpostoFcpSt;
            ValorIcmsProprioSubistituto = valorIcmsProprioSubistituto;
            ValorAliquotaIcmsInterna = valorAliquotaIcmsInterna;
            ValorAliquotaIcmsInternaEstadual = valorAliquotaIcmsInternaEstadual;
            EnquadramentoIpi = enquadramentoIpi;
            ValorReducaoIpiPercentual = valorReducaoIpiPercentual;
            IpiEmbutido = ipiEmbutido;
            DifalTipoCalculoPorDentro = difalTipoCalculoPorDentro;
            TipoReducaoIpi = tipoReducaoIpi;
            TipoCalculoBaseIcmsSt = tipoCalculoBaseIcmsSt;
            ValorUnitFixadoIcmsSt = valorUnitFixadoIcmsSt;
            ValorBaseDeCalculoDifal = valorBaseDeCalculoDifal;
            ValorImpostoDevidoDifal = valorImpostoDevidoDifal;
            ValorImpostoDevidoRecolherSt = valorImpostoDevidoRecolherSt;
            ValorImpostoDevidoFcp = valorImpostoDevidoFcp;
            ValorIcmsIsento = valorIcmsIsento;
            ValorIcmsOutros = valorIcmsOutros;
            IcmsObservacao = icmsObservacao;
            ValorIpiIsento = valorIpiIsento;
            ValorIpiOutros = valorIpiOutros;
            IpiObservacao = ipiObservacao;
        }

        /// <summary>Porte fiel de VendaItemImposto.Alterar (dados cadastrais/alíquotas).</summary>
        public void Alterar(EOrigemMercadoria origem, ECodigoSituacaoTributariaIcms cstIcms, ECodigoSituacaoOperacaoSimplesNacional csosn, EModalidadeBaseDeCalculosIcms modalidadeDeterminacaoBaseCalculoIcms, decimal percentualReducaoBaseDeCalculoIcms, decimal aliquotaIcms, EModalidadeBaseDeCalculosST modalidadeBaseDeCalculosST, decimal percentualMvaBaseDeCalculoST, decimal percentualReducaoBaseDeCalculoST, decimal aliquotaSt, EMotivoDesoneracaoIcms motivoDesoneracaoIcms, decimal percentualCreditoSimplesNacionalIcms, ECodigoSituacaoTributariaIpi cstIpiSaida, decimal aliquotaIpi, ECodigoSituacaoTributariaPisCofins cstPis, decimal aliquotaPis, decimal aliquotaPorUnidadeVendidaPis, ECodigoSituacaoTributariaPisCofins cstCofins, decimal aliquotaCofins, decimal aliquotaPorUnidadeVendidaCofins, string alteradoPor)
        {
            Origem = origem;
            CstIcms = cstIcms;
            Csosn = csosn;
            ModalidadeDeterminacaoBaseCalculoIcms = modalidadeDeterminacaoBaseCalculoIcms;
            PercentualReducaoBaseDeCalculoIcms = percentualReducaoBaseDeCalculoIcms;
            AliquotaIcms = aliquotaIcms;
            ModalidadeBaseDeCalculosST = modalidadeBaseDeCalculosST;
            PercentualMvaBaseDeCalculoST = percentualMvaBaseDeCalculoST;
            PercentualReducaoBaseDeCalculoST = percentualReducaoBaseDeCalculoST;
            AliquotaSt = aliquotaSt;
            MotivoDesoneracaoIcms = motivoDesoneracaoIcms;
            PercentualCreditoSimplesNacionalIcms = percentualCreditoSimplesNacionalIcms;
            CstIpiSaida = cstIpiSaida;
            AliquotaIpi = aliquotaIpi;
            CstPis = cstPis;
            AliquotaPis = aliquotaPis;
            AliquotaPorUnidadeVendidaPis = aliquotaPorUnidadeVendidaPis;
            CstCofins = cstCofins;
            AliquotaCofins = aliquotaCofins;
            AliquotaPorUnidadeVendidaCofins = aliquotaPorUnidadeVendidaCofins;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaItemImposto.DefinirValoresCalculados.</summary>
        public void DefinirValoresCalculados(decimal valorBaseDeCalculoIcms, decimal valorImpostoIcms, decimal valorBaseDeCalculoSt, decimal valorImpostoSt, decimal valorBaseDeCalculoStRetido, decimal valorImpostoStRetido, decimal valorImpostoCreditoSimplesNacionalIcms, decimal valorBaseDeCalculoFcp, decimal valorImpostoFcp, decimal valorImpostoDiferimentoIcms, decimal valorBaseDeCalculoIpi, decimal valorImpostoDiferimentoIpi, decimal valorBaseDeCalculoPis, decimal valorBaseDeCalculoCofins)
        {
            ValorBaseDeCalculoIcms = valorBaseDeCalculoIcms;
            ValorImpostoIcms = valorImpostoIcms;
            ValorBaseDeCalculoSt = valorBaseDeCalculoSt;
            ValorImpostoSt = valorImpostoSt;
            ValorBaseDeCalculoStRetido = valorBaseDeCalculoStRetido;
            ValorImpostoStRetido = valorImpostoStRetido;
            ValorImpostoCreditoSimplesNacionalIcms = valorImpostoCreditoSimplesNacionalIcms;
            ValorBaseDeCalculoFcp = valorBaseDeCalculoFcp;
            ValorImpostoFcp = valorImpostoFcp;
            ValorImpostoDiferimentoIcms = valorImpostoDiferimentoIcms;
            ValorBaseDeCalculoIpi = valorBaseDeCalculoIpi;
            ValorImpostoDiferimentoIpi = valorImpostoDiferimentoIpi;
            ValorBaseDeCalculoPis = valorBaseDeCalculoPis;
            ValorBaseDeCalculoCofins = valorBaseDeCalculoCofins;
        }

        /// <summary>Porte fiel de VendaItemImposto.Duplicar (novo Id/FK, todos os campos preservados).</summary>
        public VendaItemImposto Duplicar(Guid novoItemId, string criadoPor)
            => new(novoItemId, Origem, CstIcms, Csosn, ModalidadeDeterminacaoBaseCalculoIcms, ValorBaseDeCalculoIcms,
                   PercentualReducaoBaseDeCalculoIcms, AliquotaIcms, ValorImpostoIcms, ModalidadeBaseDeCalculosST,
                   PercentualMvaBaseDeCalculoST, PercentualReducaoBaseDeCalculoST, ValorBaseDeCalculoSt, AliquotaSt,
                   ValorImpostoSt, MotivoDesoneracaoIcms, ValorBaseDeCalculoStRetido, ValorImpostoStRetido,
                   PercentualCreditoSimplesNacionalIcms, ValorImpostoCreditoSimplesNacionalIcms, ValorBaseDeCalculoFcp,
                   PercentualFcp, ValorImpostoFcp, ValorOperacaoDiferimentoIcms, PercentualDiferimentoIcms,
                   ValorImpostoDiferimentoIcms, CstIpiSaida, ValorBaseDeCalculoIpi, AliquotaIpi, ValorImpostoDiferimentoIpi,
                   ValorQuantidadeTotalParaTributacaoIpi, ValorPorUnidadeTributavelIpi, CstPis, ValorBaseDeCalculoPis,
                   AliquotaPis, ValorQuantidadeVendidaProdutoPis, AliquotaPorUnidadeVendidaPis, ValorImpostoDiferimentoPis,
                   CstCofins, ValorBaseDeCalculoCofins, AliquotaCofins, ValorQuantidadeVendidaProdutoCofins,
                   AliquotaPorUnidadeVendidaCofins, ValorImpostoDiferimentoCofins, TipoReducaoIcms, TipoReducaoIcmsSt,
                   ValorBaseDeCalculoFcpSt, PercentualFcpSt, ValorImpostoFcpSt, ValorIcmsProprioSubistituto,
                   ValorAliquotaIcmsInterna, ValorAliquotaIcmsInternaEstadual, EnquadramentoIpi, ValorReducaoIpiPercentual,
                   IpiEmbutido, DifalTipoCalculoPorDentro, TipoReducaoIpi, TipoCalculoBaseIcmsSt, ValorUnitFixadoIcmsSt,
                   ValorBaseDeCalculoDifal, ValorImpostoDevidoDifal, ValorImpostoDevidoRecolherSt, ValorImpostoDevidoFcp,
                   ValorIcmsIsento, ValorIcmsOutros, IcmsObservacao, ValorIpiIsento, ValorIpiOutros, IpiObservacao,
                   TenantId, criadoPor);
    }
}
