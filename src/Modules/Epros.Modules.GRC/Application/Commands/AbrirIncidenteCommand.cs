using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    public record AbrirIncidenteCommand(
        string Titulo,
        string Descricao,
        string Origem, // SoD, Denuncia, Auditoria, Seguranca
        string Gravidade // Baixa, Media, Alta, Critica
    ) : ICommand;

    public class AbrirIncidenteCommandValidator : AbstractValidator<AbrirIncidenteCommand>
    {
        public AbrirIncidenteCommandValidator()
        {
            RuleFor(c => c.Titulo).NotEmpty().WithMessage("O título do incidente é obrigatório.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descrição do incidente é obrigatória.");
            RuleFor(c => c.Origem).Must(o => o == "SoD" || o == "Denuncia" || o == "Auditoria" || o == "Seguranca")
                .WithMessage("A origem deve ser 'SoD', 'Denuncia', 'Auditoria' ou 'Seguranca'.");
            RuleFor(c => c.Gravidade).Must(g => g == "Baixa" || g == "Media" || g == "Alta" || g == "Critica")
                .WithMessage("A gravidade deve ser 'Baixa', 'Media', 'Alta' ou 'Critica'.");
        }
    }
}
