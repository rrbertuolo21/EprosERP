namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class TransporteNfceDto
    {
        public TransporteNfceDto() { }

        public TransporteNfceDto(TransporteTransportadoraDto transportadora)
        {

            Transportadora = transportadora;
            //Veiculo = veiculo;
            //Volumes = volumes;
        }

        public TransporteTransportadoraDto Transportadora { get; set; } = null!;
        //public TransporteVeiculoDto Veiculo { get; set; }
        //public TransporteVolumeDto[] Volumes { get; set; }
    }

    public class TransporteNfeDto
    {
        public TransporteNfeDto() { }

        public TransporteNfeDto(TransporteTransportadoraDto? transportadora, TransporteVeiculoDto? veiculo, TransporteVolumeDto[]? volumes, TransporteReboqueDto[]? reboques)
        {
            Transportadora = transportadora;
            Veiculo = veiculo;
            Volumes = volumes;
            Reboques = reboques;
        }

        public TransporteTransportadoraDto? Transportadora { get; set; }
        public TransporteVeiculoDto? Veiculo { get; set; }
        public TransporteVolumeDto[]? Volumes { get; set; }
        public TransporteReboqueDto[]? Reboques { get; set; }
    }
}