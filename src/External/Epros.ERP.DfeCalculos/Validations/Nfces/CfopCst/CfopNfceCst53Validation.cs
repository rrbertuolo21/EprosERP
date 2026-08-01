namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCst
{
    public class CfopNfceCst53Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCst53Validos = ["5405", "5656", "5667"];
            return cfopsNfceCst53Validos.Contains(cfop);
        }
    }
}
