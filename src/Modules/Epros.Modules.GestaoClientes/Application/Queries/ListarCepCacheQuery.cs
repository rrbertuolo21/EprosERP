using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarCepCacheQuery() : IQuery<IEnumerable<CepCacheDto>>;

    public class CepCacheDto
    {
        public Guid Id { get; set; }
        public string CodigoPostal { get; set; } = string.Empty;
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? Uf { get; set; }
        public string? Provedor { get; set; }
        public DateTime ConsultadoEm { get; set; }
        public bool Falhou { get; set; }
        public string? MotivoFalha { get; set; }
    }
}
