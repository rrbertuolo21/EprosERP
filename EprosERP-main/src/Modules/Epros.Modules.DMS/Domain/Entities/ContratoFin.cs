using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class ContratoFin : EntidadeSaaSBase
    {
        public Guid? PropostaId { get; private set; }
        public Guid VendaId { get; private set; }
        public string NumeroContrato { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Ativo";
        public string? CondicaoFinalJson { get; private set; }

        protected ContratoFin() { } // EF Core

        public ContratoFin(
            Guid? propostaId,
            Guid vendaId,
            string numeroContrato,
            string? condicaoFinalJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ContratoFin>()
                .Requires()
                .AreNotEquals(vendaId, Guid.Empty, nameof(VendaId), "A venda é obrigatória.")
                .IsNotNullOrEmpty(numeroContrato, nameof(NumeroContrato), "O número do contrato é obrigatório.")
            );

            PropostaId = propostaId;
            VendaId = vendaId;
            NumeroContrato = numeroContrato;
            CondicaoFinalJson = condicaoFinalJson;
            Status = "Ativo";
        }

        public void Liquidar(string usuario)
        {
            Status = "Liquidado";
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            Status = "Cancelado";
            MarcarAlterado(usuario);
        }
    }
}
