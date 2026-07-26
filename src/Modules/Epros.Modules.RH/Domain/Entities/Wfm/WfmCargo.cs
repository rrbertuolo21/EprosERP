using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_cargo). Fidelidade campo a campo.</summary>
    public partial class WfmCargo : EntidadeSaaSBase
    {
        public Guid? FilialId { get; private set; }
        public Guid? DepartamentoId { get; private set; }
        public Guid? CargoPaiId { get; private set; }
        public string? Nome { get; private set; }
        public string? Descricao { get; private set; }
        public string? Cbo { get; private set; }
        public string? TipoCargo { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }
        public bool Ativo { get; private set; }

        protected WfmCargo() { } // EF Core

        public WfmCargo(
            Guid? filialId,
            Guid? departamentoId,
            Guid? cargoPaiId,
            string? nome,
            string? descricao,
            string? cbo,
            string? tipoCargo,
            Guid? criadoPorId,
            Guid? ownerId,
            bool ativo,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            FilialId = filialId;
            DepartamentoId = departamentoId;
            CargoPaiId = cargoPaiId;
            Nome = nome;
            Descricao = descricao;
            Cbo = cbo;
            TipoCargo = tipoCargo;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmCargo>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
