namespace Epros.ERP.Shared.Formatting.Contatos
{
    public class TelefoneFormatacao
    {
        public static string Formatar(string telefone)
        {
            telefone = telefone.Trim();
            if (string.IsNullOrEmpty(telefone)) return "";
            switch (telefone.Length)
            {
                case 10: return string.Format(@"{0:00\ 0000\ 0000}", Convert.ToInt64(telefone));
                case 11: return string.Format(@"{0:00\ 00000\ 0000}", Convert.ToInt64(telefone));
            }
            return "";
        }
    }
}
