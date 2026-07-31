using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    /// <summary>
    /// Política de senha central (REG-032): mínimo 8 caracteres + ao menos uma letra e um número.
    /// Reutilizada no registro, no reset e na troca de senha para evitar divergência entre fluxos.
    /// </summary>
    public static class PoliticaSenhaValidacao
    {
        public static IRuleBuilderOptions<T, string> AplicarPoliticaSenha<T>(this IRuleBuilder<T, string> regra)
        {
            return regra
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(8).WithMessage("A senha deve ter no mínimo 8 caracteres.")
                .Matches("[A-Za-z]").WithMessage("A senha deve conter ao menos uma letra.")
                .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.");
        }
    }

    public record AutenticarUsuarioCommand(
        string Email,
        string Senha,
        string IpAddress,
        string UserAgent
    ) : ICommand;

    public class AutenticarUsuarioCommandValidator : AbstractValidator<AutenticarUsuarioCommand>
    {
        public AutenticarUsuarioCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("O e-mail informado é inválido.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória.");
        }
    }

    public record AutenticarUsuarioInternoCommand(
        string Email,
        string Senha,
        string IpAddress,
        string UserAgent
    ) : ICommand;

    public class AutenticarUsuarioInternoCommandValidator : AbstractValidator<AutenticarUsuarioInternoCommand>
    {
        public AutenticarUsuarioInternoCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("O e-mail informado é inválido.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória.");
        }
    }

    public record SelecionarEmpresaCommand(
        Guid UsuarioId,
        Guid EmpresaId
    ) : ICommand;

    public class SelecionarEmpresaCommandValidator : AbstractValidator<SelecionarEmpresaCommand>
    {
        public SelecionarEmpresaCommandValidator()
        {
            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("O ID do usuário é obrigatório.");

            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("O ID da empresa é obrigatório.");
        }
    }

    public record SolicitarRecuperacaoSenhaCommand(
        string Email
    ) : ICommand;

    public class SolicitarRecuperacaoSenhaCommandValidator : AbstractValidator<SolicitarRecuperacaoSenhaCommand>
    {
        public SolicitarRecuperacaoSenhaCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("O e-mail informado é inválido.");
        }
    }

    public record ResetarSenhaCommand(
        string Email,
        string Token,
        string NovaSenha,
        string ConfirmacaoSenha
    ) : ICommand;

    public class ResetarSenhaCommandValidator : AbstractValidator<ResetarSenhaCommand>
    {
        public ResetarSenhaCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("O e-mail informado é inválido.");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("O token de reset é obrigatório.");

            RuleFor(x => x.NovaSenha)
                .AplicarPoliticaSenha();

            RuleFor(x => x.ConfirmacaoSenha)
                .NotEmpty().WithMessage("A confirmação da nova senha é obrigatória.")
                .Equal(x => x.NovaSenha).WithMessage("A confirmação da nova senha deve ser igual à nova senha.");
        }
    }

    public record AlterarSenhaUsuarioCommand(
        Guid UsuarioId,
        string SenhaAtual,
        string NovaSenha
    ) : ICommand;

    public class AlterarSenhaUsuarioCommandValidator : AbstractValidator<AlterarSenhaUsuarioCommand>
    {
        public AlterarSenhaUsuarioCommandValidator()
        {
            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("O ID do usuário é obrigatório.");

            RuleFor(x => x.SenhaAtual)
                .NotEmpty().WithMessage("A senha atual é obrigatória.");

            RuleFor(x => x.NovaSenha)
                .AplicarPoliticaSenha()
                .NotEqual(x => x.SenhaAtual).WithMessage("A nova senha não pode ser igual à senha atual.");
        }
    }

    /// <summary>
    /// Self-register de tenant. REG-036 (decisão de negócio — fonte: overlay fiscal): quem emitirá
    /// documento fiscal precisa de cadastro íntegro. Exige documento fiscal válido (CNPJ ou CPF),
    /// município IBGE e telefone com tipo; não se aceitam placeholders fiscais.
    /// </summary>
    public record RegistrarNovoTenantCommand(
        string NomeEmpresa,
        string Cnpj,
        string NomeAdmin,
        string EmailAdmin,
        string SenhaAdmin,
        long CodigoIbgeMunicipio,
        string Telefone,
        string TipoTelefone,
        string? Cpf = null
    ) : ICommand;

    public class RegistrarNovoTenantCommandValidator : AbstractValidator<RegistrarNovoTenantCommand>
    {
        // Tipos de telefone aceitos no cadastro (REG-036 — "tipo de telefone" obrigatório).
        private static readonly string[] TiposTelefoneValidos = { "Fixo", "Celular", "Comercial", "Whatsapp" };

        public RegistrarNovoTenantCommandValidator()
        {
            RuleFor(x => x.NomeEmpresa)
                .NotEmpty().WithMessage("O nome da empresa é obrigatório.");

            // REG-036: documento fiscal íntegro — CNPJ (14) OU CPF (11), validados no handler por
            // dígito verificador. Aqui garantimos que ao menos um documento plausível foi informado.
            RuleFor(x => x.Cnpj)
                .NotEmpty().When(x => string.IsNullOrWhiteSpace(x.Cpf))
                .WithMessage("Informe um documento fiscal válido: CNPJ (empresa) ou CPF (pessoa física).");

            RuleFor(x => x.NomeAdmin)
                .NotEmpty().WithMessage("O nome do administrador é obrigatório.");

            RuleFor(x => x.EmailAdmin)
                .NotEmpty().WithMessage("O e-mail do administrador é obrigatório.")
                .EmailAddress().WithMessage("O e-mail do administrador informado é inválido.");

            RuleFor(x => x.SenhaAdmin)
                .AplicarPoliticaSenha();

            // REG-036: município IBGE obrigatório (7 dígitos) — validado contra o cadastro no handler.
            RuleFor(x => x.CodigoIbgeMunicipio)
                .GreaterThan(0).WithMessage("O município (código IBGE) é obrigatório.")
                .Must(c => c.ToString().Length == 7).WithMessage("O código IBGE do município deve ter 7 dígitos.");

            // REG-036: telefone + tipo de telefone obrigatórios.
            RuleFor(x => x.Telefone)
                .NotEmpty().WithMessage("O telefone é obrigatório.");

            RuleFor(x => x.TipoTelefone)
                .NotEmpty().WithMessage("O tipo de telefone é obrigatório.")
                .Must(t => Array.Exists(TiposTelefoneValidos, v => string.Equals(v, t, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Tipo de telefone inválido. Valores aceitos: Fixo, Celular, Comercial, Whatsapp.");
        }
    }

    /// <summary>Logout / revogação de sessão (REG-013). Revoga as sessões ativas do usuário autenticado.</summary>
    public record EncerrarSessaoCommand(
        Guid UsuarioId
    ) : ICommand;

    public class EncerrarSessaoCommandValidator : AbstractValidator<EncerrarSessaoCommand>
    {
        public EncerrarSessaoCommandValidator()
        {
            RuleFor(x => x.UsuarioId)
                .NotEmpty().WithMessage("O ID do usuário é obrigatório.");
        }
    }
}
