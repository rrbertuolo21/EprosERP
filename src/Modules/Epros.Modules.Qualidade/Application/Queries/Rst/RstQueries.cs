using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Qualidade.Application.Queries.Rst
{
    /// <summary>Lista campanhas de recall, com filtro por etapa.</summary>
    public record ListarCampanhasRecallQuery(string? Etapa, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    /// <summary>Monta a arvore de genealogia (MP->WIP->PA) da campanha via motor de genealogia.</summary>
    public record ObterGenealogiaRecallQuery(Guid CampanhaId) : IQuery<CommandResult>;
}
