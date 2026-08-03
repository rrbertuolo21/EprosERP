using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-CIA — Amostra de teste de controle (grc_cia_amostra). D-CIA-02: método híbrido
    /// (Estatistico/Dirigido/Censo) com JUSTIFICATIVA obrigatória (RN-CIA-001). Gap genuíno da EF
    /// que o código não tinha (EF×código #8).
    /// </summary>
    public class AmostraAuditoria : EntidadeSaaSBase
    {
        public Guid? TesteControleId { get; private set; }
        public Guid? PlanoAuditoriaId { get; private set; }
        // Estatistico, Dirigido, Censo
        public string Metodo { get; private set; } = "Dirigido";
        public int Tamanho { get; private set; }
        public string? Criterio { get; private set; }
        public string Justificativa { get; private set; } = string.Empty;

        protected AmostraAuditoria() { } // EF Core

        public AmostraAuditoria(
            Guid? testeControleId,
            Guid? planoAuditoriaId,
            string metodo,
            int tamanho,
            string? criterio,
            string justificativa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AmostraAuditoria>()
                .Requires()
                .IsTrue(metodo == "Estatistico" || metodo == "Dirigido" || metodo == "Censo", nameof(Metodo),
                    "O metodo deve ser 'Estatistico', 'Dirigido' ou 'Censo'.")
                .IsTrue(tamanho > 0, nameof(Tamanho), "O tamanho da amostra deve ser positivo.")
                // D-CIA-02 — justificativa obrigatória do método de amostragem.
                .IsNotNullOrEmpty(justificativa, nameof(Justificativa), "A justificativa do metodo de amostragem e obrigatoria.")
                .IsTrue(testeControleId != null || planoAuditoriaId != null, nameof(TesteControleId),
                    "A amostra deve estar vinculada a um teste de controle ou plano de auditoria.")
            );

            TesteControleId = testeControleId;
            PlanoAuditoriaId = planoAuditoriaId;
            Metodo = metodo;
            Tamanho = tamanho;
            Criterio = criterio;
            Justificativa = justificativa;
        }
    }
}
