using Epros.Shared.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace Epros.API.Providers
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public const string TenantIdItemKey = "TenantId";

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetTenantId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return "system";

            if (httpContext.Items.TryGetValue(TenantIdItemKey, out var tenantId) && tenantId is string tid)
            {
                return tid;
            }

            return "system";
        }

        public bool EhTenantDemo()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return false;

            if (httpContext.Items.TryGetValue("IsDemo", out var isDemoObj) && isDemoObj is bool isDemo)
            {
                return isDemo;
            }

            return false;
        }
    }
}
