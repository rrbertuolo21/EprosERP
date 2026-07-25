using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Producao.Application.Queries
{
    public record ObterOrdensProducaoQuery() : IQuery<CommandResult>;

    public record ObterListaMateriaisQuery(string ProdutoAcabadoSku) : IQuery<CommandResult>;
}
