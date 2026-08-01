namespace Epros.ERP.Shared.Formatting.Documentos
{
    public class CepFormatacao
    {
        public static string Formatar(string cpfCnpj)
        {
            cpfCnpj = cpfCnpj.Trim();
            if (string.IsNullOrEmpty(cpfCnpj)) return "";
            switch (cpfCnpj.Length)
            {
                case 11: return string.Format(@"{0:00\.000\-000}", Convert.ToInt64(cpfCnpj));
            }
            return "";
        }
    }
}
