using System;
using System.Text.RegularExpressions;
using Flunt.Notifications;

namespace Epros.Modules.GestaoClientes.Domain.ValueObjects
{
    public class Placa : Notifiable<Notification>
    {
        public string Valor { get; private set; } = string.Empty;

        protected Placa() { } // EF Core

        public Placa(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                AddNotification(nameof(Placa), "A Placa deve estar no padrão Mercosul (ABC1D23) ou padrão Antigo (ABC1234).");
                return;
            }

            Valor = valor.Trim().ToUpper();
            var placaLimpa = Valor.Replace("-", "");

            var matchAntigo = Regex.IsMatch(placaLimpa, @"^[A-Z]{3}[0-9]{4}$");
            var matchMercosul = Regex.IsMatch(placaLimpa, @"^[A-Z]{3}[0-9][A-Z][0-9]{2}$");

            if (!matchAntigo && !matchMercosul)
            {
                AddNotification(nameof(Placa), "A Placa deve estar no padrão Mercosul (ABC1D23) ou padrão Antigo (ABC1234).");
            }
            else
            {
                Valor = placaLimpa;
            }
        }

        public override string ToString() => Valor;
    }
}
