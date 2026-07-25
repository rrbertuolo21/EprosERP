using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ObterMunicipioPorIbgeQuery(long CodigoIbge) : IQuery<MunicipioDto?>;

    public class MunicipioDto
    {
        public Guid Id { get; set; }
        public Guid PaisId { get; set; }
        public Guid SubdivisaoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public long CodigoIbge { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool Ativo { get; set; }
        public string Uf { get; set; } = string.Empty;
        public string PaisNome { get; set; } = string.Empty;
    }
}
