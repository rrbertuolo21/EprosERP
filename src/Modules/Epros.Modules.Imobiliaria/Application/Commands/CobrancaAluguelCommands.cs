using System;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Imobiliaria.Application.Commands
{
    /// <summary>
    /// Gera a cobranca recorrente do aluguel por competencia (ID8/NF-01). Idempotente por
    /// (locacao+competencia+tipo). Publica evento consumido pelo CONTAS_RECEBER (origina o titulo).
    /// O valor default e o da locacao; encargos/juros/multa/desconto = FINANCEIRO (nao aqui).
    /// </summary>
    public record GerarCobrancaAluguelCommand(
        Guid LocacaoId,
        int Ano,
        int Mes,
        ETipoCobrancaAluguel Tipo = ETipoCobrancaAluguel.Aluguel,
        decimal? ValorOverride = null
    ) : ICommand;

    public class GerarCobrancaAluguelCommandValidator : AbstractValidator<GerarCobrancaAluguelCommand>
    {
        public GerarCobrancaAluguelCommandValidator()
        {
            RuleFor(c => c.LocacaoId).NotEmpty();
            RuleFor(c => c.Ano).InclusiveBetween(2000, 2100);
            RuleFor(c => c.Mes).InclusiveBetween(1, 12);
        }
    }

    /// <summary>
    /// Reflete a baixa vinda do FINANCEIRO/CONTAS_RECEBER (ID8/NF-01). Nao governa juros/multa/desconto.
    /// </summary>
    public record RefletirBaixaAluguelCommand(
        Guid CobrancaId,
        decimal ValorPago,
        string? ReceberRef = null,
        DateTime? DataBaixa = null
    ) : ICommand;

    public class RefletirBaixaAluguelCommandValidator : AbstractValidator<RefletirBaixaAluguelCommand>
    {
        public RefletirBaixaAluguelCommandValidator()
        {
            RuleFor(c => c.CobrancaId).NotEmpty();
            RuleFor(c => c.ValorPago).GreaterThan(0);
        }
    }

    /// <summary>Reflete o estorno da baixa do FINANCEIRO (ID8/NF-01).</summary>
    public record EstornarBaixaAluguelCommand(Guid CobrancaId) : ICommand;

    /// <summary>
    /// Emite o recibo da cobranca baixada (ID8/NF-05). Numero pela numeracao central T9.
    /// A exigencia fiscal do recibo de aluguel e valida-contador (NF-05).
    /// </summary>
    public record EmitirReciboAluguelCommand(Guid CobrancaId) : ICommand;

    public class EmitirReciboAluguelCommandValidator : AbstractValidator<EmitirReciboAluguelCommand>
    {
        public EmitirReciboAluguelCommandValidator()
        {
            RuleFor(c => c.CobrancaId).NotEmpty();
        }
    }
}
