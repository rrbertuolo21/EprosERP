using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class ContratoRede : EntidadeSaaSBase
    {
        public Guid? RedeNoId { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public DateTime Inicio { get; private set; }
        public DateTime Fim { get; private set; }
        public string Status { get; private set; } = "Ativo";

        protected ContratoRede() { } // EF Core

        public ContratoRede(
            Guid? redeNoId,
            string tipo,
            DateTime inicio,
            DateTime fim,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ContratoRede>()
                .Requires()
                .IsNotNullOrEmpty(tipo, nameof(Tipo), "O tipo do contrato é obrigatório.")
            );

            if (fim <= inicio)
            {
                AddNotification(nameof(Fim), "A data de término deve ser posterior ao início.");
            }

            RedeNoId = redeNoId;
            Tipo = tipo;
            Inicio = inicio;
            Fim = fim;
            Status = "Ativo";
        }

        public void Encerrar(string usuario)
        {
            Status = "Encerrado";
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            Status = "Cancelado";
            MarcarAlterado(usuario);
        }
    }
}
