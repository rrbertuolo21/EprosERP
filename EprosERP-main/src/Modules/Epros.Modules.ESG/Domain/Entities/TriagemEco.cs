using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Recebimento e classificacao de item para destino (EF ECONOMIA_CIRCULAR 11.4).</summary>
    public class TriagemEco : EntidadeSaaSBase
    {
        public Guid FluxoId { get; private set; }
        public Guid? ItemDevolucaoId { get; private set; }
        public decimal QuantidadeRecebida { get; private set; }
        public string Unidade { get; private set; } = string.Empty;
        public string Condicao { get; private set; } = string.Empty;
        public string DestinoProposto { get; private set; } = string.Empty;
        public string? Motivo { get; private set; }
        public Guid ResponsavelId { get; private set; }
        public DateTime DataTriagem { get; private set; }

        protected TriagemEco() { } // EF Core

        public TriagemEco(
            Guid fluxoId,
            Guid? itemDevolucaoId,
            decimal quantidadeRecebida,
            string unidade,
            string condicao,
            string destinoProposto,
            string? motivo,
            Guid responsavelId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            FluxoId = fluxoId;
            ItemDevolucaoId = itemDevolucaoId;
            QuantidadeRecebida = quantidadeRecebida;
            Unidade = unidade;
            Condicao = condicao;
            DestinoProposto = destinoProposto;
            Motivo = motivo;
            ResponsavelId = responsavelId;
            DataTriagem = DateTime.UtcNow;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TriagemEco>()
                .Requires()
                .AreNotEquals(FluxoId, Guid.Empty, nameof(FluxoId), "O fluxo e obrigatorio. [Origem: TriagemEco]")
                .IsGreaterThan(QuantidadeRecebida, 0, nameof(QuantidadeRecebida), "A quantidade recebida deve ser maior que zero. [Origem: TriagemEco]")
                .IsNotNullOrEmpty(Unidade, nameof(Unidade), "A unidade e obrigatoria. [Origem: TriagemEco]")
                .IsNotNullOrEmpty(Condicao, nameof(Condicao), "A condicao e obrigatoria. [Origem: TriagemEco]")
                .IsNotNullOrEmpty(DestinoProposto, nameof(DestinoProposto), "O destino proposto e obrigatorio. [Origem: TriagemEco]")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio. [Origem: TriagemEco]"));
        }
    }
}
