using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PLN, tabela rh_pln_cenario_movimento). Fidelidade campo a campo.</summary>
    public partial class PlnCenarioMovimento : EntidadeSaaSBase
    {
        public Guid VersaoId { get; private set; }
        public string? TipoMovimento { get; private set; }
        public Guid? DepartamentoId { get; private set; }
        public Guid? CargoId { get; private set; }
        public int? Quantidade { get; private set; }
        public decimal? ImpactoFinanceiro { get; private set; }
        public string? Observacao { get; private set; }

        protected PlnCenarioMovimento() { } // EF Core

        public PlnCenarioMovimento(
            Guid versaoId,
            string? tipoMovimento,
            Guid? departamentoId,
            Guid? cargoId,
            int? quantidade,
            decimal? impactoFinanceiro,
            string? observacao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            VersaoId = versaoId;
            TipoMovimento = tipoMovimento;
            DepartamentoId = departamentoId;
            CargoId = cargoId;
            Quantidade = quantidade;
            ImpactoFinanceiro = impactoFinanceiro;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PlnCenarioMovimento>().Requires();
            contract.AreNotEquals(VersaoId, Guid.Empty, nameof(VersaoId), "O campo VersaoId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
