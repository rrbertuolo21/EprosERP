namespace Epros.ERP.DfeCalculos.Validations.Nfces.CfopCsosns
{
    public class CfopNfceCsosnValidation
    {
        public static string Validar(string cfop, string csosn)
        {
            switch (csosn)
            {
                case "02":
                    if (!CfopNfceCsosn02Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inváldio para CSOSN: {csosn} (NFC-e)";
                    break;
                case "15":
                    if (!CfopNfceCsosn15Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inváldio para CSOSN: {csosn} (NFC-e)";
                    break;
                case "53":
                    if (!CfopNfceCsosn53Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inváldio para CSOSN: {csosn} (NFC-e)";
                    break;

                case "61":
                    if (!CfopNfceCsosn61Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inváldio para CSOSN: {csosn} (NFC-e)";
                    break;

                case "102":
                    if (!CfopNfceCsosn102Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inváldio para CSOSN: {csosn} (NFC-e)";
                    break;

                case "103":
                    if (!CfopNfceCsosn103Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inváldio para CSOSN: {csosn} (NFC-e)";
                    break;

                case "300":
                    if (!CfopNfceCsosn300Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inváldio para CSOSN: {csosn} (NFC-e)";
                    break;

                case "400":
                    if (!CfopNfceCsosn400Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inváldio para CSOSN: {csosn} (NFC-e)";
                    break;

                case "500":
                    if (!CfopNfceCsosn500Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inváldio para CSOSN: {csosn} (NFC-e)";
                    break;

                case "900":
                    if (!CfopNfceCsosn900Validation.Validar(cfop.ToString()))
                        return $"CFOP: {cfop}, inváldio para CSOSN: {csosn} (NFC-e)";
                    break;
                default:
                    return $"CFOP: {cfop}, inváldio para CSOSN: {csosn} (NFC-e)";
            }

            return "";
        }
    }
}
