using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record CriarRedeNoCommand(
        string Codigo,
        string TipoNo,
        Guid? PaiId,
        Guid? PessoaEmpresaId,
        Guid? LocalId,
        DateTime? InicioVigencia,
        DateTime? FimVigencia
    ) : ICommand;

    public class CriarRedeNoCommandValidator : AbstractValidator<CriarRedeNoCommand>
    {
        public CriarRedeNoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty();
            RuleFor(c => c.TipoNo).NotEmpty();
        }
    }

    public record CriarContratoRedeCommand(
        Guid? RedeNoId,
        string Tipo,
        DateTime Inicio,
        DateTime Fim
    ) : ICommand;

    public class CriarContratoRedeCommandValidator : AbstractValidator<CriarContratoRedeCommand>
    {
        public CriarContratoRedeCommandValidator()
        {
            RuleFor(c => c.Tipo).NotEmpty();
            RuleFor(c => c.Fim).GreaterThan(c => c.Inicio).WithMessage("A data de término deve ser posterior ao início.");
        }
    }
}
