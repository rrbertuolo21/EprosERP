using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Impostos (ICMS, ST, FCP, IPI, PIS, COFINS, difal) de um item de compra. Porte fiel campo a campo do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraItemImposto (~70 campos).
    /// Enums: ECodigoSituacaoTributariaIcms / ECodigoSituacaoOperacaoSimplesNacional / ECodigoSituacaoTributariaPisCofins
    /// vêm de Epros.Shared.Domain.Enums; os demais foram portados como enums do próprio módulo Estoque.
    /// </summary>
    public class CompraItemImposto : EntidadeSaaSBase
    {
        public Guid CompraItemId { get; private set; }
        public EOrigemMercadoria Origem { get; private set; }
        public ECodigoSituacaoTributariaIcms CstIcms { get; private set; }  // 2
        public ECodigoSituacaoOperacaoSimplesNacional Csosn { get; private set; }  // 3
        public EModalidadeBaseDeCalculosIcms ModalidadeDeterminacaoBaseCalculoIcms { get; private set; }
        public decimal ValorBaseDeCalculoIcms { get; private set; }  // 15,2
        public decimal PercentualReducaoBaseDeCalculoIcms { get; private set; }  // 5,2
        public decimal AliquotaIcms { get; private set; }  // 5,2
        public decimal ValorImpostoIcms { get; private set; }  // 15,2
        public EModalidadeBaseDeCalculosST ModalidadeBaseDeCalculosST { get; private set; }
        public decimal PercentualMvaBaseDeCalculoST { get; private set; }  // 5,2
        public decimal PercentualReducaoBaseDeCalculoST { get; private set; }  // 5,2
        public decimal ValorBaseDeCalculoSt { get; private set; }  // 15,2
        public decimal AliquotaSt { get; private set; }  // 5,2
        public decimal ValorImpostoSt { get; private set; }  // 15,2
        public EMotivoDesoneracaoIcms MotivoDesoneracaoIcms { get; private set; }
        public decimal ValorBaseDeCalculoStRetido { get; private set; }  // 15,2
        public decimal ValorImpostoStRetido { get; private set; }  // 15,2
        public decimal PercentualCreditoSimplesNacionalIcms { get; private set; }  // 5,2
        public decimal ValorImpostoCreditoSimplesNacionalIcms { get; private set; }  // 15,2
        public decimal ValorBaseDeCalculoFcp { get; private set; }  // 15,2
        public decimal PercentualFcp { get; private set; }  // 5,2
        public decimal ValorImpostoFcp { get; private set; }  // 15,2
        public decimal ValorOperacaoDiferimentoIcms { get; private set; }  // 15,2
        public decimal PercentualDiferimentoIcms { get; private set; }  // 5,2
        public decimal ValorImpostoDiferimentoIcms { get; private set; }  // 15,2
        public ECodigoSituacaoTributariaIpi CstIpiSaida { get; private set; }  // 2
        public decimal ValorBaseDeCalculoIpi { get; private set; }  // 15,2
        public decimal AliquotaIpi { get; private set; }  // 5,2
        public decimal ValorImpostoDiferimentoIpi { get; private set; }  // 15,2
        public decimal ValorQuantidadeTotalParaTributacaoIpi { get; private set; }  // 16,4
        public decimal ValorPorUnidadeTributavelIpi { get; private set; }  // 15,4
        public ECodigoSituacaoTributariaPisCofins CstPis { get; private set; }  // 2
        public decimal ValorBaseDeCalculoPis { get; private set; }  // 15,2
        public decimal AliquotaPis { get; private set; }  // 5,2
        public decimal ValorQuantidadeVendidaProdutoPis { get; private set; }  // 16,4
        public decimal AliquotaPorUnidadeVendidaPis { get; private set; }  // 15,4
        public decimal ValorImpostoDiferimentoPis { get; private set; }  // 15,2
        public ECodigoSituacaoTributariaPisCofins CstCofins { get; private set; }  // 2
        public decimal ValorBaseDeCalculoCofins { get; private set; }  // 15,2
        public decimal AliquotaCofins { get; private set; }  // 5,2
        public decimal ValorQuantidadeVendidaProdutoCofins { get; private set; }  // 16,4
        public decimal AliquotaPorUnidadeVendidaCofins { get; private set; }  // 15,4
        public decimal ValorImpostoDiferimentoCofins { get; private set; }  // 15,2
        public ETipoReducaoBaseDeCalculo TipoReducaoIcms { get; private set; }
        public ETipoReducaoBaseDeCalculo TipoReducaoIcmsSt { get; private set; }
        public decimal ValorBaseDeCalculoFcpSt { get; private set; }  // 15,2
        public decimal PercentualFcpSt { get; private set; }  // 5,2
        public decimal ValorImpostoFcpSt { get; private set; }  // 15,2
        public decimal ValorIcmsProprioSubistituto { get; private set; }  // 15,2
        public decimal ValorAliquotaIcmsInterna { get; private set; }  // 15,2
        public decimal ValorAliquotaIcmsInternaEstadual { get; private set; }  // 15,2
        public int EnquadramentoIpi { get; private set; }
        public decimal ValorReducaoIpiPercentual { get; private set; }  // 15,2
        public bool IpiEmbutido { get; private set; }
        public bool DifalTipoCalculoPorDentro { get; private set; }
        public ETipoReducaoBaseDeCalculo TipoReducaoIpi { get; private set; }
        public EDeterminacaoBaseIcmsSt TipoCalculoBaseIcmsSt { get; private set; }
        public decimal ValorUnitFixadoIcmsSt { get; private set; }  // 15,2
        public decimal ValorBaseDeCalculoDifal { get; private set; }  // 15,2
        public decimal ValorImpostoDevidoDifal { get; private set; }  // 15,2
        public decimal ValorImpostoDevidoRecolherSt { get; private set; }  // 15,2
        public decimal ValorImpostoDevidoFcp { get; private set; }  // 15,2
        public decimal ValorIcmsIsento { get; private set; }
        public decimal ValorIcmsOutros { get; private set; }
        public string? IcmsObservacao { get; private set; }
        public decimal ValorIpiIsento { get; private set; }
        public decimal ValorIpiOutros { get; private set; }
        public string? IpiObservacao { get; private set; }

        // Navegação intra-módulo
        public CompraItem? CompraItem { get; private set; }

        protected CompraItemImposto() { } // EF Core

        public CompraItemImposto(Guid compraItemId, EOrigemMercadoria origem, ECodigoSituacaoTributariaIcms cstIcms, ECodigoSituacaoOperacaoSimplesNacional csosn, EModalidadeBaseDeCalculosIcms modalidadeDeterminacaoBaseCalculoIcms, decimal valorBaseDeCalculoIcms, decimal percentualReducaoBaseDeCalculoIcms, decimal aliquotaIcms, decimal valorImpostoIcms, EModalidadeBaseDeCalculosST modalidadeBaseDeCalculosST, decimal percentualMvaBaseDeCalculoST, decimal percentualReducaoBaseDeCalculoST, decimal valorBaseDeCalculoSt, decimal aliquotaSt, decimal valorImpostoSt, EMotivoDesoneracaoIcms motivoDesoneracaoIcms, decimal valorBaseDeCalculoStRetido, decimal valorImpostoStRetido, decimal percentualCreditoSimplesNacionalIcms, decimal valorImpostoCreditoSimplesNacionalIcms, decimal valorBaseDeCalculoFcp, decimal percentualFcp, decimal valorImpostoFcp, decimal valorOperacaoDiferimentoIcms, decimal percentualDiferimentoIcms, decimal valorImpostoDiferimentoIcms, ECodigoSituacaoTributariaIpi cstIpiSaida, decimal valorBaseDeCalculoIpi, decimal aliquotaIpi, decimal valorImpostoDiferimentoIpi, decimal valorQuantidadeTotalParaTributacaoIpi, decimal valorPorUnidadeTributavelIpi, ECodigoSituacaoTributariaPisCofins cstPis, decimal valorBaseDeCalculoPis, decimal aliquotaPis, decimal valorQuantidadeVendidaProdutoPis, decimal aliquotaPorUnidadeVendidaPis, decimal valorImpostoDiferimentoPis, ECodigoSituacaoTributariaPisCofins cstCofins, decimal valorBaseDeCalculoCofins, decimal aliquotaCofins, decimal valorQuantidadeVendidaProdutoCofins, decimal aliquotaPorUnidadeVendidaCofins, decimal valorImpostoDiferimentoCofins, ETipoReducaoBaseDeCalculo tipoReducaoIcms, ETipoReducaoBaseDeCalculo tipoReducaoIcmsSt, decimal valorBaseDeCalculoFcpSt, decimal percentualFcpSt, decimal valorImpostoFcpSt, decimal valorIcmsProprioSubistituto, decimal valorAliquotaIcmsInterna, decimal valorAliquotaIcmsInternaEstadual, int enquadramentoIpi, decimal valorReducaoIpiPercentual, bool ipiEmbutido, bool difalTipoCalculoPorDentro, ETipoReducaoBaseDeCalculo tipoReducaoIpi, EDeterminacaoBaseIcmsSt tipoCalculoBaseIcmsSt, decimal valorUnitFixadoIcmsSt, decimal valorBaseDeCalculoDifal, decimal valorImpostoDevidoDifal, decimal valorImpostoDevidoRecolherSt, decimal valorImpostoDevidoFcp, decimal valorIcmsIsento, decimal valorIcmsOutros, string? icmsObservacao, decimal valorIpiIsento, decimal valorIpiOutros, string? ipiObservacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraItemId = compraItemId;
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

        /// <summary>Porte fiel de CompraItemImposto.Alterar(): atualiza CSTs/alíquotas/modalidades definidas pelo usuário.</summary>
        public void Alterar(EOrigemMercadoria origem, ECodigoSituacaoTributariaIcms cstIcms, ECodigoSituacaoOperacaoSimplesNacional csosn, EModalidadeBaseDeCalculosIcms modalidadeDeterminacaoBaseCalculoIcms, decimal percentualReducaoBaseDeCalculoIcms, decimal aliquotaIcms, EModalidadeBaseDeCalculosST modalidadeBaseDeCalculosST, decimal percentualMvaBaseDeCalculoST, decimal percentualReducaoBaseDeCalculoST, decimal aliquotaSt, EMotivoDesoneracaoIcms motivoDesoneracaoIcms, decimal percentualCreditoSimplesNacionalIcms, ECodigoSituacaoTributariaIpi cstIpiSaida, decimal aliquotaIpi, ECodigoSituacaoTributariaPisCofins cstPis, decimal aliquotaPis, decimal aliquotaPorUnidadeVendidaPis, ECodigoSituacaoTributariaPisCofins cstCofins, decimal aliquotaCofins, decimal aliquotaPorUnidadeVendidaCofins, string usuario)
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
            MarcarAlterado(usuario);
        }

        /// <summary>Porte fiel de CompraItemImposto.DefinirValoresCalculados(): grava valores calculados pelo motor fiscal.</summary>
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
    }
}
