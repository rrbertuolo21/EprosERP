using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Assinatura
{
    /// <summary>
    /// PLT · ASSINATURA ELETRÔNICA ICP (PD-02 / T10) — solicitação de assinatura de um documento do
    /// GED canônico. CONSTRUÍDA DO ZERO: NÃO reaproveita <c>AssinaturaCliente</c> (plano SaaS).
    /// O ato de assinar passa pela abstração <c>IAssinaturaDigitalService</c>; sem provedor/certificado
    /// ICP a solicitação permanece <c>EmAssinatura</c> e o documento "pendente" — NUNCA se forja validade.
    /// </summary>
    public class SolicitacaoAssinatura : EntidadeSaaSBase
    {
        public Guid DocumentoId { get; private set; }
        public int VersaoDocumento { get; private set; }
        /// <summary>Rascunho, EmAssinatura, Concluida, Recusada, Cancelada.</summary>
        public string Estado { get; private set; } = "EmAssinatura";
        public bool LinkPublico { get; private set; }
        public int MinAssinaturas { get; private set; }
        public bool ExigeOrdem { get; private set; }
        /// <summary>Extensível (T10): "ICP-Brasil-A1", "ICP-Brasil-A3", futuros provedores.</summary>
        public string TipoAssinatura { get; private set; } = "ICP-Brasil";

        protected SolicitacaoAssinatura() { }

        public SolicitacaoAssinatura(Guid documentoId, int versaoDocumento, int minAssinaturas, bool exigeOrdem,
            bool linkPublico, string tipoAssinatura, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<SolicitacaoAssinatura>()
                .Requires()
                .IsTrue(documentoId != Guid.Empty, nameof(DocumentoId), "O documento é obrigatório.")
                .IsGreaterThan(minAssinaturas, 0, nameof(MinAssinaturas), "É necessária ao menos uma assinatura."));

            DocumentoId = documentoId;
            VersaoDocumento = versaoDocumento;
            MinAssinaturas = minAssinaturas;
            ExigeOrdem = exigeOrdem;
            LinkPublico = linkPublico;
            TipoAssinatura = string.IsNullOrWhiteSpace(tipoAssinatura) ? "ICP-Brasil" : tipoAssinatura;
            Estado = "EmAssinatura";
        }

        public bool EmAndamento => Estado == "EmAssinatura";

        public void Concluir(string usuario)
        {
            Estado = "Concluida";
            MarcarAlterado(usuario);
        }

        public void Recusar(string usuario)
        {
            Estado = "Recusada";
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            Estado = "Cancelada";
            MarcarAlterado(usuario);
        }
    }

    /// <summary>Signatário (interno ou externo por link público) de uma solicitação de assinatura.</summary>
    public class SignatarioAssinatura : EntidadeSaaSBase
    {
        public Guid SolicitacaoId { get; private set; }
        public int Ordem { get; private set; }
        public bool Obrigatorio { get; private set; }
        public bool Externo { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Identificacao { get; private set; } = string.Empty; // e-mail / CPF / login
        public string? LinkToken { get; private set; }
        public DateTime? RevogadoEm { get; private set; }
        /// <summary>Pendente, Assinado, Recusado.</summary>
        public string Status { get; private set; } = "Pendente";

        protected SignatarioAssinatura() { }

        public SignatarioAssinatura(Guid solicitacaoId, int ordem, bool obrigatorio, bool externo,
            string nome, string identificacao, string? linkToken, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<SignatarioAssinatura>()
                .Requires()
                .IsTrue(solicitacaoId != Guid.Empty, nameof(SolicitacaoId), "A solicitação é obrigatória.")
                .IsGreaterOrEqualsThan(ordem, 1, nameof(Ordem), "A ordem deve ser >= 1.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do signatário é obrigatório.")
                .IsNotNullOrEmpty(identificacao, nameof(Identificacao), "A identificação do signatário é obrigatória."));

            SolicitacaoId = solicitacaoId;
            Ordem = ordem;
            Obrigatorio = obrigatorio;
            Externo = externo;
            Nome = nome;
            Identificacao = identificacao;
            LinkToken = linkToken;
            Status = "Pendente";
        }

        public bool LinkValido => Externo && LinkToken != null && RevogadoEm == null;

        public void MarcarAssinado(string usuario)
        {
            Status = "Assinado";
            MarcarAlterado(usuario);
        }

        public void MarcarRecusado(string usuario)
        {
            Status = "Recusado";
            MarcarAlterado(usuario);
        }

        public void RevogarLink(string usuario)
        {
            RevogadoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>Registro imutável de uma assinatura efetivada (com carimbo de tempo).</summary>
    public class RegistroAssinatura : EntidadeSaaSBase
    {
        public Guid SolicitacaoId { get; private set; }
        public Guid SignatarioId { get; private set; }
        public DateTime AssinadoEm { get; private set; }
        public string? CarimboTempo { get; private set; }

        protected RegistroAssinatura() { }

        public RegistroAssinatura(Guid solicitacaoId, Guid signatarioId, string? carimboTempo,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            SolicitacaoId = solicitacaoId;
            SignatarioId = signatarioId;
            AssinadoEm = DateTime.UtcNow;
            CarimboTempo = carimboTempo;
        }
    }

    /// <summary>
    /// Evidência criptográfica da assinatura (REG-ASS-022/062): hash, série do certificado, cadeia ICP,
    /// payload. O certificado/cadeia reais vêm do provedor ICP (dependência externa); quando ausentes,
    /// permanecem nulos — a evidência registra o que o provedor devolveu, sem inventar.
    /// </summary>
    public class EvidenciaAssinatura : EntidadeSaaSBase
    {
        public Guid RegistroId { get; private set; }
        public string Hash { get; private set; } = string.Empty;
        public string? CertificadoSerial { get; private set; }
        public string? CadeiaIcp { get; private set; }
        public string? ValorEvidencia { get; private set; }

        protected EvidenciaAssinatura() { }

        public EvidenciaAssinatura(Guid registroId, string hash, string? certificadoSerial, string? cadeiaIcp,
            string? valorEvidencia, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            RegistroId = registroId;
            Hash = hash;
            CertificadoSerial = certificadoSerial;
            CadeiaIcp = cadeiaIcp;
            ValorEvidencia = valorEvidencia;
        }
    }

    /// <summary>Política de assinatura por tipo documental (min. assinaturas, tipo, ordem). Extensível.</summary>
    public class PoliticaAssinatura : EntidadeSaaSBase
    {
        public string TipoDocumento { get; private set; } = string.Empty;
        public int MinAssinaturas { get; private set; }
        public string TipoAssinatura { get; private set; } = "ICP-Brasil";
        public bool ExigeOrdem { get; private set; }

        protected PoliticaAssinatura() { }

        public PoliticaAssinatura(string tipoDocumento, int minAssinaturas, string tipoAssinatura,
            bool exigeOrdem, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PoliticaAssinatura>()
                .Requires()
                .IsNotNullOrEmpty(tipoDocumento, nameof(TipoDocumento), "O tipo de documento é obrigatório.")
                .IsGreaterThan(minAssinaturas, 0, nameof(MinAssinaturas), "O mínimo de assinaturas deve ser > 0."));

            TipoDocumento = tipoDocumento;
            MinAssinaturas = minAssinaturas;
            TipoAssinatura = string.IsNullOrWhiteSpace(tipoAssinatura) ? "ICP-Brasil" : tipoAssinatura;
            ExigeOrdem = exigeOrdem;
        }

        public void Atualizar(int minAssinaturas, string tipoAssinatura, bool exigeOrdem, string usuario)
        {
            if (minAssinaturas > 0) MinAssinaturas = minAssinaturas;
            if (!string.IsNullOrWhiteSpace(tipoAssinatura)) TipoAssinatura = tipoAssinatura;
            ExigeOrdem = exigeOrdem;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>Trilha de ações da solicitação de assinatura (append-only por convenção).</summary>
    public class HistoricoAssinatura : EntidadeSaaSBase
    {
        public Guid SolicitacaoId { get; private set; }
        public string Acao { get; private set; } = string.Empty;
        public string? Detalhe { get; private set; }

        protected HistoricoAssinatura() { }

        public HistoricoAssinatura(Guid solicitacaoId, string acao, string? detalhe, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            SolicitacaoId = solicitacaoId;
            Acao = acao;
            Detalhe = detalhe;
        }
    }
}
