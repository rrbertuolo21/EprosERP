using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EAnexoSimlesNacional
    {
        [Description("Não se Aplica")]
        NaoSeAplica = 0,

        [Description("Locação de bens móveis")]
        LocacaoDeBensMoveis = 1,

        [Description("Escritório de Serviços Contábeis")]
        EscritorioDeServicosContabeis = 2,

        [Description("ANEXO III - Ativ. não sujeita ao fator R")]
        AnexoIII = 3,

        [Description("ANEXO IV")]
        AnexoIV = 4,

        [Description("Ativ. sujeita ao fator \"r\" - ANEXO III ou V (conforme CGSN nº 94/2011 art. 25-A § 1º inciso V)")]
        AtividadeSujeitaAoFatorRAnexoIIIOuV = 5
    }
}
