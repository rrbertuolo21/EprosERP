using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Modules.RH.Domain.Sst.Calculo;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    public record FolhaPagamentoVerbaInput(
        string Descricao,
        string Tipo, // Provento, Desconto
        decimal Valor
    );

    /// <summary>
    /// Processa a folha mensal pelo motor legal. Além das verbas manuais, aceita entradas de
    /// JORNADA (horas extras / adicional noturno) e SST (insalubridade / periculosidade) — os
    /// serviços de domínio (MotorJornada, MotorAdicionalSst) convertem essas entradas em proventos
    /// que compõem as bases (INSS/IRRF/FGTS) antes da apuração.
    /// </summary>
    public record ProcessarFolhaPagamentoCommand(
        Guid ColaboradorId,
        int MesCompetencia,
        int AnoCompetencia,
        List<FolhaPagamentoVerbaInput> Verbas,
        // Jornada (RH-PNT) — alimenta a folha como proventos.
        decimal HorasExtras = 0m,
        decimal AdicionalHorasExtras = 0.5m,
        decimal HorasNoturnas = 0m,
        decimal DivisorHoras = 220m,
        // SST — insalubridade/periculosidade (grau enquadrado por laudo; o sistema só calcula).
        GrauInsalubridade GrauInsalubridade = GrauInsalubridade.Nenhum,
        bool TemPericulosidade = false
    ) : ICommand;

    public class ProcessarFolhaPagamentoCommandValidator : AbstractValidator<ProcessarFolhaPagamentoCommand>
    {
        public ProcessarFolhaPagamentoCommandValidator()
        {
            RuleFor(c => c.ColaboradorId)
                .NotEmpty().WithMessage("O ID do colaborador é obrigatório.");

            RuleFor(c => c.MesCompetencia)
                .InclusiveBetween(1, 12).WithMessage("O mês de competência deve ser de 1 a 12.");

            RuleFor(c => c.AnoCompetencia)
                .InclusiveBetween(2000, 2100).WithMessage("O ano de competência é inválido.");

            RuleForEach(c => c.Verbas).ChildRules(verba =>
            {
                verba.RuleFor(v => v.Descricao)
                    .NotEmpty().WithMessage("A descrição da verba é obrigatória.");

                verba.RuleFor(v => v.Tipo)
                    .Must(t => t == "Provento" || t == "Desconto")
                    .WithMessage("O tipo de verba deve ser 'Provento' ou 'Desconto'.");

                verba.RuleFor(v => v.Valor)
                    .GreaterThan(0).WithMessage("O valor da verba deve ser maior que zero.");
            });
        }
    }
}
