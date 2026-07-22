using System;
using System.Collections.Generic;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    public record InsumoConsumidoNotification(
        string InsumoSku,
        decimal QuantidadeConsumida
    );

    /// <summary>
    /// Evento publicado quando uma ordem de produção é encerrada.
    /// Consumido pelo Estoque para dar entrada no produto acabado e baixar os insumos utilizados.
    /// </summary>
    public record OrdemProducaoEncerradaEventNotification(
        Guid OrdemProducaoId,
        string Codigo,
        string ProdutoAcabadoSku,
        decimal QuantidadeProduzida,
        decimal QuantidadeRefugada,
        string TenantId,
        List<InsumoConsumidoNotification> InsumosConsumidos
    ) : INotification;
}
