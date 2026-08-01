using DFe.Classes.Entidades;
using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaTransporteReboque : Notifiable<Notification>
    {
        public VendaTransporteReboque() { }
        public VendaTransporteReboque(string placa, string uf, string rntc)
        {
            Placa = placa;
            Rntc = string.IsNullOrEmpty(rntc) ? null : rntc;
            Enum.TryParse(uf, true, out Estado estado);
            Uf = (estado == 0 ? null : estado);
            Validar();
        }

        public string Placa { get; private set; } = null!;
        public Estado? Uf { get; private set; }
        public string? Rntc { get; private set; }

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
            .Requires()
            .IsBetween((Placa ?? "").Length, 1, 8, "Placa", "Placa do reboque do transporte de conter entre 1 e 8 caracteres")
            .IsLowerOrEqualsThan(20, (Rntc ?? "").Length, "Rntc", "RNTC do reboque do transporte, pode conter no max 20 caracteres")
            );

            if (!Enum.TryParse(Uf.GetHashCode().ToString(), out Estado estado))
                AddNotification("Uf", $"UF do endereço do emitente, inválido");
        }
    }
}
