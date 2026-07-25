namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCsosn300Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCsosn300Validos = ["5101", "5102", "5103", "5104", "5115"];
            return cfopsNfceCsosn300Validos.Contains(cfop);
        }
    }
}
