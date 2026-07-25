using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Projetos.Application.Commands
{
    // ===== PRJ-REC (Gestao de Recursos) =====

    /// <summary>RN-REC-001..005: apontamento de horas.</summary>
    public record RegistrarApontamentoCommand(
        Guid? UsuarioId,
        Guid? ProjetoId,
        Guid? TarefaId,
        DateTime Data,
        int Horas,
        int Minutos,
        string? Notas,
        ETimesheetTipo Tipo
    ) : ICommand;

    public record AprovarApontamentoCommand(Guid TimesheetId) : ICommand;

    public record SubmeterApontamentoCommand(Guid TimesheetId) : ICommand;

    /// <summary>RN-REC-010: alocacao de recurso a projeto/tarefa.</summary>
    public record CriarAlocacaoRecursoCommand(
        Guid RecursoId,
        Guid ProjetoId,
        Guid? TarefaId,
        string? PapelNoProjeto,
        DateTime? DataInicio,
        DateTime? DataFim,
        decimal? CargaPlanejadaHoras
    ) : ICommand;
}
