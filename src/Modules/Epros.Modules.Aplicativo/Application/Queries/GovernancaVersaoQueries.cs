using System;
using System.Collections.Generic;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    public record SolicitacaoUpgradeVersaoDto(
        Guid Id,
        string VersaoAtual,
        string VersaoAlvo,
        string Motivo,
        EStatusUpgradeVersao Status,
        string? SolicitadoPor,
        string? AprovadoPor,
        string? Comentario,
        DateTime? AprovadoEm,
        DateTime? ExecutadoEm,
        bool RollbackDisponivel,
        DateTime CriadoEm);

    public record ListarSolicitacoesUpgradeQuery() : IQuery<IEnumerable<SolicitacaoUpgradeVersaoDto>>;
}
