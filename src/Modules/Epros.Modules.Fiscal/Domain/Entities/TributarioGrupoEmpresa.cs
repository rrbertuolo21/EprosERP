using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    // Join N:N legado (TributarioGrupo.Empresas). Empresa vive em outro módulo:
    // vínculo restaurado por Guid EmpresaId, sem navegação cross-module.
    public class TributarioGrupoEmpresa : EntidadeSaaSBase
    {
        public Guid TributarioGrupoId { get; private set; }
        public Guid EmpresaId { get; private set; }

        protected TributarioGrupoEmpresa() { } // EF Core

        public TributarioGrupoEmpresa(Guid tributarioGrupoId, Guid empresaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (tributarioGrupoId == Guid.Empty)
                AddNotification(nameof(TributarioGrupoId), "O ID do grupo tributário é obrigatório [Origem: TributarioGrupoEmpresa]");
            if (empresaId == Guid.Empty)
                AddNotification(nameof(EmpresaId), "O campo EmpresaId é obrigatório [Origem: TributarioGrupoEmpresa]");

            TributarioGrupoId = tributarioGrupoId;
            EmpresaId = empresaId;
        }
    }
}
