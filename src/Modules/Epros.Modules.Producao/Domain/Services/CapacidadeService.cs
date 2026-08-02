using System;
using Epros.Modules.Producao.Domain.Entities;

namespace Epros.Modules.Producao.Domain.Services
{
    /// <summary>
    /// PRD-PLN/CTR — Motor de capacidade fabril e ATP/CTP (PD4 · DP-PLN-010/011/012).
    /// Capacidade por centro de trabalho a partir de turnos/dias úteis/eficiência; confronta carga × capacidade
    /// e sinaliza gargalo. Carga acima do limite ⇒ PENDENTE_CAPACIDADE (doc integração §8) — não simula
    /// viabilidade além do desenhado. ATP simples = quanto ainda cabe (disponível para prometer).
    /// Motor puro de domínio, testável.
    /// </summary>
    public sealed class CapacidadeService
    {
        public enum EStatusCapacidade
        {
            Ok = 0,
            PendenteCapacidade = 1
        }

        public sealed record AvaliacaoCarga(
            decimal Capacidade,
            decimal Carga,
            decimal Folga,
            decimal UtilizacaoPercentual,
            EStatusCapacidade Status);

        /// <summary>Capacidade diária de um centro em minutos (turnos × minutos/turno × eficiência).</summary>
        public decimal CapacidadeDiariaMinutos(CentroTrabalho centro)
        {
            if (centro == null) throw new ArgumentNullException(nameof(centro));
            return centro.MinutosPorTurno * centro.TurnosPorDia * (centro.EficienciaPercentual / 100m);
        }

        /// <summary>Capacidade do centro num período de <paramref name="diasUteis"/> dias.</summary>
        public decimal CapacidadePeriodoMinutos(CentroTrabalho centro, int diasUteis)
        {
            if (diasUteis < 0) diasUteis = 0;
            return CapacidadeDiariaMinutos(centro) * diasUteis;
        }

        /// <summary>Confronta carga × capacidade; carga acima ⇒ PendenteCapacidade (gargalo).</summary>
        public AvaliacaoCarga Avaliar(decimal capacidade, decimal carga)
        {
            if (carga < 0m) carga = 0m;
            var folga = capacidade - carga;
            var utilizacao = capacidade > 0m
                ? Math.Round(carga / capacidade * 100m, 4)
                : (carga > 0m ? 100m : 0m);
            var status = carga > capacidade ? EStatusCapacidade.PendenteCapacidade : EStatusCapacidade.Ok;
            return new AvaliacaoCarga(capacidade, carga, folga, utilizacao, status);
        }

        /// <summary>ATP/CTP simplificado: capacidade ainda disponível para prometer (nunca negativa).</summary>
        public decimal DisponivelParaPrometer(decimal capacidade, decimal cargaComprometida)
        {
            var atp = capacidade - cargaComprometida;
            return atp < 0m ? 0m : atp;
        }
    }
}
