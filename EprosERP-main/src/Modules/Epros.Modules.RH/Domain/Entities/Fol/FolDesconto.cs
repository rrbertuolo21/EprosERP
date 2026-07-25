using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolDesconto : EntidadeSaaSBase
    {
        public Guid? ColaboradorId { get; private set; }
        public Guid? TipoDescontoId { get; private set; }
        public string? TipoCalculo { get; private set; }
        public decimal? Valor { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected FolDesconto() { } // EF Core

        public FolDesconto(
            Guid? colaboradorId,
            Guid? tipoDescontoId,
            string? tipoCalculo,
            decimal? valor,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            TipoDescontoId = tipoDescontoId;
            TipoCalculo = tipoCalculo;
            Valor = valor;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolDesconto>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
