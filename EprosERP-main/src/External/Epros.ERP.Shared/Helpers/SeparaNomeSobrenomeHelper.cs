namespace Epros.ERP.Shared.Helpers
{
    public class SeparaNomeSobrenomeHelper
    {
        public static NomeSobrenome Separar(string nomeCompleto)
        {
            nomeCompleto = nomeCompleto ?? "";

            if (nomeCompleto.Length > 200) return new NomeSobrenome(nomeCompleto.Substring(0, 190), "");

            if (string.IsNullOrEmpty(nomeCompleto.Trim())) return new NomeSobrenome("", "");

            var n = "";
            var s = "";
            var nomes = nomeCompleto.Split(" ");
            n = nomes[0];

            if (nomes.Count() > 1)
            {
                for (int i = 1; i < nomes.Count(); i++)
                    s += nomes[i] + " ";
            }

            s = s.TrimEnd(' ');

            var ns = new NomeSobrenome((n.Count() > 200 ? n.Substring(0, 190) : n), (s.Count() > 200 ? s.Substring(0, 190) : s));

            return ns;
        }
    }

    public record NomeSobrenome(string nome, string sobrenome);
}