using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using FluentValidation;
using System;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record AtualizarServicoCommand(
        Guid Id,
        Guid UnidadeMedidaId,
        Guid CodigoServicoSefazId,
        string Codigo,
        string Descricao,
        decimal Valor,
        string? InformacaoAdicional,
        bool ServicoAtivo,
        int Cnae,
        string CodigoNbs,
        bool IndicadorIss,
        bool IndicadorIncentivo,
        string? CstIbsCbs,
        string? CClassTrib,
        decimal AliquotaIss,
        decimal AliquotaIssRetido,
        decimal AliquotaIrrfRetido,
        decimal AliquotaInss,
        ECodigoSituacaoTributariaPisCofins CstPisCofins,
        decimal AliquotaPis,
        decimal AliquotaCofins,
        bool CalcularRetencao,
        EAnexoSimlesNacional AnexoSimplesNacional
    ) : ICommand;

    public class AtualizarServicoCommandValidator : AbstractValidator<AtualizarServicoCommand>
    {
        public Microsoft.EntityFrameworkCore.Metadata.IModel? EntityFrameworkCoreModel { get; set; }

        public AtualizarServicoCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("O ID do serviço é obrigatório.");

            RuleFor(c => c.UnidadeMedidaId)
                .NotEmpty().WithMessage("O ID da unidade de medida é obrigatório.");

            RuleFor(c => c.CodigoServicoSefazId)
                .NotEmpty().WithMessage("O ID do código de serviço SEFAZ é obrigatório.");

            RuleFor(c => c.Codigo)
                .NotEmpty().WithMessage("O código é obrigatório.")
                .MaximumLength(10).WithMessage("O código deve ter no máximo 10 caracteres.");

            RuleFor(c => c.Descricao)
                .NotEmpty().WithMessage("A descrição é obrigatória.")
                .MaximumLength(120).WithMessage("A descrição deve ter no máximo 120 caracteres.");

            RuleFor(c => c.InformacaoAdicional)
                .MaximumLength(120).WithMessage("A informação adicional deve ter no máximo 120 caracteres.");

            RuleFor(c => c.Cnae)
                .GreaterThan(0).WithMessage("O código CNAE é obrigatório.");

            RuleFor(c => c.CodigoNbs)
                .MaximumLength(9).WithMessage("O código NBS deve ter no máximo 9 caracteres.");

            RuleFor(c => c.CstIbsCbs)
                .MaximumLength(5000).WithMessage("O CST IBS/CBS deve ter no máximo 5000 caracteres.");

            RuleFor(c => c.CClassTrib)
                .MaximumLength(5000).WithMessage("A classificação tributária deve ter no máximo 5000 caracteres.");

            RuleFor(c => c.CstPisCofins)
                .IsInEnum().WithMessage("CST PIS/COFINS inválido.");

            RuleFor(c => c.AnexoSimplesNacional)
                .IsInEnum().WithMessage("Anexo Simples Nacional inválido.");
        }
    }
}
