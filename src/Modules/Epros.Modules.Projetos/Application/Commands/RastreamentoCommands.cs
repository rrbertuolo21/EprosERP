using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Projetos.Application.Commands
{
    // ===== PRJ-RST (Planejamento e Rastreamento) =====

    public record CriarEstagioTarefaCommand(string Nome, string? Cor, bool IndicadorConclusao, int Ordem) : ICommand;

    public record CriarTarefaProjetoCommand(
        Guid ProjetoId,
        string Titulo,
        string? Descricao,
        Guid? EstagioId,
        Guid? MarcoId,
        string? Prioridade,
        DateTime? DataInicio,
        DateTime? DataTermino,
        decimal? Duracao,
        decimal? EsforcoEstimado,
        Guid? TarefaSuperiorId,
        bool IndicadorMarco,
        string? Visibilidade,
        int Ordem
    ) : ICommand;

    /// <summary>PRJ-RST-RN-019: mover tarefa entre estagios/posicoes.</summary>
    public record MoverTarefaQuadroCommand(Guid TarefaId, Guid EstagioId, int NovaOrdem) : ICommand;

    /// <summary>PRJ-RST-RN-009: atualizar progresso da tarefa.</summary>
    public record AtualizarProgressoTarefaProjetoCommand(Guid TarefaId, decimal PercentualConcluido) : ICommand;

    /// <summary>PRJ-RST-RN-013: concluir tarefa (bloqueada por dependencia nao conclui).</summary>
    public record ConcluirTarefaProjetoCommand(Guid TarefaId) : ICommand;

    public record CriarDependenciaTarefaCommand(
        Guid TarefaDependenteId,
        Guid TarefaPredecessoraId,
        ETipoDependencia TipoDependencia,
        string? Observacao
    ) : ICommand;
}
