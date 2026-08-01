namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class IntermediadorDto
    {
        public IntermediadorDto() { }

        public IntermediadorDto(string documento, string? identificadorIntermediador)
        {
            Documento = documento;
            IdentificadorIntermediador = identificadorIntermediador;
        }

        public string Documento { get; set; } = null!;
        public string? IdentificadorIntermediador { get; set; }
    }
}
