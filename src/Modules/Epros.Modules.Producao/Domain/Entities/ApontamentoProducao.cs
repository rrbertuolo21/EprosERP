using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    public class ApontamentoProducao : EntidadeSaaSBase
    {
        public Guid OrdemProducaoId { get; private set; }
        public decimal QuantidadeApontada { get; private set; }
        public decimal QuantidadeRefugada { get; private set; }
        public string Operador { get; private set; } = string.Empty;
        public DateTime DataHora { get; private set; }

        protected ApontamentoProducao() { } // EF Core

        public ApontamentoProducao(Guid ordemProducaoId, decimal quantidadeApontada, decimal quantidadeRefugada, string operador, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ApontamentoProducao>()
                .Requires()
                .AreNotEquals(ordemProducaoId, Guid.Empty, nameof(OrdemProducaoId), "O ID da ordem de produção é obrigatório.")
                .IsGreaterThan(quantidadeApontada, -0.01m, nameof(QuantidadeApontada), "A quantidade apontada não pode ser negativa.")
                .IsGreaterThan(quantidadeRefugada, -0.01m, nameof(QuantidadeRefugada), "A quantidade refugada não pode ser negativa.")
                .IsNotNullOrEmpty(operador, nameof(Operador), "O operador é obrigatório.")
            );

            // Deve haver alguma produção ou refugo
            if (quantidadeApontada == 0 && quantidadeRefugada == 0)
            {
                AddNotification(nameof(QuantidadeApontada), "Deve ser apontada pelo menos uma peça produzida ou uma peça refugada.");
            }

            OrdemProducaoId = ordemProducaoId;
            QuantidadeApontada = quantidadeApontada;
            QuantidadeRefugada = quantidadeRefugada;
            Operador = operador;
            DataHora = DateTime.UtcNow;
        }
    }
}
