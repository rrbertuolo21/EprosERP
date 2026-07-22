namespace Epros.ERP.DfeCalculos.Validations
{
    public class CsosnNfceValidation
    {
        public static bool Validar(string csosn)
        {
            string[] nfceCsosnValidos = ["102", "103", "300", "400", "500", "900", "02", "15", "53", "61"];
            return nfceCsosnValidos.Contains(csosn);
        }
    }
}
