using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Acao corretiva de tratamento de incidente/risco (EF GESTAO_AMBIENTAL_EHS 11.6).</summary>
    public class AcaoCorretiva : EntidadeSaaSBase
    {
        public Guid? IncidenteId { get; private set; }
        public Guid? FatorRiscoId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string? Causa { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public DateTime? Prazo { get; private set; }
        public string Status { get; private set; } = "Aberta"; // Aberta, EmTratamento, Concluida, Cancelada
        public DateTime? DataConclusao { get; private set; }
        public string? Eficacia { get; private set; }
        public Guid? EvidenciaArquivoId { get; private set; }

        protected AcaoCorretiva() { } // EF Core

        public AcaoCorretiva(
            Guid? incidenteId,
            Guid? fatorRiscoId,
            string descricao,
            string? causa,
            Guid responsavelId,
            DateTime? prazo,
            Guid? evidenciaArquivoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            IncidenteId = incidenteId;
            FatorRiscoId = fatorRiscoId;
            Descricao = descricao;
            Causa = causa;
            ResponsavelId = responsavelId;
            Prazo = prazo?.Date;
            EvidenciaArquivoId = evidenciaArquivoId;
            Status = "Aberta";
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<AcaoCorretiva>()
                .Requires()
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao da acao e obrigatoria. [Origem: AcaoCorretiva]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: AcaoCorretiva]")
                .IsTrue(IncidenteId.HasValue || FatorRiscoId.HasValue, nameof(IncidenteId),
                    "A acao deve referenciar um incidente ou um fator de risco. [Origem: AcaoCorretiva]"));
        }

        public void Concluir(string eficacia, string usuario)
        {
            if (Status == "Cancelada")
            {
                AddNotification(nameof(Status), "Acao cancelada nao pode ser concluida.");
                return;
            }
            Status = "Concluida";
            Eficacia = eficacia;
            DataConclusao = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }
}
