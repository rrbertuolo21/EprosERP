using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    public class Caixa : EntidadeSaaSBase
    {
        public string OperadorId { get; private set; } = string.Empty;
        public ECaixaStatus Status { get; private set; } = ECaixaStatus.Aberto;
        public decimal SaldoAbertura { get; private set; }
        public decimal? SaldoFechamento { get; private set; }
        public decimal? DiferencaFechamento { get; private set; }
        public DateTime? FechadoEm { get; private set; }

        protected Caixa() { } // EF Core

        public Caixa(Guid id, Guid syncId, string operadorId, decimal saldoAbertura, string tenantId, string criadoPor, DateTime criadoEm)
            : base(id, syncId, tenantId, criadoPor, criadoEm)
        {
            AddNotifications(new Contract<Caixa>()
                .Requires()
                .IsNotNullOrEmpty(operadorId, nameof(OperadorId), "O Operador do caixa é obrigatório.")
                .IsGreaterThan(saldoAbertura, -0.01m, nameof(SaldoAbertura), "O saldo de abertura não pode ser negativo.")
            );

            OperadorId = operadorId;
            SaldoAbertura = saldoAbertura;
            Status = ECaixaStatus.Aberto;
        }

        public void Fechar(decimal saldoInformado, decimal saldoCalculado, string alteradoPor)
        {
            if (Status == ECaixaStatus.Fechado)
            {
                AddNotification(nameof(Status), "O caixa já está fechado.");
                return;
            }

            AddNotifications(new Contract<Caixa>()
                .Requires()
                .IsGreaterThan(saldoInformado, -0.01m, nameof(SaldoFechamento), "O saldo de fechamento não pode ser negativo.")
            );

            Status = ECaixaStatus.Fechado;
            SaldoFechamento = saldoInformado;
            DiferencaFechamento = saldoInformado - saldoCalculado;
            FechadoEm = DateTime.UtcNow;

            MarcarAlterado(alteradoPor);
        }
    }
}
