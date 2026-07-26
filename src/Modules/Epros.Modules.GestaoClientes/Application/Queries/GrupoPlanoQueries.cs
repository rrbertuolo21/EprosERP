using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarGruposPlanoQuery(
        int Pagina = 1,
        int TamanhoPagina = 25,
        string? Search = null
    ) : IQuery<PagedQueryResult<GrupoPlanoDto>>;

    public record ObterGrupoPlanoPorIdQuery(Guid Id) : IQuery<GrupoPlanoDto>;

    public class GrupoPlanoDto
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
