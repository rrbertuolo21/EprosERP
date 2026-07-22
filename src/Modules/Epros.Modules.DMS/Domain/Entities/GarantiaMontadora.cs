using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class GarantiaMontadora : EntidadeSaaSBase
    {
        public Guid OrdemServicoDmsId { get; private set; }
        public string Chassi { get; private set; } = string.Empty;
        public string PecaReclamada { get; private set; } = string.Empty;
        public decimal ValorReclamado { get; private set; }
        public string Status { get; private set; } = "Solicitada"; // Solicitada, Aprovada, Rejeitada
        public string? ParecerMontadora { get; private set; }

        protected GarantiaMontadora() { } // EF Core

        public GarantiaMontadora(
            Guid ordemServicoDmsId,
            string chassi,
            string pecaReclamada,
            decimal valorReclamado,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<GarantiaMontadora>()
                .Requires()
                .AreNotEquals(ordemServicoDmsId, Guid.Empty, nameof(OrdemServicoDmsId), "O ID da ordem de serviço é obrigatório.")
                .IsNotNullOrEmpty(chassi, nameof(Chassi), "O número do chassi é obrigatório.")
                .AreEquals(chassi?.Length ?? 0, 17, nameof(Chassi), "O chassi deve conter exatamente 17 caracteres.")
                .IsNotNullOrEmpty(pecaReclamada, nameof(PecaReclamada), "A identificação da peça reclamada é obrigatória.")
                .IsGreaterThan(valorReclamado, 0, nameof(ValorReclamado), "O valor de reembolso reclamado deve ser maior que zero.")
            );

            OrdemServicoDmsId = ordemServicoDmsId;
            Chassi = chassi?.ToUpperInvariant() ?? string.Empty;
            PecaReclamada = pecaReclamada;
            ValorReclamado = valorReclamado;
            Status = "Solicitada";
        }

        public void Julgar(string novoStatus, string parecer, string usuario)
        {
            if (Status != "Solicitada")
            {
                AddNotification(nameof(Status), "Essa solicitação de garantia já foi julgada anteriormente.");
                return;
            }

            if (novoStatus != "Aprovada" && novoStatus != "Rejeitada")
            {
                AddNotification(nameof(Status), "O status de julgamento da garantia deve ser 'Aprovada' ou 'Rejeitada'.");
                return;
            }

            Status = novoStatus;
            ParecerMontadora = parecer;
            MarcarAlterado(usuario);
        }
    }
}
