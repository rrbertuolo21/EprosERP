using System;
using System.Linq;
using Flunt.Notifications;

namespace Epros.Modules.GestaoClientes.Domain.ValueObjects
{
    public class Cpf : Notifiable<Notification>
    {
        public string Valor { get; private set; } = string.Empty;

        protected Cpf() { } // EF Core

        public Cpf(string valor)
        {
            Valor = Limpar(valor);
            if (!ValidarCpf(Valor))
            {
                AddNotification(nameof(Cpf), "CPF inválido");
            }
        }

        private static string Limpar(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return string.Empty;
            return string.Concat(valor.Where(char.IsDigit));
        }

        private static bool ValidarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;
            if (cpf.Length != 11) return false;

            if (cpf.Distinct().Count() == 1) return false;

            var multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            var tempCpf = cpf.Substring(0, 9);
            var soma = 0;

            for (var i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            var resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            var digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (var i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();
            return cpf.EndsWith(digito);
        }

        public override string ToString() => Valor;
    }
}
