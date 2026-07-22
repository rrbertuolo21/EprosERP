using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Aplicativo.Application.Dtos;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    public record ListarPedidosClienteQuery(
        int PageIndex = 1,
        int PageSize = 50
    ) : IQuery<IEnumerable<PedidoSaaSDto>>;

    public record ObterTransferenciasPendentesQuery(
        int PageIndex = 1,
        int PageSize = 50
    ) : IQuery<IEnumerable<TransferenciaPendenteDto>>;
}
