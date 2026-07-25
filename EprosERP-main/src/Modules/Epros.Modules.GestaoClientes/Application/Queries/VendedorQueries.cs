using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarVendedoresQuery(
        int Pagina = 1,
        int TamanhoPagina = 25,
        string? Search = null
    ) : IQuery<PagedQueryResult<VendedorDto>>;

    public record ObterVendedorPorIdQuery(Guid Id) : IQuery<VendedorDto>;

    public class VendedorDto
    {
        public Guid Id { get; set; }
        public Guid? RevendaId { get; set; }
        public string? RevendaNome { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public decimal PercentualComissao { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
