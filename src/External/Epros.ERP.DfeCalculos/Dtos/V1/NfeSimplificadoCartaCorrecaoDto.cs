using System.ComponentModel.DataAnnotations;

namespace Epros.ERP.DfeCalculos.Dtos.V1
{
    public class NfeSimplificadoCartaCorrecaoDto
    {
        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [MaxLength(44, ErrorMessage = "O campo {0} deve conter 44 caracteres")]
        [MinLength(44, ErrorMessage = "O campo {0} deve conter 44 caracteres")]
        public string Chave { get; set; } = null!;

        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [MaxLength(5000, ErrorMessage = "O campo Texto Correção deve conter entre 15 e 1000 caracteres")]
        [MinLength(15, ErrorMessage = "O campo Texto Correção deve conter entre 15 e 1000 caracteres")]
        public string TextoCorrecao { get; set; } = null!;

        public int Ambiente { get; set; } = 1;
    }
}
