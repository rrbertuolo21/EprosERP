using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Qualidade.Application.Queries.Qps
{
    /// <summary>Lista registros de qualidade de fornecedor, com filtro por status de homologacao.</summary>
    public record ListarQpsRegistrosQuery(string? StatusHomologacao, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
}
