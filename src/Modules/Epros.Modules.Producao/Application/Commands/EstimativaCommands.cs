using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    public record CriarEstimativaComponenteInput(
        string TipoComponente,
        Guid? ReferenciaId = null,
        decimal? Quantidade = null,
        decimal? TempoEstimado = null,
        decimal? TaxaEstimada = null,
        decimal? CustoPrevisto = null,
        string? Observacao = null);

    public record CriarEstimativaCommand(
        string Codigo,
        Guid ResponsavelId,
        Guid? PropostaReferenciaId = null,
        Guid? EstruturaRascunhoId = null,
        decimal? CustoPrevistoTotal = null,
        List<CriarEstimativaComponenteInput>? Componentes = null
    ) : ICommand;

    public class CriarEstimativaCommandValidator : AbstractValidator<CriarEstimativaCommand>
    {
        public CriarEstimativaCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código da estimativa é obrigatório.");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsável é obrigatório.");
        }
    }

    public record SubmeterEstimativaCommand(Guid Id) : ICommand;
    public record AprovarEstimativaCommand(Guid Id) : ICommand;
    public record RejeitarEstimativaCommand(Guid Id, string Motivo) : ICommand;
    public record ConverterEstimativaCommand(Guid Id, Guid PlanejamentoOrigemId) : ICommand;
    public record InativarEstimativaCommand(Guid Id) : ICommand;
    public record ReativarEstimativaCommand(Guid Id) : ICommand;
    public record EncerrarEstimativaCommand(Guid Id) : ICommand;
}
