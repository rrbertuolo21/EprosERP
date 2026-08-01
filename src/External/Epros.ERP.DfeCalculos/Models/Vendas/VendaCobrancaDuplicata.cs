using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaCobrancaDuplicata : Notifiable<Notification>
    {
        public VendaCobrancaDuplicata(string? numeroDuplicata, DateTime dataVencimento, decimal valorDuplicata)
        {
            NumeroDuplicata = numeroDuplicata;
            DataVencimento = dataVencimento;
            ValorDuplicata = valorDuplicata;
        }

        public string? NumeroDuplicata { get; private set; }
        public DateTime DataVencimento { get; private set; }
        public decimal ValorDuplicata { get; private set; }

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                //.IsGreaterThan(decimal.Zero, ValorDuplicata, "ValorDuplicata", "Valor Duplicata precisa ser maior que zero")
                .Requires()
                .IsGreaterThan(DateTime.MinValue, DataVencimento, "DataVencimento", "Data Vencimento é obrigatório")
                .IsGreaterOrEqualsThan(60, (NumeroDuplicata ?? "").Length, "NumeroDuplicata", "Número Duplicata pode conter até 60 caracteres")
                );
        }
    }
}
