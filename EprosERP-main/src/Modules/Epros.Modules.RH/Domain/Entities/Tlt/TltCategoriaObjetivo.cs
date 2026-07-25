using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_categoria_objetivo). Fidelidade campo a campo.</summary>
    public partial class TltCategoriaObjetivo : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public string? Codigo { get; private set; }
        public string? Descricao { get; private set; }
        public bool? Ativo { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected TltCategoriaObjetivo() { } // EF Core

        public TltCategoriaObjetivo(
            string? nome,
            string? codigo,
            string? descricao,
            bool? ativo,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            Codigo = codigo;
            Descricao = descricao;
            Ativo = ativo;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltCategoriaObjetivo>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
