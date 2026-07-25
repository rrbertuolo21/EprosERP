using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>KPI de circularidade medido por periodo (EF ECONOMIA_CIRCULAR 11.6).</summary>
    public class MedicaoCircular : EntidadeSaaSBase
    {
        public Guid FluxoId { get; private set; }
        public string TipoIndicador { get; private set; } = string.Empty;
        public string Periodo { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public string Unidade { get; private set; } = string.Empty;
        public decimal? Numerador { get; private set; }
        public decimal? Denominador { get; private set; }
        public string? Fonte { get; private set; }
        public DateTime DataApuracao { get; private set; }

        protected MedicaoCircular() { } // EF Core

        public MedicaoCircular(
            Guid fluxoId,
            string tipoIndicador,
            string periodo,
            decimal? numerador,
            decimal? denominador,
            string unidade,
            string? fonte,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            FluxoId = fluxoId;
            TipoIndicador = tipoIndicador;
            Periodo = periodo;
            Numerador = numerador;
            Denominador = denominador;
            Unidade = unidade;
            Fonte = fonte;
            // KPI de conteudo reciclado = numerador / denominador (quando informados).
            Valor = (denominador.HasValue && denominador.Value != 0 && numerador.HasValue)
                ? numerador.Value / denominador.Value
                : 0m;
            DataApuracao = DateTime.UtcNow;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<MedicaoCircular>()
                .Requires()
                .AreNotEquals(FluxoId, Guid.Empty, nameof(FluxoId), "O fluxo e obrigatorio. [Origem: MedicaoCircular]")
                .IsNotNullOrEmpty(TipoIndicador, nameof(TipoIndicador), "O tipo de indicador e obrigatorio. [Origem: MedicaoCircular]")
                .IsNotNullOrEmpty(Periodo, nameof(Periodo), "O periodo e obrigatorio. [Origem: MedicaoCircular]")
                .IsNotNullOrEmpty(Unidade, nameof(Unidade), "A unidade e obrigatoria. [Origem: MedicaoCircular]"));
        }
    }
}
