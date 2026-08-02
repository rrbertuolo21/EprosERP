using System.ComponentModel;
namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Status da cotação publicada ao fornecedor (EF Portal do Fornecedor §11 — proposto).</summary>
    public enum EStatusCotacaoPublicada
    {
        [Description("Aberta")] Aberta = 0,
        [Description("Respondida")] Respondida = 1,
        [Description("Em análise")] EmAnalise = 2,
        [Description("Encerrada")] Encerrada = 3,
        [Description("Cancelada")] Cancelada = 4
    }
}
