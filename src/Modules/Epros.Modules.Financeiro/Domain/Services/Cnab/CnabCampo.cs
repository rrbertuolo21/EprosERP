using System;
using System.Globalization;
using System.Text;

namespace Epros.Modules.Financeiro.Domain.Services.Cnab
{
    /// <summary>
    /// Utilitários de campos de largura fixa CNAB (FEBRABAN). Um registro CNAB é uma linha de largura
    /// fixa (240 ou 400 posições) composta por campos alfanuméricos (alinhados à esquerda, preenchidos
    /// com espaço) e numéricos (alinhados à direita, preenchidos com zero); valores monetários vão em
    /// centavos, sem separador. Estas rotinas são a MECÂNICA universal do formato; as POSIÇÕES exatas de
    /// cada campo por banco/layout são convenção bancária (// valida-contador).
    /// </summary>
    public static class CnabCampo
    {
        /// <summary>Campo alfanumérico: maiúsculo, sem acento, à esquerda, completado com espaço à direita.</summary>
        public static string Alfa(string? valor, int tamanho)
        {
            var s = RemoverAcentos(valor ?? string.Empty).ToUpperInvariant();
            if (s.Length > tamanho) s = s.Substring(0, tamanho);
            return s.PadRight(tamanho, ' ');
        }

        /// <summary>Campo numérico: só dígitos, à direita, completado com zero à esquerda.</summary>
        public static string Num(long valor, int tamanho)
        {
            var s = valor.ToString(CultureInfo.InvariantCulture);
            if (s.Length > tamanho) s = s.Substring(s.Length - tamanho, tamanho); // trunca à esquerda
            return s.PadLeft(tamanho, '0');
        }

        /// <summary>Campo numérico a partir de string (extrai só dígitos).</summary>
        public static string NumStr(string? valor, int tamanho)
        {
            var digitos = SomenteDigitos(valor);
            if (digitos.Length > tamanho) digitos = digitos.Substring(digitos.Length - tamanho, tamanho);
            return digitos.PadLeft(tamanho, '0');
        }

        /// <summary>Valor monetário em centavos, numérico à direita com zeros.</summary>
        public static string Valor(decimal valor, int tamanho)
        {
            var centavos = (long)Math.Round(valor * 100m, MidpointRounding.AwayFromZero);
            return Num(centavos, tamanho);
        }

        /// <summary>Data no formato ddMMyy (6) ou ddMMyyyy (8).</summary>
        public static string Data(DateTime data, bool anoCompleto = false)
            => data.ToString(anoCompleto ? "ddMMyyyy" : "ddMMyy", CultureInfo.InvariantCulture);

        // ----- Leitura -----

        /// <summary>Extrai um campo [inicio1based, tamanho] de uma linha (1-based, inclusive).</summary>
        public static string Ler(string linha, int inicio1based, int tamanho)
        {
            var idx = inicio1based - 1;
            if (linha == null || idx < 0 || idx >= linha.Length) return string.Empty;
            var disp = Math.Min(tamanho, linha.Length - idx);
            return linha.Substring(idx, disp);
        }

        public static long LerNum(string linha, int inicio1based, int tamanho)
        {
            var s = SomenteDigitos(Ler(linha, inicio1based, tamanho));
            return long.TryParse(s, out var v) ? v : 0L;
        }

        /// <summary>Lê valor monetário em centavos e converte para decimal.</summary>
        public static decimal LerValor(string linha, int inicio1based, int tamanho)
            => LerNum(linha, inicio1based, tamanho) / 100m;

        /// <summary>Lê data ddMMyy; retorna null se zerada/inválida.</summary>
        public static DateTime? LerDataDdMMyy(string linha, int inicio1based)
        {
            var s = SomenteDigitos(Ler(linha, inicio1based, 6));
            if (s.Length != 6 || s == "000000") return null;
            if (DateTime.TryParseExact(s, "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            return null;
        }

        public static string SomenteDigitos(string? valor)
        {
            if (string.IsNullOrEmpty(valor)) return string.Empty;
            var sb = new StringBuilder(valor.Length);
            foreach (var c in valor) if (c >= '0' && c <= '9') sb.Append(c);
            return sb.ToString();
        }

        private static string RemoverAcentos(string texto)
        {
            var normalizado = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalizado.Length);
            foreach (var c in normalizado)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
