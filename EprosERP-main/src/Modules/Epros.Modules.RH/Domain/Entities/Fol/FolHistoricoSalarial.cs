using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolHistoricoSalarial : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public string Competencia { get; private set; } = string.Empty;
        public decimal? SalarioAtual { get; private set; }
        public decimal? PercentualAumento { get; private set; }
        public decimal? SalarioNovo { get; private set; }
        public string ValidoAPartir { get; private set; } = string.Empty;
        public string Motivo { get; private set; } = string.Empty;

        protected FolHistoricoSalarial() { } // EF Core

        public FolHistoricoSalarial(
            Guid colaboradorId,
            string competencia,
            decimal? salarioAtual,
            decimal? percentualAumento,
            decimal? salarioNovo,
            string validoAPartir,
            string motivo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            Competencia = competencia;
            SalarioAtual = salarioAtual;
            PercentualAumento = percentualAumento;
            SalarioNovo = salarioNovo;
            ValidoAPartir = validoAPartir;
            Motivo = motivo;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolHistoricoSalarial>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Competencia, nameof(Competencia), "O campo Competencia e obrigatorio.");
            contract.IsNotNullOrEmpty(ValidoAPartir, nameof(ValidoAPartir), "O campo ValidoAPartir e obrigatorio.");
            contract.IsNotNullOrEmpty(Motivo, nameof(Motivo), "O campo Motivo e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
