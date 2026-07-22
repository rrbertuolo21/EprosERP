using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarPaisesQuery() : IQuery<IEnumerable<PaisDto>>;

    public class PaisDto
    {
        public System.Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CodigoIsoAlpha2 { get; set; } = string.Empty;
        public string CodigoIsoAlpha3 { get; set; } = string.Empty;
        public string CodigoNumerico { get; set; } = string.Empty;
        public string? Capital { get; set; }
        public string? CodigoDiscagem { get; set; }
        public bool Ativo { get; set; }
    }
}
