using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
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
                .NotEmpty().WithMessage("A nova senha é obrigatória.")
                .MinimumLength(8).WithMessage("A nova senha deve ter no mínimo 8 caracteres.");

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
                .NotEmpty().WithMessage("A nova senha é obrigatória.")
                .MinimumLength(8).WithMessage("A nova senha deve ter no mínimo 8 caracteres.")
                .NotEqual(x => x.SenhaAtual).WithMessage("A nova senha não pode ser igual à senha atual.");
        }
    }

    public record RegistrarNovoTenantCommand(
        string NomeEmpresa,
        string Cnpj,
        string NomeAdmin,
        string EmailAdmin,
        string SenhaAdmin
    ) : ICommand;

    public class RegistrarNovoTenantCommandValidator : AbstractValidator<RegistrarNovoTenantCommand>
    {
        public RegistrarNovoTenantCommandValidator()
        {
            RuleFor(x => x.NomeEmpresa)
                .NotEmpty().WithMessage("O nome da empresa é obrigatório.");

            RuleFor(x => x.Cnpj)
                .NotEmpty().WithMessage("O CNPJ é obrigatório.")
                .Length(14).WithMessage("O CNPJ deve conter exatamente 14 dígitos.");

            RuleFor(x => x.NomeAdmin)
                .NotEmpty().WithMessage("O nome do administrador é obrigatório.");

            RuleFor(x => x.EmailAdmin)
                .NotEmpty().WithMessage("O e-mail do administrador é obrigatório.")
                .EmailAddress().WithMessage("O e-mail do administrador informado é inválido.");

            RuleFor(x => x.SenhaAdmin)
                .NotEmpty().WithMessage("A senha do administrador é obrigatória.")
                .MinimumLength(8).WithMessage("A senha do administrador deve ter no mínimo 8 caracteres.");
        }
    }
}
