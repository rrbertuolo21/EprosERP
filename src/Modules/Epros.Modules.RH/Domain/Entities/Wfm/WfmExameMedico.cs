using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_exame_medico). Fidelidade campo a campo.</summary>
    public partial class WfmExameMedico : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public DateTime? DataUltimoExame { get; private set; }
        public DateTime? DataVencimentoExame { get; private set; }
        public string? Observacao { get; private set; }

        protected WfmExameMedico() { } // EF Core

        public WfmExameMedico(
            Guid colaboradorId,
            DateTime? dataUltimoExame,
            DateTime? dataVencimentoExame,
            string? observacao,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            DataUltimoExame = dataUltimoExame;
            DataVencimentoExame = dataVencimentoExame;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmExameMedico>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
