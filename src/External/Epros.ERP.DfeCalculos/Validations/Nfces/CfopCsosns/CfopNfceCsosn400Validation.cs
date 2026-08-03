namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCsosn400Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCsosn400Validos = ["5101", "5102", "5103", "5104", "5115"];
            return cfopsNfceCsosn400Validos.Contains(cfop);
        }
    }
}
