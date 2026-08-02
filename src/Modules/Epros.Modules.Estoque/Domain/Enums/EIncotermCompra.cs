namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Incoterm da operação de importação (CD1 / EF COMERCIO_EXTERIOR §5.3, CEX-005). Termo internacional
    /// que define responsabilidades de custo/risco entre exportador e importador. NaoInformado = operação
    /// não declara incoterm (compra nacional ou não preenchido). O incoterm NÃO altera cálculo fiscal
    /// (valida-contador); é dado factual da operação.
    /// </summary>
    public enum EIncotermCompra
    {
        NaoInformado = 0,
        EXW = 1,   // Ex Works
        FCA = 2,   // Free Carrier
        FAS = 3,   // Free Alongside Ship
        FOB = 4,   // Free On Board
        CFR = 5,   // Cost and Freight
        CIF = 6,   // Cost, Insurance and Freight
        CPT = 7,   // Carriage Paid To
        CIP = 8,   // Carriage and Insurance Paid To
        DAP = 9,   // Delivered At Place
        DPU = 10,  // Delivered at Place Unloaded
        DDP = 11   // Delivered Duty Paid
    }
}
