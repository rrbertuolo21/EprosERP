using System;
using System.Collections.Generic;
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

        // 1.01 — campos financeiros/consulta materializados (EF 11.7/11.19).
        public bool Quitada { get; private set; }
        public decimal? ValorPago { get; private set; }
        public string? Numero { get; private set; }
        public string? Observacoes { get; private set; }

        // 1.01 — itens/composição da fatura emitida (EF 11.8).
        public List<FaturaItem> Itens { get; private set; } = new();

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
            Quitada = true;
            ValorPago = Valor;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Baixa manual com data de pagamento informada (landlord).</summary>
        public void BaixarManual(DateTime dataPagamento, string alteradoPor)
        {
            Status = FaturaStatus.Paga;
            DataPagamento = dataPagamento;
            Quitada = true;
            ValorPago = Valor;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Define dados de consulta/emissão (número e observações) — EF 11.19.</summary>
        public void DefinirDadosEmissao(string? numero, string? observacoes, string alteradoPor)
        {
            Numero = numero;
            Observacoes = observacoes;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Adiciona um item (composição) à fatura emitida — EF 11.8.</summary>
        public FaturaItem AdicionarItem(string descricao, decimal valor, string criadoPor)
        {
            var item = new FaturaItem(Id, descricao, valor, TenantId, criadoPor);
            Itens.Add(item);
            return item;
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

        /// <summary>
        /// 1.08E — Estorna a fatura (refund do pagamento do ciclo): move para <see cref="FaturaStatus.Estornada"/>
        /// e reabre os campos de quitação (o valor deixou de estar recebido). Reversível por nova cobrança.
        /// </summary>
        public void Estornar(string alteradoPor)
        {
            Status = FaturaStatus.Estornada;
            Quitada = false;
            ValorPago = null;
            DataPagamento = null;
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
