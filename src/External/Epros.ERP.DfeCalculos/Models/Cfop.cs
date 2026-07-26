namespace Epros.ERP.DfeCalculos.Models
{
    public class Cfop
    {
        public Cfop(string codigo, string naturezaOperacao)
        {
            Codigo = codigo;
            NaturezaOperacao = naturezaOperacao;
        }

        public string Codigo { get; private set; }
        public string NaturezaOperacao { get; private set; }
    }

    public class CfopCarregar
    {
        public static IEnumerable<Cfop> ObterCfops()
        {
            var diretorio = Directory.GetCurrentDirectory();
            string caminhoCsv = Path.Combine(diretorio, "wwwroot", "NaturezasOperacaoes", "CFO_LISTA_FIN.csv");

            ICollection<Cfop> cfops = new List<Cfop>();
            try
            {
                using (StreamReader sr = new StreamReader(caminhoCsv))
                {
                    string? linhaArquivo = null;
                    linhaArquivo = sr.ReadLine();
                    while ((linhaArquivo = sr.ReadLine()) != null)
                    {
                        var cfop = linhaArquivo.Split(";");
                        cfops.Add(new Cfop(cfop[0], cfop[1]));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }

            return cfops;
        }

    }
}
