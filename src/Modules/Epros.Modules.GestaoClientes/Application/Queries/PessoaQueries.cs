using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Domain.Entities;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarPessoasQuery(
        string? Localizar,
        string? InscricaoEstadual,
        string? Uf,
        bool? EhCliente,
        bool? EhFornecedor,
        bool? EhTransportadora,
        bool? EhMotorista,
        bool? EhPrestadorServico,
        bool? EhFuncionario,
        bool? EhProdutorRural,
        EStatusPessoa? Status,
        int Pagina = 1,
        int TamanhoPagina = 10
    ) : IQuery<CommandResult>;

    public record ObterPessoaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public record LocalizarClienteQuery(string Termo) : IQuery<CommandResult>;
}
