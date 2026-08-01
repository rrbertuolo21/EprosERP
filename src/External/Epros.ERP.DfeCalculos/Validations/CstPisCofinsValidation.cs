namespace Epros.ERP.DfeCalculos.Validations
{
    public class CstNfceValCstPisCofinsValidationidation
    {
        public static bool Validar(string cstPisCofins)
        {
            string[] nfceCstValidos = ["01", "02", "03", "04", "05", "06", "07", "08", "09", "49", "99"];
            return nfceCstValidos.Contains(cstPisCofins);
        }
    }
}
