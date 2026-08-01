namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCst
{
    public class CfopNfceCst00Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCst00Validos = ["5101", "5102", "5103", "5104", "5115"];
            return cfopsNfceCst00Validos.Contains(cfop);
        }
    }
}
