using System.ComponentModel;
namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Status do documento enviado pelo fornecedor (EF Portal do Fornecedor §11 — proposto).</summary>
    public enum EStatusDocumentoFornecedor
    {
        [Description("Enviado")] Enviado = 0,
        [Description("Em análise")] EmAnalise = 1,
        [Description("Aceito")] Aceito = 2,
        [Description("Rejeitado")] Rejeitado = 3,
        [Description("Cancelado")] Cancelado = 4
    }
}
