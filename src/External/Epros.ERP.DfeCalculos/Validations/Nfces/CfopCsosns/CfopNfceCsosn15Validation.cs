namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCsosn15Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCsosn15Validos = ["5405", "5656", "5667"];
            return cfopsNfceCsosn15Validos.Contains(cfop);
        }
    }
}
