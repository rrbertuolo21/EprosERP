using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Projetos.Application.Queries
{
    // PRJ-ENC (Encerramento de Projeto)
    public record ObterEncerramentosQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterEncerramentoPorIdQuery(Guid Id) : IQuery<CommandResult>;
    public record ObterEncerramentosPorProjetoQuery(Guid ProjetoId) : IQuery<CommandResult>;

    // PRJ-RSK (Gestao de Riscos de Projeto)
    public record ObterRiscosPorProjetoQuery(Guid ProjetoId, string? Prioridade) : IQuery<CommandResult>;
    public record ObterRiscoPorIdQuery(Guid Id) : IQuery<CommandResult>;
    public record ObterEstagiosRiscoQuery() : IQuery<CommandResult>;

    // PRJ-PRT (Portfolio e Priorizacao)
    public record ObterPortfoliosQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterPortfolioPorIdQuery(Guid Id) : IQuery<CommandResult>;
}
