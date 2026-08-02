using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Balancete consolidado de um grupo por período (EF FIN-CON §10.5 con_balancete_consolidado).
    /// Agrega linhas por conta (con_balancete_linha).
    /// </summary>
    public class BalanceteConsolidado : EntidadeSaaSBase
    {
        public Guid GrupoConsolidacaoId { get; private set; }
        public string Periodo { get; private set; } = string.Empty;
        public EEstadoConsolidacao Estado { get; private set; } = EEstadoConsolidacao.Provisorio;
        public decimal? TotalDebito { get; private set; }
        public decimal? TotalCredito { get; private set; }
        public decimal? SaldoFinal { get; private set; }
        /// <summary>Total das eliminações intercompany já aplicadas a este balancete (agregação).</summary>
        public decimal? TotalEliminacoes { get; private set; }
        public bool EliminacoesAplicadas { get; private set; }

        private readonly List<BalanceteLinha> _linhas = new();
        public IReadOnlyCollection<BalanceteLinha> Linhas => _linhas.AsReadOnly();

        protected BalanceteConsolidado() { } // EF Core

        public BalanceteConsolidado(Guid grupoConsolidacaoId, string periodo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            GrupoConsolidacaoId = grupoConsolidacaoId;
            Periodo = periodo;
            Estado = EEstadoConsolidacao.Provisorio;
            Validar();
        }

        public void AdicionarLinha(Guid? contaId, string? codigoConta, string? nomeConta, decimal? saldoAnterior, decimal? debito, decimal? credito, decimal? saldoFinal, string tenantId, string criadoPor)
        {
            _linhas.Add(new BalanceteLinha(Id, contaId, codigoConta, nomeConta, saldoAnterior, debito, credito, saldoFinal, tenantId, criadoPor));
        }

        public void AtualizarTotais(decimal totalDebito, decimal totalCredito, decimal saldoFinal, string usuario)
        {
            TotalDebito = totalDebito;
            TotalCredito = totalCredito;
            SaldoFinal = saldoFinal;
            MarcarAlterado(usuario);
        }

        /// <summary>
        /// Adiciona uma linha de eliminação intercompany (partida balanceada débito = crédito) e recomputa
        /// os totais a partir de TODAS as linhas. A eliminação preserva a partida dobrada do balancete; a
        /// conta contábil de cada perna (a receber/a pagar intercompany) é parametrização — // valida-contador.
        /// </summary>
        public BalanceteLinha AplicarLinhaEliminacao(decimal valor, string? codigoConta, string? descricao, string tenantId, string usuario)
        {
            var linha = new BalanceteLinha(Id, null, codigoConta ?? "ELIM", descricao, null, valor, valor, 0m, tenantId, usuario);
            _linhas.Add(linha);
            TotalEliminacoes = (TotalEliminacoes ?? 0m) + valor;
            RecomputarTotais(usuario);
            return linha;
        }

        /// <summary>Recomputa TotalDebito/Credito/SaldoFinal somando todas as linhas (agregação).</summary>
        public void RecomputarTotais(string usuario)
        {
            var debito = _linhas.Sum(l => l.Debito ?? 0m);
            var credito = _linhas.Sum(l => l.Credito ?? 0m);
            TotalDebito = debito;
            TotalCredito = credito;
            SaldoFinal = debito - credito;
            MarcarAlterado(usuario);
        }

        public void MarcarEliminacoesAplicadas(string usuario)
        {
            EliminacoesAplicadas = true;
            MarcarAlterado(usuario);
        }

        public void Publicar(string usuario)
        {
            Estado = EEstadoConsolidacao.Publicado;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<BalanceteConsolidado>()
                .Requires()
                .IsNotEmpty(GrupoConsolidacaoId, nameof(GrupoConsolidacaoId), "O grupo de consolidação é obrigatório [Origem: BalanceteConsolidado]")
                .IsNotNullOrEmpty(Periodo, nameof(Periodo), "O período é obrigatório [Origem: BalanceteConsolidado]"));
        }
    }

    /// <summary>Linha de balancete consolidado por conta (EF FIN-CON §10.6 con_balancete_linha).</summary>
    public class BalanceteLinha : EntidadeSaaSBase
    {
        public Guid BalanceteConsolidadoId { get; private set; }
        public Guid? ContaId { get; private set; }
        public string? CodigoConta { get; private set; }
        public string? NomeConta { get; private set; }
        public decimal? SaldoAnterior { get; private set; }
        public decimal? Debito { get; private set; }
        public decimal? Credito { get; private set; }
        public decimal? SaldoFinal { get; private set; }

        public BalanceteConsolidado BalanceteConsolidado { get; private set; } = null!;

        protected BalanceteLinha() { } // EF Core

        public BalanceteLinha(Guid balanceteConsolidadoId, Guid? contaId, string? codigoConta, string? nomeConta,
            decimal? saldoAnterior, decimal? debito, decimal? credito, decimal? saldoFinal, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            BalanceteConsolidadoId = balanceteConsolidadoId;
            ContaId = contaId;
            CodigoConta = codigoConta;
            NomeConta = nomeConta;
            SaldoAnterior = saldoAnterior;
            Debito = debito;
            Credito = credito;
            SaldoFinal = saldoFinal;
        }
    }
}
