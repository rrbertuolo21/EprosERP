namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCst
{
    public class CfopNfceCst41Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCst41Validos = ["5101", "5102", "5103", "5104", "5115"];
            return cfopsNfceCst41Validos.Contains(cfop);
        }
    }
}
