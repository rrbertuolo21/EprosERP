using System.ComponentModel;
namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Status do pré-aviso de embarque/ASN (EF Portal do Fornecedor §11 — proposto).</summary>
    public enum EStatusPreAvisoEmbarque
    {
        [Description("Rascunho")] Rascunho = 0,
        [Description("Enviado")] Enviado = 1,
        [Description("Recebido parcial")] RecebidoParcial = 2,
        [Description("Recebido total")] RecebidoTotal = 3,
        [Description("Cancelado")] Cancelado = 4
    }
}
