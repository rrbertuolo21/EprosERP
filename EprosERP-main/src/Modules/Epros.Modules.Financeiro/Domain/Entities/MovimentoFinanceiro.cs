using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Movimento financeiro de caixa/banco/CP/CR/cheque/pagamento (EF FIN-TS §11.4 movimento_financeiro).
    /// FKs legadas portadas como Guid? (fontes mutuamente alternativas). Cross-module por Guid FK.
    /// </summary>
    public class MovimentoFinanceiro : EntidadeSaaSBase
    {
        public Guid? EmpresaId { get; private set; }
        public DateTime Emissao { get; private set; }
        public Guid? CaixaId { get; private set; }
        public Guid? ContaId { get; private set; }
        public decimal Credito { get; private set; }
        public decimal Debito { get; private set; }
        public Guid? ChequeId { get; private set; }
        public Guid? ContasPagarId { get; private set; }
        public Guid? ContasReceberId { get; private set; }
        public bool Conciliado { get; private set; }
        public Guid? PagamentoId { get; private set; }
        public bool Planejamento { get; private set; }

        protected MovimentoFinanceiro() { } // EF Core

        public MovimentoFinanceiro(Guid? empresaId, DateTime emissao, Guid? caixaId, Guid? contaId, decimal credito, decimal debito,
            Guid? chequeId, Guid? contasPagarId, Guid? contasReceberId, Guid? pagamentoId, bool planejamento, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            Emissao = emissao;
            CaixaId = caixaId;
            ContaId = contaId;
            Credito = credito;
            Debito = debito;
            ChequeId = chequeId;
            ContasPagarId = contasPagarId;
            ContasReceberId = contasReceberId;
            PagamentoId = pagamentoId;
            Planejamento = planejamento;
            Conciliado = false;
            Validar();
        }

        public void Conciliar(string usuario) { Conciliado = true; MarcarAlterado(usuario); }
        public void DesconciliarMovimento(string usuario) { Conciliado = false; MarcarAlterado(usuario); }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<MovimentoFinanceiro>()
                .Requires()
                .IsGreaterThan(Emissao, DateTime.MinValue, nameof(Emissao), "A data de emissão é obrigatória [Origem: MovimentoFinanceiro]"));
            if (Credito <= 0 && Debito <= 0)
                AddNotification(nameof(Credito), "Informe crédito ou débito maior que zero [Origem: MovimentoFinanceiro]");
        }
    }
}
