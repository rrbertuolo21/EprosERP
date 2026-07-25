using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_departamento). Fidelidade campo a campo.</summary>
    public partial class WfmDepartamento : EntidadeSaaSBase
    {
        public Guid? FilialId { get; private set; }
        public Guid? DepartamentoPaiId { get; private set; }
        public string? Nome { get; private set; }
        public string? Descricao { get; private set; }
        public Guid? CentroCustoId { get; private set; }
        public Guid? GestorId { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }
        public bool Ativo { get; private set; }

        protected WfmDepartamento() { } // EF Core

        public WfmDepartamento(
            Guid? filialId,
            Guid? departamentoPaiId,
            string? nome,
            string? descricao,
            Guid? centroCustoId,
            Guid? gestorId,
            Guid? criadoPorId,
            Guid? ownerId,
            bool ativo,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            FilialId = filialId;
            DepartamentoPaiId = departamentoPaiId;
            Nome = nome;
            Descricao = descricao;
            CentroCustoId = centroCustoId;
            GestorId = gestorId;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmDepartamento>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
