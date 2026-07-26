using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    // ===================== PRD-MRP — MRP / Planejamento Integrado IBP =====================
    // Motor MRP (explosão BOM, netting, sugestões) é lacuna controlada — ver DP-MRP-002..006 na EF.

    public record CriarMrpPlanejamentoCommand(string Codigo, Guid ResponsavelId) : ICommand;

    public class CriarMrpPlanejamentoCommandValidator : AbstractValidator<CriarMrpPlanejamentoCommand>
    {
        public CriarMrpPlanejamentoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código do planejamento é obrigatório. (RN-MRP-002)");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsável é obrigatório. (RN-MRP-004)");
        }
    }

    public record SubmeterMrpPlanejamentoCommand(Guid Id) : ICommand;
    public record AprovarMrpPlanejamentoCommand(Guid Id) : ICommand;
    public record RejeitarMrpPlanejamentoCommand(Guid Id, string Motivo) : ICommand;
    public record InativarMrpPlanejamentoCommand(Guid Id) : ICommand;
    public record ReativarMrpPlanejamentoCommand(Guid Id) : ICommand;
    public record EncerrarMrpPlanejamentoCommand(Guid Id) : ICommand;
}
