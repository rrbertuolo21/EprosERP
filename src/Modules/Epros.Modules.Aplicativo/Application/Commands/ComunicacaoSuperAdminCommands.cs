using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record EnviarComunicacaoSuperAdminCommand(
        List<string> BusinessIds,
        string Subject,
        string Message,
        List<string>? Canais = null
    ) : ICommand;

    public class EnviarComunicacaoSuperAdminCommandValidator : AbstractValidator<EnviarComunicacaoSuperAdminCommand>
    {
        public EnviarComunicacaoSuperAdminCommandValidator()
        {
            RuleFor(x => x.BusinessIds)
                .NotEmpty().WithMessage("Pelo menos um destinatário (business_id) é obrigatório.");

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("O assunto é obrigatório.");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("A mensagem é obrigatória.");

            RuleFor(x => x.Canais)
                .Must(canais => canais == null || canais.All(c => c == "Email" || c == "SMS" || c == "WhatsApp"))
                .WithMessage("Os canais permitidos são 'Email', 'SMS' e 'WhatsApp'.");
        }
    }

    public record AtualizarStatusComunicacaoCommand(
        Guid ComunicacaoId,
        string Status
    ) : ICommand;

    public class AtualizarStatusComunicacaoCommandValidator : AbstractValidator<AtualizarStatusComunicacaoCommand>
    {
        public AtualizarStatusComunicacaoCommandValidator()
        {
            RuleFor(x => x.ComunicacaoId)
                .NotEmpty().WithMessage("O ID da comunicação é obrigatório.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("O status é obrigatório.")
                .Must(x => x == "Sucesso" || x == "Falha")
                .WithMessage("O status deve ser 'Sucesso' ou 'Falha'.");
        }
    }
}
