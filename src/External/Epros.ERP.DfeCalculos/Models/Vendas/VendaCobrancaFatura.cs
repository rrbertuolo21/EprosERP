using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{

    public class VendaCobrancaFatura : Notifiable<Notification>
    {
        public VendaCobrancaFatura(decimal valorOriginal, decimal valorDesconto, decimal valorLiquido, string? numeroFatura)
        {
            ValorOriginal = valorOriginal;
            ValorDesconto = valorDesconto;
            ValorLiquido = valorLiquido;
            NumeroFatura = numeroFatura;
            Validar();
        }

        public decimal ValorOriginal { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorLiquido { get; private set; }
        public string? NumeroFatura { get; private set; }

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
                .Requires()
                //.IsGreaterThan(decimal.Zero, ValorOriginal, "ValorOriginal", "Valor Original precisa ser maior que zero")
                .Requires()
                //.IsGreaterThan(decimal.Zero, ValorLiquido, "ValorLiquido", "Valor Liquído precisa ser maior que zero")
                .IsGreaterOrEqualsThan(60, (NumeroFatura ?? "").Length, "NumeroFatura", "Número Fatura pode conter até 60 caracteres")
                );
        }
    }
}
