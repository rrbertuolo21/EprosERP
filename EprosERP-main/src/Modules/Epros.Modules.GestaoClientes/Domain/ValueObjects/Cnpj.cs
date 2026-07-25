using System;
using System.Linq;
using Flunt.Notifications;

namespace Epros.Modules.GestaoClientes.Domain.ValueObjects
{
    public class Cnpj : Notifiable<Notification>
    {
        public string Valor { get; private set; } = string.Empty;

        protected Cnpj() { } // EF Core

        public Cnpj(string valor)
        {
            Valor = Limpar(valor);
            if (!ValidarCnpj(Valor))
            {
                AddNotification(nameof(Cnpj), "CNPJ inválido");
            }
        }

        private static string Limpar(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return string.Empty;
            return string.Concat(valor.Where(char.IsDigit));
        }

        private static bool ValidarCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj)) return false;
            if (cnpj.Length != 14) return false;

            if (cnpj.Distinct().Count() == 1) return false;

            var multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var tempCnpj = cnpj.Substring(0, 12);
            var soma = 0;

            for (var i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            var resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            var digito = resto.ToString();
            tempCnpj = tempCnpj + digito;
            soma = 0;
            for (var i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();
            return cnpj.EndsWith(digito);
        }

        public override string ToString() => Valor;
    }
}
