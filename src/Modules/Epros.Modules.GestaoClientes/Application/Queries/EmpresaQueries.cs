using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarEmpresasQuery() : IQuery<CommandResult>;

    public record ObterEmpresaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public record ObterEmpresaPorCnpjQuery(string Cnpj) : IQuery<CommandResult>;

    public record ObterEmpresaPorCpfQuery(string Cpf) : IQuery<CommandResult>;

    public record ListarCertificadosEmpresaQuery(Guid EmpresaId) : IQuery<CommandResult>;
}
