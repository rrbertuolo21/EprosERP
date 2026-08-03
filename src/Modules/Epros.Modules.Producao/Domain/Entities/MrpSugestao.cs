using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-MRP — Sugestão de suprimento gerada pelo motor MRP (prd_mrp_sugestao — PD21/PD3).
    /// Estados (DP-MRP-018): Calculada → PendenteAprovacao → Aprovada → Convertida → Cancelada.
    /// A conversão preserva a identidade e é idempotente: não duplica documento já convertido
    /// (aceite PRD-INT-CA-001/002). Sugestão de compra → COMPRAS; de produção → PLANEJAMENTO.
    /// </summary>
    public class MrpSugestao : EntidadeSaaSBase
    {
        public Guid PlanejamentoId { get; private set; }
        public Guid ItemId { get; private set; }
        public ETipoSugestaoMrp Tipo { get; private set; }
        public decimal Quantidade { get; private set; }
        public DateTime DataSugerida { get; private set; }
        public EEstadoSugestaoMrp Estado { get; private set; } = EEstadoSugestaoMrp.Calculada;
        public Guid? DocumentoGeradoId { get; private set; }
        public bool ImpactoFinanceiro { get; private set; }
        public string? MotivoCancelamento { get; private set; }

        protected MrpSugestao() { } // EF Core

        public MrpSugestao(
            Guid planejamentoId,
            Guid itemId,
            ETipoSugestaoMrp tipo,
            decimal quantidade,
            DateTime dataSugerida,
            string tenantId,
            string criadoPor,
            bool impactoFinanceiro = true)
            : base(tenantId, criadoPor)
        {
            PlanejamentoId = planejamentoId;
            ItemId = itemId;
            Tipo = tipo;
            Quantidade = quantidade;
            DataSugerida = dataSugerida;
            Estado = EEstadoSugestaoMrp.Calculada;
            ImpactoFinanceiro = impactoFinanceiro;

            AddNotifications(new Contract<MrpSugestao>()
                .Requires()
                .AreNotEquals(planejamentoId, Guid.Empty, nameof(PlanejamentoId), "O planejamento é obrigatório [Origem: MrpSugestao].")
                .AreNotEquals(itemId, Guid.Empty, nameof(ItemId), "O item é obrigatório [Origem: MrpSugestao].")
                .IsGreaterThan(quantidade, 0m, nameof(Quantidade), "A quantidade sugerida deve ser maior que zero [Origem: MrpSugestao].")
            );
        }

        /// <summary>DP-MRP-009/RN-MRP-009/PD20 — sugestão com impacto financeiro exige aprovação antes de virar documento.</summary>
        public void SubmeterAprovacao(string alteradoPor)
        {
            if (Estado != EEstadoSugestaoMrp.Calculada)
            {
                AddNotification(nameof(Estado), "Apenas sugestão Calculada pode ser submetida para aprovação.");
                return;
            }
            Estado = EEstadoSugestaoMrp.PendenteAprovacao;
            MarcarAlterado(alteradoPor);
        }

        public void Aprovar(string alteradoPor)
        {
            // impacto financeiro passa por PendenteAprovacao; sem impacto pode aprovar direto de Calculada
            if (Estado != EEstadoSugestaoMrp.PendenteAprovacao &&
                !(Estado == EEstadoSugestaoMrp.Calculada && !ImpactoFinanceiro))
            {
                AddNotification(nameof(Estado), "Sugestão não está em estado aprovável (Calculada sem impacto ou PendenteAprovacao).");
                return;
            }
            Estado = EEstadoSugestaoMrp.Aprovada;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// Converte a sugestão aprovada em documento (pedido de compra / ordem de produção), preservando a
        /// identidade. Idempotente: se já convertida com o MESMO documento, é no-op; com documento diferente, rejeita
        /// (não duplica — aceite PRD-INT-CA-002).
        /// </summary>
        public void Converter(Guid documentoGeradoId, string alteradoPor)
        {
            if (documentoGeradoId == Guid.Empty)
            {
                AddNotification(nameof(DocumentoGeradoId), "O documento gerado é obrigatório para conversão.");
                return;
            }
            if (Estado == EEstadoSugestaoMrp.Convertida)
            {
                if (DocumentoGeradoId != documentoGeradoId)
                    AddNotification(nameof(Estado), "Sugestão já convertida em outro documento — não pode ser duplicada (PRD-INT-CA-002).");
                return; // idempotente para o mesmo documento
            }
            if (Estado != EEstadoSugestaoMrp.Aprovada)
            {
                AddNotification(nameof(Estado), "Apenas sugestão Aprovada pode ser convertida em documento.");
                return;
            }
            Estado = EEstadoSugestaoMrp.Convertida;
            DocumentoGeradoId = documentoGeradoId;
            MarcarAlterado(alteradoPor);
        }

        public void Cancelar(string motivo, string alteradoPor)
        {
            if (Estado == EEstadoSugestaoMrp.Convertida)
            {
                AddNotification(nameof(Estado), "Sugestão convertida não pode ser cancelada.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoCancelamento), "O motivo do cancelamento é obrigatório.");
                return;
            }
            Estado = EEstadoSugestaoMrp.Cancelada;
            MotivoCancelamento = motivo;
            MarcarAlterado(alteradoPor);
        }
    }
}
