using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record InscreverNewsletterCommand(
        string Email,
        bool ConsentimentoLGPD = true,
        string TermosVersao = "v1.0-legacy",
        string IpRegistro = "127.0.0.1"
    ) : ICommand;

    public class InscreverNewsletterCommandValidator : AbstractValidator<InscreverNewsletterCommand>
    {
        public InscreverNewsletterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("O e-mail informado é inválido.");

            RuleFor(x => x.ConsentimentoLGPD)
                .Equal(true).WithMessage("O consentimento com os termos de privacidade é obrigatório.");

            RuleFor(x => x.TermosVersao)
                .NotEmpty().WithMessage("A versão dos termos é obrigatória.");

            RuleFor(x => x.IpRegistro)
                .NotEmpty().WithMessage("O IP de registro é obrigatório.");
        }
    }

    public record CancelarNewsletterCommand(
        Guid NewsletterSubscriberId
    ) : ICommand;

    public class CancelarNewsletterCommandValidator : AbstractValidator<CancelarNewsletterCommand>
    {
        public CancelarNewsletterCommandValidator()
        {
            RuleFor(x => x.NewsletterSubscriberId)
                .NotEmpty().WithMessage("O ID do assinante é obrigatório.");
        }
    }

    public record ReativarNewsletterCommand(
        Guid NewsletterSubscriberId
    ) : ICommand;

    public class ReativarNewsletterCommandValidator : AbstractValidator<ReativarNewsletterCommand>
    {
        public ReativarNewsletterCommandValidator()
        {
            RuleFor(x => x.NewsletterSubscriberId)
                .NotEmpty().WithMessage("O ID do assinante é obrigatório.");
        }
    }

    public record CancelarNewsletterPorTokenCommand(
        Guid TokenDescadastro
    ) : ICommand;

    public class CancelarNewsletterPorTokenCommandValidator : AbstractValidator<CancelarNewsletterPorTokenCommand>
    {
        public CancelarNewsletterPorTokenCommandValidator()
        {
            RuleFor(x => x.TokenDescadastro)
                .NotEmpty().WithMessage("O token de descadastro é obrigatório.");
        }
    }
}
