using System;
using Epros.Shared.Application.Contracts;
using Epros.Modules.GestaoClientes.Application.Queries;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record CriarCupomCommand(
        string Codigo,
        string Tipo,
        decimal ValorDesconto,
        int? LimiteUso,
        DateTime? ValidoAte,
        string Nome = ""
    ) : ICommand;

    public class CriarCupomCommandValidator : AbstractValidator<CriarCupomCommand>
    {
        public CriarCupomCommandValidator()
        {
            RuleFor(c => c.Codigo)
                .NotEmpty().WithMessage("Código do cupom é obrigatório.");

            RuleFor(c => c.Tipo)
                .NotEmpty().WithMessage("Tipo de desconto é obrigatório.")
                .Must(t => t == "Fixo" || t == "Percentual")
                .WithMessage("Tipo de desconto aceito: 'Fixo' ou 'Percentual'.");

            RuleFor(c => c.ValorDesconto)
                .GreaterThan(0).WithMessage("O valor do desconto deve ser maior que zero.");

            RuleFor(c => c.LimiteUso)
                .GreaterThan(0).When(c => c.LimiteUso.HasValue)
                .WithMessage("O limite de uso deve ser maior que zero.");
        }
    }

    public record AtualizarCupomCommand(
        Guid Id,
        string Nome,
        string Tipo,
        decimal ValorDesconto,
        int? LimiteUso,
        DateTime? ValidoAte
    ) : ICommand;

    public record ExcluirCupomCommand(Guid Id) : ICommand;

    public record ListarCuponsQuery(int Pagina = 1, int TamanhoPagina = 50) : IQuery<PagedQueryResult<CupomDto>>;
    public record ObterCupomQuery(Guid Id) : IQuery<CupomDto>;

    public class CupomDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Fixo";
        public decimal ValorDesconto { get; set; }
        public int? LimiteUso { get; set; }
        public int QuantidadeUsos { get; set; }
        public bool Ativo { get; set; }
        public DateTime? ValidoAte { get; set; }
        public bool Global { get; set; }
    }

    public record ValidarCupomCommand(
        string Codigo,
        Guid PlanoId
    ) : ICommand;

    public class ValidarCupomCommandValidator : AbstractValidator<ValidarCupomCommand>
    {
        public ValidarCupomCommandValidator()
        {
            RuleFor(c => c.Codigo)
                .NotEmpty().WithMessage("Código do cupom é obrigatório.");

            RuleFor(c => c.PlanoId)
                .NotEmpty().WithMessage("O ID do plano é obrigatório.");
        }
    }
}
