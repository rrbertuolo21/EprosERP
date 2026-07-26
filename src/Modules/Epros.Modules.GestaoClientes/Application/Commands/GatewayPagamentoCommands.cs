using System;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    /// <summary>Cria uma configuração de gateway de pagamento (landlord/plataforma).</summary>
    public record CriarGatewayPagamentoCommand(
        EProvedorGateway Provedor,
        EAmbienteGateway Ambiente,
        string AccessToken,
        string? PublicKey = null,
        string? WebhookSecret = null,
        string Moeda = "BRL",
        string? NotificationUrl = null,
        string? TenantId = null,
        bool Ativo = true
    ) : ICommand;

    public class CriarGatewayPagamentoCommandValidator : AbstractValidator<CriarGatewayPagamentoCommand>
    {
        public CriarGatewayPagamentoCommandValidator()
        {
            RuleFor(g => g.AccessToken)
                .NotEmpty().WithMessage("O Access Token é obrigatório.");
            RuleFor(g => g.Moeda)
                .NotEmpty().WithMessage("A Moeda é obrigatória.")
                .MaximumLength(3).WithMessage("A Moeda deve ter no máximo 3 caracteres (ISO 4217).");
        }
    }

    /// <summary>
    /// Atualiza uma configuração de gateway. AccessToken/WebhookSecret vazios preservam o segredo atual.
    /// </summary>
    public record AtualizarGatewayPagamentoCommand(
        Guid Id,
        EProvedorGateway Provedor,
        EAmbienteGateway Ambiente,
        string? AccessToken = null,
        string? PublicKey = null,
        string? WebhookSecret = null,
        string Moeda = "BRL",
        string? NotificationUrl = null,
        string? TenantId = null,
        bool Ativo = true
    ) : ICommand;

    public class AtualizarGatewayPagamentoCommandValidator : AbstractValidator<AtualizarGatewayPagamentoCommand>
    {
        public AtualizarGatewayPagamentoCommandValidator()
        {
            RuleFor(g => g.Id)
                .NotEmpty().WithMessage("O ID do gateway é obrigatório.");
            RuleFor(g => g.Moeda)
                .NotEmpty().WithMessage("A Moeda é obrigatória.")
                .MaximumLength(3).WithMessage("A Moeda deve ter no máximo 3 caracteres (ISO 4217).");
        }
    }

    /// <summary>Exclui (soft-delete) uma configuração de gateway.</summary>
    public record ExcluirGatewayPagamentoCommand(Guid Id) : ICommand;

    /// <summary>Testa a conexão com o gateway (GET leve validando o access token).</summary>
    public record TestarConexaoGatewayPagamentoCommand(Guid Id) : ICommand;
}
