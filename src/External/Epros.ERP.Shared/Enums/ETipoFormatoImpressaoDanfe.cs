

using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoFormatoImpressaoDanfe
    {
        [Description("Sem geração de DANFE")]
        SemGeracaoDanfe = 0,
        [Description("DANFE normal, Retrato")]
        DanfeNormalRetrato = 1,
        [Description("DANFE normal, Paisagem")]
        DanfeNormalPaisagem = 2,
        [Description("DANFE Simplificado")]
        DanfeSimplificado = 3,
        [Description("DANFE NFC-e")]
        DanfeNfce = 4,
        [Description("DANFE NFC-e em mensagem eletrônica (o envio de mensagem eletrônica pode ser feita de forma simultânea com a impressão do DANFE; usar o tpImp - 5 quando esta for a única forma de disponibilização do DANFE).")]
        DanfeNfceEmMensagemEletronica = 5
    }
}
