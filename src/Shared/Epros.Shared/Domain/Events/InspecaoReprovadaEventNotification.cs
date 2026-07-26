using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado quando um lote de produto é reprovado na inspeção de qualidade.
    /// Consumido pelo Estoque para bloquear/ajustar o saldo correspondente.
    /// </summary>
    public record InspecaoReprovadaEventNotification(
        Guid InspecaoLoteId,
        string TenantId,
        string Sku,
        string NomeProduto,
        decimal QuantidadeLote,
        DateTime? DataInspecao,
        string Responsavel
    ) : INotification;
}
