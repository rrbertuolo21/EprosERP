using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    /// <summary>Provento adicional informado para o cálculo legal (HE, adicionais, comissões, abonos).</summary>
    public record ProventoFolhaInput(
        string Codigo,
        string Descricao,
        decimal Valor,
        bool IncideInss = true,
        bool IncideIrrf = true,
        bool IncideFgts = true);

    /// <summary>Desconto diverso (adiantamento, pensão via rubrica própria, etc.).</summary>
    public record DescontoFolhaInput(string Codigo, string Descricao, decimal Valor);

    /// <summary>
    /// RH-FP — Cálculo LEGAL da folha mensal (motor real: INSS/IRRF/FGTS na ordem de apuração da skill).
    /// Simulação/apuração: NÃO persiste — devolve o holerite calculado. Substitui a regra manual inventada
    /// (líquido = bruto − descontos) pelo cálculo legal citando departamento-pessoal/folha.
    /// </summary>
    public record CalcularFolhaLegalCommand(
        Guid ColaboradorId,
        int MesCompetencia,
        int AnoCompetencia,
        List<ProventoFolhaInput>? Proventos = null,
        List<DescontoFolhaInput>? Descontos = null,
        int NumDependentes = 0,
        decimal DescontoValeTransporte = 0m,
        decimal PensaoAlimenticia = 0m,
        bool Aprendiz = false
    ) : ICommand;

    public class CalcularFolhaLegalCommandValidator : AbstractValidator<CalcularFolhaLegalCommand>
    {
        public CalcularFolhaLegalCommandValidator()
        {
            RuleFor(c => c.ColaboradorId).NotEmpty().WithMessage("O ID do colaborador é obrigatório.");
            RuleFor(c => c.MesCompetencia).InclusiveBetween(1, 12).WithMessage("O mês de competência deve ser de 1 a 12.");
            RuleFor(c => c.AnoCompetencia).InclusiveBetween(2000, 2100).WithMessage("O ano de competência é inválido.");
            RuleFor(c => c.NumDependentes).GreaterThanOrEqualTo(0);
        }
    }
}
