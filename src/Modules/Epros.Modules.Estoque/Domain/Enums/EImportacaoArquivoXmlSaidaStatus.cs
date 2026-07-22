namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Status do processamento em lote de arquivos XML de saída. Porte fiel do legado
    /// Epros.ERP.Shared.Enums.EImportacaoArquivoXmlSaidaStatus.
    /// </summary>
    public enum EImportacaoArquivoXmlSaidaStatus
    {
        Verificando,
        Processando,
        Finalizado,
        Erro
    }
}
