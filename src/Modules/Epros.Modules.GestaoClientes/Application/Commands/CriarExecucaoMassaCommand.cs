using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarExecucaoMassaCommand(
        string TipoOperacao,
        string Parametros
    ) : ICommand;

    public class CriarExecucaoMassaCommandValidator : AbstractValidator<CriarExecucaoMassaCommand>
    {
        public CriarExecucaoMassaCommandValidator()
        {
            RuleFor(c => c.TipoOperacao)
                .NotEmpty().WithMessage("O tipo de operação é obrigatório.");

            RuleFor(c => c.Parametros)
                .NotEmpty().WithMessage("Os parâmetros são obrigatórios.");
        }
    }
}
