using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    public class RelatorioESG : EntidadeSaaSBase
    {
        public int AnoFiscal { get; private set; }
        public string NomeRelatorio { get; private set; } = string.Empty;
        public decimal TotalEscopo1 { get; private set; }
        public decimal TotalEscopo2 { get; private set; }
        public decimal TotalEscopo3 { get; private set; }
        public decimal TotalGeralCo2e { get; private set; }
        public string Status { get; private set; } = "Rascunho"; // Rascunho, Auditado, Publicado
        public string? ParecerAuditoria { get; private set; }

        protected RelatorioESG() { } // EF Core

        public RelatorioESG(
            int anoFiscal,
            string nomeRelatorio,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<RelatorioESG>()
                .Requires()
                .IsGreaterThan(anoFiscal, 1999, nameof(AnoFiscal), "Ano fiscal inválido.")
                .IsNotNullOrEmpty(nomeRelatorio, nameof(NomeRelatorio), "O nome do relatório é obrigatório.")
            );

            AnoFiscal = anoFiscal;
            NomeRelatorio = nomeRelatorio;
            TotalEscopo1 = 0;
            TotalEscopo2 = 0;
            TotalEscopo3 = 0;
            TotalGeralCo2e = 0;
            Status = "Rascunho";
        }

        public void ConsolidarValores(decimal escopo1, decimal escopo2, decimal escopo3, string usuario)
        {
            if (Status == "Publicado")
            {
                AddNotification(nameof(Status), "Não é possível consolidar novos valores em um relatório já publicado.");
                return;
            }

            TotalEscopo1 = escopo1;
            TotalEscopo2 = escopo2;
            TotalEscopo3 = escopo3;
            TotalGeralCo2e = escopo1 + escopo2 + escopo3;
            MarcarAlterado(usuario);
        }

        public void Auditar(string parecer, string usuario)
        {
            if (Status != "Rascunho")
            {
                AddNotification(nameof(Status), "Apenas relatórios em rascunho podem ser auditados.");
                return;
            }

            if (string.IsNullOrWhiteSpace(parecer))
            {
                AddNotification(nameof(ParecerAuditoria), "O parecer de auditoria é obrigatório.");
                return;
            }

            ParecerAuditoria = parecer;
            Status = "Auditado";
            MarcarAlterado(usuario);
        }

        public void Publicar(string usuario)
        {
            if (Status != "Auditado")
            {
                AddNotification(nameof(Status), "Apenas relatórios previamente auditados podem ser publicados.");
                return;
            }

            Status = "Publicado";
            MarcarAlterado(usuario);
        }
    }
}
