using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarZonasEntregaQuery() : IQuery<IEnumerable<ZonaEntregaDto>>;

    public class ZonaEntregaDto
    {
        public System.Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CepInicio { get; set; } = string.Empty;
        public string CepFim { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }
}
