using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class ReservaVeiculo : EntidadeSaaSBase
    {
        public Guid EstoqueVeiculoId { get; private set; }
        public Guid OportunidadeId { get; private set; }
        public DateTime Inicio { get; private set; }
        public DateTime Fim { get; private set; }
        public string Status { get; private set; } = "Ativa";
        public string? Motivo { get; private set; }

        protected ReservaVeiculo() { } // EF Core

        public ReservaVeiculo(
            Guid estoqueVeiculoId,
            Guid oportunidadeId,
            DateTime inicio,
            DateTime fim,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ReservaVeiculo>()
                .Requires()
                .AreNotEquals(estoqueVeiculoId, Guid.Empty, nameof(EstoqueVeiculoId), "A unidade em estoque é obrigatória.")
                .AreNotEquals(oportunidadeId, Guid.Empty, nameof(OportunidadeId), "A oportunidade é obrigatória.")
            );

            if (fim <= inicio)
            {
                AddNotification(nameof(Fim), "A data/hora de término deve ser posterior ao início.");
            }

            EstoqueVeiculoId = estoqueVeiculoId;
            OportunidadeId = oportunidadeId;
            Inicio = inicio;
            Fim = fim;
            Status = "Ativa";
        }

        public void Expirar(string usuario)
        {
            if (Status == "Ativa")
            {
                Status = "Expirada";
                MarcarAlterado(usuario);
            }
        }

        public void Cancelar(string motivo, string usuario)
        {
            Status = "Cancelada";
            Motivo = motivo;
            MarcarAlterado(usuario);
        }
    }
}
