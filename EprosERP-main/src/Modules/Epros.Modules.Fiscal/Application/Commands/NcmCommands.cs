using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarNcmCommand(
        string CodigoNcm,
        string Descricao,
        DateTime DataInicio,
        DateTime? DataFim,
        string? TipoAtoIni,
        string? NumeroAtoIni,
        string? AnoAtoIni
    ) : ICommand;

    public record AtualizarNcmCommand(
        Guid Id,
        string CodigoNcm,
        string Descricao,
        DateTime DataInicio,
        DateTime? DataFim,
        string? TipoAtoIni,
        string? NumeroAtoIni,
        string? AnoAtoIni
    ) : ICommand;

    public record DeletarNcmCommand(Guid Id) : ICommand;

    public class CriarNcmCommandValidator : AbstractValidator<CriarNcmCommand>
    {
        public CriarNcmCommandValidator()
        {
            RuleFor(c => c.CodigoNcm)
                .NotEmpty().WithMessage("O código NCM é obrigatório.")
                .MaximumLength(8).WithMessage("O código NCM deve possuir no máximo 8 caracteres.");
            RuleFor(c => c.Descricao)
                .MaximumLength(1500).WithMessage("A descrição deve possuir no máximo 1500 caracteres.");
        }
    }

    public class AtualizarNcmCommandValidator : AbstractValidator<AtualizarNcmCommand>
    {
        public AtualizarNcmCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID do NCM é obrigatório.");
            RuleFor(c => c.CodigoNcm)
                .NotEmpty().WithMessage("O código NCM é obrigatório.")
                .MaximumLength(8).WithMessage("O código NCM deve possuir no máximo 8 caracteres.");
            RuleFor(c => c.Descricao)
                .MaximumLength(1500).WithMessage("A descrição deve possuir no máximo 1500 caracteres.");
        }
    }
}
