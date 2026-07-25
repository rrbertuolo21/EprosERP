using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class OportunidadeConcessionaria : EntidadeSaaSBase
    {
        public Guid ProspectId { get; private set; }
        public string Etapa { get; private set; } = "Visita";
        public decimal? ValorEstimado { get; private set; }
        public decimal? Probabilidade { get; private set; }
        public Guid? VendaId { get; private set; }
        public Guid? MotivoPerdaId { get; private set; }

        protected OportunidadeConcessionaria() { } // EF Core

        public OportunidadeConcessionaria(
            Guid prospectId,
            decimal? valorEstimado,
            decimal? probabilidade,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<OportunidadeConcessionaria>()
                .Requires()
                .AreNotEquals(prospectId, Guid.Empty, nameof(ProspectId), "O prospect é obrigatório.")
            );

            ProspectId = prospectId;
            ValorEstimado = valorEstimado;
            Probabilidade = probabilidade;
            Etapa = "Visita";
        }

        public void AvancarEtapa(string novaEtapa, string usuario)
        {
            Etapa = novaEtapa;
            MarcarAlterado(usuario);
        }

        public void RegistrarPerda(Guid motivoPerdaId, string usuario)
        {
            if (motivoPerdaId == Guid.Empty)
            {
                AddNotification(nameof(MotivoPerdaId), "Motivo de perda é obrigatório.");
                return;
            }

            MotivoPerdaId = motivoPerdaId;
            Etapa = "Perdida";
            MarcarAlterado(usuario);
        }

        public void Converter(Guid vendaId, string usuario)
        {
            if (VendaId != null)
            {
                AddNotification(nameof(VendaId), "Oportunidade já convertida.");
                return;
            }

            VendaId = vendaId;
            Etapa = "Convertida";
            MarcarAlterado(usuario);
        }
    }
}
