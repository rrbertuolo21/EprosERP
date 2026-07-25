using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record SalvarConfiguracaoEmpresaCommand(
        Guid EmpresaId,
        string Nome,
        string Email,
        string? Telefone,
        string? Endereco,
        int TimeZoneId,
        string DateFormat,
        int CurrencyId,
        decimal VatPercentage,
        int VatType,
        int CurrencyPosition,
        string? FooterText,
        string? Logo,
        string? Favicon
    ) : ICommand;

    public class SalvarConfiguracaoEmpresaCommandValidator : AbstractValidator<SalvarConfiguracaoEmpresaCommand>
    {
        public SalvarConfiguracaoEmpresaCommandValidator()
        {
            RuleFor(x => x.EmpresaId).NotEmpty().WithMessage("O ID da empresa é obrigatório.");
            RuleFor(x => x.Nome).NotEmpty().WithMessage("O Nome da empresa é obrigatório.").MaximumLength(250).WithMessage("O Nome da empresa deve ter no máximo 250 caracteres.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("O E-mail da empresa é obrigatório.").EmailAddress().WithMessage("O E-mail da empresa deve ser um endereço válido.");
            RuleFor(x => x.TimeZoneId).GreaterThan(0).WithMessage("O TimeZoneId deve ser válido.");
            RuleFor(x => x.CurrencyId).GreaterThan(0).WithMessage("O CurrencyId deve ser válido.");
            RuleFor(x => x.VatPercentage).GreaterThanOrEqualTo(0).WithMessage("O percentual de imposto não pode ser negativo.");
            RuleFor(x => x.VatType).Must(x => x == 1 || x == 2).WithMessage("O tipo de imposto (VatType) deve ser 1 (Inclusivo) ou 2 (Exclusivo).");
            RuleFor(x => x.CurrencyPosition).Must(x => x == 1 || x == 2).WithMessage("A posição da moeda deve ser 1 (Esquerda) ou 2 (Direita).");
        }
    }

    public record HabilitarIdiomaCommand(string Code) : ICommand;

    public class HabilitarIdiomaCommandValidator : AbstractValidator<HabilitarIdiomaCommand>
    {
        public HabilitarIdiomaCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty().WithMessage("O código do idioma é obrigatório.").MaximumLength(10).WithMessage("O código do idioma deve ter no máximo 10 caracteres.");
        }
    }

    public record DesabilitarIdiomaCommand(string Code) : ICommand;

    public class DesabilitarIdiomaCommandValidator : AbstractValidator<DesabilitarIdiomaCommand>
    {
        public DesabilitarIdiomaCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty().WithMessage("O código do idioma é obrigatório.").MaximumLength(10).WithMessage("O código do idioma deve ter no máximo 10 caracteres.");
        }
    }

    /// <summary>
    /// APP-TEN-002 (RF-6.6): onboarding encadeia a criação do Cliente SaaS (dono = GestaoClientes) e,
    /// opcionalmente, semeia a configuração inicial da empresa. Executado de forma transacional.
    /// </summary>
    public record CriarClienteSaaSOnboardingCommand(
        string TenantId,
        string RazaoSocial,
        string Cnpj,
        string Email,
        Guid PlanoId,
        int DiaVencimento,
        string? Telefone,
        string? NomeContato,
        bool IsDemo) : ICommand;

    public class CriarClienteSaaSOnboardingCommandValidator : AbstractValidator<CriarClienteSaaSOnboardingCommand>
    {
        public CriarClienteSaaSOnboardingCommandValidator()
        {
            RuleFor(x => x.TenantId).NotEmpty().WithMessage("O TenantId do cliente é obrigatório.");
            RuleFor(x => x.RazaoSocial).NotEmpty().WithMessage("A razão social é obrigatória.").MaximumLength(250);
            RuleFor(x => x.Cnpj).NotEmpty().WithMessage("O CNPJ é obrigatório.");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("O e-mail deve ser válido.");
            RuleFor(x => x.PlanoId).NotEmpty().WithMessage("O plano SaaS é obrigatório.");
            RuleFor(x => x.DiaVencimento).InclusiveBetween(1, 31).WithMessage("O dia de vencimento deve estar entre 1 e 31.");
        }
    }
}
