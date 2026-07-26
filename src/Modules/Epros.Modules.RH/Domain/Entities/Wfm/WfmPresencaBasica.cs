using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_presenca_basica). Fidelidade campo a campo.</summary>
    public partial class WfmPresencaBasica : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public DateTime? Entrada { get; private set; }
        public DateTime? Saida { get; private set; }
        public string? TempoPermanencia { get; private set; }
        public bool Ativo { get; private set; }

        protected WfmPresencaBasica() { } // EF Core

        public WfmPresencaBasica(
            Guid colaboradorId,
            DateTime? entrada,
            DateTime? saida,
            string? tempoPermanencia,
            bool ativo,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            Entrada = entrada;
            Saida = saida;
            TempoPermanencia = tempoPermanencia;
            Ativo = ativo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmPresencaBasica>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
