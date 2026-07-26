using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Exposição cambial de valores sujeitos a risco (EF FIN-CAM §10.5 cam_exposicao_cambial).
    /// Ciclo: Aberta → Hedgeada → Encerrada (§9). Origem cross-module apenas por Guid FK.
    /// </summary>
    public class ExposicaoCambial : EntidadeSaaSBase
    {
        public Guid MoedaId { get; private set; }
        public decimal ValorExposto { get; private set; }
        public EStatusExposicaoCambial Status { get; private set; } = EStatusExposicaoCambial.Aberta;
        public DateTime? DataReferencia { get; private set; }
        public string? OrigemExposicao { get; private set; }
        public string? EntidadeOrigemTipo { get; private set; }
        public Guid? EntidadeOrigemId { get; private set; }
        public Guid? TaxaReferenciaId { get; private set; }
        public decimal? ValorMoedaBase { get; private set; }

        protected ExposicaoCambial() { } // EF Core

        public ExposicaoCambial(Guid moedaId, decimal valorExposto, DateTime? dataReferencia, string? origemExposicao,
            string? entidadeOrigemTipo, Guid? entidadeOrigemId, Guid? taxaReferenciaId, decimal? valorMoedaBase,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            MoedaId = moedaId;
            ValorExposto = valorExposto;
            DataReferencia = dataReferencia;
            OrigemExposicao = origemExposicao;
            EntidadeOrigemTipo = entidadeOrigemTipo;
            EntidadeOrigemId = entidadeOrigemId;
            TaxaReferenciaId = taxaReferenciaId;
            ValorMoedaBase = valorMoedaBase;
            Status = EStatusExposicaoCambial.Aberta;
            Validar();
        }

        public void MarcarHedgeada(string usuario)
        {
            if (Status != EStatusExposicaoCambial.Aberta)
            {
                AddNotification(nameof(Status), "Somente exposição aberta pode ser marcada como hedgeada.");
                return;
            }
            Status = EStatusExposicaoCambial.Hedgeada;
            MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            if (Status == EStatusExposicaoCambial.Encerrada || Status == EStatusExposicaoCambial.Excluida)
            {
                AddNotification(nameof(Status), "Exposição já encerrada ou excluída.");
                return;
            }
            Status = EStatusExposicaoCambial.Encerrada;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ExposicaoCambial>()
                .Requires()
                .IsNotEmpty(MoedaId, nameof(MoedaId), "A moeda é obrigatória [Origem: ExposicaoCambial]")
                .IsGreaterThan(ValorExposto, 0, nameof(ValorExposto), "O valor exposto deve ser maior que zero [Origem: ExposicaoCambial]")
            );
        }
    }
}
