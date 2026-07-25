using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-PNT). Fidelidade campo a campo.</summary>
    public partial class PntPeriodoApuracao : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Competencia { get; private set; } = string.Empty;
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public DateTime? FechadoEm { get; private set; }
        public DateTime? ExportadoEm { get; private set; }

        protected PntPeriodoApuracao() { } // EF Core

        public PntPeriodoApuracao(
            Guid empresaId,
            string competencia,
            DateTime dataInicio,
            DateTime dataFim,
            string status,
            DateTime? fechadoEm,
            DateTime? exportadoEm,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Competencia = competencia;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Status = status;
            FechadoEm = fechadoEm;
            ExportadoEm = exportadoEm;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<PntPeriodoApuracao>().Requires();
            contract.AreNotEquals(EmpresaId, Guid.Empty, nameof(EmpresaId), "O campo EmpresaId e obrigatorio.");
            contract.IsNotNullOrEmpty(Competencia, nameof(Competencia), "O campo Competencia e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
