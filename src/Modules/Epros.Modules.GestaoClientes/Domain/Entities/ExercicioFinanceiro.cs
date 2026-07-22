using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class ExercicioFinanceiro : EntidadeSaaSBase
    {
        public DateTime FromDate { get; private set; }
        public DateTime ToDate { get; private set; }
        public string? FiscalYear { get; private set; }
        public string Status { get; private set; } = "Aberto";

        protected ExercicioFinanceiro() { } // EF Core

        public ExercicioFinanceiro(DateTime fromDate, DateTime toDate, string? fiscalYear, string status, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ExercicioFinanceiro>()
                .Requires()
                .IsNotNullOrEmpty(status, nameof(Status), "Status é obrigatório.")
                .IsTrue(toDate > fromDate, nameof(ToDate), "Data final deve ser maior que a data inicial.")
            );

            if (fiscalYear != null)
            {
                AddNotifications(new Contract<ExercicioFinanceiro>()
                    .Requires()
                    .HasMaxLen(fiscalYear, 10, nameof(FiscalYear), "FiscalYear deve ter no máximo 10 caracteres.")
                );
            }

            FromDate = fromDate;
            ToDate = toDate;
            FiscalYear = fiscalYear;
            Status = status;
        }

        public void Fechar(string alteradoPor)
        {
            Status = "Fechado";
            MarcarAlterado(alteradoPor);
        }

        public void Reabrir(string alteradoPor)
        {
            Status = "Aberto";
            MarcarAlterado(alteradoPor);
        }

        public void AtualizarPeriodo(DateTime fromDate, DateTime toDate, string? fiscalYear, string alteradoPor)
        {
            AddNotifications(new Contract<ExercicioFinanceiro>()
                .Requires()
                .IsTrue(toDate > fromDate, nameof(ToDate), "Data final deve ser maior que a data inicial.")
            );

            if (fiscalYear != null)
            {
                AddNotifications(new Contract<ExercicioFinanceiro>()
                    .Requires()
                    .HasMaxLen(fiscalYear, 10, nameof(FiscalYear), "FiscalYear deve ter no máximo 10 caracteres.")
                );
            }

            if (IsValid)
            {
                FromDate = fromDate;
                ToDate = toDate;
                FiscalYear = fiscalYear;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
