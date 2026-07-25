using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_renda_recorrente). Fidelidade campo a campo.</summary>
    public partial class WfmRendaRecorrente : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid RendaId { get; private set; }
        public string? Descricao { get; private set; }
        public decimal Valor { get; private set; }
        public bool Ativo { get; private set; }

        protected WfmRendaRecorrente() { } // EF Core

        public WfmRendaRecorrente(
            Guid colaboradorId,
            Guid rendaId,
            string? descricao,
            decimal valor,
            bool ativo,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            RendaId = rendaId;
            Descricao = descricao;
            Valor = valor;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmRendaRecorrente>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.AreNotEquals(RendaId, Guid.Empty, nameof(RendaId), "O campo RendaId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
