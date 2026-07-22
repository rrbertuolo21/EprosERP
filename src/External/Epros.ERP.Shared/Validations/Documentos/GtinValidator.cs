namespace Epros.ERP.Shared.Validations.Documentos
{
    public class GtinValidator
    {
        public static (bool valido, string mensagemErro) GtinValido(string gtin)
        {
            if (string.IsNullOrWhiteSpace(gtin))
            {
                var msg = "O código não pode ser nulo ou conter espaço em branco.";
                return (false, msg);
            }


            // Remove espaços e caracteres não numéricos
            gtin = new string(gtin.Where(char.IsDigit).ToArray());

            // GTINs válidos têm 8, 12, 13 ou 14 dígitos
            if (!(gtin.Length == 8 || gtin.Length == 12 || gtin.Length == 13 || gtin.Length == 14))
            {
                var msg = "O tamanho do código pode conter somente 8, 12, 13 ou 14 dígitos.";
                return (false, msg);
            }

            int sum = 0;
            bool multiplyByThree = true;

            // Percorre do penúltimo dígito até o primeiro
            for (int i = gtin.Length - 2; i >= 0; i--)
            {
                int digit = gtin[i] - '0';
                sum += digit * (multiplyByThree ? 3 : 1);
                multiplyByThree = !multiplyByThree;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            int lastDigit = gtin[gtin.Length - 1] - '0';

            if (checkDigit != lastDigit)
            {
                var msg = "Dígito verificador incompatível.";
                return (false, msg);
            }

            return (true, "");
        }
    }
}
