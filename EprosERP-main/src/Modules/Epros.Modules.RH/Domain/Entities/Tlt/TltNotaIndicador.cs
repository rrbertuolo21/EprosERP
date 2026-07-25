using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_nota_indicador). Fidelidade campo a campo.</summary>
    public partial class TltNotaIndicador : EntidadeSaaSBase
    {
        public Guid AvaliacaoId { get; private set; }
        public Guid IndicadorId { get; private set; }
        public int Nota { get; private set; }
        public string? Observacao { get; private set; }

        protected TltNotaIndicador() { } // EF Core

        public TltNotaIndicador(
            Guid avaliacaoId,
            Guid indicadorId,
            int nota,
            string? observacao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            AvaliacaoId = avaliacaoId;
            IndicadorId = indicadorId;
            Nota = nota;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltNotaIndicador>().Requires();
            contract.AreNotEquals(AvaliacaoId, Guid.Empty, nameof(AvaliacaoId), "O campo AvaliacaoId e obrigatorio.");
            contract.AreNotEquals(IndicadorId, Guid.Empty, nameof(IndicadorId), "O campo IndicadorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
