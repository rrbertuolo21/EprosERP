using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ConsultarCepQuery(string Cep) : IQuery<CepResultadoDto?>;

    public class CepResultadoDto
    {
        public string Cep { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Localidade { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public string Ibge { get; set; } = string.Empty;
        public string Gia { get; set; } = string.Empty;
        public string Ddd { get; set; } = string.Empty;
        public string Siafi { get; set; } = string.Empty;
        public System.Guid? MunicipioId { get; set; }
        public System.Guid PaisId { get; set; }
    }
}
