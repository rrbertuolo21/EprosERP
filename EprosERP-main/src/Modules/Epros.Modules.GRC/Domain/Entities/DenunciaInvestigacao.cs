using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-DEN — Investigacao (grc_den_investigacao). Controla o ciclo investigativo, equipe,
    /// SLA e conclusao proposta. Fiel a EF_13_GRC_INVESTIGACOES_E_DENUNCIAS_V1 (secao 11.5).
    /// RN-DEN-006: investigador nao pode ser denunciado nem beneficiario (validado no handler).
    /// </summary>
    public class DenunciaInvestigacao : EntidadeSaaSBase
    {
        public Guid DenunciaId { get; private set; }
        public Guid InvestigadorId { get; private set; }
        // Aberta, EmAndamento, Concluida, Cancelada
        public string Status { get; private set; } = "Aberta";
        public DateTime? DataInicio { get; private set; }
        public DateTime? PrazoSla { get; private set; }
        public string? ConclusaoProposta { get; private set; }
        public DateTime? DataConclusao { get; private set; }

        protected DenunciaInvestigacao() { } // EF Core

        public DenunciaInvestigacao(
            Guid denunciaId,
            Guid investigadorId,
            DateTime? prazoSla,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DenunciaInvestigacao>()
                .Requires()
                .IsTrue(denunciaId != Guid.Empty, nameof(DenunciaId), "A denúncia da investigação é obrigatória.")
                .IsTrue(investigadorId != Guid.Empty, nameof(InvestigadorId), "O investigador é obrigatório.")
            );

            DenunciaId = denunciaId;
            InvestigadorId = investigadorId;
            PrazoSla = prazoSla;
            DataInicio = DateTime.UtcNow;
            Status = "EmAndamento";
        }

        /// <summary>Registra a conclusao proposta da apuracao.</summary>
        public void Concluir(string conclusaoProposta, DateTime dataConclusao, string usuario)
        {
            if (Status == "Concluida" || Status == "Cancelada")
            {
                AddNotification(nameof(Status), "A investigação já está concluída ou cancelada.");
                return;
            }
            if (string.IsNullOrWhiteSpace(conclusaoProposta))
            {
                AddNotification(nameof(ConclusaoProposta), "A conclusão proposta é obrigatória.");
                return;
            }
            if (dataConclusao == default || dataConclusao == DateTime.MinValue)
            {
                AddNotification(nameof(DataConclusao), "A data de conclusão deve ser válida.");
                return;
            }

            ConclusaoProposta = conclusaoProposta;
            DataConclusao = dataConclusao;
            Status = "Concluida";
            MarcarAlterado(usuario);
        }

        public void Cancelar(string motivo, string usuario)
        {
            if (Status == "Concluida")
            {
                AddNotification(nameof(Status), "Investigação concluída não pode ser cancelada.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(Status), "O motivo do cancelamento é obrigatório.");
                return;
            }
            ConclusaoProposta = motivo;
            Status = "Cancelada";
            MarcarAlterado(usuario);
        }
    }
}
