using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Qualidade.Application.Commands
{
    /// <summary>QLD-NCR — Abre um registro formal de Nao Conformidade.</summary>
    public record CriarNcrCommand(
        string Codigo,
        string Titulo,
        string Descricao,
        ENcrOrigem OrigemPrincipal,
        ENcrPrioridade Prioridade,
        Guid ResponsavelId,
        string? Severidade,
        DateTime? DataOcorrencia
    ) : ICommand;

    public class CriarNcrCommandValidator : AbstractValidator<CriarNcrCommand>
    {
        public CriarNcrCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30).WithMessage("O codigo da NCR e obrigatorio (max 30).");
            RuleFor(c => c.Titulo).NotEmpty().MaximumLength(255).WithMessage("O titulo da NCR e obrigatorio.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descricao da NCR e obrigatoria.");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsavel pela NCR e obrigatorio.");
        }
    }
}
