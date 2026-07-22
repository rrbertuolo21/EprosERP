using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class CodigoBeneficioFiscalCst : EntidadeSaaSBase
    {
        public Guid CodigoBeneficioFiscalId { get; private set; }
        public ECodigoSituacaoTributariaIcms Cst { get; private set; }

        protected CodigoBeneficioFiscalCst() { } // EF Core

        public CodigoBeneficioFiscalCst(
            Guid codigoBeneficioFiscalId,
            ECodigoSituacaoTributariaIcms cst,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            CodigoBeneficioFiscalId = codigoBeneficioFiscalId;
            Cst = cst;
        }

        public void Alterar(ECodigoSituacaoTributariaIcms cst, string alteradoPor)
        {
            Cst = cst;
            MarcarAlterado(alteradoPor);
        }
    }
}
