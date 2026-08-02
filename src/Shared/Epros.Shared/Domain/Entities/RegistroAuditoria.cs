using System;

namespace Epros.Shared.Domain.Entities
{
    /// <summary>
    /// TRANSVERSAL T8 — TRILHA DE AUDITORIA IMUTÁVEL CENTRAL.
    /// Registro append-only único da plataforma: antes → depois + usuário + IP + timestamp,
    /// para todo agregado auditável. Substitui as tabelas *_historico fragmentadas por módulo
    /// (que dependiam de o handler "lembrar" de gravar). Ver DECISOES-TRANSVERSAIS.md · T8.
    ///
    /// Imutável POR CONSTRUÇÃO: sem setters públicos e sem mutadores — não deriva de
    /// <see cref="EntidadeSaaSBase"/> de propósito (não há soft-delete nem MarcarAlterado;
    /// um registro de auditoria nunca é alterado nem apagado). O isolamento por tenant é
    /// aplicado explicitamente pelo serviço de auditoria (filtra por <see cref="TenantId"/>).
    /// </summary>
    public class RegistroAuditoria
    {
        public Guid Id { get; private set; }
        public string TenantId { get; private set; } = string.Empty;

        /// <summary>Tipo/entidade auditada (ex.: "Venda", "Pessoa", "DocumentoFiscal").</summary>
        public string Entidade { get; private set; } = string.Empty;

        /// <summary>Identificador da instância auditada (chave natural ou Guid em texto).</summary>
        public string EntidadeId { get; private set; } = string.Empty;

        /// <summary>Ação (ex.: "Criado", "Alterado", "Cancelado", "Anonimizado").</summary>
        public string Acao { get; private set; } = string.Empty;

        /// <summary>Snapshot serializado (JSON) do estado ANTERIOR (null em criação).</summary>
        public string? ValoresAntes { get; private set; }

        /// <summary>Snapshot serializado (JSON) do estado POSTERIOR (null em exclusão).</summary>
        public string? ValoresDepois { get; private set; }

        /// <summary>Usuário responsável (id/login) — PII sensível já deve chegar mascarada (T1).</summary>
        public string? Usuario { get; private set; }

        public string? IpOrigem { get; private set; }
        public DateTime OcorridoEm { get; private set; }

        protected RegistroAuditoria() { } // EF Core

        private RegistroAuditoria(
            string tenantId, string entidade, string entidadeId, string acao,
            string? valoresAntes, string? valoresDepois, string? usuario, string? ipOrigem)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId ?? string.Empty;
            Entidade = entidade ?? string.Empty;
            EntidadeId = entidadeId ?? string.Empty;
            Acao = acao ?? string.Empty;
            ValoresAntes = valoresAntes;
            ValoresDepois = valoresDepois;
            Usuario = usuario;
            IpOrigem = ipOrigem;
            OcorridoEm = DateTime.UtcNow;
        }

        /// <summary>Fábrica: única forma de criar um registro. Depois de criado, é imutável.</summary>
        public static RegistroAuditoria Criar(
            string tenantId, string entidade, string entidadeId, string acao,
            string? valoresAntes = null, string? valoresDepois = null,
            string? usuario = null, string? ipOrigem = null) =>
            new(tenantId, entidade, entidadeId, acao, valoresAntes, valoresDepois, usuario, ipOrigem);
    }
}
