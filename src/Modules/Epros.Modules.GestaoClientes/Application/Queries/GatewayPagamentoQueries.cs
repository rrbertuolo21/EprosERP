using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarGatewaysPagamentoQuery(
        int Pagina = 1,
        int TamanhoPagina = 25
    ) : IQuery<PagedQueryResult<GatewayPagamentoListaDto>>;

    public record ObterGatewayPagamentoPorIdQuery(Guid Id) : IQuery<GatewayPagamentoDetalhadoDto>;

    /// <summary>Item de lista de gateways. O AccessToken NUNCA é exposto em texto (apenas máscara).</summary>
    public class GatewayPagamentoListaDto
    {
        public Guid Id { get; set; }
        public string Provedor { get; set; } = string.Empty;
        public string Ambiente { get; set; } = string.Empty;
        public string? PublicKey { get; set; }
        public string Moeda { get; set; } = "BRL";
        public string? NotificationUrl { get; set; }
        /// <summary>Escopo: null = configuração global; preenchido = override por tenant.</summary>
        public string? TenantId { get; set; }
        public bool Ativo { get; set; }
        /// <summary>Máscara do access token (ex.: "••••1234").</summary>
        public string AccessTokenMascarado { get; set; } = string.Empty;
        /// <summary>Indica se há um webhook secret configurado (sem revelar o valor).</summary>
        public bool PossuiWebhookSecret { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class GatewayPagamentoDetalhadoDto
    {
        public Guid Id { get; set; }
        public string Provedor { get; set; } = string.Empty;
        public string Ambiente { get; set; } = string.Empty;
        public string? PublicKey { get; set; }
        public string Moeda { get; set; } = "BRL";
        public string? NotificationUrl { get; set; }
        public string? TenantId { get; set; }
        public bool Ativo { get; set; }
        public string AccessTokenMascarado { get; set; } = string.Empty;
        public bool PossuiWebhookSecret { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
