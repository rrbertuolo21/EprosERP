namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCst
{
    public class CfopNfceCst20Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCst20Validos = ["5101", "5102", "5103", "5104", "5115"];
            return cfopsNfceCst20Validos.Contains(cfop);
        }
    }
}
