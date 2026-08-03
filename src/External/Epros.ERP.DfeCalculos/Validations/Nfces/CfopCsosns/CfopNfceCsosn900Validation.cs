namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCsosn900Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCsosn900Validos = ["5101", "5102", "5103", "5104", "5115"];
            return cfopsNfceCsosn900Validos.Contains(cfop);
        }
    }
}
