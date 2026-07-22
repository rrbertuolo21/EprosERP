namespace Epros.Modules.Fiscal.Domain.Enums
{
    /// <summary>
    /// Tipo de emissão (tpEmis) de DF-e (NF-e/NFC-e). Valores IDÊNTICOS ao código oficial da SEFAZ e ao
    /// enum legado <c>Epros.ERP.Shared.Enums.ETipoEmissao</c> (motor imutável), para permitir o cast
    /// direto por inteiro para o <c>NFe.Classes...TipoEmissao</c> do Hercules na transmissão.
    ///
    /// Normal = emissão online contra o webservice da UF.
    /// Contingências online (SVC-AN, SVC-RS, EPEC) = transmissão redirecionada a servidor de contingência.
    /// Contingência offline (NFC-e, tpEmis=9) = documento emitido/assinado sem transmissão imediata,
    /// transmitido depois que a SEFAZ volta.
    /// </summary>
    public enum ETipoEmissaoFiscal
    {
        Normal = 1,
        ContingenciaFsIa = 2,
        ContingenciaScan = 3,
        ContingenciaEpec = 4,
        ContingenciaFsDa = 5,
        ContingenciaSvcAn = 6,
        ContingenciaSvcRs = 7,
        ContingenciaOffline = 9
    }
}
