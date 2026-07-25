using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolGuiaAcumulada : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string GpsTipo { get; private set; } = string.Empty;
        public string GpsCompetencia { get; private set; } = string.Empty;
        public decimal? GpsValorInss { get; private set; }
        public decimal? GpsValorOutrasEntidades { get; private set; }
        public DateTime? GpsDataPagamento { get; private set; }
        public string IrrfCompetencia { get; private set; } = string.Empty;
        public int? IrrfCodigoRecolhimento { get; private set; }
        public decimal? IrrfValorAcumulado { get; private set; }
        public DateTime? IrrfDataPagamento { get; private set; }
        public string PisCompetencia { get; private set; } = string.Empty;
        public decimal? PisValorAcumulado { get; private set; }
        public DateTime? PisDataPagamento { get; private set; }

        protected FolGuiaAcumulada() { } // EF Core

        public FolGuiaAcumulada(
            Guid empresaId,
            string gpsTipo,
            string gpsCompetencia,
            decimal? gpsValorInss,
            decimal? gpsValorOutrasEntidades,
            DateTime? gpsDataPagamento,
            string irrfCompetencia,
            int? irrfCodigoRecolhimento,
            decimal? irrfValorAcumulado,
            DateTime? irrfDataPagamento,
            string pisCompetencia,
            decimal? pisValorAcumulado,
            DateTime? pisDataPagamento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            GpsTipo = gpsTipo;
            GpsCompetencia = gpsCompetencia;
            GpsValorInss = gpsValorInss;
            GpsValorOutrasEntidades = gpsValorOutrasEntidades;
            GpsDataPagamento = gpsDataPagamento;
            IrrfCompetencia = irrfCompetencia;
            IrrfCodigoRecolhimento = irrfCodigoRecolhimento;
            IrrfValorAcumulado = irrfValorAcumulado;
            IrrfDataPagamento = irrfDataPagamento;
            PisCompetencia = pisCompetencia;
            PisValorAcumulado = pisValorAcumulado;
            PisDataPagamento = pisDataPagamento;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolGuiaAcumulada>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            contract.IsNotNullOrEmpty(GpsTipo, nameof(GpsTipo), "O campo GpsTipo e obrigatorio.");
            contract.IsNotNullOrEmpty(GpsCompetencia, nameof(GpsCompetencia), "O campo GpsCompetencia e obrigatorio.");
            contract.IsNotNullOrEmpty(IrrfCompetencia, nameof(IrrfCompetencia), "O campo IrrfCompetencia e obrigatorio.");
            contract.IsNotNullOrEmpty(PisCompetencia, nameof(PisCompetencia), "O campo PisCompetencia e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
