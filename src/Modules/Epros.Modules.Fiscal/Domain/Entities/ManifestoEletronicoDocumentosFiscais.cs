using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    /// <summary>
    /// MDF-e — Manifesto Eletrônico de Documentos Fiscais (modelo 58). Porte do fluxo legado
    /// <c>Dfe.API/MdfeController</c> (emitir/encerrar). No legado a transmissão era um TODO/mock;
    /// aqui persistimos o documento e delegamos a transmissão real ao <c>IMdfeFiscalService</c> (Hercules),
    /// que degrada de forma controlada quando não há emitente/certificado configurado.
    /// </summary>
    public class ManifestoEletronicoDocumentosFiscais : EntidadeSaaSBase
    {
        // Legado: SequenciaTenantId (somente exibição/UX).
        public long? SequenciaExibicao { get; private set; }

        /// <summary>Empresa emitente — resolve certificado/parâmetros DF-e via Lookup.</summary>
        public Guid? EmpresaId { get; private set; }

        public int Serie { get; private set; }
        public long Numero { get; private set; }

        /// <summary>1=Produção, 2=Homologação.</summary>
        public int Ambiente { get; private set; } = 2;

        /// <summary>Modal: 1=Rodoviário, 2=Aéreo, 3=Aquaviário, 4=Ferroviário.</summary>
        public int Modal { get; private set; } = 1;

        /// <summary>Tipo do emitente: 1=Prestador de serviço de transporte, 2=Transportador de carga própria.</summary>
        public int TipoEmitente { get; private set; } = 1;

        public string UfInicio { get; private set; } = string.Empty;
        public string UfFim { get; private set; } = string.Empty;
        public int QuantidadeCarregados { get; private set; }
        public decimal ValorCarga { get; private set; }

        /// <summary>Rascunho, Pendente, Autorizado, Rejeitado, Encerrado, Cancelado.</summary>
        public string Status { get; private set; } = "Rascunho";
        public string? ChaveAcesso { get; private set; }
        public string? Protocolo { get; private set; }
        public int? StatusSefaz { get; private set; }
        public string? MotivoRejeicao { get; private set; }
        public string? XmlEnvio { get; private set; }
        public string? XmlRetorno { get; private set; }

        // ---- Encerramento ----
        public string? MunicipioEncerramentoIbge { get; private set; }
        public string? ProtocoloEncerramento { get; private set; }
        public DateTime? DataEncerramento { get; private set; }

        public DateTime? DataAutorizacao { get; private set; }
        public DateTime? DataCancelamento { get; private set; }
        public DateTime DataEmissao { get; private set; }

        protected ManifestoEletronicoDocumentosFiscais() { } // EF Core

        public ManifestoEletronicoDocumentosFiscais(
            int serie,
            long numero,
            int ambiente,
            int modal,
            int tipoEmitente,
            string ufInicio,
            string ufFim,
            int quantidadeCarregados,
            decimal valorCarga,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Serie = serie;
            Numero = numero;
            Ambiente = ambiente == 1 ? 1 : 2;
            Modal = modal;
            TipoEmitente = tipoEmitente;
            UfInicio = ufInicio;
            UfFim = ufFim;
            QuantidadeCarregados = quantidadeCarregados;
            ValorCarga = valorCarga;
            DataEmissao = DateTime.UtcNow;
            Status = "Rascunho";

            Validar();
        }

        public void VincularEmpresaEmitente(Guid empresaId) => EmpresaId = empresaId;

        public void Autorizar(string chaveAcesso, string protocolo, int statusSefaz, string? xmlEnvio, string? xmlRetorno)
        {
            Status = "Autorizado";
            ChaveAcesso = chaveAcesso;
            Protocolo = protocolo;
            StatusSefaz = statusSefaz;
            if (!string.IsNullOrWhiteSpace(xmlEnvio)) XmlEnvio = xmlEnvio;
            if (!string.IsNullOrWhiteSpace(xmlRetorno)) XmlRetorno = xmlRetorno;
            MotivoRejeicao = null;
            DataAutorizacao = DateTime.UtcNow;
        }

        public void Rejeitar(int statusSefaz, string motivo, string? xmlRetorno)
        {
            Status = "Rejeitado";
            StatusSefaz = statusSefaz;
            MotivoRejeicao = motivo;
            if (!string.IsNullOrWhiteSpace(xmlRetorno)) XmlRetorno = xmlRetorno;
        }

        public void Encerrar(string municipioEncerramentoIbge, string? protocoloEncerramento, string? xmlRetorno, string usuario)
        {
            Status = "Encerrado";
            MunicipioEncerramentoIbge = municipioEncerramentoIbge;
            ProtocoloEncerramento = protocoloEncerramento;
            DataEncerramento = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(xmlRetorno)) XmlRetorno = xmlRetorno;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string? xmlRetorno, string usuario)
        {
            Status = "Cancelado";
            DataCancelamento = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(xmlRetorno)) XmlRetorno = xmlRetorno;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ManifestoEletronicoDocumentosFiscais>()
                .Requires()
                .IsNotNullOrEmpty(UfInicio, nameof(UfInicio), "A UF de início é obrigatória [Origem: ManifestoEletronicoDocumentosFiscais]")
                .IsNotNullOrEmpty(UfFim, nameof(UfFim), "A UF de fim é obrigatória [Origem: ManifestoEletronicoDocumentosFiscais]")
                .IsGreaterThan(ValorCarga, -0.01m, nameof(ValorCarga), "O valor da carga não pode ser negativo [Origem: ManifestoEletronicoDocumentosFiscais]")
                .IsTrue(Ambiente == 1 || Ambiente == 2, nameof(Ambiente), "O ambiente deve ser 1 (Produção) ou 2 (Homologação) [Origem: ManifestoEletronicoDocumentosFiscais]"));
        }
    }
}
