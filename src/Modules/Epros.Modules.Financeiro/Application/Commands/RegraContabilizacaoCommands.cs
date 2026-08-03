using System;
using Epros.Shared.Application.Models;
using MediatR;

namespace Epros.Modules.Financeiro.Application.Commands
{
    // ----- DE-PARA evento→conta (motor de contabilização automática — TEC-8/FD-NF3) -----
    // ⛔ valida-contador: as contas do plano do cliente são parâmetro do contador (nullable até definir).

    public record DefinirRegraContabilizacaoCommand(
        string TipoEvento, Guid? ContaDebitoId, Guid? ContaCreditoId, string? Historico) : IRequest<CommandResult>;

    public record RemoverRegraContabilizacaoCommand(Guid Id) : IRequest<CommandResult>;
}
