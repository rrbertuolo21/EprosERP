using System;
using System.Collections.Generic;

namespace Epros.Modules.Aplicativo.Application.Dtos
{
    public record InstalacaoStateDto(
        bool IsCompleted,
        DateTime? CompletedAt,
        string? CompletedBy,
        bool DatabaseInitialized,
        bool AdminCreated,
        bool SystemSettingsSeeded
    );

    public record RequisitosCheckResultDto(
        bool Pass,
        IEnumerable<RequisitoItemDto> Itens
    );

    public record RequisitoItemDto(
        string Nome,
        bool Status,
        string Detalhe
    );

    public record UpdateLogDto(
        Guid Id,
        string VersaoAlvo,
        DateTime ExecutadoEm,
        string ExecutadoPor,
        bool Sucesso,
        string? Log
    );
}
