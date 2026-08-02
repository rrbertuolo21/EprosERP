using System.ComponentModel;

namespace Epros.Shared.Domain.Enums
{
    /// <summary>
    /// TRANSVERSAL T10 — estado de assinatura de um documento do GED canônico.
    /// Sem certificado ICP/provedor (dependência externa) o documento permanece
    /// <see cref="PendenteAssinatura"/> — NUNCA se inventa validade jurídica.
    /// </summary>
    public enum EStatusAssinaturaDocumento
    {
        [Description("Não requer assinatura")]
        NaoRequerAssinatura = 1,

        [Description("Pendente de assinatura")]
        PendenteAssinatura = 2,

        [Description("Assinado")]
        Assinado = 3,

        [Description("Assinatura recusada")]
        Recusado = 4
    }
}
