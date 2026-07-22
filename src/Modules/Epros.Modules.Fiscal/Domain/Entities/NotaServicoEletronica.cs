using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    /// <summary>
    /// Nota Fiscal de Serviço Eletrônica (NFS-e) emitida a partir de um RPS. Porte fiel do fluxo
    /// legado <c>Dfe.API/NFSe</c> + <c>VendaNfseService</c> (OpenAC.Net.NFSe/GISS), no modelo novo:
    /// persiste o RPS (número/série/tipo), prestador/tomador, dados do serviço e valores, além do
    /// retorno da prefeitura (número da NFS-e, protocolo, código de verificação, status e XMLs).
    ///
    /// A transmissão real ao webservice municipal é feita pelo <c>INfseFiscalService</c> (adapter),
    /// pois cada município tem um provedor/URL distinto; esta entidade guarda o estado do documento.
    /// </summary>
    public class NotaServicoEletronica : EntidadeSaaSBase
    {
        // Legado: SequenciaTenantId (somente exibição/UX).
        public long? SequenciaExibicao { get; private set; }

        /// <summary>Empresa emitente (prestador) — resolve certificado/parâmetros municipais via Lookup.</summary>
        public Guid? EmpresaId { get; private set; }

        // ---- RPS ----
        public string RpsNumero { get; private set; } = string.Empty;
        public string RpsSerie { get; private set; } = string.Empty;
        /// <summary>Tipo de RPS: 1=RPS, 2=Nota Fiscal Conjugada, 3=Cupom.</summary>
        public int RpsTipo { get; private set; } = 1;
        public int NumeroLote { get; private set; }

        /// <summary>1=Produção, 2=Homologação.</summary>
        public int Ambiente { get; private set; } = 2;
        public int NaturezaOperacao { get; private set; } = 1;
        public int RegimeEspecialTributacao { get; private set; }
        public bool OptanteSimplesNacional { get; private set; }
        public bool IncentivoFiscal { get; private set; }
        public DateTime? Competencia { get; private set; }

        // ---- Prestador / Tomador ----
        public string PrestadorDocumento { get; private set; } = string.Empty;
        public string? PrestadorInscricaoMunicipal { get; private set; }
        public int PrestadorCodigoMunicipioIbge { get; private set; }
        public string TomadorDocumento { get; private set; } = string.Empty;
        public string? TomadorRazaoSocial { get; private set; }

        // ---- Serviço ----
        public string ItemListaServico { get; private set; } = string.Empty;
        public string? CodigoTributacaoMunicipio { get; private set; }
        public string? CodigoCnae { get; private set; }
        public string? CodigoNbs { get; private set; }
        public string? Discriminacao { get; private set; }
        public int CodigoMunicipioIbge { get; private set; }
        public int ExigibilidadeIss { get; private set; } = 1;
        public int MunicipioIncidencia { get; private set; }

        // ---- Valores ----
        public decimal ValorServicos { get; private set; }
        public decimal ValorDeducoes { get; private set; }
        public decimal ValorIss { get; private set; }
        public decimal ValorIssRetido { get; private set; }
        public decimal AliquotaIss { get; private set; }
        public decimal DescontoIncondicionado { get; private set; }
        public decimal DescontoCondicionado { get; private set; }
        public int IssRetido { get; private set; } = 1;

        // ---- Retorno da prefeitura ----
        /// <summary>Rascunho, Enviada, Autorizada, Rejeitada, Cancelada.</summary>
        public string Status { get; private set; } = "Rascunho";
        public string? NumeroNfse { get; private set; }
        public string? CodigoVerificacao { get; private set; }
        public string? Protocolo { get; private set; }
        public int? StatusPrefeitura { get; private set; }
        public string? MotivoRejeicao { get; private set; }
        public DateTime? DataAutorizacao { get; private set; }
        public DateTime? DataCancelamento { get; private set; }
        public string? XmlEnvio { get; private set; }
        public string? XmlRetorno { get; private set; }
        public string? PdfCaminho { get; private set; }
        public string? XmlCaminho { get; private set; }

        public DateTime DataEmissao { get; private set; }

        protected NotaServicoEletronica() { } // EF Core

        public NotaServicoEletronica(
            string rpsNumero,
            string rpsSerie,
            int rpsTipo,
            int numeroLote,
            int ambiente,
            int naturezaOperacao,
            int regimeEspecialTributacao,
            bool optanteSimplesNacional,
            bool incentivoFiscal,
            DateTime? competencia,
            string prestadorDocumento,
            string? prestadorInscricaoMunicipal,
            int prestadorCodigoMunicipioIbge,
            string tomadorDocumento,
            string? tomadorRazaoSocial,
            string itemListaServico,
            string? codigoTributacaoMunicipio,
            string? codigoCnae,
            string? codigoNbs,
            string? discriminacao,
            int codigoMunicipioIbge,
            int exigibilidadeIss,
            int municipioIncidencia,
            decimal valorServicos,
            decimal valorDeducoes,
            decimal valorIss,
            decimal valorIssRetido,
            decimal aliquotaIss,
            decimal descontoIncondicionado,
            decimal descontoCondicionado,
            int issRetido,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RpsNumero = rpsNumero;
            RpsSerie = rpsSerie;
            RpsTipo = rpsTipo;
            NumeroLote = numeroLote;
            Ambiente = ambiente == 1 ? 1 : 2;
            NaturezaOperacao = naturezaOperacao;
            RegimeEspecialTributacao = regimeEspecialTributacao;
            OptanteSimplesNacional = optanteSimplesNacional;
            IncentivoFiscal = incentivoFiscal;
            Competencia = competencia;
            PrestadorDocumento = prestadorDocumento;
            PrestadorInscricaoMunicipal = prestadorInscricaoMunicipal;
            PrestadorCodigoMunicipioIbge = prestadorCodigoMunicipioIbge;
            TomadorDocumento = tomadorDocumento;
            TomadorRazaoSocial = tomadorRazaoSocial;
            ItemListaServico = itemListaServico;
            CodigoTributacaoMunicipio = codigoTributacaoMunicipio;
            CodigoCnae = codigoCnae;
            CodigoNbs = codigoNbs;
            Discriminacao = discriminacao;
            CodigoMunicipioIbge = codigoMunicipioIbge;
            ExigibilidadeIss = exigibilidadeIss;
            MunicipioIncidencia = municipioIncidencia;
            ValorServicos = valorServicos;
            ValorDeducoes = valorDeducoes;
            ValorIss = valorIss;
            ValorIssRetido = valorIssRetido;
            AliquotaIss = aliquotaIss;
            DescontoIncondicionado = descontoIncondicionado;
            DescontoCondicionado = descontoCondicionado;
            IssRetido = issRetido;
            DataEmissao = DateTime.UtcNow;
            Status = "Rascunho";

            Validar();
        }

        public void VincularEmpresaEmitente(Guid empresaId) => EmpresaId = empresaId;

        public void MarcarEnviada(string? protocolo, string? xmlEnvio)
        {
            Status = "Enviada";
            Protocolo = protocolo;
            if (!string.IsNullOrWhiteSpace(xmlEnvio)) XmlEnvio = xmlEnvio;
        }

        public void Autorizar(string numeroNfse, string? codigoVerificacao, string? protocolo, int statusPrefeitura, string? xmlRetorno)
        {
            Status = "Autorizada";
            NumeroNfse = numeroNfse;
            CodigoVerificacao = codigoVerificacao;
            if (!string.IsNullOrWhiteSpace(protocolo)) Protocolo = protocolo;
            StatusPrefeitura = statusPrefeitura;
            XmlRetorno = xmlRetorno;
            MotivoRejeicao = null;
            DataAutorizacao = DateTime.UtcNow;
        }

        public void Rejeitar(int statusPrefeitura, string motivo, string? xmlRetorno)
        {
            Status = "Rejeitada";
            StatusPrefeitura = statusPrefeitura;
            MotivoRejeicao = motivo;
            if (!string.IsNullOrWhiteSpace(xmlRetorno)) XmlRetorno = xmlRetorno;
        }

        public void Cancelar(string xmlRetorno, string usuario)
        {
            Status = "Cancelada";
            DataCancelamento = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(xmlRetorno)) XmlRetorno = xmlRetorno;
            MarcarAlterado(usuario);
        }

        public void DefinirCaminhosArquivos(string? xmlCaminho, string? pdfCaminho)
        {
            if (!string.IsNullOrWhiteSpace(xmlCaminho)) XmlCaminho = xmlCaminho;
            if (!string.IsNullOrWhiteSpace(pdfCaminho)) PdfCaminho = pdfCaminho;
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<NotaServicoEletronica>()
                .Requires()
                .IsNotNullOrEmpty(RpsNumero, nameof(RpsNumero), "O número do RPS é obrigatório [Origem: NotaServicoEletronica]")
                .IsNotNullOrEmpty(RpsSerie, nameof(RpsSerie), "A série do RPS é obrigatória [Origem: NotaServicoEletronica]")
                .IsNotNullOrEmpty(PrestadorDocumento, nameof(PrestadorDocumento), "O documento do prestador é obrigatório [Origem: NotaServicoEletronica]")
                .IsNotNullOrEmpty(ItemListaServico, nameof(ItemListaServico), "O item da lista de serviço é obrigatório [Origem: NotaServicoEletronica]")
                .IsGreaterThan(ValorServicos, 0, nameof(ValorServicos), "O valor dos serviços deve ser maior que zero [Origem: NotaServicoEletronica]")
                .IsTrue(Ambiente == 1 || Ambiente == 2, nameof(Ambiente), "O ambiente deve ser 1 (Produção) ou 2 (Homologação) [Origem: NotaServicoEletronica]"));
        }
    }
}
