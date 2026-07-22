using System;
using MediatR;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// Evento publicado após commit da criação de uma Pessoa (CAD-PEM REG-PEM-160/161: pessoa.criada).
    /// Consumido por Vendas, Compras, Financeiro, Fiscal e Plataforma.
    /// </summary>
    public record PessoaCriadaEventNotification(
        Guid PessoaId,
        string TenantId,
        int TipoPessoa,
        string? Documento,
        bool EhCliente,
        bool EhFornecedor,
        DateTime CriadoEm,
        string UserId
    ) : INotification;
}
