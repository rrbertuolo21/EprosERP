using System;
using Epros.Modules.GestaoClientes.Application.Documentos;
using Epros.Modules.GestaoClientes.Application.Queries;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Epros.Modules.GestaoClientes.Infrastructure.Documentos
{
    /// <summary>
    /// 1.08F — Renderiza fatura e recibo como PDF real (QuestPDF, licença Community — padrão .NET).
    /// O conteúdo textual vem de <see cref="DocumentoFinanceiroConteudo"/> (fonte única), então o PDF sempre
    /// carrega nº/valor/itens da fatura e nº/pagador/valor do recibo.
    /// </summary>
    public sealed class QuestPdfDocumentoFinanceiroRenderer : IDocumentoFinanceiroRenderer
    {
        static QuestPdfDocumentoFinanceiroRenderer()
        {
            // Licença Community (gratuita para faturamento anual < US$1M) — obrigatória antes de gerar.
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public DocumentoRenderizado RenderFatura(FaturaDetalhadaDto fatura)
        {
            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);
                    page.Header().Column(h =>
                    {
                        h.Item().Text("FATURA").FontSize(22).Bold();
                        h.Item().Text("Assinatura SaaS — Epros").FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(6);
                        foreach (var (rotulo, valor) in DocumentoFinanceiroConteudo.LinhasFatura(fatura))
                            LinhaRotulada(col, rotulo, valor);

                        if (fatura.Itens.Count > 0)
                        {
                            col.Item().PaddingTop(12).Text("Itens").Bold();
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); });
                                foreach (var item in fatura.Itens)
                                {
                                    table.Cell().PaddingVertical(2).Text(item.Descricao);
                                    table.Cell().PaddingVertical(2).AlignRight()
                                        .Text(DocumentoFinanceiroConteudo.Moeda(item.Valor));
                                }
                            });
                        }
                    });

                    Rodape(page, $"Documento gerado em {DocumentoFinanceiroConteudo.Data(DateTime.UtcNow)}");
                });
            }).GeneratePdf();

            return new DocumentoRenderizado(bytes, "application/pdf", DocumentoFinanceiroConteudo.NomeArquivoFatura(fatura));
        }

        public DocumentoRenderizado RenderRecibo(ReciboPagamentoDto recibo)
        {
            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);
                    page.Header().Column(h =>
                    {
                        h.Item().Text("RECIBO DE PAGAMENTO").FontSize(22).Bold();
                        h.Item().Text("Documento simples — não é NFS-e").FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(6);
                        foreach (var (rotulo, valor) in DocumentoFinanceiroConteudo.LinhasRecibo(recibo))
                            LinhaRotulada(col, rotulo, valor);
                    });

                    Rodape(page, "A emissão fiscal (NFS-e/ISS) está diferida — este recibo não substitui a nota fiscal.");
                });
            }).GeneratePdf();

            return new DocumentoRenderizado(bytes, "application/pdf", DocumentoFinanceiroConteudo.NomeArquivoRecibo(recibo));
        }

        private static void ConfigurarPagina(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(35);
            page.DefaultTextStyle(x => x.FontSize(11));
        }

        private static void LinhaRotulada(ColumnDescriptor col, string rotulo, string valor)
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(150).Text(rotulo).SemiBold().FontColor(Colors.Grey.Darken2);
                row.RelativeItem().Text(valor);
            });
        }

        private static void Rodape(PageDescriptor page, string texto)
        {
            page.Footer().PaddingTop(10).Text(texto).FontSize(8).FontColor(Colors.Grey.Darken1);
        }
    }
}
