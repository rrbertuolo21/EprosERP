using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    // GRC-CIA — Controles Internos e Auditoria (aditivo ao CriarControleCommand ja existente)

    public record CriarPlanoAuditoriaCommand(
        string Codigo,
        string Titulo,
        string Descricao,
        string? Ciclo
    ) : ICommand;

    public class CriarPlanoAuditoriaCommandValidator : AbstractValidator<CriarPlanoAuditoriaCommand>
    {
        public CriarPlanoAuditoriaCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O codigo do plano de auditoria e obrigatorio.");
            RuleFor(c => c.Titulo).NotEmpty().WithMessage("O titulo do plano de auditoria e obrigatorio.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descricao do plano de auditoria e obrigatoria.");
        }
    }

    public record RegistrarTesteControleCommand(
        Guid ControleId,
        Guid? PlanoAuditoriaId,
        string Resultado, // Conforme, NaoConforme, NaoAplicavel
        string? Observacao
    ) : ICommand;

    public class RegistrarTesteControleCommandValidator : AbstractValidator<RegistrarTesteControleCommand>
    {
        public RegistrarTesteControleCommandValidator()
        {
            RuleFor(c => c.ControleId).NotEmpty().WithMessage("O controle e obrigatorio.");
            RuleFor(c => c.Resultado).Must(r => r == "Conforme" || r == "NaoConforme" || r == "NaoAplicavel")
                .WithMessage("O resultado deve ser 'Conforme', 'NaoConforme' ou 'NaoAplicavel'.");
        }
    }

    public record RegistrarAchadoCommand(
        Guid? TesteControleId,
        string Titulo,
        string Descricao,
        string Severidade, // Baixa, Media, Alta, Critica
        DateTime? PrazoRemediacao
    ) : ICommand;

    public class RegistrarAchadoCommandValidator : AbstractValidator<RegistrarAchadoCommand>
    {
        public RegistrarAchadoCommandValidator()
        {
            RuleFor(c => c.Titulo).NotEmpty().WithMessage("O titulo do achado e obrigatorio.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descricao do achado e obrigatoria.");
            RuleFor(c => c.Severidade).Must(s => s == "Baixa" || s == "Media" || s == "Alta" || s == "Critica")
                .WithMessage("A severidade deve ser 'Baixa', 'Media', 'Alta' ou 'Critica'.");
        }
    }
}
