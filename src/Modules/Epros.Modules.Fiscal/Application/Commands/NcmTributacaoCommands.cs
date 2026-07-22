using System;
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarNcmTributacaoCommand(
        Guid TributarioGrupoId,
        Guid? CodigoBeneficioFiscalId,
        int CodRegra,
        string Descricao,
        int CfopNotaConsumidor,
        int CfopNotaFiscal,
        int CfopNotaFiscalInterestadual,
        EOrigemMercadoria Origem,
        ECodigoSituacaoOperacaoSimplesNacional CsosnNotaConsumidor,
        ECodigoSituacaoTributariaIcms CstIcmsNotaConsumidor,
        ECodigoSituacaoOperacaoSimplesNacional CsosnNotaFiscal,
        ECodigoSituacaoTributariaIcms CstIcmsNotaFiscalInterna,
        ECodigoSituacaoTributariaIcms CstIcmsNotaFiscalInterstadual,
        ECodigoSituacaoTributariaPisCofins CstPis,
        ECodigoSituacaoTributariaPisCofins CstCofins,
        decimal ValorUnitFixoPis,
        decimal ValorUnitFixoCofins,
        decimal ValorAliquotaPis,
        decimal ValorAliquotaCofins,
        ECodigoSituacaoTributariaPisCofins CstPisCofinsEntrada,
        ECodigoSituacaoTributariaIpi CstIpiSaida,
        ECodigoSituacaoTributariaIpi CstIpiEntrada,
        decimal ValorAliquotaIpi,
        decimal ValorPercentualReducacaoBcIpi,
        ETipoReducaoBaseDeCalculo TipoReducaoIpi,
        EDestinoReducao DestinoReducaoIpi,
        bool IpiEmbutido,
        string? EnquadramentoIpi,
        ECodigoValorFiscalIcms CodigoValorFiscalIcmsInterna,
        ECodigoValorFiscalIcms CodigoValorFiscalcmsInterstadual,
        decimal ValorAliquotaIcmsInterna,
        decimal ValorPercentualReducacaoBcIcmsInterna,
        ETipoReducaoBaseDeCalculo TipoReducaoIcmsInterna,
        EDestinoReducao DestinoReducaoIcmsInterna,
        decimal ValorAliquotaIcmsInterstadual,
        decimal ValorPercentualReducacaoBcIcmsInterstadual,
        ETipoReducaoBaseDeCalculo TipoReducaoIcmsInterstadual,
        EDestinoReducao DestinoReducaoIcmsInterstadual,
        string? CodigoBeneficioFiscalIcms,
        int MotivoDesoneracaoIcms,
        string? InformacoesComplementares,
        string? InformacoesAdicionaisAoFisco,
        string? CstIbsCbsNfe,
        string? CClassTribNfe,
        string? CstIbsCbsNfce,
        string? CClassTribNfce
    ) : ICommand;

    public record AtualizarNcmTributacaoCommand(
        Guid Id,
        Guid TributarioGrupoId,
        Guid? CodigoBeneficioFiscalId,
        int CodRegra,
        string Descricao,
        int CfopNotaConsumidor,
        int CfopNotaFiscal,
        int CfopNotaFiscalInterestadual,
        EOrigemMercadoria Origem,
        ECodigoSituacaoOperacaoSimplesNacional CsosnNotaConsumidor,
        ECodigoSituacaoTributariaIcms CstIcmsNotaConsumidor,
        ECodigoSituacaoOperacaoSimplesNacional CsosnNotaFiscal,
        ECodigoSituacaoTributariaIcms CstIcmsNotaFiscalInterna,
        ECodigoSituacaoTributariaIcms CstIcmsNotaFiscalInterstadual,
        ECodigoSituacaoTributariaPisCofins CstPis,
        ECodigoSituacaoTributariaPisCofins CstCofins,
        decimal ValorUnitFixoPis,
        decimal ValorUnitFixoCofins,
        decimal ValorAliquotaPis,
        decimal ValorAliquotaCofins,
        ECodigoSituacaoTributariaPisCofins CstPisCofinsEntrada,
        ECodigoSituacaoTributariaIpi CstIpiSaida,
        ECodigoSituacaoTributariaIpi CstIpiEntrada,
        decimal ValorAliquotaIpi,
        decimal ValorPercentualReducacaoBcIpi,
        ETipoReducaoBaseDeCalculo TipoReducaoIpi,
        EDestinoReducao DestinoReducaoIpi,
        bool IpiEmbutido,
        string? EnquadramentoIpi,
        ECodigoValorFiscalIcms CodigoValorFiscalIcmsInterna,
        ECodigoValorFiscalIcms CodigoValorFiscalcmsInterstadual,
        decimal ValorAliquotaIcmsInterna,
        decimal ValorPercentualReducacaoBcIcmsInterna,
        ETipoReducaoBaseDeCalculo TipoReducaoIcmsInterna,
        EDestinoReducao DestinoReducaoIcmsInterna,
        decimal ValorAliquotaIcmsInterstadual,
        decimal ValorPercentualReducacaoBcIcmsInterstadual,
        ETipoReducaoBaseDeCalculo TipoReducaoIcmsInterstadual,
        EDestinoReducao DestinoReducaoIcmsInterstadual,
        string? CodigoBeneficioFiscalIcms,
        int MotivoDesoneracaoIcms,
        string? InformacoesComplementares,
        string? InformacoesAdicionaisAoFisco
    ) : ICommand;

    public record DeletarNcmTributacaoCommand(Guid Id) : ICommand;

    // IBS/CBS
    public record DefinirIbsCbsNcmTributacaoCommand(Guid NcmTributacaoId, string? CstIbsCbsNfe, string? CClassTribNfe, string? CstIbsCbsNfce, string? CClassTribNfce) : ICommand;

    // NcmTributacaoSt
    public record AdicionarNcmTributacaoStCommand(Guid NcmTributacaoId, string Uf, ETipoCalculo TipoCalculo, decimal ValorAliquotaIcmsSt, decimal ValorMva, decimal ValorReducaoBcIcmsSt, int TipoReducaoIcmsSt, decimal ValorUnitarioSt, decimal ValorPercentualFcpSt) : ICommand;
    public record AlterarNcmTributacaoStCommand(Guid NcmTributacaoId, Guid NcmTributacaoStId, string Uf, ETipoCalculo TipoCalculo, decimal ValorAliquotaIcmsSt, decimal ValorMva, decimal ValorReducaoBcIcmsSt, int TipoReducaoIcmsSt, decimal ValorUnitarioSt, decimal ValorPercentualFcpSt) : ICommand;
    public record DeletarNcmTributacaoStCommand(Guid NcmTributacaoId, Guid NcmTributacaoStId) : ICommand;

    // NcmTributacaoFundoCombatePobreza
    public record AdicionarNcmTributacaoFundoCombatePobrezaCommand(Guid NcmTributacaoId, string Uf, decimal ValorPercentual) : ICommand;
    public record AlterarNcmTributacaoFundoCombatePobrezaCommand(Guid NcmTributacaoId, Guid FundoCombatePobrezaId, string Uf, decimal ValorPercentual) : ICommand;
    public record DeletarNcmTributacaoFundoCombatePobrezaCommand(Guid NcmTributacaoId, Guid FundoCombatePobrezaId) : ICommand;

    // NcmConfiguracao
    public record AdicionarNcmConfiguracaoCommand(Guid NcmTributacaoId, Guid NcmId) : ICommand;
    public record AlterarNcmConfiguracaoCommand(Guid NcmTributacaoId, Guid NcmConfiguracaoId, Guid NcmId) : ICommand;
    public record DeletarNcmConfiguracaoCommand(Guid NcmTributacaoId, Guid NcmConfiguracaoId) : ICommand;

    public class CriarNcmTributacaoCommandValidator : AbstractValidator<CriarNcmTributacaoCommand>
    {
        public CriarNcmTributacaoCommandValidator()
        {
            RuleFor(c => c.TributarioGrupoId).NotEmpty().WithMessage("O grupo tributário é obrigatório.");
            RuleFor(c => c.CodRegra).GreaterThan(0).WithMessage("O código da regra deve ser maior que zero.");
            RuleFor(c => c.Descricao).MaximumLength(200).WithMessage("A descrição deve possuir no máximo 200 caracteres.");
            RuleFor(c => c.Origem).IsInEnum().WithMessage("Origem inválida.");
        }
    }

    public class AtualizarNcmTributacaoCommandValidator : AbstractValidator<AtualizarNcmTributacaoCommand>
    {
        public AtualizarNcmTributacaoCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID é obrigatório.");
            RuleFor(c => c.TributarioGrupoId).NotEmpty().WithMessage("O grupo tributário é obrigatório.");
            RuleFor(c => c.CodRegra).GreaterThan(0).WithMessage("O código da regra deve ser maior que zero.");
            RuleFor(c => c.Descricao).MaximumLength(200).WithMessage("A descrição deve possuir no máximo 200 caracteres.");
            RuleFor(c => c.Origem).IsInEnum().WithMessage("Origem inválida.");
        }
    }
}
