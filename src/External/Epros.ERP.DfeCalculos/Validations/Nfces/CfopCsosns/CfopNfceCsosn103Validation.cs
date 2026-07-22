namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCsosn103Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCsosn103Validos = ["5101", "5102", "5103", "5104", "5115"];
            return cfopsNfceCsosn103Validos.Contains(cfop);
        }
    }
}
