using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Qualidade.Application.Commands
{
    // ============ QLD-INS ============
    public record CriarPlanoInspecaoCommand(
        string Codigo,
        string Descricao,
        EContextoPlano Contexto,
        Guid ResponsavelId,
        Guid? ProdutoId,
        Guid? ProcessoId,
        Guid? EtapaId,
        DateTime? DataInicioVigencia
    ) : ICommand;

    public class CriarPlanoInspecaoCommandValidator : AbstractValidator<CriarPlanoInspecaoCommand>
    {
        public CriarPlanoInspecaoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30).WithMessage("O codigo do plano e obrigatorio (max 30).");
            RuleFor(c => c.Descricao).NotEmpty().MaximumLength(500).WithMessage("A descricao do plano e obrigatoria.");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsavel pelo plano e obrigatorio.");
        }
    }

    // ============ QLD-ACR ============
    public record CriarAnaliseAcrCommand(
        string Codigo,
        string Descricao,
        ETipoAnaliseAcr TipoAnalise,
        Guid ResponsavelId,
        Guid? LocalId,
        Guid? DocumentoFiscalId
    ) : ICommand;

    public class CriarAnaliseAcrCommandValidator : AbstractValidator<CriarAnaliseAcrCommand>
    {
        public CriarAnaliseAcrCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30).WithMessage("O codigo da analise e obrigatorio (max 30).");
            RuleFor(c => c.Descricao).NotEmpty().MaximumLength(500).WithMessage("A descricao da analise e obrigatoria.");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsavel pela analise e obrigatorio.");
        }
    }

    // ============ QLD-ADM ============
    public record CriarRegistroAdmCommand(
        string Codigo,
        string Descricao,
        Guid ResponsavelId
    ) : ICommand;

    public class CriarRegistroAdmCommandValidator : AbstractValidator<CriarRegistroAdmCommand>
    {
        public CriarRegistroAdmCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30).WithMessage("O codigo e obrigatorio (max 30).");
            RuleFor(c => c.Descricao).NotEmpty().MaximumLength(500).WithMessage("A descricao e obrigatoria.");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsavel e obrigatorio.");
        }
    }

    // ============ QLD-ATR ============
    public record CriarAtributoCommand(
        string Codigo,
        string NomeInterno,
        string Rotulo,
        ETipoAtributo TipoAtributo,
        ETipoDadoAtributo TipoDado,
        EEscopoAtributo Escopo,
        bool ExibirFormularioPadrao,
        bool Obrigatorio,
        ETipoCaracteristica? TipoCaracteristica,
        bool SensivelLgpd,
        int? Posicao,
        Guid? ResponsavelId
    ) : ICommand;

    public class CriarAtributoCommandValidator : AbstractValidator<CriarAtributoCommand>
    {
        public CriarAtributoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30).WithMessage("O codigo do atributo e obrigatorio (max 30).");
            RuleFor(c => c.NomeInterno).NotEmpty().MaximumLength(100).WithMessage("O nome interno e obrigatorio.");
            RuleFor(c => c.Rotulo).NotEmpty().MaximumLength(255).WithMessage("O rotulo e obrigatorio.");
        }
    }
}
