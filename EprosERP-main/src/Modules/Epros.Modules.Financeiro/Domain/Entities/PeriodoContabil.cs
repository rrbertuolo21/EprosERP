using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Período contábil de controle (EF FIN-CGL §5 / §9.3 / §11.8 cgl_periodo_contabil).
    /// Controla abertura/fechamento/reabertura e bloqueio de lançamentos.
    /// </summary>
    public class PeriodoContabil : EntidadeSaaSBase
    {
        public int AnoFiscal { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public DateTime? DataFechamento { get; private set; }
        public EEstadoPeriodoContabil Estado { get; private set; } = EEstadoPeriodoContabil.Aberto;
        public Guid? UsuarioFechamentoId { get; private set; }
        public Guid? UsuarioReaberturaId { get; private set; }
        public string? MotivoReabertura { get; private set; }

        protected PeriodoContabil() { } // EF Core

        public PeriodoContabil(int anoFiscal, DateTime? dataInicio, DateTime? dataFim, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AnoFiscal = anoFiscal;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Estado = EEstadoPeriodoContabil.Aberto;
            Validar();
        }

        public bool BloqueiaLancamento => Estado == EEstadoPeriodoContabil.Fechado || Estado == EEstadoPeriodoContabil.EmFechamento;

        /// <summary>Inicia o fechamento (Aberto/Reaberto → EmFechamento).</summary>
        public void IniciarFechamento(string usuario)
        {
            if (Estado != EEstadoPeriodoContabil.Aberto && Estado != EEstadoPeriodoContabil.Reaberto)
            {
                AddNotification(nameof(Estado), "Somente período aberto ou reaberto pode entrar em fechamento.");
                return;
            }
            Estado = EEstadoPeriodoContabil.EmFechamento;
            MarcarAlterado(usuario);
        }

        /// <summary>Conclui o fechamento (EmFechamento → Fechado). Bloqueia lançamentos.</summary>
        public void Fechar(Guid? usuarioFechamentoId, DateTime dataFechamento, string usuario)
        {
            if (Estado != EEstadoPeriodoContabil.EmFechamento && Estado != EEstadoPeriodoContabil.Aberto && Estado != EEstadoPeriodoContabil.Reaberto)
            {
                AddNotification(nameof(Estado), "Período não pode ser fechado a partir do estado atual.");
                return;
            }
            Estado = EEstadoPeriodoContabil.Fechado;
            DataFechamento = dataFechamento;
            UsuarioFechamentoId = usuarioFechamentoId;
            MarcarAlterado(usuario);
        }

        /// <summary>Reabre por exceção auditada (Fechado → Reaberto).</summary>
        public void Reabrir(Guid? usuarioReaberturaId, string motivo, string usuario)
        {
            if (Estado != EEstadoPeriodoContabil.Fechado)
            {
                AddNotification(nameof(Estado), "Somente período fechado pode ser reaberto.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoReabertura), "A reabertura exige motivo (exceção auditada).");
                return;
            }
            Estado = EEstadoPeriodoContabil.Reaberto;
            UsuarioReaberturaId = usuarioReaberturaId;
            MotivoReabertura = motivo;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PeriodoContabil>()
                .Requires()
                .IsGreaterThan(AnoFiscal, 0, nameof(AnoFiscal), "O ano fiscal é obrigatório.")
            );
        }
    }
}
