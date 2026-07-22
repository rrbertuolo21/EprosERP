using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaNfeIntermediador (intermediador/marketplace da NF-e).
    /// FK long -> Guid; VO Documento -> string; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaNfeIntermediador : EntidadeSaaSBase
    {
        public Guid VendaNfeId { get; private set; }
        public string Documento { get; private set; } = string.Empty;
        public string? IdentificadorIntermediador { get; private set; }

        // Navegação intra-módulo
        public VendaNfe VendaNfe { get; private set; } = null!;

        protected VendaNfeIntermediador() { } // EF Core

        public VendaNfeIntermediador(Guid vendaNfeId, string documento, string? identificadorIntermediador, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendaNfeId = vendaNfeId;
            Documento = documento;
            IdentificadorIntermediador = identificadorIntermediador;
            Validar();
        }

        public void Alterar(string documento, string? identificadorIntermediador, string alteradoPor)
        {
            Documento = documento;
            IdentificadorIntermediador = identificadorIntermediador;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaNfeIntermediador.Validar (sem regras no legado).</summary>
        public void Validar()
        {
        }
    }
}
