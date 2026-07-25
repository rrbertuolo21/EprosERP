using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolPagamentoDiario : EntidadeSaaSBase
    {
        public DateTime? DataReferencia { get; private set; }
        public string? NumeroComprovante { get; private set; }
        public decimal? Valor { get; private set; }
        public string? Status { get; private set; }

        protected FolPagamentoDiario() { } // EF Core

        public FolPagamentoDiario(
            DateTime? dataReferencia,
            string? numeroComprovante,
            decimal? valor,
            string? status,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            DataReferencia = dataReferencia;
            NumeroComprovante = numeroComprovante;
            Valor = valor;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolPagamentoDiario>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
