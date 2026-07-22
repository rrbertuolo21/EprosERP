namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Status interno do documento fiscal. Porte fiel do legado
    /// Epros.ERP.Shared.Enums.EDocumentoFiscalStatus.
    /// </summary>
    public enum EDocumentoFiscalStatus
    {
        Recebido = 0,
        Autorizado = 1,
        Rejeitado = 2,
        Cancelado = 3
    }
}
