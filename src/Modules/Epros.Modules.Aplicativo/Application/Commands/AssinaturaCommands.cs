using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record ContratarPlanoCommand(
        Guid PlanoId,
        string MetodoPagamento
    ) : ICommand;

    public class ContratarPlanoCommandValidator : AbstractValidator<ContratarPlanoCommand>
    {
        public ContratarPlanoCommandValidator()
        {
            RuleFor(c => c.PlanoId)
                .NotEmpty().WithMessage("O ID do plano é obrigatório.");

            RuleFor(c => c.MetodoPagamento)
                .NotEmpty().WithMessage("O método de pagamento é obrigatório.")
                .Must(m => m == "PIX" || m == "Manual")
                .WithMessage("Método de pagamento aceito: 'PIX' ou 'Manual'.");
        }
    }

    public record GerarPixFaturaCommand(
        Guid FaturaId
    ) : ICommand;

    public class GerarPixFaturaCommandValidator : AbstractValidator<GerarPixFaturaCommand>
    {
        public GerarPixFaturaCommandValidator()
        {
            RuleFor(c => c.FaturaId)
                .NotEmpty().WithMessage("O ID da fatura é obrigatório.");
        }
    }

    // 1.08D — Mudança de plano self-service (upgrade/downgrade com proração pro-rata por dias).
    public record MudarPlanoCommand(
        Guid NovoPlanoId
    ) : ICommand;

    public class MudarPlanoCommandValidator : AbstractValidator<MudarPlanoCommand>
    {
        public MudarPlanoCommandValidator()
        {
            RuleFor(c => c.NovoPlanoId)
                .NotEmpty().WithMessage("O ID do novo plano é obrigatório.");
        }
    }

    // 1.08D — Cancelamento self-service governado (registra motivo). Entra na janela de export de 30 dias.
    public record CancelarAssinaturaCommand(
        string? Motivo
    ) : ICommand;

    // 1.08D — Reativação self-service dentro da janela de 30 dias (volta a Ativo).
    public record ReativarAssinaturaCommand() : ICommand;
}
