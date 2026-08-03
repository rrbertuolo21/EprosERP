using Flunt.Notifications;
using Flunt.Validations;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaTransporteVolume : Notifiable<Notification>
    {
        protected VendaTransporteVolume() { }
        public VendaTransporteVolume(int quantidadeVolumes, string especie, string numeroVolumes, decimal pesoLiquido, decimal pesoBruto, string marca)
        {
            QuantidadeVolumes = (quantidadeVolumes == 0 ? null : quantidadeVolumes);
            Especie = string.IsNullOrEmpty(especie) ? null : especie;
            NumeroVolumes = string.IsNullOrEmpty(numeroVolumes) ? null : numeroVolumes;
            PesoLiquido = (pesoLiquido == 0 ? null : pesoLiquido);
            PesoBruto = (pesoBruto == 0 ? null : pesoBruto);
            Marca = string.IsNullOrEmpty(marca) ? null : marca;
            Validar();
        }


        public int? QuantidadeVolumes { get; private set; }
        public string? Especie { get; private set; }
        public string? NumeroVolumes { get; private set; }
        public decimal? PesoLiquido { get; private set; }
        public decimal? PesoBruto { get; private set; }
        public string? Marca { get; private set; }

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()

            .IsLowerOrEqualsThan(60, (Marca ?? "").Length, "Marca", "Marca do frete pode conter no max 60 caracteres")

            .IsLowerOrEqualsThan(60, (Especie ?? "").Length, "Especie", "Especie do frete pode conter no max 60 caracteres")

            //.IsGreaterOrEqualsThan(QuantidadeVolumes, decimal.Zero, "QuantidadeVolumes", "Quantidade Volumes da do frete, deve ser maior ou igual a zero")

            //.IsGreaterOrEqualsThan(NumeroVolumes, decimal.Zero, "NumeroVolumes", "Desconto do frete, deve ser maior ou igual a zero")

            //.IsGreaterOrEqualsThan(PesoLiquido, decimal.Zero, "PesoLiquido", "Desconto do frete, deve ser maior ou igual a zero")

            //.IsGreaterOrEqualsThan(PesoBruto, decimal.Zero, "PesoBruto", "Peso Bruto do frete deve ser maior ou igual a zero")

            //.IsGreaterOrEqualsThan(ValorFrete, decimal.Zero, "ValorFrete", "Valor Frete do frete deve ser maior ou igual a zero")

            );
        }
    }
}
