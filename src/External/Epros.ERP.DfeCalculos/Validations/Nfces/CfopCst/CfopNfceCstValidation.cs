using Epros.ERP.DfeCalculos.Validations.Nfces.CfopCst;

namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCstValidation
    {
        public static string Validar(string cfop, string cst)
        {
            switch (cst)
            {
                case "00":
                    if (!CfopNfceCst00Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inválidio para CST: {cst} (NFC-e)";
                    break;
                case "02":
                    if (!CfopNfceCst02Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inválidio para CST: {cst} (NFC-e)";
                    break;
                case "15":
                    if (!CfopNfceCst15Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inválidio para CST: {cst} (NFC-e)";
                    break;
                case "20":
                    if (!CfopNfceCst20Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inválidio para CST: {cst} (NFC-e)";
                    break;

                case "40":
                    if (!CfopNfceCst40Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inválidio para CST: {cst} (NFC-e)";
                    break;

                case "41":
                    if (!CfopNfceCst41Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inválidio para CST: {cst} (NFC-e)";
                    break;

                case "53":
                    if (!CfopNfceCst53Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inválidio para CST: {cst} (NFC-e)";
                    break;

                case "60":
                    if (!CfopNfceCst60Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inválidio para CST: {cst} (NFC-e)";
                    break;

                case "61":
                    if (!CfopNfceCst60Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inválidio para CST: {cst} (NFC-e)";
                    break;

                case "90":
                    if (!CfopNfceCst90Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inválidio para CST: {cst} (NFC-e)";
                    break;
                default:
                    return $"CFOP: {cfop}, inválidio para CST: {cst} (NFC-e)";
            }

            return "";
        }
    }
}
