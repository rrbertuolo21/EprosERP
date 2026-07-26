using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record DefinirSystemSettingCommand(
        string Chave,
        string Valor,
        string Escopo,
        bool IsSecret
    ) : ICommand;

    public class DefinirSystemSettingCommandValidator : AbstractValidator<DefinirSystemSettingCommand>
    {
        public DefinirSystemSettingCommandValidator()
        {
            RuleFor(x => x.Chave)
                .NotEmpty().WithMessage("A chave de configuração é obrigatória.");

            RuleFor(x => x.Valor)
                .NotEmpty().WithMessage("O valor da configuração é obrigatório.");

            RuleFor(x => x.Escopo)
                .NotEmpty().WithMessage("O escopo é obrigatório.")
                .Must(x => x == "global" || x == "landlord" || x == "tenant" || x == "gateway" || x == "install")
                .WithMessage("O escopo informado é inválido. Escopos permitidos: global, landlord, tenant, gateway, install.");
        }
    }
}
