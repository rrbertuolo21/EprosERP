using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record RegistrarVendaVeiculoCommand(
        string Chassi,
        string Modelo,
        string Marca,
        int AnoModelo,
        decimal PrecoVenda,
        string ClienteNome
    ) : ICommand;

    public class RegistrarVendaVeiculoCommandValidator : AbstractValidator<RegistrarVendaVeiculoCommand>
    {
        public RegistrarVendaVeiculoCommandValidator()
        {
            RuleFor(c => c.Chassi).NotEmpty().Length(17).WithMessage("O chassi deve possuir exatamente 17 caracteres.");
            RuleFor(c => c.Modelo).NotEmpty().WithMessage("O modelo do veículo é obrigatório.");
            RuleFor(c => c.Marca).NotEmpty().WithMessage("A marca do veículo é obrigatória.");
            RuleFor(c => c.AnoModelo).GreaterThan(1900).WithMessage("Ano de modelo inválido.");
            RuleFor(c => c.PrecoVenda).GreaterThan(0).WithMessage("O preço de venda deve ser maior que zero.");
            RuleFor(c => c.ClienteNome).NotEmpty().WithMessage("O nome do cliente é obrigatório.");
        }
    }
}
