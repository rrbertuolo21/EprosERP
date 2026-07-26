namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCst
{
    public class CfopNfceCst02Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCst02Validos = ["5405", "5656", "5667"];
            return cfopsNfceCst02Validos.Contains(cfop);
        }
    }
}
