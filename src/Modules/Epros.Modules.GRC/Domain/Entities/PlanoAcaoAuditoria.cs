using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-CIA — Plano de acao de remediacao (grc_cia_plano_acao). Trata um achado com
    /// responsavel, prazo e evidencia. Fiel a EF_13_GRC_CONTROLES_INTERNOS_E_AUDITORIA_V1 (secoes 10.2 e 12).
    /// </summary>
    public class PlanoAcaoAuditoria : EntidadeSaaSBase
    {
        public Guid AchadoId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public Guid ResponsavelId { get; private set; }
        public DateTime Prazo { get; private set; }
        // Aberta, EmAndamento, Concluida, Atrasada, Cancelada, Rejeitada, Encerrada
        public string Status { get; private set; } = "Aberta";

        protected PlanoAcaoAuditoria() { } // EF Core

        public PlanoAcaoAuditoria(
            Guid achadoId,
            string descricao,
            Guid responsavelId,
            DateTime prazo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PlanoAcaoAuditoria>()
                .Requires()
                .IsTrue(achadoId != Guid.Empty, nameof(AchadoId), "O achado do plano de acao e obrigatorio.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do plano de acao e obrigatoria.")
                .IsTrue(responsavelId != Guid.Empty, nameof(ResponsavelId), "O responsavel pelo plano de acao e obrigatorio.")
            );

            AchadoId = achadoId;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            Prazo = prazo;
            Status = "Aberta";
        }

        public void Iniciar(string usuario)
        {
            if (Status != "Aberta")
            {
                AddNotification(nameof(Status), "Somente acoes abertas podem ser iniciadas.");
                return;
            }
            Status = "EmAndamento";
            MarcarAlterado(usuario);
        }

        public void Concluir(string usuario)
        {
            if (Status != "EmAndamento" && Status != "Atrasada")
            {
                AddNotification(nameof(Status), "Somente acoes em andamento ou atrasadas podem ser concluidas.");
                return;
            }
            Status = "Concluida";
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            if (Status == "Concluida" || Status == "Encerrada")
            {
                AddNotification(nameof(Status), "Nao e possivel cancelar uma acao concluida ou encerrada.");
                return;
            }
            Status = "Cancelada";
            MarcarAlterado(usuario);
        }
    }
}
