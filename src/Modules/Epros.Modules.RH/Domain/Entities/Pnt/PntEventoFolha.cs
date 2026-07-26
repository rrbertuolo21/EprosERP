using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntEventoFolha : EntidadeSaaSBase
    {
        public Guid PeriodoId { get; private set; }
        public Guid ColaboradorId { get; private set; }
        public string TipoEvento { get; private set; } = string.Empty;
        public string Quantidade { get; private set; } = string.Empty;
        public decimal? Percentual { get; private set; }
        public string? Origem { get; private set; }
        public string Status { get; private set; } = string.Empty;

        protected PntEventoFolha() { } // EF Core

        public PntEventoFolha(
            Guid periodoId,
            Guid colaboradorId,
            string tipoEvento,
            string quantidade,
            decimal? percentual,
            string? origem,
            string status,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PeriodoId = periodoId;
            ColaboradorId = colaboradorId;
            TipoEvento = tipoEvento;
            Quantidade = quantidade;
            Percentual = percentual;
            Origem = origem;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntEventoFolha>().Requires();
            contract.AreNotEquals(PeriodoId, Guid.Empty, nameof(PeriodoId), "O campo PeriodoId e obrigatorio.");
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(TipoEvento, nameof(TipoEvento), "O campo TipoEvento e obrigatorio.");
            contract.IsNotNullOrEmpty(Quantidade, nameof(Quantidade), "O campo Quantidade e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
