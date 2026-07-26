namespace Epros.ERP.DfeCalculos.Validations
{
    public class CodigoBarrasValidation
    {
        public static bool QuantidadeDeCaracteresDoCodigoBarrasValidar(string codigoBarras)
        {
            switch (codigoBarras.Length)
            {
                case 8: return true;
                case 12: return true;
                case 13: return true;
                case 14: return true;
            }
            return false;
        }

        public static bool CodigoBarrasContemApenasNumero(string codigoBarras)
        {
            return double.TryParse(codigoBarras, out double codigo);
        }

        public static bool Validar(string codigoBarras)
        {
            decimal apenasNumero = decimal.Zero;
            if (!decimal.TryParse(codigoBarras, out apenasNumero))
                return false;

            if (string.IsNullOrEmpty(codigoBarras)) return false;
            codigoBarras = codigoBarras.Trim();
            if (!QuantidadeDeCaracteresDoCodigoBarrasValidar(codigoBarras)) return false;
            if (!CodigoBarrasContemApenasNumero(codigoBarras)) return false;

            var digitoVerificador = codigoBarras.Substring(codigoBarras.Length - 1);
            var codigoCompleto = codigoBarras.Substring(0, codigoBarras.Length - 1).PadLeft(12, '0');
            int[] fixos = { 1, 3, 1, 3, 1, 3, 1, 3, 1, 3, 1, 3 };
            int soma = 0;
            int multiploDe10 = 0;
            string digitoCalculado = "";

            for (int i = 0; i < fixos.Length; i++)
            {
                var item = int.Parse(codigoCompleto[i].ToString());
                soma += (fixos[i] * item);
            }

            for (int i = 0; i < 100; i++)
            {
                multiploDe10 += 10;
                if (multiploDe10 > soma)
                {
                    digitoCalculado = (multiploDe10 - soma).ToString();
                    if (digitoCalculado.Length > 1) digitoCalculado = digitoCalculado.Substring(digitoCalculado.Length - 1, 1);
                    break;
                }
            }

            return digitoCalculado.Equals(digitoVerificador);
        }
    }
}