namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Situação da compra/venda (documento fiscal). Porte fiel do legado Epros.ERP.Shared.Enums.EVendaStatus.
    /// </summary>
    public enum EVendaStatus
    {
        Salvar = 0,
        SalvarTransmitir = 1,
        Transmitido = 2,
        SalvarImprimirMei = 3,
        Errado = 9
    }
}
