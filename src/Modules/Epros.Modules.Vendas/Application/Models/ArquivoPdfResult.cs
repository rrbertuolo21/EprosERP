namespace Epros.Modules.Vendas.Application.Models
{
    /// <summary>
    /// DTO simples para transportar bytes de um PDF (cupom não fiscal / DANFE pré-visualização)
    /// dentro de CommandResult.Dados, permitindo que o controller responda com File(...).
    /// </summary>
    public sealed record ArquivoPdfResult(byte[] Conteudo, string NomeArquivo, string TipoConteudo = "application/pdf");
}
