using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    public record CriarPlanejamentoProducaoCommand(string Codigo, Guid ResponsavelId) : ICommand;

    public class CriarPlanejamentoProducaoCommandValidator : AbstractValidator<CriarPlanejamentoProducaoCommand>
    {
        public CriarPlanejamentoProducaoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código do planejamento é obrigatório.");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsável é obrigatório.");
        }
    }

    public record AdicionarSnapshotPlanejamentoCommand(
        Guid PlanejamentoId,
        Guid? OrdemProducaoId = null,
        DateTime? Inicio = null,
        DateTime? PrevisaoEntrega = null,
        DateTime? Termino = null,
        decimal? PorcentoVenda = null,
        decimal? PorcentoEstoque = null,
        decimal? CustoTotalPrevisto = null) : ICommand;

    public record SubmeterPlanejamentoProducaoCommand(Guid Id) : ICommand;
    public record AprovarPlanejamentoProducaoCommand(Guid Id) : ICommand;
    public record RejeitarPlanejamentoProducaoCommand(Guid Id, string Motivo) : ICommand;
    public record InativarPlanejamentoProducaoCommand(Guid Id) : ICommand;
    public record ReativarPlanejamentoProducaoCommand(Guid Id) : ICommand;
    public record EncerrarPlanejamentoProducaoCommand(Guid Id) : ICommand;
}
