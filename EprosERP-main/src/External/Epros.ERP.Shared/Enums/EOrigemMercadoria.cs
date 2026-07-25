using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EOrigemMercadoria
    {
        [Description("0 - Nacional exceto as indicadas nos códigos 3, 4, 5 e 8")]
        OmNacional = 0,

        [Description("1 - Estrangeira - Importação direta")]
        OmEstrangeiraImportacaoDireta = 1,

        [Description("2 - Estrangeira - Adquirida no mercado interno")]
        OmEstrangeiraAdquiridaBrasil = 2,

        [Description("3 - Nacional, conteudo superior 40% e inferior ou igual a 70%")]
        OmNacionalConteudoImportacaoSuperior40 = 3,

        [Description("4 - Nacional, processos produtivos básicos")]
        OmNacionalProcessosBasicos = 4,

        [Description("5 - Nacional, conteudo inferior 40%")]
        OmNacionalConteudoImportacaoInferiorIgual40 = 5,

        [Description("6 - Estrangeira - Importação direta, com similar nacional, lista CAMEX")]
        OmEstrangeiraImportacaoDiretaSemSimilar = 6,

        [Description("7 - Estrangeira - mercado interno, sem simular,lista CAMEX")]
        OmEstrangeiraAdquiridaBrasilSemSimilar = 7,

        [Description("8 - Nacional, Conteúdo de Importação superior a 70%")]
        OmNacionalConteudoImportacaoSuperior70 = 8
    }
}
