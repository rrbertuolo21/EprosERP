using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaNfeExportacao (dados de exportação da NF-e).
    /// FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaNfeExportacao : EntidadeSaaSBase
    {
        public Guid VendaNfeId { get; private set; }
        public EEstado UfSaidaPais { get; private set; }
        public string LocalExportacao { get; private set; } = string.Empty;
        public string? LocalDespacho { get; private set; }

        // Navegação intra-módulo
        public VendaNfe VendaNfe { get; private set; } = null!;

        protected VendaNfeExportacao() { } // EF Core

        public VendaNfeExportacao(Guid vendaNfeId, EEstado ufSaidaPais, string localExportacao, string? localDespacho, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendaNfeId = vendaNfeId;
            UfSaidaPais = ufSaidaPais;
            LocalExportacao = localExportacao;
            LocalDespacho = localDespacho;
        }

        public void Alterar(EEstado ufSaidaPais, string localExportacao, string? localDespacho, string alteradoPor)
        {
            UfSaidaPais = ufSaidaPais;
            LocalExportacao = localExportacao;
            LocalDespacho = localDespacho;
            MarcarAlterado(alteradoPor);
        }
    }
}
