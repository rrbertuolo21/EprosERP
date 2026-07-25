using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record AtualizarLandingPageSettingsCommand(
        string SettingsJson
    ) : ICommand;

    public class AtualizarLandingPageSettingsCommandValidator : AbstractValidator<AtualizarLandingPageSettingsCommand>
    {
        public AtualizarLandingPageSettingsCommandValidator()
        {
            RuleFor(x => x.SettingsJson)
                .NotEmpty().WithMessage("O conteúdo da configuração JSON é obrigatório.");
        }
    }

    public record AtualizarMarketplaceSettingsCommand(
        string Modulo,
        string SettingsJson
    ) : ICommand;

    public class AtualizarMarketplaceSettingsCommandValidator : AbstractValidator<AtualizarMarketplaceSettingsCommand>
    {
        public AtualizarMarketplaceSettingsCommandValidator()
        {
            RuleFor(x => x.Modulo)
                .NotEmpty().WithMessage("O nome do módulo é obrigatório.");

            RuleFor(x => x.SettingsJson)
                .NotEmpty().WithMessage("O conteúdo da configuração JSON é obrigatório.");
        }
    }
}
