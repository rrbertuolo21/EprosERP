using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_categoria_indicador). Fidelidade campo a campo.</summary>
    public partial class TltCategoriaIndicador : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public Guid? CriadoPorId { get; private set; }
        public Guid OwnerId { get; private set; }

        protected TltCategoriaIndicador() { } // EF Core

        public TltCategoriaIndicador(
            string nome,
            string? descricao,
            string status,
            Guid? criadoPorId,
            Guid ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Descricao = descricao;
            Status = status;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltCategoriaIndicador>().Requires();
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.AreNotEquals(OwnerId, Guid.Empty, nameof(OwnerId), "O campo OwnerId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
