using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    public record CriarCustoReferenciaInput(
        string TipoReferencia,
        Guid? ReferenciaId = null,
        decimal? CustoPrevisto = null,
        decimal? CustoRealizado = null,
        decimal? CustoExtra = null,
        string? TipoCustoProducao = null,
        decimal? PercentualCustoProducao = null);

    public record CriarCustoProducaoCommand(
        string Codigo,
        Guid ResponsavelId,
        string? ReferenciaOrigem = null,
        Guid? ReferenciaId = null,
        decimal? CustoTotalPrevisto = null,
        decimal? CustoTotalRealizado = null,
        List<CriarCustoReferenciaInput>? Referencias = null
    ) : ICommand;

    public class CriarCustoProducaoCommandValidator : AbstractValidator<CriarCustoProducaoCommand>
    {
        public CriarCustoProducaoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código do custo é obrigatório.");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsável é obrigatório.");
        }
    }

    public record SubmeterCustoProducaoCommand(Guid Id) : ICommand;
    public record AprovarCustoProducaoCommand(Guid Id) : ICommand;
    public record RejeitarCustoProducaoCommand(Guid Id, string Motivo) : ICommand;
    public record InativarCustoProducaoCommand(Guid Id) : ICommand;
    public record ReativarCustoProducaoCommand(Guid Id) : ICommand;
    public record EncerrarCustoProducaoCommand(Guid Id) : ICommand;
}
