using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PLN, tabela rh_pln_headcount_item). Fidelidade campo a campo.</summary>
    public partial class PlnHeadcountItem : EntidadeSaaSBase
    {
        public Guid VersaoId { get; private set; }
        public Guid? DepartamentoId { get; private set; }
        public Guid? CargoId { get; private set; }
        public int? QuantidadeAutorizada { get; private set; }
        public decimal? CustoPrevisto { get; private set; }
        public string? Observacao { get; private set; }

        protected PlnHeadcountItem() { } // EF Core

        public PlnHeadcountItem(
            Guid versaoId,
            Guid? departamentoId,
            Guid? cargoId,
            int? quantidadeAutorizada,
            decimal? custoPrevisto,
            string? observacao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            VersaoId = versaoId;
            DepartamentoId = departamentoId;
            CargoId = cargoId;
            QuantidadeAutorizada = quantidadeAutorizada;
            CustoPrevisto = custoPrevisto;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PlnHeadcountItem>().Requires();
            contract.AreNotEquals(VersaoId, Guid.Empty, nameof(VersaoId), "O campo VersaoId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
