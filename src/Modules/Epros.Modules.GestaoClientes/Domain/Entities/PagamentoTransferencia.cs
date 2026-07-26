using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PagamentoTransferencia : EntidadeSaaSBase
    {
        public Guid? FaturaId { get; private set; }
        public Guid? PedidoId { get; private set; }
        public decimal Valor { get; private set; }
        public string Moeda { get; private set; } = "BRL";
        public PagamentoTransferenciaStatus Status { get; private set; } = PagamentoTransferenciaStatus.Pending;
        public string? Justificativa { get; private set; }
        public DateTime? DataAnalise { get; private set; }
        public string? AnalisadoPor { get; private set; }

        protected PagamentoTransferencia() { } // EF Core

        public PagamentoTransferencia(
            Guid? faturaId,
            Guid? pedidoId,
            decimal valor,
            string moeda,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PagamentoTransferencia>()
                .Requires()
                .IsGreaterThan(valor, 0, nameof(Valor), "Valor deve ser maior que zero")
                .IsNotNullOrEmpty(moeda, nameof(Moeda), "Moeda é obrigatória")
            );

            if (faturaId == null && pedidoId == null)
            {
                AddNotification("Referencia", "FaturaId ou PedidoId deve ser informado.");
            }

            FaturaId = faturaId;
            PedidoId = pedidoId;
            Valor = valor;
            Moeda = moeda;
            Status = PagamentoTransferenciaStatus.Pending;
        }

        public void Aprovar(string analisadoPor, string alteradoPor)
        {
            Status = PagamentoTransferenciaStatus.Approved;
            AnalisadoPor = analisadoPor;
            DataAnalise = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Rejeitar(string analisadoPor, string justificativa, string alteradoPor)
        {
            AddNotifications(new Contract<PagamentoTransferencia>()
                .Requires()
                .IsNotNullOrEmpty(justificativa, nameof(Justificativa), "A justificativa é obrigatória na rejeição do comprovante.")
            );

            if (IsValid)
            {
                Status = PagamentoTransferenciaStatus.Rejected;
                AnalisadoPor = analisadoPor;
                Justificativa = justificativa;
                DataAnalise = DateTime.UtcNow;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
