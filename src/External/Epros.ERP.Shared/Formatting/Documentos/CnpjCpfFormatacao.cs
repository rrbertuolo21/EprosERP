using System.Text.RegularExpressions;

namespace Epros.ERP.Shared.Formatting.Documentos
{
    public static class CnpjCpfFormatacao
    {
        public static string Formatar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            valor = Regex.Replace(valor.Trim(), @"[^\w]", "");

            return valor.Length switch
            {
                11 => Convert.ToUInt64(valor)
                        .ToString(@"000\.000\.000\-00"),

                14 => string.Format("{0}.{1}.{2}/{3}-{4}",
                        valor[..2],
                        valor.Substring(2, 3),
                        valor.Substring(5, 3),
                        valor.Substring(8, 4),
                        valor.Substring(12, 2)),

                _ => string.Empty
            };
        }
    }
}
