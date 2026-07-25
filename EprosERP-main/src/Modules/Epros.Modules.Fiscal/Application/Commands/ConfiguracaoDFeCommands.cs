using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarConfiguracaoDFeCommand(
        Guid EmpresaId,
        string? NFeSerieProducao,
        string? NFeUltimoNrProducao,
        string? NFeSerieHomologacao,
        string? NFeUltimoNrHomologacao,
        string? NfceCscProducao,
        string? NfceIdCscProducao,
        string? NfceSerieProducao,
        string? NfceUltimoNrProducao,
        string? NfceCscHomologacao,
        string? NfceIdCscHomologacao,
        string? NfceSerieHomologacao,
        string? NfceUltimoNrHomologacao
    ) : ICommand;

    public record AtualizarConfiguracaoDFeCommand(
        Guid Id,
        string? NFeSerieProducao,
        string? NFeUltimoNrProducao,
        string? NFeSerieHomologacao,
        string? NFeUltimoNrHomologacao,
        string? NfceCscProducao,
        string? NfceIdCscProducao,
        string? NfceSerieProducao,
        string? NfceUltimoNrProducao,
        string? NfceCscHomologacao,
        string? NfceIdCscHomologacao,
        string? NfceSerieHomologacao,
        string? NfceUltimoNrHomologacao
    ) : ICommand;

    public record DeletarConfiguracaoDFeCommand(Guid Id) : ICommand;

    public class CriarConfiguracaoDFeCommandValidator : AbstractValidator<CriarConfiguracaoDFeCommand>
    {
        public CriarConfiguracaoDFeCommandValidator()
        {
            RuleFor(c => c.EmpresaId).NotEmpty().WithMessage("A empresa é obrigatória.");
            RuleFor(c => c.NFeSerieProducao).MaximumLength(3).WithMessage("A série da NF-e (produção) deve possuir no máximo 3 caracteres.");
            RuleFor(c => c.NFeSerieHomologacao).MaximumLength(3).WithMessage("A série da NF-e (homologação) deve possuir no máximo 3 caracteres.");
            RuleFor(c => c.NfceSerieProducao).MaximumLength(3).WithMessage("A série da NFC-e (produção) deve possuir no máximo 3 caracteres.");
            RuleFor(c => c.NfceSerieHomologacao).MaximumLength(3).WithMessage("A série da NFC-e (homologação) deve possuir no máximo 3 caracteres.");
        }
    }

    public class AtualizarConfiguracaoDFeCommandValidator : AbstractValidator<AtualizarConfiguracaoDFeCommand>
    {
        public AtualizarConfiguracaoDFeCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID é obrigatório.");
            RuleFor(c => c.NFeSerieProducao).MaximumLength(3).WithMessage("A série da NF-e (produção) deve possuir no máximo 3 caracteres.");
            RuleFor(c => c.NFeSerieHomologacao).MaximumLength(3).WithMessage("A série da NF-e (homologação) deve possuir no máximo 3 caracteres.");
            RuleFor(c => c.NfceSerieProducao).MaximumLength(3).WithMessage("A série da NFC-e (produção) deve possuir no máximo 3 caracteres.");
            RuleFor(c => c.NfceSerieHomologacao).MaximumLength(3).WithMessage("A série da NFC-e (homologação) deve possuir no máximo 3 caracteres.");
        }
    }
}
