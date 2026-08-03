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

    /// <summary>1.08A — Recibo(s) de pagamento de uma fatura (documento simples; NFS-e diferida).</summary>
    public record ObterReciboPorFaturaQuery(Guid FaturaId) : IQuery<ReciboPagamentoDto?>;

    public class ReciboPagamentoDto
    {
        public Guid Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public Guid FaturaId { get; set; }
        public Guid ClienteId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataPagamento { get; set; }
        public string MeioPagamento { get; set; } = string.Empty;
        public string? PagadorNome { get; set; }
        public string? PagadorDocumento { get; set; }
    }

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
        public bool Quitada { get; set; }
        public decimal ValorPago { get; set; }
        public string? Numero { get; set; }
        public string? Observacoes { get; set; }
        public DateTime CriadoEm { get; set; }
        public List<FaturaItemDto> Itens { get; set; } = new();
        public List<PagamentoFaturaDto> Pagamentos { get; set; } = new();
    }

    public class FaturaItemDto
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
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
