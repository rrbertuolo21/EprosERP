using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EModalidadeFrete
    {
        [Description("Contratação do Frete por conta do Remetente (CIF)")]
        mfContaEmitenteOumfContaRemetente = 0,

        [Description("Contratação do Frete por conta do Destinatário(FOB)")]
        mfContaDestinatario = 1,

        [Description("Contratação do Frete por conta de Terceiros")]
        mfContaTerceiros = 2,

        [Description("Transporte Próprio por conta do Remetente")]
        mfProprioContaRemente = 3,

        [Description("Transporte Próprio por conta do Destinatário")]
        mfProprioContaDestinatario = 4,

        [Description("Sem Ocorrência de Transporte")]
        mfSemFrete = 9
    }
}
