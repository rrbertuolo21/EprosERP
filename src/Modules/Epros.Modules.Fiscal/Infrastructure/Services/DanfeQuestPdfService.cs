using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Epros.Modules.Fiscal.Infrastructure.Services
{
    /// <summary>
    /// Implementação do <see cref="IDanfeService"/> usando QuestPDF (licença Community).
    ///
    /// Gera um DANFE simplificado, porém REAL e legível, a partir dos dados persistidos do
    /// <see cref="DocumentoFiscal"/> (cabeçalho, emitente/destinatário, itens com impostos calculados,
    /// totais, chave de acesso e protocolo). Não depende das DLLs FastReport/NFe.Danfe do legado.
    ///
    /// Layout: A4 (retrato) para NF-e (modelo 55); bobina 80mm para o cupom NFC-e (modelo 65).
    /// Marca d'água "SEM VALOR FISCAL" quando o documento não está autorizado.
    ///
    /// Códigos gráficos obrigatórios:
    /// - NFC-e (65): QR Code (Anexo II Ajuste SINIEF 07/05). A URL do QR (<c>qrCode</c>) e a URL de
    ///   consulta (<c>urlChave</c>) são extraídas do bloco <c>infNFeSupl</c> do XML autorizado —
    ///   o motor Hercules já as calcula com o hash do CSC. NÃO fabricamos hash aqui.
    /// - NF-e (55): código de barras Code128C da chave de acesso de 44 dígitos.
    /// </summary>
    public class DanfeQuestPdfService : IDanfeService
    {
        private static readonly CultureInfo PtBr = new("pt-BR");

        static DanfeQuestPdfService()
        {
            // Licença Community (gratuita) — obrigatório configurar uma vez antes de gerar.
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GerarPdf(DocumentoFiscal documento)
        {
            if (documento is null)
                throw new ArgumentNullException(nameof(documento));

            return documento.Modelo == "65"
                ? GerarCupomNfce(documento)
                : GerarDanfeNfe(documento);
        }

        private bool Autorizado(DocumentoFiscal d) =>
            d.Status == "Autorizado" && !string.IsNullOrWhiteSpace(d.ChaveAcesso);

        // ---------------------------------------------------------------------------------------
        // NF-e (modelo 55) — DANFE A4
        // ---------------------------------------------------------------------------------------
        private byte[] GerarDanfeNfe(DocumentoFiscal documento)
        {
            var autorizado = Autorizado(documento);
            var chaveDigitos = SomenteDigitos(documento.ChaveAcesso);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Black));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("DANFE").Bold().FontSize(16);
                                c.Item().Text("Documento Auxiliar da Nota Fiscal Eletrônica").FontSize(8);
                            });
                            row.ConstantItem(160).AlignRight().Column(c =>
                            {
                                c.Item().Text($"NF-e nº {documento.Numero:000000000}").Bold();
                                c.Item().Text($"Série {documento.Serie}");
                                c.Item().Text($"Emissão: {documento.DataEmissao.ToLocalTime():dd/MM/yyyy HH:mm}");
                            });
                        });
                        if (!autorizado)
                            col.Item().PaddingTop(4).Text("SEM VALOR FISCAL — pré-visualização (documento não autorizado)")
                                .Bold().FontColor(Colors.Red.Medium);
                    });

                    page.Content().PaddingVertical(8).Column(col =>
                    {
                        col.Spacing(8);

                        // Chave de acesso / protocolo + código de barras Code128C (chave de 44 dígitos).
                        col.Item().Border(1).Padding(6).Column(c =>
                        {
                            c.Item().Text("CHAVE DE ACESSO").FontSize(7).Bold();
                            c.Item().Text(string.IsNullOrWhiteSpace(documento.ChaveAcesso) ? "(não autorizado)" : FormatarChave(documento.ChaveAcesso)).FontSize(10);
                            if (chaveDigitos.Length == 44)
                                c.Item().PaddingTop(4).Element(e => DesenharCode128(e, chaveDigitos, alturaPt: 40));
                            if (!string.IsNullOrWhiteSpace(documento.Protocolo))
                                c.Item().PaddingTop(2).Text($"Protocolo de autorização: {documento.Protocolo}").FontSize(8);
                        });

                        // Destinatário
                        col.Item().Border(1).Padding(6).Column(c =>
                        {
                            c.Item().Text("DESTINATÁRIO").FontSize(7).Bold();
                            c.Item().Text(documento.DestinatarioNome);
                            c.Item().Text($"CPF/CNPJ: {documento.DestinatarioCnpjCpf}").FontSize(8);
                        });

                        // Itens
                        col.Item().Text("PRODUTOS / SERVIÇOS").FontSize(8).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1.2f); // SKU
                                c.RelativeColumn(3.5f); // descrição
                                c.RelativeColumn(1f);   // NCM
                                c.RelativeColumn(0.8f); // CFOP
                                c.RelativeColumn(0.8f); // qtd
                                c.RelativeColumn(1.2f); // vl unit
                                c.RelativeColumn(1.2f); // vl total
                                c.RelativeColumn(1.2f); // ICMS
                            });

                            table.Header(h =>
                            {
                                void Head(string t) => h.Cell().Background(Colors.Grey.Lighten2).Border(0.5f).Padding(2).Text(t).FontSize(7).Bold();
                                Head("Código");
                                Head("Descrição");
                                Head("NCM");
                                Head("CFOP");
                                Head("Qtd");
                                Head("Vl.Unit");
                                Head("Vl.Total");
                                Head("ICMS");
                            });

                            void Cel(string t) => table.Cell().Border(0.5f).Padding(2).Text(t ?? string.Empty).FontSize(7);
                            foreach (var item in documento.Itens)
                            {
                                Cel(item.Sku);
                                Cel(item.NomeProduto);
                                Cel(item.Ncm);
                                Cel(item.Cfop.ToString());
                                Cel(item.Quantidade.ToString("0.##", PtBr));
                                Cel(Moeda(item.ValorUnitario));
                                Cel(Moeda(item.ValorTotal));
                                Cel(Moeda(item.ValorIcms));
                            }
                        });

                        // Totais
                        var totalIcms = documento.Itens.Sum(i => i.ValorIcms);
                        var totalIpi = documento.Itens.Sum(i => i.ValorIpi);
                        var totalPis = documento.Itens.Sum(i => i.ValorPis);
                        var totalCofins = documento.Itens.Sum(i => i.ValorCofins);
                        var totalSt = documento.Itens.Sum(i => i.ValorIcmsSt);

                        col.Item().Border(1).Padding(6).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Base ICMS / ICMS: {Moeda(documento.Itens.Sum(i => i.BaseCalculoIcms))} / {Moeda(totalIcms)}").FontSize(8);
                                c.Item().Text($"ICMS ST: {Moeda(totalSt)}   IPI: {Moeda(totalIpi)}").FontSize(8);
                                c.Item().Text($"PIS: {Moeda(totalPis)}   COFINS: {Moeda(totalCofins)}").FontSize(8);
                            });
                            row.ConstantItem(180).AlignRight().Column(c =>
                            {
                                c.Item().Text("VALOR TOTAL DA NOTA").FontSize(8).Bold();
                                c.Item().Text(Moeda(documento.Total)).FontSize(14).Bold();
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Gerado por EprosERP em ").FontSize(7);
                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm", PtBr)).FontSize(7);
                    });
                });
            }).GeneratePdf();
        }

        // ---------------------------------------------------------------------------------------
        // NFC-e (modelo 65) — cupom em bobina 80mm
        // ---------------------------------------------------------------------------------------
        private byte[] GerarCupomNfce(DocumentoFiscal documento)
        {
            var autorizado = Autorizado(documento);
            var (urlQrCode, urlChave) = ExtrairDadosQrCodeNfce(documento);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    // 80mm de largura; altura contínua.
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(4);
                    page.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Black));

                    page.Content().Column(col =>
                    {
                        col.Spacing(3);
                        col.Item().AlignCenter().Text("CUPOM FISCAL ELETRÔNICO").Bold();
                        col.Item().AlignCenter().Text("NFC-e").FontSize(10).Bold();
                        col.Item().AlignCenter().Text($"Nº {documento.Numero:000000000} — Série {documento.Serie}");
                        col.Item().AlignCenter().Text($"Emissão: {documento.DataEmissao.ToLocalTime():dd/MM/yyyy HH:mm}");

                        if (!autorizado)
                            col.Item().AlignCenter().Text("SEM VALOR FISCAL").Bold().FontColor(Colors.Red.Medium);

                        col.Item().LineHorizontal(0.5f);

                        foreach (var item in documento.Itens)
                        {
                            col.Item().Text($"{item.Sku} {item.NomeProduto}").FontSize(8);
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"{item.Quantidade.ToString("0.##", PtBr)} x {Moeda(item.ValorUnitario)}").FontSize(8);
                                row.ConstantItem(60).AlignRight().Text(Moeda(item.ValorTotal)).FontSize(8);
                            });
                        }

                        col.Item().LineHorizontal(0.5f);
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("TOTAL").Bold();
                            row.ConstantItem(70).AlignRight().Text(Moeda(documento.Total)).Bold();
                        });

                        col.Item().PaddingTop(4).Text("CONSUMIDOR").FontSize(7).Bold();
                        col.Item().Text($"{documento.DestinatarioNome} — {documento.DestinatarioCnpjCpf}").FontSize(7);

                        col.Item().PaddingTop(4).Text("CHAVE DE ACESSO").FontSize(7).Bold();
                        col.Item().Text(string.IsNullOrWhiteSpace(documento.ChaveAcesso) ? "(não autorizado)" : FormatarChave(documento.ChaveAcesso)).FontSize(7);
                        if (!string.IsNullOrWhiteSpace(documento.Protocolo))
                            col.Item().Text($"Protocolo: {documento.Protocolo}").FontSize(7);

                        // -------- QR Code obrigatório da NFC-e (Anexo II Ajuste SINIEF) --------
                        col.Item().PaddingTop(6).AlignCenter().Text("Consulte pela Chave de Acesso em:").FontSize(6);
                        if (!string.IsNullOrWhiteSpace(urlChave))
                            col.Item().AlignCenter().Text(urlChave).FontSize(6);

                        if (!string.IsNullOrWhiteSpace(urlQrCode))
                        {
                            var pngQr = GerarQrCodePng(urlQrCode!);
                            if (pngQr.Length > 0)
                                col.Item().PaddingTop(3).AlignCenter().Width(140).Image(pngQr).FitWidth();
                        }
                        else if (autorizado)
                        {
                            // Autorizado mas sem infNFeSupl no XML (ex.: XML importado incompleto): honesto.
                            col.Item().PaddingTop(3).AlignCenter().Text("(QR Code indisponível: XML autorizado sem bloco infNFeSupl)")
                                .FontSize(6).FontColor(Colors.Red.Medium);
                        }

                        col.Item().PaddingTop(6).AlignCenter().Text("EprosERP").FontSize(7);
                    });
                });
            }).GeneratePdf();
        }

        // ---------------------------------------------------------------------------------------
        // QR Code da NFC-e
        // ---------------------------------------------------------------------------------------

        /// <summary>
        /// Extrai a URL do QR Code (<c>qrCode</c>) e a URL de consulta (<c>urlChave</c>) do bloco
        /// <c>infNFeSupl</c> do XML autorizado (nfeProc/NFe). O motor Hercules calcula essas URLs com o
        /// hash do CSC no momento da emissão — aqui apenas lemos, nunca recomputamos o hash.
        /// Retorna (null, null) quando não há XML ou o bloco não está presente (documento não autorizado
        /// ou XML importado sem suplementar).
        /// </summary>
        private static (string? urlQrCode, string? urlChave) ExtrairDadosQrCodeNfce(DocumentoFiscal documento)
        {
            var xml = documento.XmlRetorno;
            if (string.IsNullOrWhiteSpace(xml))
                xml = documento.XmlEnvio;
            if (string.IsNullOrWhiteSpace(xml))
                return (null, null);

            try
            {
                var doc = XDocument.Parse(xml);
                // Namespace-agnóstico: localiza infNFeSupl por LocalName (portal NFe usa ns padrão).
                var supl = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "infNFeSupl");
                if (supl is null)
                    return (null, null);

                var qr = supl.Elements().FirstOrDefault(e => e.Name.LocalName == "qrCode")?.Value?.Trim();
                var url = supl.Elements().FirstOrDefault(e => e.Name.LocalName == "urlChave")?.Value?.Trim();
                return (string.IsNullOrWhiteSpace(qr) ? null : qr, string.IsNullOrWhiteSpace(url) ? null : url);
            }
            catch (System.Xml.XmlException)
            {
                // XML malformado: degrada sem QR (o cupom ainda sai com a chave textual).
                return (null, null);
            }
        }

        /// <summary>Gera o PNG do QR Code a partir de uma URL (QRCoder, multiplataforma, sem System.Drawing).</summary>
        private static byte[] GerarQrCodePng(string conteudo)
        {
            try
            {
                using var generator = new QRCodeGenerator();
                using var data = generator.CreateQrCode(conteudo, QRCodeGenerator.ECCLevel.M);
                var png = new PngByteQRCode(data);
                return png.GetGraphic(6); // 6 px por módulo — legível em bobina 80mm.
            }
            catch (Exception)
            {
                return Array.Empty<byte>();
            }
        }

        // ---------------------------------------------------------------------------------------
        // Code128C da chave (DANFE NF-e)
        // ---------------------------------------------------------------------------------------

        /// <summary>
        /// Desenha o código de barras Code128 (subconjunto C, dígitos em pares) da chave de acesso de 44
        /// dígitos diretamente no PDF (barras pretas em Canvas do QuestPDF). Evita dependência de fontes
        /// de barcode. A DANFE oficial exige este código sob a chave de acesso.
        /// </summary>
        private static void DesenharCode128(IContainer container, string digitos, float alturaPt)
        {
            var padrao = CodificarCode128C(digitos);
            if (padrao.Count == 0)
            {
                container.Text(string.Empty);
                return;
            }

            const float larguraModulo = 1.0f; // pt por módulo

            // Barras desenhadas com primitivos nativos do QuestPDF (Row de blocos coloridos), sem
            // depender de SkiaSharp Canvas nem de fontes de barcode. Alterna preto (barra) / branco (espaço),
            // começando por barra.
            container.Height(alturaPt).Row(row =>
            {
                bool barra = true;
                foreach (var modulos in padrao)
                {
                    var largura = modulos * larguraModulo;
                    var cor = barra ? Colors.Black : Colors.White;
                    row.ConstantItem(largura).Background(cor);
                    barra = !barra;
                }
            });
        }

        /// <summary>
        /// Codifica uma string de dígitos em larguras de barra/espaço no padrão Code128C.
        /// Retorna a lista de larguras (em módulos) alternando barra/espaço, iniciando por barra.
        /// Inclui Start C, dados em pares, checksum e Stop. Se o comprimento for ímpar, muda para o
        /// subconjunto B no último dígito (mantém validade do símbolo).
        /// </summary>
        private static List<int> CodificarCode128C(string digitos)
        {
            var resultado = new List<int>();
            if (string.IsNullOrEmpty(digitos) || digitos.Any(c => !char.IsDigit(c)))
                return resultado;

            const int startC = 105;
            const int stop = 106;

            var valores = new List<int> { startC };
            int i = 0;
            for (; i + 1 < digitos.Length; i += 2)
                valores.Add(int.Parse(digitos.Substring(i, 2), CultureInfo.InvariantCulture));

            // Comprimento ímpar: codifica o último dígito no Code128B (Code Set B = 100, valor = ascii-32).
            if (i < digitos.Length)
            {
                valores.Add(100); // Code B
                valores.Add((digitos[i] - '0') + 16); // '0'..'9' => valores 16..25 em Code B
            }

            // Checksum: (start + Σ valor_i * posição) mod 103
            long soma = valores[0];
            for (int p = 1; p < valores.Count; p++)
                soma += (long)valores[p] * p;
            valores.Add((int)(soma % 103));
            valores.Add(stop);

            foreach (var v in valores)
            {
                var padrao = Code128Patterns[v];
                foreach (var ch in padrao)
                    resultado.Add(ch - '0');
            }

            return resultado;
        }

        /// <summary>
        /// Tabela oficial dos 108 padrões Code128 (larguras dos 6 elementos por símbolo, em módulos).
        /// Índice = valor Code128 (0..106). Cada string tem 6 dígitos: barra,espaço,barra,espaço,barra,espaço.
        /// </summary>
        private static readonly string[] Code128Patterns =
        {
            "212222","222122","222221","121223","121322","131222","122213","122312","132212","221213",
            "221312","231212","112232","122132","122231","113222","123122","123221","223211","221132",
            "221231","213212","223112","312131","311222","321122","321221","312212","322112","322211",
            "212123","212321","232121","111323","131123","131321","112313","132113","132311","211313",
            "231113","231311","112133","112331","132131","113123","113321","133121","313121","211331",
            "231131","213113","213311","213131","311123","311321","331121","312113","312311","332111",
            "314111","221411","431111","111224","111422","121124","121421","141122","141221","112214",
            "112412","122114","122411","142112","142211","241211","221114","413111","241112","134111",
            "111242","121142","121241","114212","124112","124211","411212","421112","421211","212141",
            "214121","412121","111143","111341","131141","114113","114311","411113","411311","113141",
            "114131","311141","411131","211412","211214","211232","2331112"
        };

        // ---------------------------------------------------------------------------------------
        private static string Moeda(decimal valor) => valor.ToString("C2", PtBr);

        private static string SomenteDigitos(string? valor)
            => string.IsNullOrEmpty(valor) ? string.Empty : new string(valor.Where(char.IsDigit).ToArray());

        private static string FormatarChave(string chave)
        {
            var limpa = new string(chave.Where(char.IsDigit).ToArray());
            if (limpa.Length != 44) return chave;
            return string.Join(" ", Enumerable.Range(0, 11).Select(i => limpa.Substring(i * 4, 4)));
        }
    }
}
