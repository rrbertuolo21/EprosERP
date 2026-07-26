using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class JornadaFin : EntidadeSaaSBase
    {
        public Guid OportunidadeId { get; private set; }
        public Guid? VendaId { get; private set; }
        public Guid ClienteId { get; private set; }
        public Guid VeiculoId { get; private set; }
        public string Status { get; private set; } = "Aberta";

        protected JornadaFin() { } // EF Core

        public JornadaFin(
            Guid oportunidadeId,
            Guid? vendaId,
            Guid clienteId,
            Guid veiculoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<JornadaFin>()
                .Requires()
                .AreNotEquals(oportunidadeId, Guid.Empty, nameof(OportunidadeId), "A oportunidade é obrigatória.")
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "O cliente é obrigatório.")
                .AreNotEquals(veiculoId, Guid.Empty, nameof(VeiculoId), "O veículo é obrigatório.")
            );

            OportunidadeId = oportunidadeId;
            VendaId = vendaId;
            ClienteId = clienteId;
            VeiculoId = veiculoId;
            Status = "Aberta";
        }

        public void Encerrar(string usuario)
        {
            Status = "Encerrada";
            MarcarAlterado(usuario);
        }
    }
}
