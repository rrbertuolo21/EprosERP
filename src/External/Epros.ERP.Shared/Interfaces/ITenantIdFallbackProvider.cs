namespace Epros.ERP.Shared.Interfaces
{
    /// <summary>
    /// Tenant usado quando <see cref="ITenantProvider.GetTenantId"/> está vazio
    /// (ex.: processamento em background sem HttpContext).
    /// </summary>
    public interface ITenantIdFallbackProvider
    {
        /// <summary>Retorna null quando não há tenant de fallback aplicável.</summary>
        string? ObterTenantIdQuandoProviderPrincipalVazio();
    }
}
