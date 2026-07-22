using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ObterClienteDetalhadoQuery(Guid Id) : IQuery<ClienteDetalhadoDto>;

    public class ClienteDetalhadoDto
    {
        public Guid Id { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid PlanoId { get; set; }
        public string PlanoNome { get; set; } = string.Empty;
        public Guid? RevendaId { get; set; }
        public string? RevendaNome { get; set; }
        public Guid? VendedorId { get; set; }
        public string? VendedorNome { get; set; }
        public int DiaVencimento { get; set; }
        public string StatusSaaS { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public string? Telefone { get; set; }
        public string? NomeContato { get; set; }
        public bool IsDemo { get; set; }
        public string? TokenAcesso { get; set; }
        public List<ClienteEnderecoDto> Enderecos { get; set; } = new();
        public List<ClienteComposicaoDto> Composicoes { get; set; } = new();
    }

    public class ClienteEnderecoDto
    {
        public Guid Id { get; set; }
        public int TipoEndereco { get; set; } // ETipoEndereco value
        public Guid PaisId { get; set; }
        public Guid MunicipioId { get; set; }
        public string MunicipioNome { get; set; } = string.Empty;
        public Guid? SubdivisaoId { get; set; }
        public string Uf { get; set; } = string.Empty;
        public string? Cep { get; set; }
        public string Logradouro { get; set; } = string.Empty;
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public string? Referencia { get; set; }
    }

    public class ClienteComposicaoDto
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataInicial { get; set; }
        public DateTime? DataFinal { get; set; }
        public bool PodeReajustar { get; set; }
    }
}
