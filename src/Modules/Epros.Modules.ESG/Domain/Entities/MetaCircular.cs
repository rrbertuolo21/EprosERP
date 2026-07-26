using System;
using Epros.Modules.ESG.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Meta de conteudo reciclado/recuperacao por periodo (EF ECONOMIA_CIRCULAR 11.6).</summary>
    public class MetaCircular : EntidadeSaaSBase
    {
        public Guid FluxoId { get; private set; }
        public string TipoIndicador { get; private set; } = string.Empty;
        public DateTime PeriodoInicio { get; private set; }
        public DateTime PeriodoFim { get; private set; }
        public decimal ValorMeta { get; private set; }
        public string Unidade { get; private set; } = string.Empty;
        public string? Formula { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public EStatusWorkflowEsg Status { get; private set; }

        protected MetaCircular() { } // EF Core

        public MetaCircular(
            Guid fluxoId,
            string tipoIndicador,
            DateTime periodoInicio,
            DateTime periodoFim,
            decimal valorMeta,
            string unidade,
            string? formula,
            Guid responsavelId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            FluxoId = fluxoId;
            TipoIndicador = tipoIndicador;
            PeriodoInicio = periodoInicio.Date;
            PeriodoFim = periodoFim.Date;
            ValorMeta = valorMeta;
            Unidade = unidade;
            Formula = formula;
            ResponsavelId = responsavelId;
            Status = EStatusWorkflowEsg.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<MetaCircular>()
                .Requires()
                .AreNotEquals(FluxoId, Guid.Empty, nameof(FluxoId), "O fluxo e obrigatorio. [Origem: MetaCircular]")
                .IsNotNullOrEmpty(TipoIndicador, nameof(TipoIndicador), "O tipo de indicador e obrigatorio. [Origem: MetaCircular]")
                .IsNotNullOrEmpty(Unidade, nameof(Unidade), "A unidade e obrigatoria. [Origem: MetaCircular]")
                // RN-ECO constraint: PeriodoInicio <= PeriodoFim.
                .IsFalse(PeriodoFim < PeriodoInicio, nameof(PeriodoFim), "O fim do periodo deve ser igual ou posterior ao inicio. [Origem: MetaCircular]"));
        }
    }
}
