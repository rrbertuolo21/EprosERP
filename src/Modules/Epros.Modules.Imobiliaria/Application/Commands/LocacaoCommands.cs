using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Imobiliaria.Application.Commands
{
    /// <summary>
    /// Formaliza a locacao com periodo, valor, vencimento e partes N:N (EF 7.3, RN-011/RN-012/RN-013).
    /// </summary>
    public record CriarLocacaoCommand(
        Guid? ImovelId,
        DateTime PeriodoInicial,
        DateTime PeriodoFinal,
        decimal Valor,
        int Vencimento,
        List<Guid> LocatarioIds,
        List<Guid>? FiadorIds
    ) : ICommand;

    public class CriarLocacaoCommandValidator : AbstractValidator<CriarLocacaoCommand>
    {
        public CriarLocacaoCommandValidator()
        {
            RuleFor(c => c.Valor).GreaterThan(0).WithMessage("O valor da locacao deve ser positivo.");
            RuleFor(c => c.Vencimento).InclusiveBetween(1, 31).WithMessage("O vencimento deve estar entre 1 e 31.");
            RuleFor(c => c.PeriodoFinal).GreaterThanOrEqualTo(c => c.PeriodoInicial)
                .WithMessage("O fim do periodo deve ser igual ou posterior ao inicio.");
            RuleFor(c => c.LocatarioIds).NotEmpty().WithMessage("A locacao exige ao menos um locatario.");
        }
    }

    /// <summary>
    /// Exclui (soft delete) a locacao e seus agregados (EF 7.3, RN-016).
    /// </summary>
    public record ExcluirLocacaoCommand(Guid LocacaoId) : ICommand;

    public class ExcluirLocacaoCommandValidator : AbstractValidator<ExcluirLocacaoCommand>
    {
        public ExcluirLocacaoCommandValidator()
        {
            RuleFor(c => c.LocacaoId).NotEmpty().WithMessage("Identificador da locacao invalido.");
        }
    }

    /// <summary>Edita uma locacao ainda Em Elaboracao (ID3/PRD-03).</summary>
    public record AlterarLocacaoCommand(
        Guid LocacaoId,
        Guid? ImovelId,
        DateTime PeriodoInicial,
        DateTime PeriodoFinal,
        decimal Valor,
        int Vencimento
    ) : ICommand;

    public class AlterarLocacaoCommandValidator : AbstractValidator<AlterarLocacaoCommand>
    {
        public AlterarLocacaoCommandValidator()
        {
            RuleFor(c => c.LocacaoId).NotEmpty();
            RuleFor(c => c.Valor).GreaterThan(0);
            RuleFor(c => c.Vencimento).InclusiveBetween(1, 31);
        }
    }

    /// <summary>Formaliza a locacao (EmElaboracao → Vigente) e loca o imovel (ID1/PRD-01).</summary>
    public record FormalizarLocacaoCommand(Guid LocacaoId) : ICommand;

    /// <summary>Encerra a locacao vigente e libera o imovel (ID1/PRD-01).</summary>
    public record EncerrarLocacaoCommand(Guid LocacaoId) : ICommand;

    /// <summary>Cancela a locacao e libera o imovel (ID1/PRD-01).</summary>
    public record CancelarLocacaoCommand(Guid LocacaoId) : ICommand;

    /// <summary>Renova (aditivo) a locacao vigente estendendo a vigencia (ID7).</summary>
    public record RenovarLocacaoCommand(Guid LocacaoId, DateTime NovoPeriodoFinal) : ICommand;

    public class RenovarLocacaoCommandValidator : AbstractValidator<RenovarLocacaoCommand>
    {
        public RenovarLocacaoCommandValidator()
        {
            RuleFor(c => c.LocacaoId).NotEmpty();
        }
    }
}
