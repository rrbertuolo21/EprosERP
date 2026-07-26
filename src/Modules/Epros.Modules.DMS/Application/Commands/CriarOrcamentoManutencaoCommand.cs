using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record CriarOrcamentoManutencaoCommand(
        Guid OrdemServicoId,
        DateTime Validade,
        decimal ValorTotal
    ) : ICommand;

    public class CriarOrcamentoManutencaoCommandValidator : AbstractValidator<CriarOrcamentoManutencaoCommand>
    {
        public CriarOrcamentoManutencaoCommandValidator()
        {
            RuleFor(c => c.OrdemServicoId).NotEmpty().WithMessage("A ordem de serviço é obrigatória no orçamento.");
            RuleFor(c => c.ValorTotal).GreaterThanOrEqualTo(0).WithMessage("O valor total do orçamento não pode ser negativo.");
        }
    }
}
