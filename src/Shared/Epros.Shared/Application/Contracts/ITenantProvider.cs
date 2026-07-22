namespace Epros.Shared.Application.Contracts
{
    public interface ITenantProvider
    {
        string GetTenantId();
        bool EhTenantDemo() => false;
    }
}
