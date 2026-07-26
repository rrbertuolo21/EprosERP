using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Projetos.Application.Queries
{
    // PRJ-ORC
    public record ObterOrcamentosPorProjetoQuery(Guid ProjetoId) : IQuery<CommandResult>;
    public record ObterOrcamentoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    // PRJ-REC
    public record ObterApontamentosQuery(Guid? ProjetoId, Guid? UsuarioId) : IQuery<CommandResult>;
    public record ObterAlocacoesRecursoQuery(Guid ProjetoId) : IQuery<CommandResult>;

    // PRJ-RST
    public record ObterTarefasPorProjetoQuery(Guid ProjetoId) : IQuery<CommandResult>;
    public record ObterTarefaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    // PRJ-FAT
    public record ObterFaturamentosQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterFaturamentoPorIdQuery(Guid Id) : IQuery<CommandResult>;
}
