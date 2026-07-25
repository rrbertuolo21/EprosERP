using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Manutencao.Application.Commands
{
    public record AdicionarPecaOrdemCommand(
        Guid OrdemManutencaoId,
        Guid ProdutoId,
        decimal Quantidade
    ) : ICommand;

    public class AdicionarPecaOrdemCommandValidator : AbstractValidator<AdicionarPecaOrdemCommand>
    {
        public AdicionarPecaOrdemCommandValidator()
        {
            RuleFor(c => c.OrdemManutencaoId)
                .NotEmpty().WithMessage("O ID da ordem de manutenção é obrigatório.");

            RuleFor(c => c.ProdutoId)
                .NotEmpty().WithMessage("O ID do produto (peça) é obrigatório.");

            RuleFor(c => c.Quantidade)
                .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
        }
    }
}
