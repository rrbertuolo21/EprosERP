using System;
using Epros.Shared.Domain.Enums;

namespace Epros.Shared.Domain.Entities
{
    /// <summary>
    /// TRANSVERSAL T10 — DOCUMENTO do GED CANÔNICO ÚNICO (estrutura central).
    /// Repositório único de metadados + versão para todo anexo (contrato, NF, laudo, certificado)
    /// em vez de byte[] fragmentado por módulo. O conteúdo binário fica no storage (referenciado por
    /// <see cref="StorageRef"/>); aqui vivem metadados, hash (dedupe/integridade), versão e o estado
    /// de assinatura. A assinatura digital ICP fica atrás de <c>IAssinaturaDigitalService</c>
    /// (provedor externo = dependência). Ver DECISOES-TRANSVERSAIS.md · T10 / PD-02 / PD-03.
    ///
    /// Herda <see cref="EntidadeSaaSBase"/> → isolamento por tenant, soft-delete governado, xmin (T7)
    /// e trilha de auditoria de campos, todos automáticos via ContextBase.
    /// </summary>
    public class DocumentoGed : EntidadeSaaSBase
    {
        /// <summary>Nome amigável do documento.</summary>
        public string Nome { get; private set; } = string.Empty;

        /// <summary>Tipo/categoria (ex.: "contrato", "nfe-xml", "laudo", "certificado-a1").</summary>
        public string TipoDocumento { get; private set; } = string.Empty;

        /// <summary>Hash do conteúdo (ex.: SHA-256) — dedupe e verificação de integridade.</summary>
        public string Hash { get; private set; } = string.Empty;

        public long TamanhoBytes { get; private set; }
        public string? MimeType { get; private set; }

        /// <summary>Versão do documento (incrementa a cada nova revisão do mesmo lógico).</summary>
        public int Versao { get; private set; }

        /// <summary>Referência opaca ao objeto no storage (provider atrás de abstração).</summary>
        public string? StorageRef { get; private set; }

        /// <summary>Módulo/entidade de origem do anexo (rastreabilidade da consolidação).</summary>
        public string? ModuloOrigem { get; private set; }
        public string? EntidadeOrigemTipo { get; private set; }
        public string? EntidadeOrigemId { get; private set; }

        public EStatusAssinaturaDocumento StatusAssinatura { get; private set; }
        public DateTime? AssinadoEm { get; private set; }

        protected DocumentoGed() { } // EF Core

        public DocumentoGed(
            string nome,
            string tipoDocumento,
            string hash,
            long tamanhoBytes,
            string tenantId,
            string criadoPor,
            string? mimeType = null,
            string? storageRef = null,
            bool requerAssinatura = false,
            string? moduloOrigem = null,
            string? entidadeOrigemTipo = null,
            string? entidadeOrigemId = null)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            TipoDocumento = tipoDocumento;
            Hash = hash;
            TamanhoBytes = tamanhoBytes;
            MimeType = mimeType;
            StorageRef = storageRef;
            ModuloOrigem = moduloOrigem;
            EntidadeOrigemTipo = entidadeOrigemTipo;
            EntidadeOrigemId = entidadeOrigemId;
            Versao = 1;
            StatusAssinatura = requerAssinatura
                ? EStatusAssinaturaDocumento.PendenteAssinatura
                : EStatusAssinaturaDocumento.NaoRequerAssinatura;
        }

        /// <summary>Nova revisão: aponta para novo conteúdo e incrementa a versão.</summary>
        public void RegistrarNovaVersao(string hash, long tamanhoBytes, string? storageRef, string alteradoPor)
        {
            Hash = hash;
            TamanhoBytes = tamanhoBytes;
            StorageRef = storageRef;
            Versao++;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Marca como pendente de assinatura (fluxo ICP inicia).</summary>
        public void MarcarPendenteAssinatura(string alteradoPor)
        {
            StatusAssinatura = EStatusAssinaturaDocumento.PendenteAssinatura;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// Confirma a assinatura. SÓ deve ser chamado pela camada de assinatura APÓS um provedor ICP
        /// válido devolver evidência — sem provedor/certificado o documento fica pendente (não se
        /// inventa validade jurídica).
        /// </summary>
        public void ConfirmarAssinatura(string alteradoPor)
        {
            StatusAssinatura = EStatusAssinaturaDocumento.Assinado;
            AssinadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void RecusarAssinatura(string alteradoPor)
        {
            StatusAssinatura = EStatusAssinaturaDocumento.Recusado;
            MarcarAlterado(alteradoPor);
        }
    }
}
