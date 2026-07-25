using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    // Join N:N legado (NcmTributacao.Empresas). Empresa vive em outro módulo:
    // vínculo restaurado por Guid EmpresaId, sem navegação cross-module.
    public class NcmTributacaoEmpresa : EntidadeSaaSBase
    {
        public Guid NcmTributacaoId { get; private set; }
        public Guid EmpresaId { get; private set; }

        protected NcmTributacaoEmpresa() { } // EF Core

        public NcmTributacaoEmpresa(Guid ncmTributacaoId, Guid empresaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (ncmTributacaoId == Guid.Empty)
                AddNotification(nameof(NcmTributacaoId), "O ID da tributação NCM é obrigatório [Origem: NcmTributacaoEmpresa]");
            if (empresaId == Guid.Empty)
                AddNotification(nameof(EmpresaId), "O campo EmpresaId é obrigatório [Origem: NcmTributacaoEmpresa]");

            NcmTributacaoId = ncmTributacaoId;
            EmpresaId = empresaId;
        }
    }
}
