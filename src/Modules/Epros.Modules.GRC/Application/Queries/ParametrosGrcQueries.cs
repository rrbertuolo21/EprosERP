using System.Collections.Generic;
using MediatR;

namespace Epros.Modules.GRC.Application.Queries
{
    /// <summary>Lista os parametros de um submodulo GRC para o tenant atual (D-TEC-04).</summary>
    public record ObterParametrosGrcQuery(string Submodulo) : IRequest<IReadOnlyList<ParametroGrcDto>>;

    public record ParametroGrcDto(string Chave, string ValorJson, bool Ativo);
}
