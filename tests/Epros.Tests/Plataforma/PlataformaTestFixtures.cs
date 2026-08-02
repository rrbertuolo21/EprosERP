using Epros.Shared.Application.Contracts;

namespace Epros.Tests.Plataforma
{
    /// <summary>Fixtures compartilhadas dos testes da PLATAFORMA COMPARTILHADA (tenant/usuário fakes).</summary>
    internal static class PlataformaTestFixtures
    {
        internal sealed class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        internal sealed class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
