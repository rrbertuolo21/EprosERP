using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolLancamento : EntidadeSaaSBase
    {
        public Guid CompetenciaId { get; private set; }
        public Guid ColaboradorId { get; private set; }
        public decimal? SalarioBase { get; private set; }
        public decimal TotalProventos { get; private set; }
        public decimal TotalDescontos { get; private set; }
        public decimal? TotalEmprestimos { get; private set; }
        public decimal? TotalHorasExtras { get; private set; }
        public decimal? ValorBruto { get; private set; }
        public decimal ValorLiquido { get; private set; }
        public decimal? DiasTrabalhados { get; private set; }
        public decimal? DiasPresentes { get; private set; }
        public decimal? MeiasDiarias { get; private set; }
        public decimal? DiasAusentes { get; private set; }
        public decimal? DiasLicencaPaga { get; private set; }
        public decimal? DiasLicencaNaoPaga { get; private set; }
        public string Status { get; private set; } = string.Empty;

        protected FolLancamento() { } // EF Core

        public FolLancamento(
            Guid competenciaId,
            Guid colaboradorId,
            decimal? salarioBase,
            decimal totalProventos,
            decimal totalDescontos,
            decimal? totalEmprestimos,
            decimal? totalHorasExtras,
            decimal? valorBruto,
            decimal valorLiquido,
            decimal? diasTrabalhados,
            decimal? diasPresentes,
            decimal? meiasDiarias,
            decimal? diasAusentes,
            decimal? diasLicencaPaga,
            decimal? diasLicencaNaoPaga,
            string status,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompetenciaId = competenciaId;
            ColaboradorId = colaboradorId;
            SalarioBase = salarioBase;
            TotalProventos = totalProventos;
            TotalDescontos = totalDescontos;
            TotalEmprestimos = totalEmprestimos;
            TotalHorasExtras = totalHorasExtras;
            ValorBruto = valorBruto;
            ValorLiquido = valorLiquido;
            DiasTrabalhados = diasTrabalhados;
            DiasPresentes = diasPresentes;
            MeiasDiarias = meiasDiarias;
            DiasAusentes = diasAusentes;
            DiasLicencaPaga = diasLicencaPaga;
            DiasLicencaNaoPaga = diasLicencaNaoPaga;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolLancamento>().Requires();
            contract.AreNotEquals(CompetenciaId, Guid.Empty, nameof(CompetenciaId), "O campo CompetenciaId e obrigatorio.");
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
