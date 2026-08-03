using System.ComponentModel;
namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Status do convite ao fornecedor (EF Portal do Fornecedor §11 — proposto, pendente MC).</summary>
    public enum EStatusConviteFornecedor
    {
        [Description("Rascunho")] Rascunho = 0,
        [Description("Enviado")] Enviado = 1,
        [Description("Aceito")] Aceito = 2,
        [Description("Expirado")] Expirado = 3,
        [Description("Cancelado")] Cancelado = 4
    }
}
