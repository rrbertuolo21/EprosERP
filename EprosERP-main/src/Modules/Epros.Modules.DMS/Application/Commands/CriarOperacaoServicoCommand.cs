using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record CriarOperacaoServicoCommand(
        Guid TipoServicoId,
        string Codigo,
        string Descricao,
        decimal TmoQuantidade,
        string TmoUnidade,
        string? NaturezaPadrao
    ) : ICommand;

    public class CriarOperacaoServicoCommandValidator : AbstractValidator<CriarOperacaoServicoCommand>
    {
        public CriarOperacaoServicoCommandValidator()
        {
            RuleFor(c => c.TipoServicoId).NotEmpty().WithMessage("O tipo de serviço é obrigatório na operação de serviço.");
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código da operação de serviço é obrigatório.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descrição da operação de serviço é obrigatória.");
            RuleFor(c => c.TmoQuantidade).GreaterThan(0).WithMessage("A quantidade de TMO deve ser maior que zero.");
            RuleFor(c => c.TmoUnidade).NotEmpty().WithMessage("A unidade de TMO é obrigatória.");
        }
    }
}
