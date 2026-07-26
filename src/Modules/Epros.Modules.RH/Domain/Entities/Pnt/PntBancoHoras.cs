using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntBancoHoras : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public DateTime? DataTrabalho { get; private set; }
        public string Quantidade { get; private set; } = string.Empty;
        public string Situacao { get; private set; } = string.Empty;
        public Guid? FechamentoJornadaId { get; private set; }

        protected PntBancoHoras() { } // EF Core

        public PntBancoHoras(
            Guid colaboradorId,
            DateTime? dataTrabalho,
            string quantidade,
            string situacao,
            Guid? fechamentoJornadaId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            DataTrabalho = dataTrabalho;
            Quantidade = quantidade;
            Situacao = situacao;
            FechamentoJornadaId = fechamentoJornadaId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntBancoHoras>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Quantidade, nameof(Quantidade), "O campo Quantidade e obrigatorio.");
            contract.IsNotNullOrEmpty(Situacao, nameof(Situacao), "O campo Situacao e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
