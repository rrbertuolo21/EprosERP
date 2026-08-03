using System;
using Epros.Shared.Application.Contracts;
using Epros.Modules.RH.Domain.Folha.Calculo;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    /// <summary>
    /// Desliga o colaborador e, quando o <see cref="TipoDesligamento"/> é informado, APURA as verbas
    /// rescisórias pelo MOTOR DE RESCISÃO (RN-07: saldo, aviso, 13º prop., férias, multa FGTS por tipo).
    /// Campos de rescisão são opcionais para manter compatibilidade com o desligamento simples.
    /// </summary>
    public record DesligarColaboradorCommand(
        Guid ColaboradorId,
        DateTime DataDemissao,
        TipoDesligamento? TipoDesligamento = null,
        decimal SaldoFgtsDepositado = 0m,
        bool TemFeriasVencidas = false,
        decimal RemuneracaoFeriasVencidas = 0m,
        decimal MediaVariaveis = 0m,
        int NumDependentes = 0,
        decimal PensaoAlimenticia = 0m
    ) : ICommand;

    public class DesligarColaboradorCommandValidator : AbstractValidator<DesligarColaboradorCommand>
    {
        public DesligarColaboradorCommandValidator()
        {
            RuleFor(c => c.ColaboradorId)
                .NotEmpty().WithMessage("O ID do colaborador é obrigatório.");

            RuleFor(c => c.DataDemissao)
                .NotEmpty().WithMessage("A data de demissão é obrigatória.");

            RuleFor(c => c.NumDependentes).GreaterThanOrEqualTo(0);
            RuleFor(c => c.SaldoFgtsDepositado).GreaterThanOrEqualTo(0);
        }
    }
}
