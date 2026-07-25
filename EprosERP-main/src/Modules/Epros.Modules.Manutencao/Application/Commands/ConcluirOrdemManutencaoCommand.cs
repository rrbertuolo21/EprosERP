using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Manutencao.Application.Commands
{
    public record ConcluirOrdemManutencaoCommand(
        Guid OrdemManutencaoId,
        string DescricaoServicoExecutado,
        decimal CustoMaoObra
    ) : ICommand;

    public class ConcluirOrdemManutencaoCommandValidator : AbstractValidator<ConcluirOrdemManutencaoCommand>
    {
        public ConcluirOrdemManutencaoCommandValidator()
        {
            RuleFor(c => c.OrdemManutencaoId)
                .NotEmpty().WithMessage("O ID da ordem de manutenção é obrigatório.");

            RuleFor(c => c.DescricaoServicoExecutado)
                .NotEmpty().WithMessage("A descrição do serviço executado é obrigatória.");

            RuleFor(c => c.CustoMaoObra)
                .GreaterThanOrEqualTo(0).WithMessage("O custo de mão de obra não pode ser negativo.");
        }
    }
}
