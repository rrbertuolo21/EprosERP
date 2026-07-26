using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    // RH-PLN — Planejamento de RH.

    public record CriarTurnoCommand(
        string? Nome,
        TimeSpan? HoraInicio,
        TimeSpan? HoraFim,
        TimeSpan? IntervaloInicio,
        TimeSpan? IntervaloFim,
        bool? TurnoNoturno,
        Guid? CriadoPorId,
        Guid? OwnerId
    ) : ICommand;

    public record CriarFeriadoCommand(
        string? Nome,
        DateTime? DataInicio,
        DateTime? DataFim,
        Guid? TipoFeriadoId,
        string? Descricao,
        bool? Remunerado,
        Guid? CriadoPorId,
        Guid? OwnerId
    ) : ICommand;

    public class CriarFeriadoCommandValidator : AbstractValidator<CriarFeriadoCommand>
    {
        public CriarFeriadoCommandValidator()
        {
            // PLN secao 17.3: feriado valida data inicial e final quando informadas.
            RuleFor(c => c).Must(c => !c.DataInicio.HasValue || !c.DataFim.HasValue || c.DataFim.Value >= c.DataInicio.Value)
                .WithMessage("A data final do feriado deve ser maior ou igual a data inicial (PLN secao 17.3).");
        }
    }

    public record DefinirHeadcountItemCommand(
        Guid VersaoId,
        Guid? DepartamentoId,
        Guid? CargoId,
        int? QuantidadeAutorizada,
        decimal? CustoPrevisto,
        string? Observacao
    ) : ICommand;

    public class DefinirHeadcountItemCommandValidator : AbstractValidator<DefinirHeadcountItemCommand>
    {
        public DefinirHeadcountItemCommandValidator()
        {
            RuleFor(c => c.VersaoId).NotEmpty();
            RuleFor(c => c.QuantidadeAutorizada).GreaterThanOrEqualTo(0).When(c => c.QuantidadeAutorizada.HasValue);
        }
    }
}
