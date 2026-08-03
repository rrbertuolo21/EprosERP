using DFe.Classes.Flags;
using Flunt.Notifications;
using Flunt.Validations;
using NFe.Classes.Informacoes.Identificacao.Tipos;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaDocumentoDfe : Notifiable<Notification>
    {
        public VendaDocumentoDfe() { }
        public VendaDocumentoDfe(TipoNFe tipoNFe, TipoAmbiente ambiente, ModeloDocumento modeloDocumento, long numeroNf, int serieNf, string certPath, string certPass, string csc, string cscId, FinalidadeNFe finalidadeNFe, DateTime? dataEmissao, DateTime? dataHoraSaida, VendaDocumentoContigencia? contigencia, ICollection<VendaDocumentoDfeNumeroReferenciadaNFe?> chavesReferenciadasNFe)
        {
            Ambiente = ambiente;
            ModeloDocumento = modeloDocumento;
            NumeroNf = numeroNf;
            SerieNf = serieNf;
            CertPath = string.IsNullOrEmpty(certPath) ? null : certPath;
            CertPass = string.IsNullOrEmpty(certPass) ? null : certPass;
            Csc = string.IsNullOrEmpty(csc) ? null : csc;
            CscId = cscId;
            FinalidadeNFe = finalidadeNFe;
            DataEmissao = dataEmissao ?? DateTime.Now;
            DataHoraSaida = dataHoraSaida;
            Contigencia = contigencia;
            ChavesReferenciadasNFe = chavesReferenciadasNFe;
            TipoNFe = tipoNFe;
            Validar();
        }

        public VendaDocumentoDfe(TipoAmbiente ambiente, ModeloDocumento modeloDocumento, FinalidadeNFe finalidadeNFe, ICollection<VendaDocumentoDfeNumeroReferenciadaNFe?> chavesReferenciadasNFe)
        {
            Ambiente = ambiente;
            ModeloDocumento = modeloDocumento;
            FinalidadeNFe = finalidadeNFe;
            ChavesReferenciadasNFe = chavesReferenciadasNFe;
            TipoNFe = TipoNFe.tnSaida;
            Validar();
        }

        public TipoNFe TipoNFe { get; private set; }
        public ModeloDocumento ModeloDocumento { get; private set; }
        public TipoAmbiente Ambiente { get; private set; }
        public long NumeroNf { get; private set; }
        public int SerieNf { get; private set; }
        public string? CertPath { get; private set; }
        public string? CertPass { get; private set; }
        public string? Csc { get; private set; }
        public string? CscId { get; private set; }
        public FinalidadeNFe FinalidadeNFe { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime? DataHoraSaida { get; set; }
        public VendaDocumentoContigencia? Contigencia { get; private set; }
        public ICollection<VendaDocumentoDfeNumeroReferenciadaNFe?> ChavesReferenciadasNFe { get; private set; } = null!;

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()
            .Requires()
            .IsNotNullOrEmpty(CertPath, "CertPath", "Caminho obrigatório")

            .Requires()
            .IsNotNullOrEmpty(CertPass, "CertPass", "Senha do certificado obrigatório")

            //.Requires()
            //.IsNotNullOrEmpty(Csc, "Csc", "Csc obrigatório")

            //.Requires()
            //.IsGreaterThan(CscId, 0, "CscId deve ser maior que zero")

            );

            if (!Enum.TryParse(ModeloDocumento.GetHashCode().ToString(), out ModeloDocumento modeloDocumento))
                AddNotification("ModeloDocumento", $"Modelo Documento inválido");

            if (!Enum.TryParse(Ambiente.GetHashCode().ToString(), out TipoAmbiente ambiente))
                AddNotification("Ambiente", $"Ambiente inválido");

            if (!Enum.IsDefined(typeof(FinalidadeNFe), FinalidadeNFe))
                AddNotification("FinalidadeNFe", $"Finalidade NF-e inválido");

            if (FinalidadeNFe != FinalidadeNFe.fnNormal && FinalidadeNFe != FinalidadeNFe.fnDevolucao)
                AddNotification("FinalidadeNFe", $"Finalidade NF-e inválido");

            if (Contigencia != null)
                AddNotifications(Contigencia.Notifications);

            if (FinalidadeNFe == FinalidadeNFe.fnDevolucao && ChavesReferenciadasNFe == null)
                AddNotification("FinalidadeNFe", $"Finalidade NF-e Devolução sem chaves, inválido");
        }

        public void DefinirModeloDocumento(ModeloDocumento modeloDocumento)
        {
            ModeloDocumento = modeloDocumento;
        }

        public void DefinirFinalidadeNFe(FinalidadeNFe finalidadeNFe)
        {
            FinalidadeNFe = finalidadeNFe;
        }
    }

    public class VendaDocumentoDfeNumeroReferenciadaNFe
    {
        public VendaDocumentoDfeNumeroReferenciadaNFe() { }
        public VendaDocumentoDfeNumeroReferenciadaNFe(string chaveReferenciadaNFe)
        {
            ChaveReferenciadaNFe = chaveReferenciadaNFe;
        }

        public string ChaveReferenciadaNFe { get; private set; } = null!;
    }
}
