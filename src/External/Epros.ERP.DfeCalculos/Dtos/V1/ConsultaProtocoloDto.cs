using System.ComponentModel.DataAnnotations;

namespace Epros.ERP.DfeCalculos.Dtos.V1
{
    public class ConsultaProtocoloDto
    {
        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [MaxLength(44, ErrorMessage = "O campo {0} deve conter exatamente 44 caracteres")]
        [MinLength(44, ErrorMessage = "O campo {0} deve conter exatamente 44 caracteres")]
        public string Chave { get; set; } = null!;

        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [Range(1, 2, ErrorMessage = "Ambiente deve conter entre 1 Produção e 2 Homologação")]
        public int Ambiente { get; set; } = 1;
    }
}
