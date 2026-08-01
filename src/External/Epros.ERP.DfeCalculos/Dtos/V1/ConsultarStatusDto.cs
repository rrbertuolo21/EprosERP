using System.ComponentModel.DataAnnotations;

namespace Epros.ERP.DfeCalculos.Dtos.V1
{
    public class ConsultarStatusDto
    {
        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [MaxLength(14, ErrorMessage = "O campo {0} deve conter exatamente 14 caracteres")]
        [MinLength(14, ErrorMessage = "O campo {0} deve conter exatamente 14 caracteres")]
        public string Documento { get; set; } = null!;

        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [MaxLength(2, ErrorMessage = "O campo {0} deve conter exatamente 2 caracteres")]
        [MinLength(2, ErrorMessage = "O campo {0} deve conter exatamente 2 caracteres")]
        public string Uf { get; set; } = null!;

        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [Range(1, 2, ErrorMessage = "Ambiente deve conter entre 1 Produção e 2 Homologação")]
        public int Ambiente { get; set; } = 1;
    }
}
