using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_deducao_recorrente). Fidelidade campo a campo.</summary>
    public partial class WfmDeducaoRecorrente : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid DeducaoId { get; private set; }
        public string? Descricao { get; private set; }
        public decimal Valor { get; private set; }
        public bool Ativo { get; private set; }

        protected WfmDeducaoRecorrente() { } // EF Core

        public WfmDeducaoRecorrente(
            Guid colaboradorId,
            Guid deducaoId,
            string? descricao,
            decimal valor,
            bool ativo,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            DeducaoId = deducaoId;
            Descricao = descricao;
            Valor = valor;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmDeducaoRecorrente>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.AreNotEquals(DeducaoId, Guid.Empty, nameof(DeducaoId), "O campo DeducaoId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
