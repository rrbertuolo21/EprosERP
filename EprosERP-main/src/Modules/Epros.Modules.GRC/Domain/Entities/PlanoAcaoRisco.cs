using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-RIS — Plano de acao do risco (grc_ris_plano_acao). Controla acoes de tratamento
    /// do risco. Fiel a EF_13_GRC_GESTAO_DE_RISCOS_CORPORATIVOS_V1 (secao 10.2).
    /// </summary>
    public class PlanoAcaoRisco : EntidadeSaaSBase
    {
        public Guid RiscoId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public Guid ResponsavelId { get; private set; }
        public DateTime Prazo { get; private set; }
        // Aberta, EmAndamento, Concluida, Atrasada, Cancelada
        public string Status { get; private set; } = "Aberta";

        protected PlanoAcaoRisco() { } // EF Core

        public PlanoAcaoRisco(
            Guid riscoId,
            string descricao,
            Guid responsavelId,
            DateTime prazo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PlanoAcaoRisco>()
                .Requires()
                .IsTrue(riscoId != Guid.Empty, nameof(RiscoId), "O risco do plano de acao e obrigatorio.")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descricao do plano de acao e obrigatoria.")
                .IsTrue(responsavelId != Guid.Empty, nameof(ResponsavelId), "O responsavel pelo plano de acao e obrigatorio.")
            );

            RiscoId = riscoId;
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
            if (Status == "Concluida")
            {
                AddNotification(nameof(Status), "Nao e possivel cancelar uma acao ja concluida.");
                return;
            }
            Status = "Cancelada";
            MarcarAlterado(usuario);
        }
    }
}
