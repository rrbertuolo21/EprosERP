using System;
using System.Text.RegularExpressions;
using Flunt.Notifications;

namespace Epros.Modules.GestaoClientes.Domain.ValueObjects
{
    public class Cep : Notifiable<Notification>
    {
        public string Valor { get; private set; } = string.Empty;

        protected Cep() { } // EF Core

        public Cep(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                AddNotification(nameof(Cep), "CEP inválido");
                return;
            }

            var cepLimpo = valor.Trim();
            if (!Regex.IsMatch(cepLimpo, @"^\d{5}-\d{3}$") && !Regex.IsMatch(cepLimpo, @"^\d{8}$"))
            {
                AddNotification(nameof(Cep), "CEP inválido");
            }
            else
            {
                Valor = cepLimpo.Replace("-", "");
            }
        }

        public override string ToString() => Valor;
    }
}
