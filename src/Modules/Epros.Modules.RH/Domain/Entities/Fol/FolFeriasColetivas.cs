using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolFeriasColetivas : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public int? DiasGozo { get; private set; }
        public DateTime? AbonoInicio { get; private set; }
        public DateTime? AbonoFim { get; private set; }
        public int? DiasAbono { get; private set; }
        public DateTime? DataPagamento { get; private set; }

        protected FolFeriasColetivas() { } // EF Core

        public FolFeriasColetivas(
            Guid empresaId,
            DateTime? dataInicio,
            DateTime? dataFim,
            int? diasGozo,
            DateTime? abonoInicio,
            DateTime? abonoFim,
            int? diasAbono,
            DateTime? dataPagamento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            DataInicio = dataInicio;
            DataFim = dataFim;
            DiasGozo = diasGozo;
            AbonoInicio = abonoInicio;
            AbonoFim = abonoFim;
            DiasAbono = diasAbono;
            DataPagamento = dataPagamento;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolFeriasColetivas>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
