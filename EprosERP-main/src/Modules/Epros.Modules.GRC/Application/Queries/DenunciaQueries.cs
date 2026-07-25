using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GRC.Application.Queries
{
    // GRC-DEN — Investigacoes e Denuncias

    public record ObterCategoriasDenunciaQuery() : IQuery<CommandResult>;

    public record ObterDenunciaDetalheQuery(Guid DenunciaId) : IQuery<CommandResult>;

    /// <summary>RN-DEN-005: por padrao nao expoe respostas internas ao denunciante.</summary>
    public record ObterRespostasDenunciaQuery(Guid DenunciaId, bool IncluirInternas) : IQuery<CommandResult>;

    public record ObterParticipantesDenunciaQuery(Guid DenunciaId) : IQuery<CommandResult>;

    public record ObterInvestigacoesDenunciaQuery(Guid DenunciaId) : IQuery<CommandResult>;

    public record ObterParametrosDenunciaQuery() : IQuery<CommandResult>;
}
