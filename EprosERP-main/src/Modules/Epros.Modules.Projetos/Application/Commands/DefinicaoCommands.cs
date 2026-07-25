using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Projetos.Application.Commands
{
    // ===== PRJ-DEF (Definicao de Projeto) — enriquecimento do cadastro mestre =====

    /// <summary>RN-DEF-005: vincular cliente ao projeto.</summary>
    public record VincularClienteProjetoCommand(Guid ProjetoId, Guid ClienteId) : ICommand;

    /// <summary>RN-DEF-004/006/007: vincular membro/gestor ao projeto.</summary>
    public record VincularMembroProjetoCommand(Guid ProjetoId, Guid UsuarioId, string Papel) : ICommand;

    /// <summary>RN-DEF-019: registrar atividade (ActivityLog) do projeto.</summary>
    public record RegistrarAtividadeProjetoCommand(Guid ProjetoId, Guid? UsuarioId, string? TipoUsuario, string TipoAtividade, string? Observacao) : ICommand;

    /// <summary>RN-DEF-020: anexar arquivo ao projeto.</summary>
    public record AnexarArquivoProjetoCommand(Guid ProjetoId, string NomeArquivo, string? CaminhoArquivo, Guid? ArquivoId) : ICommand;

    /// <summary>RN-DEF-015/016: duplicar projeto com nome derivado.</summary>
    public record DuplicarProjetoCommand(Guid ProjetoOrigemId) : ICommand;
}
