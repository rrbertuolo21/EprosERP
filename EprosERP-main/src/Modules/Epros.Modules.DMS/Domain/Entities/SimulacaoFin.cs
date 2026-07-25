using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class SimulacaoFin : EntidadeSaaSBase
    {
        public Guid JornadaId { get; private set; }
        public string ChaveIdempotencia { get; private set; } = string.Empty;
        public decimal PrecoVeiculo { get; private set; }
        public decimal Entrada { get; private set; }
        public decimal Saldo { get; private set; }
        public int PrazoQuantidade { get; private set; }
        public string PrazoUnidade { get; private set; } = "Meses";
        public string? OrigemVersao { get; private set; }
        public string? MemoriaJson { get; private set; }

        protected SimulacaoFin() { } // EF Core

        public SimulacaoFin(
            Guid jornadaId,
            string chaveIdempotencia,
            decimal precoVeiculo,
            decimal entrada,
            decimal saldo,
            int prazoQuantidade,
            string prazoUnidade,
            string? origemVersao,
            string? memoriaJson,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<SimulacaoFin>()
                .Requires()
                .AreNotEquals(jornadaId, Guid.Empty, nameof(JornadaId), "A jornada é obrigatória.")
                .IsNotNullOrEmpty(chaveIdempotencia, nameof(ChaveIdempotencia), "A chave de idempotência é obrigatória.")
                .IsGreaterThan(precoVeiculo, 0, nameof(PrecoVeiculo), "O preço do veículo deve ser maior que zero.")
                .IsGreaterThan(entrada, -0.001m, nameof(Entrada), "A entrada não pode ser negativa.")
                .IsGreaterThan(saldo, -0.001m, nameof(Saldo), "O saldo não pode ser negativo.")
                .IsGreaterThan(prazoQuantidade, 0, nameof(PrazoQuantidade), "O prazo deve ser maior que zero.")
            );

            JornadaId = jornadaId;
            ChaveIdempotencia = chaveIdempotencia;
            PrecoVeiculo = precoVeiculo;
            Entrada = entrada;
            Saldo = saldo;
            PrazoQuantidade = prazoQuantidade;
            PrazoUnidade = string.IsNullOrWhiteSpace(prazoUnidade) ? "Meses" : prazoUnidade;
            OrigemVersao = origemVersao;
            MemoriaJson = memoriaJson;
        }
    }
}
