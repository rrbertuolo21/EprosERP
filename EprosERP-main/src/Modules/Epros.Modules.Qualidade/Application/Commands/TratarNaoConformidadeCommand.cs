using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Qualidade.Application.Commands
{
    public record TratarNaoConformidadeCommand(
        Guid NaoConformidadeId,
        string CausaRaiz,
        string PlanoAcao,
        string ResolvidoPor
    ) : ICommand;

    public class TratarNaoConformidadeCommandValidator : AbstractValidator<TratarNaoConformidadeCommand>
    {
        public TratarNaoConformidadeCommandValidator()
        {
            RuleFor(c => c.NaoConformidadeId)
                .NotEmpty().WithMessage("O ID da não conformidade é obrigatório.");

            RuleFor(c => c.CausaRaiz)
                .NotEmpty().WithMessage("A análise de causa raiz é obrigatória.");

            RuleFor(c => c.PlanoAcao)
                .NotEmpty().WithMessage("O plano de ação é obrigatório.");

            RuleFor(c => c.ResolvidoPor)
                .NotEmpty().WithMessage("O responsável pela resolução é obrigatório.");
        }
    }
}
