using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    // RH-DEV — Desenvolvimento de Funcionarios.

    public record RegistrarPromocaoCommand(
        Guid ColaboradorId,
        Guid? FilialAnteriorId,
        Guid? DepartamentoAnteriorId,
        Guid? CargoAnteriorId,
        Guid? FilialAtualId,
        Guid? DepartamentoAtualId,
        Guid? CargoAtualId,
        DateTime? DataEfetiva,
        string? Motivo,
        string? Documento
    ) : ICommand;

    public class RegistrarPromocaoCommandValidator : AbstractValidator<RegistrarPromocaoCommand>
    {
        public RegistrarPromocaoCommandValidator()
        {
            RuleFor(c => c.ColaboradorId).NotEmpty().WithMessage("A promocao deve referenciar um colaborador (DEV secao 14).");
        }
    }

    public record AprovarPromocaoCommand(Guid PromocaoId) : ICommand;
    public record RejeitarPromocaoCommand(Guid PromocaoId) : ICommand;

    public record RegistrarAdvertenciaCommand(
        Guid ColaboradorId,
        Guid? TipoAdvertenciaId,
        string? Assunto,
        string? Severidade,
        DateTime? DataAdvertencia,
        string? Descricao,
        string? Documento,
        Guid? AdvertidoPor
    ) : ICommand;

    public class RegistrarAdvertenciaCommandValidator : AbstractValidator<RegistrarAdvertenciaCommand>
    {
        public RegistrarAdvertenciaCommandValidator()
        {
            RuleFor(c => c.ColaboradorId).NotEmpty().WithMessage("A advertencia deve referenciar um colaborador (DEV secao 17).");
        }
    }

    public record RegistrarDesligamentoCommand(
        Guid ColaboradorId,
        Guid? TipoDesligamentoId,
        DateTime? DataAviso,
        DateTime? DataDesligamento,
        string? Motivo,
        string? Descricao,
        string? Documento
    ) : ICommand;
}
