using System.Text.RegularExpressions;

namespace Epros.ERP.Shared.Helpers
{
    public class ChaveNfeHelper
    {
        public static string ExtrairChaveNFe(string mensagem)
        {
            var match = Regex.Match(mensagem, @"\b\d{44}\b");
            return match.Success ? match.Value : null!;
        }
    }
}
