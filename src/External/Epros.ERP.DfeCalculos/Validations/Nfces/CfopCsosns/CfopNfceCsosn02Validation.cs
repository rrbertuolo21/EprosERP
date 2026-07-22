namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCsosn02Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCsosn02Validos = ["5405", "5656", "5667"];
            return cfopsNfceCsosn02Validos.Contains(cfop);
        }
    }
}
