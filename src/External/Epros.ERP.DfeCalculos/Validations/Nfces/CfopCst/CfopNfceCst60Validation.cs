namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCst
{
    public class CfopNfceCst60Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCst60Validos = ["5405", "5656", "5667"];
            return cfopsNfceCst60Validos.Contains(cfop);
        }
    }
}
