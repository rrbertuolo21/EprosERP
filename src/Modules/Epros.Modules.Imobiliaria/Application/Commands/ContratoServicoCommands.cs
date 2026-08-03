using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Imobiliaria.Application.Commands
{
    /// <summary>
    /// Formaliza o contrato de servico ao proprietario (EF 7.2, RN-008/RN-024).
    /// </summary>
    public record CriarContratoServicoCommand(
        Guid ProprietarioId,
        Guid? ImovelId,
        string? Descricao,
        DateTime? VigenciaInicio,
        DateTime? VigenciaFim,
        decimal? Remuneracao
    ) : ICommand;

    public class CriarContratoServicoCommandValidator : AbstractValidator<CriarContratoServicoCommand>
    {
        public CriarContratoServicoCommandValidator()
        {
            RuleFor(c => c.ProprietarioId).NotEmpty().WithMessage("O contrato de servico exige proprietario.");
        }
    }

    /// <summary>
    /// Exclui (soft delete) o contrato de servico (EF 7.2, RN-010).
    /// </summary>
    public record ExcluirContratoServicoCommand(Guid ContratoId) : ICommand;

    public class ExcluirContratoServicoCommandValidator : AbstractValidator<ExcluirContratoServicoCommand>
    {
        public ExcluirContratoServicoCommandValidator()
        {
            RuleFor(c => c.ContratoId).NotEmpty().WithMessage("Identificador do contrato invalido.");
        }
    }

    /// <summary>Altera o contrato de servico (ID3/PRD-03). Remuneracao = NF-06 (valida-contador).</summary>
    public record AlterarContratoServicoCommand(
        Guid ContratoId,
        string? Descricao,
        DateTime? VigenciaInicio,
        DateTime? VigenciaFim,
        decimal? Remuneracao
    ) : ICommand;

    public class AlterarContratoServicoCommandValidator : AbstractValidator<AlterarContratoServicoCommand>
    {
        public AlterarContratoServicoCommandValidator()
        {
            RuleFor(c => c.ContratoId).NotEmpty().WithMessage("Identificador do contrato invalido.");
        }
    }
}
