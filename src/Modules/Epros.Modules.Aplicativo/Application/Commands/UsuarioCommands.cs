using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Aplicativo.Domain.Entities;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record UsuarioEmpresaInput(
        Guid EmpresaId,
        Guid? PerfilUsuarioId,
        bool EhAdmin,
        string Cargo,
        string Departamento,
        decimal LimiteDesconto
    );

    // CRUD Usuários
    public record CriarUsuarioCommand(
        string Nome,
        string Email,
        string Senha,
        string? Telefone,
        UsuarioTipo Tipo,
        UsuarioStatus Status,
        List<UsuarioEmpresaInput> Empresas
    ) : ICommand;

    public class CriarUsuarioCommandValidator : AbstractValidator<CriarUsuarioCommand>
    {
        public CriarUsuarioCommandValidator()
        {
            RuleFor(x => x.Nome).NotEmpty().WithMessage("O nome é obrigatório.").MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("O e-mail é obrigatório.").EmailAddress().WithMessage("O e-mail deve ser um endereço válido.").MaximumLength(120).WithMessage("O e-mail deve ter no máximo 120 caracteres.");
            RuleFor(x => x.Senha).NotEmpty().WithMessage("A senha é obrigatória.").MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");
            RuleFor(x => x.Empresas).NotEmpty().WithMessage("O usuário operacional deve possuir ao menos um vínculo de empresa.");
            RuleForEach(x => x.Empresas).ChildRules(empresa =>
            {
                empresa.RuleFor(e => e.EmpresaId).NotEmpty().WithMessage("O ID da empresa é obrigatório.");
                empresa.RuleFor(e => e.Cargo).NotEmpty().WithMessage("O cargo é obrigatório.");
                empresa.RuleFor(e => e.Departamento).NotEmpty().WithMessage("O departamento é obrigatório.");
                empresa.RuleFor(e => e.LimiteDesconto).GreaterThanOrEqualTo(0).WithMessage("O limite de desconto deve ser igual ou maior que zero.");
                empresa.RuleFor(e => e.PerfilUsuarioId)
                    .NotEmpty()
                    .When(e => !e.EhAdmin)
                    .WithMessage("O perfil do usuário é obrigatório quando o usuário não for administrador.");
            });
        }
    }

    public record AtualizarUsuarioCommand(
        Guid UsuarioId,
        string Nome,
        string? Telefone,
        UsuarioTipo Tipo,
        UsuarioStatus Status,
        List<UsuarioEmpresaInput> Empresas
    ) : ICommand;

    public class AtualizarUsuarioCommandValidator : AbstractValidator<AtualizarUsuarioCommand>
    {
        public AtualizarUsuarioCommandValidator()
        {
            RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O ID do usuário é obrigatório.");
            RuleFor(x => x.Nome).NotEmpty().WithMessage("O nome é obrigatório.").MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");
            RuleFor(x => x.Empresas).NotEmpty().WithMessage("O usuário operacional deve possuir ao menos um vínculo de empresa.");
            RuleForEach(x => x.Empresas).ChildRules(empresa =>
            {
                empresa.RuleFor(e => e.EmpresaId).NotEmpty().WithMessage("O ID da empresa é obrigatório.");
                empresa.RuleFor(e => e.Cargo).NotEmpty().WithMessage("O cargo é obrigatório.");
                empresa.RuleFor(e => e.Departamento).NotEmpty().WithMessage("O departamento é obrigatório.");
                empresa.RuleFor(e => e.LimiteDesconto).GreaterThanOrEqualTo(0).WithMessage("O limite de desconto deve ser igual ou maior que zero.");
                empresa.RuleFor(e => e.PerfilUsuarioId)
                    .NotEmpty()
                    .When(e => !e.EhAdmin)
                    .WithMessage("O perfil do usuário é obrigatório quando o usuário não for administrador.");
            });
        }
    }

    public record DeletarUsuarioCommand(Guid UsuarioId) : ICommand;

    public class DeletarUsuarioCommandValidator : AbstractValidator<DeletarUsuarioCommand>
    {
        public DeletarUsuarioCommandValidator()
        {
            RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O ID do usuário é obrigatório.");
        }
    }

    // Troca de Senha Administrativa
    public record AlterarSenhaAdministrativaCommand(Guid UsuarioId, string NovaSenha) : ICommand;

    public class AlterarSenhaAdministrativaCommandValidator : AbstractValidator<AlterarSenhaAdministrativaCommand>
    {
        public AlterarSenhaAdministrativaCommandValidator()
        {
            RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O ID do usuário é obrigatório.");
            RuleFor(x => x.NovaSenha).NotEmpty().WithMessage("A nova senha é obrigatória.").MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");
        }
    }

    // Impersonação
    public record IniciarImpersonacaoCommand(Guid UsuarioAlvoId, Guid? EmpresaId, string Motivo) : ICommand;

    public class IniciarImpersonacaoCommandValidator : AbstractValidator<IniciarImpersonacaoCommand>
    {
        public IniciarImpersonacaoCommandValidator()
        {
            RuleFor(x => x.UsuarioAlvoId).NotEmpty().WithMessage("O ID do usuário alvo é obrigatório.");
            RuleFor(x => x.Motivo).NotEmpty().WithMessage("O motivo da impersonação é obrigatório.");
        }
    }

    public record EncerrarImpersonacaoCommand(Guid SessaoImpersonacaoId) : ICommand;

    public class EncerrarImpersonacaoCommandValidator : AbstractValidator<EncerrarImpersonacaoCommand>
    {
        public EncerrarImpersonacaoCommandValidator()
        {
            RuleFor(x => x.SessaoImpersonacaoId).NotEmpty().WithMessage("O ID da sessão de impersonação é obrigatório.");
        }
    }

    // Landlord — Acesso de suporte da Siser a um tenant cliente (1.04 Pass 4)
    // O portador (usuário Siser com perfil de suporte) abre uma sessão de suporte num cliente.
    public record IniciarAcessoSuporteCommand(string TenantAlvo, Guid UsuarioAlvoId, Guid? EmpresaId, string Motivo) : ICommand;

    public class IniciarAcessoSuporteCommandValidator : AbstractValidator<IniciarAcessoSuporteCommand>
    {
        public IniciarAcessoSuporteCommandValidator()
        {
            RuleFor(x => x.TenantAlvo).NotEmpty().WithMessage("O tenant do cliente alvo é obrigatório.");
            RuleFor(x => x.UsuarioAlvoId).NotEmpty().WithMessage("O ID do usuário alvo é obrigatório.");
            RuleFor(x => x.Motivo).NotEmpty().WithMessage("O motivo do acesso de suporte é obrigatório.");
        }
    }

    // Landlord — Definir o perfil de suporte (papel de sistema da Siser) de um usuário interno.
    public record DefinirPerfilSuporteInternoCommand(Guid UsuarioInternoId, PerfilSuporteSiser PerfilSuporte) : ICommand;

    public class DefinirPerfilSuporteInternoCommandValidator : AbstractValidator<DefinirPerfilSuporteInternoCommand>
    {
        public DefinirPerfilSuporteInternoCommandValidator()
        {
            RuleFor(x => x.UsuarioInternoId).NotEmpty().WithMessage("O ID do usuário interno é obrigatório.");
        }
    }

    // API Keys
    public record GerarApiKeyCommand(
        Guid UsuarioId,
        int? RateLimit = null,
        int? ValidadeDias = null
    ) : ICommand;

    public class GerarApiKeyCommandValidator : AbstractValidator<GerarApiKeyCommand>
    {
        public GerarApiKeyCommandValidator()
        {
            RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O ID do usuário é obrigatório.");

            RuleFor(x => x.RateLimit)
                .GreaterThan(0).When(x => x.RateLimit.HasValue)
                .WithMessage("O limite de requisições deve ser maior que zero.");

            RuleFor(x => x.ValidadeDias)
                .GreaterThan(0).When(x => x.ValidadeDias.HasValue)
                .WithMessage("A validade em dias deve ser maior que zero.");
        }
    }

    public record RevogarApiKeyCommand(Guid UsuarioId) : ICommand;

    public class RevogarApiKeyCommandValidator : AbstractValidator<RevogarApiKeyCommand>
    {
        public RevogarApiKeyCommandValidator()
        {
            RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O ID do usuário é obrigatório.");
        }
    }

    // Preferências
    public record AtualizarPreferenciasUsuarioCommand(
        Guid UsuarioId,
        string? Idioma,
        string? Tema,
        string? Avatar,
        bool RecebeNotificacoes,
        string? Timezone,
        string? FormatoData,
        string? FormatoHora,
        string? FormatoNumero,
        string? PreferenciasJson
    ) : ICommand;

    public class AtualizarPreferenciasUsuarioCommandValidator : AbstractValidator<AtualizarPreferenciasUsuarioCommand>
    {
        public AtualizarPreferenciasUsuarioCommandValidator()
        {
            RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O ID do usuário é obrigatório.");
        }
    }
}
