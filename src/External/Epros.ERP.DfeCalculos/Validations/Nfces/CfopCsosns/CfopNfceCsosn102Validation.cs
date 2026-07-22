namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCsosn102Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCsosn102Validos = ["5101", "5102", "5103", "5104", "5115"];
            return cfopsNfceCsosn102Validos.Contains(cfop);
        }
    }
}
