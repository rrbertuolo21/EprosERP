namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCst
{
    public class CfopNfceCst61Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCst61Validos = ["5405", "5656", "5667"];
            return cfopsNfceCst61Validos.Contains(cfop);
        }
    }
}
