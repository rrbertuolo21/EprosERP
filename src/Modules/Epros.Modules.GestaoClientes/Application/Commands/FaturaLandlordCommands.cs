using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    /// <summary>Altera vencimento e/ou valor de uma fatura em aberto (landlord).</summary>
    public record AlterarFaturaCommand(
        Guid Id,
        decimal Valor,
        DateTime DataVencimento
    ) : ICommand;

    public class AlterarFaturaCommandValidator : AbstractValidator<AlterarFaturaCommand>
    {
        public AlterarFaturaCommandValidator()
        {
            RuleFor(f => f.Id)
                .NotEmpty().WithMessage("O ID da fatura é obrigatório.");

            RuleFor(f => f.Valor)
                .GreaterThan(0).WithMessage("O Valor da fatura deve ser maior que zero.");
        }
    }

    /// <summary>Baixa manual de fatura com valor pago, forma e data (landlord).</summary>
    public record BaixarFaturaManualCommand(
        Guid FaturaId,
        decimal ValorPago,
        string FormaPagamento,
        DateTime? DataPagamento = null
    ) : ICommand;

    public class BaixarFaturaManualCommandValidator : AbstractValidator<BaixarFaturaManualCommand>
    {
        public BaixarFaturaManualCommandValidator()
        {
            RuleFor(f => f.FaturaId)
                .NotEmpty().WithMessage("O ID da fatura é obrigatório.");

            RuleFor(f => f.ValorPago)
                .GreaterThan(0).WithMessage("O Valor pago deve ser maior que zero.");

            RuleFor(f => f.FormaPagamento)
                .NotEmpty().WithMessage("A Forma de pagamento é obrigatória.");
        }
    }

    /// <summary>Exclui (soft-delete) uma fatura (landlord).</summary>
    public record ExcluirFaturaCommand(Guid Id) : ICommand;
}
