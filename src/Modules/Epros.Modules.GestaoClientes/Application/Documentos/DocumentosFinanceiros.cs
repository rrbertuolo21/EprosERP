using System;
using System.Collections.Generic;
using System.Globalization;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Documentos
{
    /// <summary>1.08F — Documento binário renderizado (PDF), pronto para transporte HTTP.</summary>
    public sealed record DocumentoRenderizado(byte[] Conteudo, string ContentType, string NomeArquivo);

    /// <summary>1.08F — Link do boleto: o gateway (Mercado Pago) hospeda o PDF; aqui apenas EXPOMOS a URL.</summary>
    public sealed record BoletoLinkDto(Guid FaturaId, string? UrlBoleto, string? LinhaDigitavel);

    /// <summary>1.08F — PDF da fatura da assinatura (nº, tenant, itens, valores, vencimento, status).</summary>
    public record ObterFaturaPdfQuery(Guid FaturaId) : IQuery<DocumentoRenderizado?>;

    /// <summary>1.08F — PDF do recibo de pagamento (nº, pagador, valor, data, meio, fatura ref).</summary>
    public record ObterReciboPdfQuery(Guid FaturaId) : IQuery<DocumentoRenderizado?>;

    /// <summary>1.08F — Link do PDF do boleto que o gateway já forneceu (não gera boleto do zero).</summary>
    public record ObterBoletoLinkQuery(Guid FaturaId) : IQuery<BoletoLinkDto?>;

    /// <summary>
    /// Renderizador dos documentos financeiros do control-plane. A implementação real (QuestPDF, licença
    /// Community) devolve <c>application/pdf</c>. Se a lib não estiver disponível no ambiente, esta fatia cai
    /// para HTML print-ready (plano B) — ver MC/MANUAL_CENTRAL §4.
    /// </summary>
    public interface IDocumentoFinanceiroRenderer
    {
        DocumentoRenderizado RenderFatura(FaturaDetalhadaDto fatura);
        DocumentoRenderizado RenderRecibo(ReciboPagamentoDto recibo);
    }

    /// <summary>
    /// Fonte ÚNICA do conteúdo textual dos documentos (mapeamento DTO → linhas). O renderer de PDF e os
    /// testes consomem estas mesmas linhas, garantindo que o documento carrega os dados da fatura/recibo
    /// sem depender de inspeção binária do PDF.
    /// </summary>
    public static class DocumentoFinanceiroConteudo
    {
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

        public static string Moeda(decimal v) => v.ToString("C", PtBr);
        public static string Data(DateTime d) => d.ToString("dd/MM/yyyy", PtBr);

        public static string NumeroFatura(FaturaDetalhadaDto f)
            => string.IsNullOrWhiteSpace(f.Numero) ? $"FAT-{f.Id.ToString("N").Substring(0, 8).ToUpperInvariant()}" : f.Numero!;

        public static string NomeArquivoFatura(FaturaDetalhadaDto f) => $"fatura-{NumeroFatura(f)}.pdf";
        public static string NomeArquivoRecibo(ReciboPagamentoDto r) => $"recibo-{r.Numero}.pdf";

        /// <summary>Linhas (rótulo, valor) do cabeçalho da fatura — inclui nº e valor.</summary>
        public static IReadOnlyList<(string Rotulo, string Valor)> LinhasFatura(FaturaDetalhadaDto f)
        {
            var linhas = new List<(string, string)>
            {
                ("Número", NumeroFatura(f)),
                ("Cliente", f.ClienteRazaoSocial),
                ("Status", f.Status),
                ("Vencimento", Data(f.DataVencimento)),
                ("Valor", Moeda(f.Valor)),
            };
            if (f.Quitada)
            {
                linhas.Add(("Valor pago", Moeda(f.ValorPago)));
                if (f.DataPagamento.HasValue) linhas.Add(("Pago em", Data(f.DataPagamento.Value)));
            }
            if (!string.IsNullOrWhiteSpace(f.Observacoes)) linhas.Add(("Observações", f.Observacoes!));
            return linhas;
        }

        /// <summary>Linhas (rótulo, valor) do recibo — inclui nº, pagador, valor, data, meio, fatura ref.</summary>
        public static IReadOnlyList<(string Rotulo, string Valor)> LinhasRecibo(ReciboPagamentoDto r)
            => new List<(string, string)>
            {
                ("Recibo nº", r.Numero),
                ("Pagador", r.PagadorNome ?? "-"),
                ("Documento", r.PagadorDocumento ?? "-"),
                ("Valor", Moeda(r.Valor)),
                ("Data do pagamento", Data(r.DataPagamento)),
                ("Meio de pagamento", r.MeioPagamento),
                ("Fatura de referência", r.FaturaId.ToString()),
            };
    }
}
