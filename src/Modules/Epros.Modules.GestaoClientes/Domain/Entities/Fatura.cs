using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Fatura : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }
        public decimal Valor { get; private set; }
        public DateTime DataVencimento { get; private set; }
        public DateTime? DataPagamento { get; private set; }
        public FaturaStatus Status { get; private set; } = FaturaStatus.Pendente;
        public decimal PercentualComissaoRevenda { get; private set; }
        public decimal PercentualComissaoVendedor { get; private set; }
        public decimal ValorComissaoRevenda { get; private set; }
        public decimal ValorComissaoVendedor { get; private set; }

        protected Fatura() { } // EF Core

        public Fatura(Guid clienteId, decimal valor, DateTime dataVencimento, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Fatura>()
                .Requires()
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "ClienteId é obrigatório")
                .IsGreaterThan(valor, 0, nameof(Valor), "Valor da fatura deve ser maior que zero")
            );

            ClienteId = clienteId;
            Valor = valor;
            DataVencimento = dataVencimento;
        }

        public void Baixar(string alteradoPor)
        {
            Status = FaturaStatus.Paga;
            DataPagamento = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Baixa manual com data de pagamento informada (landlord).</summary>
        public void BaixarManual(DateTime dataPagamento, string alteradoPor)
        {
            Status = FaturaStatus.Paga;
            DataPagamento = dataPagamento;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Altera vencimento e/ou valor de uma fatura ainda em aberto.</summary>
        public void Alterar(decimal valor, DateTime dataVencimento, string alteradoPor)
        {
            AddNotifications(new Contract<Fatura>()
                .Requires()
                .IsGreaterThan(valor, 0, nameof(Valor), "Valor da fatura deve ser maior que zero")
            );

            if (!IsValid) return;

            Valor = valor;
            DataVencimento = dataVencimento;
            MarcarAlterado(alteradoPor);
        }

        public void Cancelar(string alteradoPor)
        {
            Status = FaturaStatus.Cancelada;
            MarcarAlterado(alteradoPor);
        }

        public void MarcarAtrasada(string alteradoPor)
        {
            Status = FaturaStatus.Atrasada;
            MarcarAlterado(alteradoPor);
        }

        public void CalcularSplitComissao(decimal percentualRevenda, decimal percentualVendedor)
        {
            PercentualComissaoRevenda = percentualRevenda;
            PercentualComissaoVendedor = percentualVendedor;
            ValorComissaoRevenda = Math.Round(Valor * (percentualRevenda / 100m), 2);
            ValorComissaoVendedor = Math.Round(Valor * (percentualVendedor / 100m), 2);
        }
    }
}
