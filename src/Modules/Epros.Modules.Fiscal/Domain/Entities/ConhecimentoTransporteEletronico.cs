using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    /// <summary>
    /// CT-e — Conhecimento de Transporte Eletrônico (modelo 57). Porte do fluxo legado
    /// <c>Dfe.API/CteController</c> (emitir/cancelar). No legado a transmissão era um TODO/mock;
    /// aqui persistimos o documento e delegamos a transmissão real ao <c>ICteFiscalService</c> (Hercules),
    /// que degrada de forma controlada quando não há emitente/certificado configurado.
    /// </summary>
    public class ConhecimentoTransporteEletronico : EntidadeSaaSBase
    {
        // Legado: SequenciaTenantId (somente exibição/UX).
        public long? SequenciaExibicao { get; private set; }

        /// <summary>Empresa emitente — resolve certificado/parâmetros DF-e via Lookup.</summary>
        public Guid? EmpresaId { get; private set; }

        public int Serie { get; private set; }
        public long Numero { get; private set; }

        /// <summary>1=Produção, 2=Homologação.</summary>
        public int Ambiente { get; private set; } = 2;

        /// <summary>Tipo do CT-e: 0=Normal, 1=Complemento, 2=Anulação, 3=Substituto.</summary>
        public int TipoCte { get; private set; }

        /// <summary>Modal: 1=Rodoviário, 2=Aéreo, 3=Aquaviário, 4=Ferroviário, 5=Dutoviário, 6=Multimodal.</summary>
        public int Modal { get; private set; } = 1;

        public string RemetenteDocumento { get; private set; } = string.Empty;
        public string DestinatarioDocumento { get; private set; } = string.Empty;

        public decimal ValorTotal { get; private set; }
        public decimal ValorReceber { get; private set; }

        /// <summary>Rascunho, Pendente, Autorizado, Rejeitado, Cancelado.</summary>
        public string Status { get; private set; } = "Rascunho";
        public string? ChaveAcesso { get; private set; }
        public string? Protocolo { get; private set; }
        public int? StatusSefaz { get; private set; }
        public string? MotivoRejeicao { get; private set; }
        public string? XmlEnvio { get; private set; }
        public string? XmlRetorno { get; private set; }

        public string? JustificativaCancelamento { get; private set; }
        public DateTime? DataAutorizacao { get; private set; }
        public DateTime? DataCancelamento { get; private set; }
        public DateTime DataEmissao { get; private set; }

        protected ConhecimentoTransporteEletronico() { } // EF Core

        public ConhecimentoTransporteEletronico(
            int serie,
            long numero,
            int ambiente,
            int tipoCte,
            int modal,
            string remetenteDocumento,
            string destinatarioDocumento,
            decimal valorTotal,
            decimal valorReceber,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Serie = serie;
            Numero = numero;
            Ambiente = ambiente == 1 ? 1 : 2;
            TipoCte = tipoCte;
            Modal = modal;
            RemetenteDocumento = remetenteDocumento;
            DestinatarioDocumento = destinatarioDocumento;
            ValorTotal = valorTotal;
            ValorReceber = valorReceber;
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

        public void Cancelar(string justificativa, string? xmlRetorno, string usuario)
        {
            Status = "Cancelado";
            JustificativaCancelamento = justificativa;
            DataCancelamento = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(xmlRetorno)) XmlRetorno = xmlRetorno;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ConhecimentoTransporteEletronico>()
                .Requires()
                .IsNotNullOrEmpty(RemetenteDocumento, nameof(RemetenteDocumento), "O documento do remetente é obrigatório [Origem: ConhecimentoTransporteEletronico]")
                .IsNotNullOrEmpty(DestinatarioDocumento, nameof(DestinatarioDocumento), "O documento do destinatário é obrigatório [Origem: ConhecimentoTransporteEletronico]")
                .IsGreaterThan(ValorTotal, -0.01m, nameof(ValorTotal), "O valor total não pode ser negativo [Origem: ConhecimentoTransporteEletronico]")
                .IsTrue(Ambiente == 1 || Ambiente == 2, nameof(Ambiente), "O ambiente deve ser 1 (Produção) ou 2 (Homologação) [Origem: ConhecimentoTransporteEletronico]"));
        }
    }
}
