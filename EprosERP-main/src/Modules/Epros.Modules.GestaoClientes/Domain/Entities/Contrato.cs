using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Contrato : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }
        public decimal ValorRecorrente { get; private set; }
        public int DiaVencimento { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public DateTime? UltimoFaturamento { get; private set; }
        public bool Ativo { get; private set; }
        public List<ContratoItem> Itens { get; private set; } = new();
        public string? OperadorAprovacao { get; private set; }
        public string? JustificativaAprovacao { get; private set; }

        protected Contrato() { } // EF Core

        public Contrato(
            Guid clienteId,
            int diaVencimento,
            DateTime dataInicio,
            DateTime? dataFim,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Contrato>()
                .Requires()
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "ClienteId inválido")
                .IsBetween(diaVencimento, 1, 28, nameof(DiaVencimento), "O dia de vencimento deve estar entre 1 e 28")
                .IsLowerOrEqualsThan(dataInicio, DateTime.UtcNow.AddDays(1), nameof(DataInicio), "Data de início inválida")
            );

            ClienteId = clienteId;
            DiaVencimento = diaVencimento;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Ativo = true;
            ValorRecorrente = 0;
        }

        public void AdicionarItem(string descricao, int quantidade, decimal valorUnitario, string criadoPor)
        {
            var item = new ContratoItem(Id, descricao, quantidade, valorUnitario, TenantId, criadoPor);
            if (item.IsValid)
            {
                Itens.Add(item);
                RecalcularValorRecorrente();
            }
            else
            {
                AddNotifications(item.Notifications);
            }
        }

        public void AtualizarFaturamento(DateTime dataFaturamento, string alteradoPor)
        {
            UltimoFaturamento = dataFaturamento;
            MarcarAlterado(alteradoPor);
        }

        public void AtualizarDatasEVencimento(DateTime dataInicio, DateTime? dataFim, int diaVencimento, string alteradoPor)
        {
            AddNotifications(new Contract<Contrato>()
                .Requires()
                .IsBetween(diaVencimento, 1, 28, nameof(DiaVencimento), "O dia de vencimento deve estar entre 1 e 28")
            );

            if (dataFim.HasValue && dataInicio > dataFim.Value)
            {
                AddNotification(nameof(DataInicio), "A data de início não pode ser posterior à data fim.");
            }

            if (UltimoFaturamento.HasValue && dataInicio < UltimoFaturamento.Value)
            {
                AddNotification(nameof(DataInicio), "A data de início não pode ser anterior à data do último faturamento.");
            }

            if (IsValid)
            {
                DataInicio = dataInicio;
                DataFim = dataFim;
                DiaVencimento = diaVencimento;
                MarcarAlterado(alteradoPor);
            }
        }

        public void DefinirValorRecorrenteManual(decimal valor, string alteradoPor)
        {
            AddNotifications(new Contract<Contrato>()
                .Requires()
                .IsGreaterThan(valor, 0, nameof(ValorRecorrente), "O valor recorrente deve ser maior que zero")
            );

            if (IsValid)
            {
                ValorRecorrente = valor;
                MarcarAlterado(alteradoPor);
            }
        }

        public void Inativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Ativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }

        public void AprovarManualmente(string operador, string justificativa, string alteradoPor)
        {
            Ativo = true;
            OperadorAprovacao = operador;
            JustificativaAprovacao = justificativa;
            MarcarAlterado(alteradoPor);
        }

        private void RecalcularValorRecorrente()
        {
            decimal total = 0;
            foreach (var item in Itens)
            {
                total += item.Quantidade * item.ValorUnitario;
            }
            ValorRecorrente = total;
        }
    }
}
