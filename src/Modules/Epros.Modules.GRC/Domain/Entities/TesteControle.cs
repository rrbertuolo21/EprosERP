using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-CIA — Teste de controle (grc_cia_teste_controle). Executa teste e registra resultado
    /// sobre um ControleInterno. Fiel a EF_13_GRC_CONTROLES_INTERNOS_E_AUDITORIA_V1 (secoes 10.2, 10.4 e 12).
    /// </summary>
    public class TesteControle : EntidadeSaaSBase
    {
        public Guid ControleId { get; private set; }
        public Guid? PlanoAuditoriaId { get; private set; }
        public DateTime DataTeste { get; private set; }
        // Conforme, NaoConforme, NaoAplicavel
        public string Resultado { get; private set; } = "NaoAplicavel";
        // Rascunho, Concluido, Reaberto
        public string Status { get; private set; } = "Rascunho";
        public string? Observacao { get; private set; }
        public string? MotivoReabertura { get; private set; }

        protected TesteControle() { } // EF Core

        public TesteControle(
            Guid controleId,
            Guid? planoAuditoriaId,
            string resultado,
            string? observacao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<TesteControle>()
                .Requires()
                .IsTrue(controleId != Guid.Empty, nameof(ControleId), "O controle do teste e obrigatorio.")
                .IsTrue(resultado == "Conforme" || resultado == "NaoConforme" || resultado == "NaoAplicavel",
                    nameof(Resultado), "O resultado deve ser 'Conforme', 'NaoConforme' ou 'NaoAplicavel'.")
            );

            ControleId = controleId;
            PlanoAuditoriaId = planoAuditoriaId;
            Resultado = resultado;
            Observacao = observacao;
            DataTeste = DateTime.UtcNow;
            Status = "Rascunho";
        }

        public void Concluir(string usuario)
        {
            if (Status != "Rascunho" && Status != "Reaberto")
            {
                AddNotification(nameof(Status), "Somente testes em rascunho ou reabertos podem ser concluidos.");
                return;
            }
            Status = "Concluido";
            MarcarAlterado(usuario);
        }

        public void Reabrir(string motivo, string usuario)
        {
            if (Status != "Concluido")
            {
                AddNotification(nameof(Status), "Somente testes concluidos podem ser reabertos.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoReabertura), "O motivo da reabertura e obrigatorio.");
                return;
            }
            Status = "Reaberto";
            MotivoReabertura = motivo;
            MarcarAlterado(usuario);
        }

        /// <summary>Regra funcional: teste NaoConforme habilita geracao de achado.</summary>
        public bool ExigeAchado() => Resultado == "NaoConforme";
    }
}
