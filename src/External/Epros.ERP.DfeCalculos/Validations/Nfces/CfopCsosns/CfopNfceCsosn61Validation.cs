namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCsosn61Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCsosn61Validos = ["5405", "5656", "5667"];
            return cfopsNfceCsosn61Validos.Contains(cfop);
        }
    }
}
