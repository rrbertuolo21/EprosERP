using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record AtualizarTipoOperacaoFiscalCommand(
        Guid Id,
        Guid TributarioGrupoId,
        Guid? CfopNfeId,
        Guid? CfopNfceId,
        string Descricao,
        bool SobescreveTributacaoNcm,
        EFinalidadeEmissao Finalidade,
        ETipoAtendimento Atendimento,
        EModalidadeFrete TipoFrete,
        ETipoMovimento TipoMovimento
    ) : ICommand;

    public class AtualizarTipoOperacaoFiscalCommandValidator : AbstractValidator<AtualizarTipoOperacaoFiscalCommand>
    {
        public AtualizarTipoOperacaoFiscalCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("O ID da operação é obrigatório.");

            RuleFor(c => c.TributarioGrupoId)
                .NotEmpty().WithMessage("O Grupo Tributário é obrigatório.");

            RuleFor(c => c.Descricao)
                .NotEmpty().WithMessage("A descrição é obrigatória.")
                .MaximumLength(150).WithMessage("A descrição deve possuir no máximo 150 caracteres.");

            RuleFor(c => c.Finalidade)
                .IsInEnum().WithMessage("Finalidade de emissão inválida.");

            RuleFor(c => c.Atendimento)
                .IsInEnum().WithMessage("Tipo de atendimento inválido.");

            RuleFor(c => c.TipoFrete)
                .IsInEnum().WithMessage("Modalidade de frete inválida.");

            RuleFor(c => c.TipoMovimento)
                .IsInEnum().WithMessage("Tipo de movimento inválido.");
        }
    }
}
