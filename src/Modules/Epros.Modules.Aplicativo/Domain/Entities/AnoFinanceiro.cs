using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class AnoFinanceiro : EntidadeSaaSBase
    {
        public DateTime FromDate { get; private set; }
        public DateTime ToDate { get; private set; }
        public bool IsActive { get; private set; }

        protected int FinancialYearId { get; private set; } // Legacy compatibility

        protected AnoFinanceiro() { } // EF Core

        public AnoFinanceiro(
            string tenantId,
            DateTime fromDate,
            DateTime toDate,
            bool isActive,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (fromDate >= toDate)
                AddNotification(nameof(FromDate), "A data de início deve ser menor que a data de término.");

            FromDate = fromDate;
            ToDate = toDate;
            IsActive = isActive;
            FinancialYearId = 0; // default initial setting
        }

        public void Ativar(string alteradoPor)
        {
            IsActive = true;
            MarcarAlterado(alteradoPor);
        }

        public void Desativar(string alteradoPor)
        {
            IsActive = false;
            MarcarAlterado(alteradoPor);
        }
    }
}
