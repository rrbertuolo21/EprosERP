using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Saldo de abertura por conta contábil (EF FIN-CGL §5.9 / §11.7 cgl_saldo_abertura).
    /// Marca o lançamento inicial de saldo por conta e data.
    /// </summary>
    public class SaldoAbertura : EntidadeSaaSBase
    {
        public string? Numero { get; private set; }
        public DateTime Data { get; private set; }
        public Guid ContaContabilId { get; private set; }
        public string? CodigoConta { get; private set; }
        public ETipoSaldoContabil TipoSaldo { get; private set; }
        public decimal Valor { get; private set; }
        public string Historico { get; private set; } = string.Empty;
        public bool Contabilizado { get; private set; }
        public bool Aprovado { get; private set; }
        public bool SaldoInicial { get; private set; } = true;

        public ContaContabil ContaContabil { get; private set; } = null!;

        protected SaldoAbertura() { } // EF Core

        public SaldoAbertura(string? numero, DateTime data, Guid contaContabilId, string? codigoConta, ETipoSaldoContabil tipoSaldo, decimal valor, string historico, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Numero = numero;
            Data = data;
            ContaContabilId = contaContabilId;
            CodigoConta = codigoConta;
            TipoSaldo = tipoSaldo;
            Valor = valor;
            Historico = historico;
            SaldoInicial = true;
            Validar();
        }

        public void Contabilizar(string usuario) { Contabilizado = true; MarcarAlterado(usuario); }
        public void Aprovar(string usuario) { Aprovado = true; MarcarAlterado(usuario); }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<SaldoAbertura>()
                .Requires()
                .AreNotEquals(ContaContabilId, Guid.Empty, nameof(ContaContabilId), "A conta contábil do saldo é obrigatória.")
                .IsNotNullOrEmpty(Historico, nameof(Historico), "O histórico do saldo é obrigatório.")
            );
        }
    }
}
