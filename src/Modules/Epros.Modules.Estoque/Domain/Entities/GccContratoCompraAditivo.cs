using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Aditivo contratual (EF Gestão de Contratos de Compra §16.3 `gcc_contrato_compra_aditivo`).
    /// GCC-009: alteração com impacto financeiro passa por workflow (integração externa; ver pendências).
    /// GCC-011: mantém auditoria antes/depois. Modelo proposto por autoria (§22).
    /// </summary>
    public class GccContratoCompraAditivo : EntidadeSaaSBase
    {
        public Guid ContratoCompraId { get; private set; }
        public string? NumeroAditivo { get; private set; }
        public ETipoAditivoContrato TipoAditivo { get; private set; }
        public string? Justificativa { get; private set; }
        public DateTime? DataAditivo { get; private set; }
        public DateTime? AprovadoEm { get; private set; }
        public Guid? AprovadoPor { get; private set; }

        // Navegação intra-módulo
        public GccContratoCompra? Contrato { get; private set; }

        protected GccContratoCompraAditivo() { } // EF Core

        public GccContratoCompraAditivo(Guid contratoCompraId, string? numeroAditivo, ETipoAditivoContrato tipoAditivo, string? justificativa, DateTime? dataAditivo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContratoCompraId = contratoCompraId;
            NumeroAditivo = numeroAditivo;
            TipoAditivo = tipoAditivo;
            Justificativa = justificativa;
            DataAditivo = dataAditivo ?? DateTime.UtcNow;
        }

        /// <summary>Aprova o aditivo, aplicando a alteração ao contrato — EF §10.3 / GCC-009.</summary>
        public void Aprovar(Guid aprovadoPor, string usuario)
        {
            AprovadoEm = DateTime.UtcNow;
            AprovadoPor = aprovadoPor;
            MarcarAlterado(usuario);
        }
    }
}
