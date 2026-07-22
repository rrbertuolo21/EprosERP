using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class NcmConfiguracao : EntidadeSaaSBase
    {
        public Guid NcmId { get; private set; }
        public Guid NcmTributacaoId { get; private set; }

        protected NcmConfiguracao() { } // EF Core

        public NcmConfiguracao(Guid ncmId, Guid ncmTributacaoId, string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            NcmId = ncmId;
            NcmTributacaoId = ncmTributacaoId;
            Validar();
        }

        public void Validar()
        {
            Clear();
        }

        public void Alterar(Guid ncmId, string alteradoPor)
        {
            NcmId = ncmId;
            MarcarAlterado(alteradoPor);
            Validar();
        }
    }
}
