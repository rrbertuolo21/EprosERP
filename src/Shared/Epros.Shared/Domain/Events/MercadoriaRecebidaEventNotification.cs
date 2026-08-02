using System;
using System.Collections.Generic;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento de recebimento fisico de mercadoria (Estoque LDE — <see cref="CatalogoEventosIntegracao.Estoque.MercadoriaRecebida"/>).
    /// Consumido pela Qualidade para disparar a inspecao/analise de aceite de entrada (QLD-ACR).
    /// Relayed do Outbox do Estoque para MediatR pelo job central de integracao.
    /// </summary>
    public record MercadoriaRecebidaEventNotification(
        Guid EntradaId,
        Guid? CompraId,
        Guid? FornecedorId,
        Guid? DocumentoId,
        string? ChaveAcesso,
        string? Numero,
        decimal ValorTotal,
        string TenantId,
        List<MercadoriaRecebidaItemNotification> Itens
    ) : INotification;

    public record MercadoriaRecebidaItemNotification(
        Guid? ProdutoId,
        decimal Quantidade,
        decimal? ValorItem
    );
}
