using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    public record CriarBomComponenteInput(
        Guid VariacaoComponenteId,
        decimal Quantidade,
        Guid? SubUnidadeId = null,
        decimal? MultiplicadorUnidade = null,
        decimal? PercentualDesperdicio = null,
        Guid? GrupoComponenteId = null,
        int? OrdemMontagem = null,
        decimal? CustoUnitarioComImpostos = null);

    public record CriarBomEstruturaCommand(
        Guid ProdutoId,
        Guid VariacaoId,
        string? Codigo,
        decimal PercentualDesperdicio,
        decimal CustoIngredientes,
        decimal CustoExtra,
        decimal QuantidadeTotal,
        List<CriarBomComponenteInput>? Componentes = null,
        string? IngredientesJson = null,
        string? Instrucoes = null,
        string? TipoCustoProducao = null,
        decimal? PrecoFinal = null,
        Guid? SubUnidadeId = null,
        string? Versao = null,
        DateTime? InicioVigencia = null,
        DateTime? FimVigencia = null
    ) : ICommand;

    public class CriarBomEstruturaCommandValidator : AbstractValidator<CriarBomEstruturaCommand>
    {
        public CriarBomEstruturaCommandValidator()
        {
            RuleFor(c => c.ProdutoId).NotEmpty().WithMessage("O produto é obrigatório.");
            RuleFor(c => c.VariacaoId).NotEmpty().WithMessage("A variação do produto é obrigatória.");
            RuleFor(c => c.QuantidadeTotal).GreaterThan(0).WithMessage("A quantidade total deve ser maior que zero.");
            RuleFor(c => c.CustoIngredientes).GreaterThanOrEqualTo(0).WithMessage("O custo de ingredientes deve ser maior ou igual a zero.");
        }
    }

    public record SubmeterBomEstruturaCommand(Guid Id) : ICommand;
    public record AprovarBomEstruturaCommand(Guid Id) : ICommand;
    public record RejeitarBomEstruturaCommand(Guid Id, string Motivo) : ICommand;
    public record InativarBomEstruturaCommand(Guid Id) : ICommand;
    public record ReativarBomEstruturaCommand(Guid Id) : ICommand;
    public record EncerrarBomEstruturaCommand(Guid Id) : ICommand;

    // Catálogo de instruções (BOM-REG-020)
    public record CriarBomInstrucaoCommand(string Codigo, string Descricao) : ICommand;

    public class CriarBomInstrucaoCommandValidator : AbstractValidator<CriarBomInstrucaoCommand>
    {
        public CriarBomInstrucaoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código da instrução é obrigatório.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descrição da instrução é obrigatória.");
        }
    }

    // Vínculo instrução ↔ ordem de produção (BOM-REG-022)
    public record VincularBomInstrucaoOrdemCommand(Guid InstrucaoId, Guid OrdemProducaoId) : ICommand;
}
