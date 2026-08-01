using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    /// <summary>
    /// 1.08E — Estorna (refund) um <c>PagamentoFatura</c> liquidado (área Landlord / operador interno).
    /// <see cref="Valor"/> null → estorno TOTAL; informado → parcial. Idempotente por pagamento já estornado.
    /// </summary>
    public record EstornarPagamentoFaturaCommand(
        Guid PagamentoFaturaId,
        decimal? Valor = null,
        string? Motivo = null
    ) : ICommand;

    public class EstornarPagamentoFaturaCommandValidator : AbstractValidator<EstornarPagamentoFaturaCommand>
    {
        public EstornarPagamentoFaturaCommandValidator()
        {
            RuleFor(c => c.PagamentoFaturaId)
                .NotEmpty().WithMessage("O ID do pagamento é obrigatório.");

            RuleFor(c => c.Valor)
                .GreaterThan(0).When(c => c.Valor.HasValue)
                .WithMessage("O valor do estorno deve ser maior que zero.");
        }
    }
}
