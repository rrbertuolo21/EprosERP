using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record CriarTipoServicoConcessionariaCommand(
        string Codigo,
        string Nome,
        string? Descricao
    ) : ICommand;

    public class CriarTipoServicoConcessionariaCommandValidator : AbstractValidator<CriarTipoServicoConcessionariaCommand>
    {
        public CriarTipoServicoConcessionariaCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código do tipo de serviço é obrigatório.");
            RuleFor(c => c.Nome).NotEmpty().WithMessage("O nome do tipo de serviço é obrigatório.");
        }
    }
}
