using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-TLT, tabela rh_tlt_plano_retencao). Fidelidade campo a campo.</summary>
    public partial class TltPlanoRetencao : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public string? Motivo { get; private set; }
        public string? Acao { get; private set; }
        public Guid? ResponsavelId { get; private set; }
        public string? Status { get; private set; }
        public string? Observacao { get; private set; }

        protected TltPlanoRetencao() { } // EF Core

        public TltPlanoRetencao(
            Guid colaboradorId,
            string? motivo,
            string? acao,
            Guid? responsavelId,
            string? status,
            string? observacao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            Motivo = motivo;
            Acao = acao;
            ResponsavelId = responsavelId;
            Status = status;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<TltPlanoRetencao>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
