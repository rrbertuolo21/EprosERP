using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class ComposicaoFaturamento : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public decimal Valor { get; private set; }
        public DateTime DataInicial { get; private set; }
        public DateTime? DataFinal { get; private set; }
        public bool PodeReajustar { get; private set; }

        protected ComposicaoFaturamento() { } // EF Core

        public ComposicaoFaturamento(
            Guid clienteId,
            string descricao,
            decimal valor,
            DateTime dataInicial,
            DateTime? dataFinal,
            bool podeReajustar,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ComposicaoFaturamento>()
                .Requires()
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "ClienteId é obrigatório")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "Descrição é obrigatória")
                .IsGreaterThan(valor, 0, nameof(Valor), "Valor deve ser maior que zero")
            );

            ClienteId = clienteId;
            Descricao = descricao;
            Valor = valor;
            DataInicial = dataInicial;
            DataFinal = dataFinal;
            PodeReajustar = podeReajustar;
        }

        public void Alterar(string descricao, decimal valor, DateTime dataInicial, DateTime? dataFinal, bool podeReajustar, string alteradoPor)
        {
            AddNotifications(new Contract<ComposicaoFaturamento>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "Descrição é obrigatória")
                .IsGreaterThan(valor, 0, nameof(Valor), "Valor deve ser maior que zero")
            );

            if (IsValid)
            {
                Descricao = descricao;
                Valor = valor;
                DataInicial = dataInicial;
                DataFinal = dataFinal;
                PodeReajustar = podeReajustar;
                MarcarAlterado(alteradoPor);
            }
        }

        public void ReajustarPreco(decimal novoValor, string alteradoPor)
        {
            AddNotifications(new Contract<ComposicaoFaturamento>()
                .Requires()
                .IsGreaterThan(novoValor, 0, nameof(Valor), "Novo valor deve ser maior que zero")
            );

            if (IsValid)
            {
                Valor = novoValor;
                MarcarAlterado(alteradoPor);
            }
        }

        public void Encerrar(DateTime dataFim, string alteradoPor)
        {
            DataFinal = dataFim;
            MarcarAlterado(alteradoPor);
        }
    }
}
