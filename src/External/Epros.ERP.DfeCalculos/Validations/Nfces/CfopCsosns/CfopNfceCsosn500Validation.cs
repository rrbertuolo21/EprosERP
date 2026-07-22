namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCsosn500Validation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceCsosn500Validos = ["5405", "5656", "5667"];
            return cfopsNfceCsosn500Validos.Contains(cfop);
        }
    }
}
