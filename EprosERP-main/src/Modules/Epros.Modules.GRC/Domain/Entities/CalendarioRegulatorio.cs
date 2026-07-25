using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-REG — Calendario regulatorio (grc_reg_calendario). Controla vencimentos, alertas
    /// e SLA de obrigacoes/certificados. Fiel a EF_13_GRC_COMPLIANCE_REGULATORIO_V1 (secao 10.2/10.4).
    /// </summary>
    public class CalendarioRegulatorio : EntidadeSaaSBase
    {
        public Guid? CertificadoId { get; private set; }
        public Guid? RegistroId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public DateTime DataVencimento { get; private set; }
        // Pendente, Alertado, Vencido, Encerrado, Cancelado
        public string Status { get; private set; } = "Pendente";

        protected CalendarioRegulatorio() { } // EF Core

        public CalendarioRegulatorio(
            Guid? certificadoId,
            Guid? registroId,
            string descricao,
            DateTime dataVencimento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<CalendarioRegulatorio>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do vencimento e obrigatoria.")
                .IsTrue(certificadoId != null || registroId != null, nameof(CertificadoId),
                    "O vencimento deve estar vinculado a um certificado ou registro regulatorio.")
            );

            CertificadoId = certificadoId;
            RegistroId = registroId;
            Descricao = descricao;
            DataVencimento = dataVencimento;
            Status = "Pendente";
        }

        public void Alertar(string usuario)
        {
            if (Status != "Pendente")
            {
                AddNotification(nameof(Status), "Somente vencimentos pendentes podem ser alertados.");
                return;
            }
            Status = "Alertado";
            MarcarAlterado(usuario);
        }

        public void MarcarVencido(string usuario)
        {
            if (Status == "Encerrado" || Status == "Cancelado")
            {
                AddNotification(nameof(Status), "Vencimento ja encerrado ou cancelado.");
                return;
            }
            Status = "Vencido";
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            Status = "Encerrado";
            MarcarAlterado(usuario);
        }
    }
}
