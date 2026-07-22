using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class CodigoBeneficioFiscalCsosn : EntidadeSaaSBase
    {
        public Guid CodigoBeneficioFiscalId { get; private set; }
        public ECodigoSituacaoOperacaoSimplesNacional Csosn { get; private set; }

        protected CodigoBeneficioFiscalCsosn() { } // EF Core

        public CodigoBeneficioFiscalCsosn(
            Guid codigoBeneficioFiscalId,
            ECodigoSituacaoOperacaoSimplesNacional csosn,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            CodigoBeneficioFiscalId = codigoBeneficioFiscalId;
            Csosn = csosn;
        }

        public void Alterar(ECodigoSituacaoOperacaoSimplesNacional csosn, string alteradoPor)
        {
            Csosn = csosn;
            MarcarAlterado(alteradoPor);
        }
    }
}
