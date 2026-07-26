using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-SSO). Fidelidade campo a campo.</summary>
    public partial class SsoExameMedico : EntidadeSaaSBase
    {
        public Guid? PppId { get; private set; }
        public Guid ColaboradorId { get; private set; }
        public DateTime? DataUltimo { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public string Natureza { get; private set; } = string.Empty;
        public string Exame { get; private set; } = string.Empty;
        public string IndicacaoResultados { get; private set; } = string.Empty;
        public string Status { get; private set; } = string.Empty;

        protected SsoExameMedico() { } // EF Core

        public SsoExameMedico(
            Guid? pppId,
            Guid colaboradorId,
            DateTime? dataUltimo,
            string tipo,
            string natureza,
            string exame,
            string indicacaoResultados,
            string status,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PppId = pppId;
            ColaboradorId = colaboradorId;
            DataUltimo = dataUltimo;
            Tipo = tipo;
            Natureza = natureza;
            Exame = exame;
            IndicacaoResultados = indicacaoResultados;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<SsoExameMedico>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Tipo, nameof(Tipo), "O campo Tipo e obrigatorio.");
            contract.IsNotNullOrEmpty(Natureza, nameof(Natureza), "O campo Natureza e obrigatorio.");
            contract.IsNotNullOrEmpty(Exame, nameof(Exame), "O campo Exame e obrigatorio.");
            contract.IsNotNullOrEmpty(IndicacaoResultados, nameof(IndicacaoResultados), "O campo IndicacaoResultados e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
