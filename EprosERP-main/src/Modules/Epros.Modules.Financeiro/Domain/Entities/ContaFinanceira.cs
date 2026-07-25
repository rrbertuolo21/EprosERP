using System;
using System.Collections.Generic;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Conta financeira (razão/saldo) para bancos, caixa e movimentos (EF FIN-TS §11.5 conta_financeira).
    /// Submódulo de evolução — sobe desabilitado (ABAC nega por padrão). Isolamento por tenant via ContextBase.
    /// </summary>
    public class ContaFinanceira : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? NumeroConta { get; private set; }
        public Guid? TipoContaId { get; private set; }
        public string? Nota { get; private set; }
        public bool Fechada { get; private set; }
        public decimal? SaldoAbertura { get; private set; }

        private readonly List<TransacaoContaFinanceira> _transacoes = new();
        public IReadOnlyCollection<TransacaoContaFinanceira> Transacoes => _transacoes.AsReadOnly();

        protected ContaFinanceira() { } // EF Core

        public ContaFinanceira(string nome, string? numeroConta, Guid? tipoContaId, string? nota, decimal? saldoAbertura,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            NumeroConta = numeroConta;
            TipoContaId = tipoContaId;
            Nota = nota;
            SaldoAbertura = saldoAbertura;
            Fechada = false;
            Validar();
        }

        public void Alterar(string nome, string? numeroConta, Guid? tipoContaId, string? nota, string usuario)
        {
            Nome = nome; NumeroConta = numeroConta; TipoContaId = tipoContaId; Nota = nota;
            MarcarAlterado(usuario);
            Validar();
        }

        public void Fechar(string usuario) { Fechada = true; MarcarAlterado(usuario); }
        public void Reabrir(string usuario) { Fechada = false; MarcarAlterado(usuario); }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ContaFinanceira>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome da conta financeira é obrigatório [Origem: ContaFinanceira]"));
        }
    }

    /// <summary>Transação de débito/crédito da conta financeira (EF FIN-TS §11.6 transacao_conta_financeira).</summary>
    public class TransacaoContaFinanceira : EntidadeSaaSBase
    {
        public Guid ContaFinanceiraId { get; private set; }
        public decimal Valor { get; private set; }
        public ETipoTransacaoConta Tipo { get; private set; }
        public string? Subtipo { get; private set; } // opening_balance, fund_transfer, deposit
        public DateTime DataOperacao { get; private set; }
        public string? Nota { get; private set; }
        public Guid? TransacaoOrigemId { get; private set; }
        public Guid? PagamentoTransacaoId { get; private set; }
        public Guid? TransferenciaParId { get; private set; }
        public Guid? ContaTransferenciaId { get; private set; }

        public ContaFinanceira ContaFinanceira { get; private set; } = null!;

        protected TransacaoContaFinanceira() { } // EF Core

        public TransacaoContaFinanceira(Guid contaFinanceiraId, decimal valor, ETipoTransacaoConta tipo, string? subtipo,
            DateTime dataOperacao, string? nota, Guid? transacaoOrigemId, Guid? pagamentoTransacaoId,
            Guid? transferenciaParId, Guid? contaTransferenciaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContaFinanceiraId = contaFinanceiraId;
            Valor = valor;
            Tipo = tipo;
            Subtipo = subtipo;
            DataOperacao = dataOperacao;
            Nota = nota;
            TransacaoOrigemId = transacaoOrigemId;
            PagamentoTransacaoId = pagamentoTransacaoId;
            TransferenciaParId = transferenciaParId;
            ContaTransferenciaId = contaTransferenciaId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TransacaoContaFinanceira>()
                .Requires()
                .IsNotEmpty(ContaFinanceiraId, nameof(ContaFinanceiraId), "A conta financeira é obrigatória [Origem: TransacaoContaFinanceira]")
                .IsGreaterThan(Valor, 0, nameof(Valor), "O valor da transação deve ser maior que zero [Origem: TransacaoContaFinanceira]"));
        }
    }
}
