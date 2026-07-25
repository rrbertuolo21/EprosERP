using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_comissao_colaborador). Fidelidade campo a campo.</summary>
    public partial class WfmComissaoColaborador : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public string TipoCargo { get; private set; } = string.Empty;
        public decimal ValorPercentualComissao { get; private set; }
        public bool Ativo { get; private set; }

        protected WfmComissaoColaborador() { } // EF Core

        public WfmComissaoColaborador(
            Guid colaboradorId,
            string tipoCargo,
            decimal valorPercentualComissao,
            bool ativo,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            TipoCargo = tipoCargo;
            ValorPercentualComissao = valorPercentualComissao;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmComissaoColaborador>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(TipoCargo, nameof(TipoCargo), "O campo TipoCargo e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
