using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record CriarUsuarioInternoCommand(
        string Nome,
        string Email,
        string Senha,
        string Timezone,
        bool PrimaryAdmin
    ) : ICommand;

    public class CriarUsuarioInternoCommandValidator : AbstractValidator<CriarUsuarioInternoCommand>
    {
        public CriarUsuarioInternoCommandValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome é obrigatório.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("O e-mail informado é inválido.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(8).WithMessage("A senha deve ter no mínimo 8 caracteres.");
        }
    }

    public record AlterarSenhaUsuarioInternoCommand(
        Guid UsuarioInternoId,
        string NovaSenha
    ) : ICommand;

    public class AlterarSenhaUsuarioInternoCommandValidator : AbstractValidator<AlterarSenhaUsuarioInternoCommand>
    {
        public AlterarSenhaUsuarioInternoCommandValidator()
        {
            RuleFor(x => x.UsuarioInternoId)
                .NotEmpty().WithMessage("O ID do usuário interno é obrigatório.");

            RuleFor(x => x.NovaSenha)
                .NotEmpty().WithMessage("A nova senha é obrigatória.")
                .MinimumLength(8).WithMessage("A nova senha deve ter no mínimo 8 caracteres.");
        }
    }

    public record TornarAdminPrincipalCommand(
        Guid UsuarioInternoId
    ) : ICommand;

    public class TornarAdminPrincipalCommandValidator : AbstractValidator<TornarAdminPrincipalCommand>
    {
        public TornarAdminPrincipalCommandValidator()
        {
            RuleFor(x => x.UsuarioInternoId)
                .NotEmpty().WithMessage("O ID do usuário interno é obrigatório.");
        }
    }
}
