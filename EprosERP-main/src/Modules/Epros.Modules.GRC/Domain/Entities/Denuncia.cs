using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    public class Denuncia : EntidadeSaaSBase
    {
        public string CodigoAcompanhamento { get; private set; } = string.Empty;
        public string Relato { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Recebida"; // Recebida, EmAnalise, Procedente, Improcedente
        public DateTime DataRegistro { get; private set; }
        public string? ParecerFinal { get; private set; }

        protected Denuncia() { } // EF Core

        public Denuncia(
            string relato,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Denuncia>()
                .Requires()
                .IsNotNullOrEmpty(relato, nameof(Relato), "O relato detalhado da denúncia é obrigatório.")
            );

            Relato = relato;
            Status = "Recebida";
            DataRegistro = DateTime.UtcNow;
            // Gera um código de acompanhamento aleatório e curto
            CodigoAcompanhamento = $"DEN-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        public void IniciarAnalise(string usuario)
        {
            if (Status != "Recebida")
            {
                AddNotification(nameof(Status), "A denúncia já está em análise ou já foi julgada.");
                return;
            }

            Status = "EmAnalise";
            MarcarAlterado(usuario);
        }

        public void Julgar(string statusFinal, string parecer, string usuario)
        {
            if (Status != "Recebida" && Status != "EmAnalise")
            {
                AddNotification(nameof(Status), "Apenas denúncias recebidas ou em análise podem ser julgadas.");
                return;
            }

            if (statusFinal != "Procedente" && statusFinal != "Improcedente")
            {
                AddNotification(nameof(Status), "Julgamento final deve ser 'Procedente' ou 'Improcedente'.");
                return;
            }

            if (string.IsNullOrWhiteSpace(parecer))
            {
                AddNotification(nameof(ParecerFinal), "O parecer final do julgamento é obrigatório.");
                return;
            }

            Status = statusFinal;
            ParecerFinal = parecer;
            MarcarAlterado(usuario);
        }
    }
}
