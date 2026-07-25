using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Imobiliaria.Application.Queries
{
    /// <summary>Lista os imoveis do tenant (EF 25).</summary>
    public record ListarImoveisQuery() : IQuery<CommandResult>;

    /// <summary>Consulta um imovel e seus agregados (EF 7.1, RN-004).</summary>
    public record ObterImovelQuery(Guid ImovelId) : IQuery<CommandResult>;

    /// <summary>Lista contratos de servico (EF 7.2, RN-010).</summary>
    public record ListarContratosServicoQuery() : IQuery<CommandResult>;

    /// <summary>Lista locacoes, com filtro opcional por periodo (EF 25, RN-017).</summary>
    public record ListarLocacoesQuery(DateTime? PeriodoDe, DateTime? PeriodoAte) : IQuery<CommandResult>;

    /// <summary>Produz o resumo do aluguel para localizar o recebivel (EF 7.4, RN-015/RN-022).</summary>
    public record ObterResumoAluguelQuery(Guid LocacaoId) : IQuery<CommandResult>;
}
