namespace Epros.ERP.DfeCalculos.Validations
{
    public class CfopNfceValidation
    {
        public static bool Validar(string cfop)
        {
            string[] cfopsNfceValidos = ["5101", "5102", "5103", "5104", "5115", "5405", "5653", "5656", "5667", "5933"];
            return cfopsNfceValidos.Contains(cfop);
        }
    }
}
