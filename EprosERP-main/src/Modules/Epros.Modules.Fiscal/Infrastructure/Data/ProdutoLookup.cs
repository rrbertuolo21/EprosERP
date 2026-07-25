using System;

namespace Epros.Modules.Fiscal.Infrastructure.Data
{
    public class ProdutoLookup
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
    }
}
