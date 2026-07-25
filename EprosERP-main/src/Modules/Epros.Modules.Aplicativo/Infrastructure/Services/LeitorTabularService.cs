using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Epros.Modules.Aplicativo.Infrastructure.Services
{
    /// <summary>Linha lida de um arquivo tabular, com número físico (>= 2 para dados) e valores por cabeçalho normalizado.</summary>
    public record LinhaTabular(int NumeroLinha, Dictionary<string, string> Valores);

    /// <summary>
    /// Leitor de arquivos tabulares CSV/XLSX para importação (PLT-UPL, EF UPLOAD 7.6).
    /// Cabeçalho na linha 1, dados a partir da linha 2, colunas normalizadas para minúsculas simples.
    /// XLSX é lido via ZipArchive + XML (sem dependência externa).
    /// </summary>
    public static class LeitorTabularService
    {
        public static IReadOnlyList<LinhaTabular> Ler(byte[] conteudo, string extensao)
        {
            var ext = (extensao ?? string.Empty).TrimStart('.').ToLowerInvariant();
            return ext switch
            {
                "csv" => LerCsv(conteudo),
                "xlsx" => LerXlsx(conteudo),
                _ => throw new NotSupportedException($"Extensão '{extensao}' não suportada para importação tabular. Use csv ou xlsx.")
            };
        }

        public static string NormalizarCabecalho(string coluna)
        {
            return (coluna ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static IReadOnlyList<LinhaTabular> LerCsv(byte[] conteudo)
        {
            var texto = Encoding.UTF8.GetString(conteudo);
            var linhas = texto.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

            var resultado = new List<LinhaTabular>();
            List<string>? cabecalho = null;

            for (int i = 0; i < linhas.Length; i++)
            {
                var bruto = linhas[i];
                if (i == linhas.Length - 1 && string.IsNullOrEmpty(bruto)) break;

                var campos = ParseLinhaCsv(bruto);

                if (cabecalho == null)
                {
                    cabecalho = campos.Select(NormalizarCabecalho).ToList();
                    continue;
                }

                if (campos.All(string.IsNullOrWhiteSpace)) continue;

                var valores = new Dictionary<string, string>();
                for (int c = 0; c < cabecalho.Count; c++)
                {
                    valores[cabecalho[c]] = c < campos.Count ? campos[c] : string.Empty;
                }

                // Número físico da linha: cabeçalho é linha 1, primeira linha de dados é 2.
                resultado.Add(new LinhaTabular(i + 1, valores));
            }

            return resultado;
        }

        private static List<string> ParseLinhaCsv(string linha)
        {
            var campos = new List<string>();
            var sb = new StringBuilder();
            bool emAspas = false;
            char separador = linha.Contains(';') && !linha.Contains(',') ? ';' : ',';

            for (int i = 0; i < linha.Length; i++)
            {
                var ch = linha[i];
                if (emAspas)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < linha.Length && linha[i + 1] == '"') { sb.Append('"'); i++; }
                        else emAspas = false;
                    }
                    else sb.Append(ch);
                }
                else
                {
                    if (ch == '"') emAspas = true;
                    else if (ch == separador) { campos.Add(sb.ToString().Trim()); sb.Clear(); }
                    else sb.Append(ch);
                }
            }
            campos.Add(sb.ToString().Trim());
            return campos;
        }

        private static IReadOnlyList<LinhaTabular> LerXlsx(byte[] conteudo)
        {
            using var ms = new MemoryStream(conteudo);
            using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            // Strings compartilhadas (opcional)
            var sharedStrings = new List<string>();
            var sharedEntry = zip.GetEntry("xl/sharedStrings.xml");
            if (sharedEntry != null)
            {
                using var s = sharedEntry.Open();
                var doc = XDocument.Load(s);
                foreach (var si in doc.Root!.Elements(ns + "si"))
                {
                    sharedStrings.Add(string.Concat(si.Descendants(ns + "t").Select(t => t.Value)));
                }
            }

            var sheetEntry = zip.GetEntry("xl/worksheets/sheet1.xml")
                             ?? zip.Entries.FirstOrDefault(e => e.FullName.StartsWith("xl/worksheets/sheet"));
            if (sheetEntry == null)
            {
                return new List<LinhaTabular>();
            }

            var resultado = new List<LinhaTabular>();
            List<string>? cabecalho = null;

            using var sheetStream = sheetEntry.Open();
            var sheetDoc = XDocument.Load(sheetStream);
            var rows = sheetDoc.Descendants(ns + "row").ToList();

            foreach (var row in rows)
            {
                var celulas = new Dictionary<int, string>();
                foreach (var cell in row.Elements(ns + "c"))
                {
                    var refAttr = cell.Attribute("r")?.Value ?? "A1";
                    int coluna = RefParaColuna(refAttr);
                    var tipo = cell.Attribute("t")?.Value;
                    var valorEl = cell.Element(ns + "v");
                    string valor;
                    if (tipo == "s" && valorEl != null && int.TryParse(valorEl.Value, out var idx) && idx < sharedStrings.Count)
                    {
                        valor = sharedStrings[idx];
                    }
                    else if (tipo == "inlineStr")
                    {
                        valor = string.Concat(cell.Descendants(ns + "t").Select(t => t.Value));
                    }
                    else
                    {
                        valor = valorEl?.Value ?? string.Empty;
                    }
                    celulas[coluna] = valor;
                }

                var maxCol = celulas.Count > 0 ? celulas.Keys.Max() : -1;
                var campos = new List<string>();
                for (int c = 0; c <= maxCol; c++)
                {
                    campos.Add(celulas.TryGetValue(c, out var v) ? v : string.Empty);
                }

                var numeroLinha = int.TryParse(row.Attribute("r")?.Value, out var n) ? n : resultado.Count + 2;

                if (cabecalho == null)
                {
                    cabecalho = campos.Select(NormalizarCabecalho).ToList();
                    continue;
                }

                if (campos.All(string.IsNullOrWhiteSpace)) continue;

                var valores = new Dictionary<string, string>();
                for (int c = 0; c < cabecalho.Count; c++)
                {
                    valores[cabecalho[c]] = c < campos.Count ? campos[c] : string.Empty;
                }

                resultado.Add(new LinhaTabular(numeroLinha, valores));
            }

            return resultado;
        }

        private static int RefParaColuna(string cellRef)
        {
            int coluna = 0;
            foreach (var ch in cellRef)
            {
                if (char.IsLetter(ch))
                {
                    coluna = coluna * 26 + (char.ToUpperInvariant(ch) - 'A' + 1);
                }
                else break;
            }
            return coluna - 1; // zero-based
        }
    }
}
