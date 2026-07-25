namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCst
{
    public class CfopNfceCst15Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCst15Validos = ["5405", "5656", "5667"];
            return cfopsNfceCst15Validos.Contains(cfop);
        }
    }
}
