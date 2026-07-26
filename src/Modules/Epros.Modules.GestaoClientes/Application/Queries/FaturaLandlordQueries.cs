using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarFaturasQuery(
        int Pagina = 1,
        int TamanhoPagina = 25,
        Guid? ClienteId = null,
        string? Status = null
    ) : IQuery<PagedQueryResult<FaturaListaDto>>;

    public record ObterFaturaPorIdQuery(Guid Id) : IQuery<FaturaDetalhadaDto>;

    public class FaturaListaDto
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string ClienteRazaoSocial { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
    }

    public class FaturaDetalhadaDto
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string ClienteRazaoSocial { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal PercentualComissaoRevenda { get; set; }
        public decimal PercentualComissaoVendedor { get; set; }
        public decimal ValorComissaoRevenda { get; set; }
        public decimal ValorComissaoVendedor { get; set; }
        public DateTime CriadoEm { get; set; }
        public List<PagamentoFaturaDto> Pagamentos { get; set; } = new();
    }

    public class PagamentoFaturaDto
    {
        public Guid Id { get; set; }
        public string TipoPagamento { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal ValorPago { get; set; }
        public bool PagoManualmente { get; set; }
        public DateTime? DataPagamento { get; set; }
    }
}
