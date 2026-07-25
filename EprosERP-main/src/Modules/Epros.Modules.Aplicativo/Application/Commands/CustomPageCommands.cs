using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record CriarCustomPageCommand(
        string Slug,
        string Conteudo
    ) : ICommand;

    public class CriarCustomPageCommandValidator : AbstractValidator<CriarCustomPageCommand>
    {
        public CriarCustomPageCommandValidator()
        {
            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("O slug é obrigatório.")
                .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("O slug deve conter apenas caracteres minúsculos, números e hifens.");

            RuleFor(x => x.Conteudo)
                .NotEmpty().WithMessage("O conteúdo da página é obrigatório.");
        }
    }

    public record PublicarCustomPageCommand(
        Guid CustomPageId
    ) : ICommand;

    public class PublicarCustomPageCommandValidator : AbstractValidator<PublicarCustomPageCommand>
    {
        public PublicarCustomPageCommandValidator()
        {
            RuleFor(x => x.CustomPageId)
                .NotEmpty().WithMessage("O ID da página customizada é obrigatório.");
        }
    }

    public record DefinirRascunhoCustomPageCommand(
        Guid CustomPageId
    ) : ICommand;

    public class DefinirRascunhoCustomPageCommandValidator : AbstractValidator<DefinirRascunhoCustomPageCommand>
    {
        public DefinirRascunhoCustomPageCommandValidator()
        {
            RuleFor(x => x.CustomPageId)
                .NotEmpty().WithMessage("O ID da página customizada é obrigatório.");
        }
    }
}
