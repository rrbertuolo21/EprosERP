using Flunt.Notifications;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaTransporte : Notifiable<Notification>
    {
        public VendaTransporte() { }
        public VendaTransporte(VendaTransporteTransportadora? transportadora, VendaTransporteVeiculo? veiculo, ICollection<VendaTransporteVolume>? volumes, ICollection<VendaTransporteReboque>? reboques)
        {
            Transportadora = transportadora;
            Veiculo = veiculo;
            Volumes = volumes;
            Reboques = reboques;
        }

        public VendaTransporteTransportadora? Transportadora { get; private set; }
        public VendaTransporteVeiculo? Veiculo { get; private set; }
        public ICollection<VendaTransporteVolume>? Volumes { get; private set; }
        public ICollection<VendaTransporteReboque>? Reboques { get; private set; }
    }
}
