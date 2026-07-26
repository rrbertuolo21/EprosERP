using System;
using System.Collections.Generic;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    public record OrdemManutencaoPecaDto(Guid ProdutoId, decimal Quantidade);

    /// <summary>
    /// Evento publicado quando uma Ordem de Serviço de manutenção é finalizada.
    /// Consumido pelo módulo de Estoque para realizar a baixa física das peças consumidas.
    /// </summary>
    public record OrdemManutencaoConcluidaEventNotification(
        Guid OrdemManutencaoId,
        Guid EquipamentoId,
        List<OrdemManutencaoPecaDto> Pecas,
        string TenantId
    ) : INotification;
}
