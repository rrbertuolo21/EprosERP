using System.ComponentModel;


namespace Epros.ERP.Shared.Enums
{
    public enum EMotivoDesoneracaoIcms
    {
        [Description("Táxi")]
        Taxi = 1,
        [Description("Deficiente Físico (Revogado)")]
        DeficienteFisicoRevogado = 2,
        [Description("Produtor Agropecuário")]
        ProdutorAgropecuario = 3,
        [Description("Frotista/Locadora")]
        FrotistaLocadora = 4,
        [Description("Diplomático/Consular")]
        DiplomaticoConsular = 5,
        [Description("Utilitários e Motocicletas da Amazônia Ocidental e Áreas de Livre Comércio")]
        UtilitariosMotosAmazonia = 6,
        [Description("SUFRAMA")]
        Suframa = 7,
        [Description("Venda a Órgãos Públicos")]
        VendaOrgaosPublicos = 8,
        [Description("Outros")]
        Outros = 9,
        [Description("Deficiente Condutor (Convênio ICMS 38/12)")]
        DeficienteCondutor = 10,
        [Description("Deficiente Não Condutor (Convênio ICMS 38/12)")]
        DeficienteNaoCondutor = 11,
        [Description("Órgão de fomento e desenvolvimento agropecuário")]
        OrgaoFomentoDesenvolvimentoAgropecuario = 12,
        [Description("Olimpíadas Rio 2016")]
        OlimpiadasRio2016 = 16,
        [Description("Solicitado pelo Fisco")]
        SolicitadoPeloFisco = 90,
    }
}
