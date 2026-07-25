using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record AbrirOrdemServicoDmsCommand(
        string NumeroOs,
        string VeiculoChassi,
        string DescricaoInconveniente,
        decimal ValorPecas,
        decimal ValorMaoDeObra,
        bool ReclamacaoGarantia
    ) : ICommand;

    public class AbrirOrdemServicoDmsCommandValidator : AbstractValidator<AbrirOrdemServicoDmsCommand>
    {
        public AbrirOrdemServicoDmsCommandValidator()
        {
            RuleFor(c => c.NumeroOs).NotEmpty().WithMessage("O número da OS é obrigatório.");
            RuleFor(c => c.VeiculoChassi).NotEmpty().Length(17).WithMessage("O chassi do veículo deve possuir exatamente 17 caracteres.");
            RuleFor(c => c.DescricaoInconveniente).NotEmpty().WithMessage("A descrição do inconveniente é obrigatória.");
            RuleFor(c => c.ValorPecas).GreaterThanOrEqualTo(0).WithMessage("O valor de peças não pode ser negativo.");
            RuleFor(c => c.ValorMaoDeObra).GreaterThanOrEqualTo(0).WithMessage("O valor de mão de obra não pode ser negativo.");
        }
    }
}
