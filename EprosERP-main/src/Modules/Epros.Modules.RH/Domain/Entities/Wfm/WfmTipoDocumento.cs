using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_tipo_documento). Fidelidade campo a campo.</summary>
    public partial class WfmTipoDocumento : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public string? Descricao { get; private set; }
        public bool? Obrigatorio { get; private set; }
        public Guid? CategoriaId { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected WfmTipoDocumento() { } // EF Core

        public WfmTipoDocumento(
            string? nome,
            string? descricao,
            bool? obrigatorio,
            Guid? categoriaId,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Descricao = descricao;
            Obrigatorio = obrigatorio;
            CategoriaId = categoriaId;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmTipoDocumento>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
