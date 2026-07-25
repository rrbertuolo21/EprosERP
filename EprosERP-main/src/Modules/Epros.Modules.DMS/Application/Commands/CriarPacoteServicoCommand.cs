using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record CriarPacoteServicoCommand(
        string Codigo,
        string Nome
    ) : ICommand;

    public class CriarPacoteServicoCommandValidator : AbstractValidator<CriarPacoteServicoCommand>
    {
        public CriarPacoteServicoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código do pacote de serviço é obrigatório.");
            RuleFor(c => c.Nome).NotEmpty().WithMessage("O nome do pacote de serviço é obrigatório.");
        }
    }
}
