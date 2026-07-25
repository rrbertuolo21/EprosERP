using System.Collections.Generic;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    public record ObterInstalacaoStateQuery() : IQuery<InstalacaoStateDto>;

    public record VerificarRequisitosQuery() : IQuery<RequisitosCheckResultDto>;

    public record ListarUpdateLogsQuery() : IQuery<IEnumerable<UpdateLogDto>>;
}
