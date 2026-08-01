using System.ComponentModel.DataAnnotations;

namespace Epros.ERP.DfeCalculos.Dtos.V1
{
    public class CancelamentoDto
    {
        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [MaxLength(44, ErrorMessage = "O campo {0} deve conter exatamente 44 caracteres")]
        [MinLength(44, ErrorMessage = "O campo {0} deve conter exatamente 44 caracteres")]
        public string Chave { get; set; } = null!;

        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [MaxLength(5000, ErrorMessage = "O campo Motivo deve conter entre 2 e 5000 caracteres")]
        [MinLength(2, ErrorMessage = "O campo Motivo deve conter entre 2 e 5000 caracteres")]
        public string Motivo { get; set; } = null!;

        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [Range(1, 2, ErrorMessage = "Ambiente deve conter entre 1 Produção e 2 Homologação")]
        public int Ambiente { get; set; } = 1;
        public string? LinkEmpresaLogo { get; set; }
    }
}
