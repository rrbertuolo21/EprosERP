using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaConfiguracao (configuração de emissão do documento fiscal).
    /// FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaConfiguracao : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public ETipoOperacaoNfe TipoOperacao { get; private set; } = ETipoOperacaoNfe.Saida;
        public ETipoFormatoImpressaoDanfe TipoFormatoImpressaoDanfe { get; private set; }
        public ETipoEmissao TipoEmissao { get; private set; }
        public ETipoAmbiente TipoAmbiente { get; private set; }
        public EFinalidadeEmissao FinalidadeEmissao { get; private set; }
        public EIndicadorFinalidadeOperacao IndicadorFinalidadeOperacao { get; private set; }
        public ETipoAtendimento TipoAtendimento { get; private set; }
        public EIndicadorIntermediadorMarketplace IndicadorIntermediadorMarketplace { get; private set; }

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaConfiguracao() { } // EF Core

        public VendaConfiguracao(Guid vendaId, ETipoOperacaoNfe tipoOperacao, ETipoFormatoImpressaoDanfe tipoFormatoImpressaoDanfe, ETipoEmissao tipoEmissao, ETipoAmbiente tipoAmbiente, EFinalidadeEmissao finalidadeEmissao, EIndicadorFinalidadeOperacao indicadorFinalidadeOperacao, ETipoAtendimento tipoAtendimento, EIndicadorIntermediadorMarketplace indicadorIntermediadorMarketplace,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            TipoOperacao = tipoOperacao;
            TipoFormatoImpressaoDanfe = tipoFormatoImpressaoDanfe;
            TipoEmissao = tipoEmissao;
            TipoAmbiente = tipoAmbiente;
            FinalidadeEmissao = finalidadeEmissao;
            IndicadorFinalidadeOperacao = indicadorFinalidadeOperacao;
            TipoAtendimento = tipoAtendimento;
            IndicadorIntermediadorMarketplace = indicadorIntermediadorMarketplace;
        }

        public void Alterar(ETipoOperacaoNfe tipoOperacao, ETipoFormatoImpressaoDanfe tipoFormatoImpressaoDanfe, ETipoEmissao tipoEmissao, ETipoAmbiente tipoAmbiente, EFinalidadeEmissao finalidadeEmissao, EIndicadorFinalidadeOperacao indicadorFinalidadeOperacao, ETipoAtendimento tipoAtendimento, EIndicadorIntermediadorMarketplace indicadorIntermediadorMarketplace, string alteradoPor)
        {
            TipoOperacao = tipoOperacao;
            TipoFormatoImpressaoDanfe = tipoFormatoImpressaoDanfe;
            TipoEmissao = tipoEmissao;
            TipoAmbiente = tipoAmbiente;
            FinalidadeEmissao = finalidadeEmissao;
            IndicadorFinalidadeOperacao = indicadorFinalidadeOperacao;
            TipoAtendimento = tipoAtendimento;
            IndicadorIntermediadorMarketplace = indicadorIntermediadorMarketplace;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaConfiguracao.Duplicar (novo Id/FK).</summary>
        public VendaConfiguracao Duplicar(Guid novaVendaId, string criadoPor)
            => new(novaVendaId, TipoOperacao, TipoFormatoImpressaoDanfe, TipoEmissao, TipoAmbiente, FinalidadeEmissao,
                   IndicadorFinalidadeOperacao, TipoAtendimento, IndicadorIntermediadorMarketplace, TenantId, criadoPor);

        /// <summary>Porte fiel de VendaConfiguracao.Validar.</summary>
        public void Validar()
        {
            if (!Enum.IsDefined(typeof(ETipoOperacaoNfe), TipoOperacao))
                AddNotification("TipoOperacao", "Tipo operação informado na venda inválido ");
            if (!Enum.IsDefined(typeof(ETipoFormatoImpressaoDanfe), TipoFormatoImpressaoDanfe))
                AddNotification("TipoFormatoImpressaoDanfe", "Tipo formato impressão Danfe informado na venda inválido ");
            if (!Enum.IsDefined(typeof(ETipoEmissao), TipoEmissao))
                AddNotification("TipoEmissao", "Tipo emissão informado na venda inválido ");
            if (!Enum.IsDefined(typeof(ETipoAmbiente), TipoAmbiente))
                AddNotification("TipoAmbiente", "Tipo ambiente informado na venda inválido ");
            if (!Enum.IsDefined(typeof(EFinalidadeEmissao), FinalidadeEmissao))
                AddNotification("FinalidadeEmissao", "Finalidade emissão informado na venda inválido ");
            if (!Enum.IsDefined(typeof(EIndicadorFinalidadeOperacao), IndicadorFinalidadeOperacao))
                AddNotification("IndicadorFinalidadeOperacao", "Indicador finalidade operação informado na venda inválido ");
            if (!Enum.IsDefined(typeof(ETipoAtendimento), TipoAtendimento))
                AddNotification("TipoAtendimento", "Tipo atendimento informado na venda inválido ");
            if (!Enum.IsDefined(typeof(EIndicadorIntermediadorMarketplace), IndicadorIntermediadorMarketplace))
                AddNotification("IndicadorIntermediadorMarketplace", "Indicador intermediador marketplace informado na venda inválido ");
        }
    }
}
