using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Configuração fiscal da compra (tipo operação, DANFE, emissão, ambiente etc.). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraConfiguracao.
    /// </summary>
    public class CompraConfiguracao : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public ETipoOperacaoNfe TipoOperacao { get; private set; } = ETipoOperacaoNfe.Entrada;
        public ETipoFormatoImpressaoDanfe TipoFormatoImpressaoDanfe { get; private set; }
        public ETipoEmissao TipoEmissao { get; private set; }
        public ETipoAmbiente TipoAmbiente { get; private set; }
        public EFinalidadeEmissao FinalidadeEmissao { get; private set; }
        public EIndicadorFinalidadeOperacao IndicadorFinalidadeOperacao { get; private set; }
        public ETipoAtendimento TipoAtendimento { get; private set; }
        public EIndicadorIntermediadorMarketplace IndicadorIntermediadorMarketplace { get; private set; }

        // Navegação intra-módulo
        public Compra? Compra { get; private set; }

        protected CompraConfiguracao() { } // EF Core

        public CompraConfiguracao(Guid compraId, ETipoOperacaoNfe tipoOperacao, ETipoFormatoImpressaoDanfe tipoFormatoImpressaoDanfe, ETipoEmissao tipoEmissao, ETipoAmbiente tipoAmbiente, EFinalidadeEmissao finalidadeEmissao, EIndicadorFinalidadeOperacao indicadorFinalidadeOperacao, ETipoAtendimento tipoAtendimento, EIndicadorIntermediadorMarketplace indicadorIntermediadorMarketplace, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            TipoOperacao = tipoOperacao;
            TipoFormatoImpressaoDanfe = tipoFormatoImpressaoDanfe;
            TipoEmissao = tipoEmissao;
            TipoAmbiente = tipoAmbiente;
            FinalidadeEmissao = finalidadeEmissao;
            IndicadorFinalidadeOperacao = indicadorFinalidadeOperacao;
            TipoAtendimento = tipoAtendimento;
            IndicadorIntermediadorMarketplace = indicadorIntermediadorMarketplace;
        }

        public void Alterar(ETipoOperacaoNfe tipoOperacao, ETipoFormatoImpressaoDanfe tipoFormatoImpressaoDanfe, ETipoEmissao tipoEmissao, ETipoAmbiente tipoAmbiente, EFinalidadeEmissao finalidadeEmissao, EIndicadorFinalidadeOperacao indicadorFinalidadeOperacao, ETipoAtendimento tipoAtendimento, EIndicadorIntermediadorMarketplace indicadorIntermediadorMarketplace, string usuario)
        {
            TipoOperacao = tipoOperacao;
            TipoFormatoImpressaoDanfe = tipoFormatoImpressaoDanfe;
            TipoEmissao = tipoEmissao;
            TipoAmbiente = tipoAmbiente;
            FinalidadeEmissao = finalidadeEmissao;
            IndicadorFinalidadeOperacao = indicadorFinalidadeOperacao;
            TipoAtendimento = tipoAtendimento;
            IndicadorIntermediadorMarketplace = indicadorIntermediadorMarketplace;
            MarcarAlterado(usuario);
        }
    }
}
