using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_historico). Fidelidade campo a campo.</summary>
    public partial class WfmHistorico : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public string Evento { get; private set; } = string.Empty;
        public DateTime DataEvento { get; private set; }
        public string? Origem { get; private set; }
        public Guid? UsuarioId { get; private set; }
        public string? Detalhe { get; private set; }

        protected WfmHistorico() { } // EF Core

        public WfmHistorico(
            Guid colaboradorId,
            string evento,
            DateTime dataEvento,
            string? origem,
            Guid? usuarioId,
            string? detalhe,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            Evento = evento;
            DataEvento = dataEvento;
            Origem = origem;
            UsuarioId = usuarioId;
            Detalhe = detalhe;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmHistorico>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Evento, nameof(Evento), "O campo Evento e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
