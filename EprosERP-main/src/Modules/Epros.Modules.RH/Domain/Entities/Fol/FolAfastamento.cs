using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolAfastamento : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid TipoAfastamentoId { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public int? DiasAfastado { get; private set; }

        protected FolAfastamento() { } // EF Core

        public FolAfastamento(
            Guid colaboradorId,
            Guid tipoAfastamentoId,
            DateTime? dataInicio,
            DateTime? dataFim,
            int? diasAfastado,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            TipoAfastamentoId = tipoAfastamentoId;
            DataInicio = dataInicio;
            DataFim = dataFim;
            DiasAfastado = diasAfastado;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolAfastamento>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.AreNotEquals(TipoAfastamentoId, Guid.Empty, nameof(TipoAfastamentoId), "O campo TipoAfastamentoId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
