using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record ExecutarInstalacaoCommand(
        string AdminNome,
        string AdminEmail,
        string AdminSenha
    ) : ICommand;

    public class ExecutarInstalacaoCommandValidator : AbstractValidator<ExecutarInstalacaoCommand>
    {
        public ExecutarInstalacaoCommandValidator()
        {
            RuleFor(x => x.AdminNome)
                .NotEmpty().WithMessage("O nome do administrador é obrigatório.");

            RuleFor(x => x.AdminEmail)
                .NotEmpty().WithMessage("O e-mail do administrador é obrigatório.")
                .EmailAddress().WithMessage("O e-mail deve ser um endereço válido.");

            RuleFor(x => x.AdminSenha)
                .NotEmpty().WithMessage("A senha do administrador é obrigatória.")
                .MinimumLength(8).WithMessage("A senha deve possuir no mínimo 8 caracteres.");
        }
    }

    public record ExecutarAtualizacaoCommand(
        string VersaoAlvo
    ) : ICommand;

    public class ExecutarAtualizacaoCommandValidator : AbstractValidator<ExecutarAtualizacaoCommand>
    {
        public ExecutarAtualizacaoCommandValidator()
        {
            RuleFor(x => x.VersaoAlvo)
                .NotEmpty().WithMessage("A versão alvo da atualização é obrigatória.");
        }
    }
}
