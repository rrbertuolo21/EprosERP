using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ProsisPDV.Domain.ValueObjects.Geral
{
    public class TextoHelper
    {
        public static string RetornaApenasNumeros(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            return Regex.Replace(texto, @"\D", "");
        }

        public static string TrocaCaracteresEspeciais(string texto)
        {
            if (string.IsNullOrEmpty(texto ?? "".Trim())) return "";

            string saida = texto!.Replace("&", "e");
            saida = saida.Replace(">", "");
            saida = saida.Replace("<", "");
            saida = saida.Replace("'", " ");
            saida = saida.Replace('"', ' ');
            saida = saida.Replace("º", "");
            saida = saida.Replace("ª", "");
            saida = saida.Replace("°", "");
            saida = saida.Replace("Ü", "U");
            saida = saida.Replace("$", "");
            saida = saida.Replace("#", "");
            saida = saida.Replace("&", "e");
            saida = saida.Replace("¬", "");
            saida = saida.Replace("¹", "");
            saida = saida.Replace("²", "");
            saida = saida.Replace("³", "");
            saida = saida.Replace("¢", "");
            saida = saida.Replace("£", "");
            saida = saida.Replace("§", "");
            saida = saida.Replace(";", "");
            saida = saida.Replace(".", "");
            saida = saida.Replace(",", "");
            saida = saida.Replace("/", "");
            saida = saida.Replace("-", "");
            return saida;
        }

        public static string RemoveCaracteresEspeciais(string texto)
        {
            if (string.IsNullOrEmpty(texto ?? "".Trim())) return "";

            byte[] bytes = Encoding.GetEncoding("iso-8859-8").GetBytes(texto!);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string RemoveMascaras(string texto)
        {
            if (string.IsNullOrEmpty(texto ?? "".Trim())) return "";

            string saida =
                texto!.Replace("(", "");
            saida = saida.Replace(")", "");
            saida = saida.Replace("/", "");
            saida = saida.Replace(@"\", "");
            saida = saida.Replace("-", "");
            saida = saida.Replace("_", "");
            saida = saida.Replace(";", "");
            saida = saida.Replace(".", "");
            saida = saida.Replace(",", "");
            saida = saida.Replace(" ", "");
            return saida;
        }

        public static string TrocaVirgulaPorPonto(decimal numero)
        {
            if (numero == decimal.Zero) return "";
            return numero.ToString().Replace(".", "").Replace(",", ".");
        }

        public static bool VerificarSeExisteCaracteresEspeciais(string texto)
        {
            if (string.IsNullOrEmpty(texto ?? "".Trim())) return false;
            string caracteres = "<>'.;-*";
            foreach (var caracter in caracteres)
                if ((texto ?? "").IndexOf(caracter) > 0) return true;
            return false;
        }

        public static string RemoverAcentos(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;

            var normalizado = texto.Normalize(NormalizationForm.FormD);

            var stringBuilder = new StringBuilder();
            foreach (var caracter in normalizado)
            {
                var caracterUnicode = CharUnicodeInfo.GetUnicodeCategory(caracter);
                if (caracterUnicode != UnicodeCategory.NonSpacingMark) stringBuilder.Append(caracterUnicode);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
