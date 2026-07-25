using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ObterEmpresaConfiguracoesQuery() : IQuery<CommandResult>;
    public record ObterPreferenciasQuery() : IQuery<CommandResult>;
    public record ObterConfiguracaoEmailQuery() : IQuery<CommandResult>;
    public record ListarCategoriasQuery(string? NomeFiltro = null) : IQuery<CommandResult>;
    public record ListarUnidadesMedidaQuery() : IQuery<CommandResult>;
    public record ListarArmazensQuery() : IQuery<CommandResult>;
    public record ListarProjetosQuery() : IQuery<CommandResult>;
    public record ListarImpostosQuery() : IQuery<CommandResult>;
    public record ListarConversoesUnidadesQuery() : IQuery<CommandResult>;
    public record ListarLogsAuditoriaQuery() : IQuery<CommandResult>;
    public record ListarFusosHorariosQuery() : IQuery<CommandResult>;
    public record ListarMoedasQuery() : IQuery<CommandResult>;
}
