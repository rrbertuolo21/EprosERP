using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarCfopCommand(
        int CfopCodigo,
        string Descricao,
        string NaturezaOperacao,
        string CfopCorrelacao,
        bool IntegraFaturamento,
        bool IndicadorNfe,
        bool IndicadorComunicacao,
        bool IndicadorTransporte,
        bool IndicadorDevolucao,
        bool IndicadorRetorno,
        bool IndicadorAnulacao,
        bool IndicadorRemessa,
        bool IndicadorCombustivel,
        bool IndicadorTransferencia,
        bool IndicadorNfce,
        bool IndicadorCiap,
        bool IndicadorUsoConsumo,
        bool IndicadorUsoSemOperacao,
        bool IndicadorSt,
        bool IndicadorMei,
        EIncidenciaSimples IncidenciaSimples,
        string? CfopDevolucao
    ) : ICommand;

    public class CriarCfopCommandValidator : AbstractValidator<CriarCfopCommand>
    {
        public CriarCfopCommandValidator()
        {
            RuleFor(c => c.CfopCodigo)
                .InclusiveBetween(1000, 9999).WithMessage("O CFOP deve possuir exatamente 4 dígitos.");

            RuleFor(c => c.Descricao)
                .NotEmpty().WithMessage("A descrição é obrigatória.")
                .MaximumLength(1000).WithMessage("A descrição deve possuir no máximo 1000 caracteres.");

            RuleFor(c => c.NaturezaOperacao)
                .NotEmpty().WithMessage("A natureza de operação é obrigatória.")
                .MaximumLength(1000).WithMessage("A natureza de operação deve possuir no máximo 1000 caracteres.");

            RuleFor(c => c.CfopCorrelacao)
                .NotEmpty().WithMessage("O CFOP de correlação é obrigatório.")
                .Length(4).WithMessage("O CFOP de correlação deve possuir exatamente 4 caracteres.");

            RuleFor(c => c.IncidenciaSimples)
                .IsInEnum().WithMessage("Incidência Simples Nacional inválida.");
        }
    }
}
