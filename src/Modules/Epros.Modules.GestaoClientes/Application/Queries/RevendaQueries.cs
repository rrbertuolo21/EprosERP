using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarRevendasQuery(
        int Pagina = 1,
        int TamanhoPagina = 25,
        string? Search = null
    ) : IQuery<PagedQueryResult<RevendaDto>>;

    public record ObterRevendaPorIdQuery(Guid Id) : IQuery<RevendaDto>;

    public class RevendaDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal PercentualComissao { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public record PagedQueryResult<T>(
        IEnumerable<T> Items,
        int TotalRegistros,
        int TotalPaginas
    );
}
