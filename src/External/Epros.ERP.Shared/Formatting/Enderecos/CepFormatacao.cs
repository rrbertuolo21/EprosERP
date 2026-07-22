namespace Epros.ERP.Shared.Formatting.Enderecos
{
    public class CepFormatacao
    {
        public static string Formatar(string cep)
        {
            cep = cep.Trim();
            if (string.IsNullOrEmpty(cep) || cep.Length != 8) return "";
            return string.Format(@"{0:00\.000\-000}", Convert.ToInt64(cep));
        }
    }
}