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

    // ===== MM-d: workflow do mestre exposto (submeter/aprovar/suspender/retomar/encerrar/inativar) =====
    public record SubmeterProjetoCommand(Guid ProjetoId) : ICommand;
    public record AprovarProjetoCommand(Guid ProjetoId) : ICommand;
    public record SuspenderProjetoCommand(Guid ProjetoId) : ICommand;
    public record RetomarProjetoCommand(Guid ProjetoId) : ICommand;
    public record EncerrarProjetoCommand(Guid ProjetoId, string Motivo) : ICommand;
    public record InativarProjetoCommand(Guid ProjetoId) : ICommand;

    /// <summary>RN-DEF-012/013: converte o projeto em template (ParaTemplate=true) ou de volta em projeto normal.</summary>
    public record ConverterProjetoTemplateCommand(Guid ProjetoId, bool ParaTemplate) : ICommand;
}
