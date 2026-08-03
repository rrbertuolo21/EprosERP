using System.ComponentModel;
namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Status da resposta de cotação do fornecedor (EF Portal do Fornecedor §11 — proposto).</summary>
    public enum EStatusRespostaCotacao
    {
        [Description("Rascunho")] Rascunho = 0,
        [Description("Enviada")] Enviada = 1,
        [Description("Aceita")] Aceita = 2,
        [Description("Rejeitada")] Rejeitada = 3,
        [Description("Substituída")] Substituida = 4
    }
}
