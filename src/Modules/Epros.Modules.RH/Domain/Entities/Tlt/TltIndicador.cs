using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_indicador). Fidelidade campo a campo.</summary>
    public partial class TltIndicador : EntidadeSaaSBase
    {
        public Guid CategoriaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public string? UnidadeMedida { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public Guid? CriadoPorId { get; private set; }
        public Guid OwnerId { get; private set; }

        protected TltIndicador() { } // EF Core

        public TltIndicador(
            Guid categoriaId,
            string nome,
            string? descricao,
            string? unidadeMedida,
            string status,
            Guid? criadoPorId,
            Guid ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            CategoriaId = categoriaId;
            Nome = nome;
            Descricao = descricao;
            UnidadeMedida = unidadeMedida;
            Status = status;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltIndicador>().Requires();
            contract.AreNotEquals(CategoriaId, Guid.Empty, nameof(CategoriaId), "O campo CategoriaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Nome, nameof(Nome), "O campo Nome e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            contract.AreNotEquals(OwnerId, Guid.Empty, nameof(OwnerId), "O campo OwnerId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
