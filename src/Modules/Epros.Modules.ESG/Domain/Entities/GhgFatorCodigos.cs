namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>
    /// Codigos canonicos de fator de emissao esperados pelos calculos automaticos (event handlers de
    /// venda/compra). Referenciam linhas do catalogo versionado <c>esg.ghg_fator_emissao</c>
    /// (FatorEmissaoGee), populado a partir de base oficial homologada — NUNCA constante de valor.
    ///
    /// Fonte de negocio: skill <c>Negocio-acumulado/esg-carbono</c> (GHG Protocol Scope 3):
    ///   - cat. 9 "Transporte e distribuicao downstream" (venda faturada);
    ///   - cat. 1 "Bens e servicos comprados" (compra lancada).
    /// O valor/versao/vigencia de cada fator vem da tabela oficial (MCTI-SIN / IPCC / DEFRA) e exige
    /// // valida-humano antes do go-live. Enquanto nao populado, o calculo fica "pendente de fator".
    /// </summary>
    public static class GhgFatorCodigos
    {
        /// <summary>Escopo 3 · cat. 9 — Transporte e distribuicao downstream, por peca transportada (venda).</summary>
        public const string TransporteDistribuicaoDownstreamPorPeca = "S3_TRANSP_DIST_DOWNSTREAM_PC";

        /// <summary>Escopo 3 · cat. 1 — Bens e servicos adquiridos, por peca adquirida (compra).</summary>
        public const string BensAdquiridosPorPeca = "S3_BENS_ADQUIRIDOS_PC";

        /// <summary>
        /// Escopo 3 · cat. 4/9 — Transporte, por tonelada-quilometro (tkm). Fator COMPARTILHADO com o
        /// GHG (NF-07): o TSU le do mesmo catalogo versionado esg.ghg_fator_emissao, evitando dupla
        /// contagem (NF-04). Valor por modal/combustivel vem da base oficial homologada — // valida-humano.
        /// </summary>
        public const string TransportePorTkm = "S3_TRANSPORTE_TKM";
    }
}
