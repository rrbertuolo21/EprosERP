namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCsosn53Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCsosn53Validos = ["5405", "5656", "5667"];
            return cfopsNfceCsosn53Validos.Contains(cfop);
        }
    }
}
