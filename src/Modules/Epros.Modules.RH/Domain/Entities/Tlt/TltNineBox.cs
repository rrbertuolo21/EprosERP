using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_nine_box). Fidelidade campo a campo.</summary>
    public partial class TltNineBox : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid CicloAvaliacaoId { get; private set; }
        public string? EixoDesempenho { get; private set; }
        public string? EixoPotencial { get; private set; }
        public string? Quadrante { get; private set; }
        public string? Observacao { get; private set; }

        protected TltNineBox() { } // EF Core

        public TltNineBox(
            Guid colaboradorId,
            Guid cicloAvaliacaoId,
            string? eixoDesempenho,
            string? eixoPotencial,
            string? quadrante,
            string? observacao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            CicloAvaliacaoId = cicloAvaliacaoId;
            EixoDesempenho = eixoDesempenho;
            EixoPotencial = eixoPotencial;
            Quadrante = quadrante;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltNineBox>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.AreNotEquals(CicloAvaliacaoId, Guid.Empty, nameof(CicloAvaliacaoId), "O campo CicloAvaliacaoId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
