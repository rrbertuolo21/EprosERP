using System.ComponentModel.DataAnnotations;

namespace Epros.ERP.DfeCalculos.Dtos.V1
{
    public class InutilizacaoDto
    {
        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [MaxLength(14, ErrorMessage = "O campo {0} deve conter entre 11 e 14 caracteres")]
        [MinLength(11, ErrorMessage = "O campo {0} deve conter entre 11 e 14 caracteres")]
        public string Documento { get; set; } = null!;

        //[Required(ErrorMessage = "{0}, campo obrigatório")]
        //[MaxLength(2, ErrorMessage = "O campo {0} deve conter apenas 2 caracteres, exemplo 2025 -> 25")]
        //[MinLength(2, ErrorMessage = "O campo {0} deve conter apenas 2 caracteres, exemplo 2025 -> 25")]
        public int Ano { get; set; }
        public int Serie { get; set; }
        public long NrNfInicial { get; set; }
        public long NrNfFinal { get; set; }
        public string Uf { get; set; } = null!;

        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [Range(55, 65, ErrorMessage = "Modelo Documento [55] NF-e ou [65] NFC-e")]
        public int ModeloDocumento { get; set; }

        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [MaxLength(5000, ErrorMessage = "O campo Motivo deve conter entre 2 e 5000 caracteres")]
        [MinLength(2, ErrorMessage = "O campo Motivo deve conter entre 2 e 5000 caracteres")]
        public string Justificativa { get; set; } = null!;

        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [Range(1, 2, ErrorMessage = "Ambiente deve conter entre 1 Produção e 2 Homologação")]
        public int Ambiente { get; set; } = 1;
    }

    public class InutilizacaoSimplificadoDto
    {
        public int Serie { get; set; }
        public long NrNfInicial { get; set; }
        public long NrNfFinal { get; set; }

        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [Range(55, 65, ErrorMessage = "Modelo Documento [55] NF-e ou [65] NFC-e")]
        public int ModeloDocumento { get; set; }

        [Required(ErrorMessage = "{0}, campo obrigatório")]
        [MaxLength(5000, ErrorMessage = "O campo Motivo deve conter entre 2 e 5000 caracteres")]
        [MinLength(2, ErrorMessage = "O campo Motivo deve conter entre 2 e 5000 caracteres")]
        public string Justificativa { get; set; } = null!;
    }
}
