using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_servico_colaborador). Fidelidade campo a campo.</summary>
    public partial class WfmServicoColaborador : EntidadeSaaSBase
    {
        public Guid ServicoId { get; private set; }
        public Guid ColaboradorId { get; private set; }

        protected WfmServicoColaborador() { } // EF Core

        public WfmServicoColaborador(
            Guid servicoId,
            Guid colaboradorId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ServicoId = servicoId;
            ColaboradorId = colaboradorId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmServicoColaborador>().Requires();
            contract.AreNotEquals(ServicoId, Guid.Empty, nameof(ServicoId), "O campo ServicoId e obrigatorio.");
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
