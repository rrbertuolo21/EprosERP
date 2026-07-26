using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Cabeçalho de lançamento contábil (EF FIN-CGL §5.10 / §8.8 / §11 cgl_lancamento).
    /// Regra-chave: partida dobrada — total de débitos deve igualar total de créditos e ser maior que zero
    /// (CGL-VAL-015 / CGL-VAL-016). Lançamento em rascunho não impacta saldos; confirmação exige balanceamento.
    /// </summary>
    public class LancamentoContabil : EntidadeSaaSBase
    {
        public Guid? PeriodoContabilId { get; private set; }
        public string? NumeroLancamento { get; private set; }
        public DateTime Data { get; private set; }
        public EEstadoLancamentoContabil Estado { get; private set; } = EEstadoLancamentoContabil.Rascunho;
        public string? Historico { get; private set; }
        public Guid? LancamentoEstornoId { get; private set; }

        private readonly List<LancamentoContabilLinha> _linhas = new();
        public IReadOnlyCollection<LancamentoContabilLinha> Linhas => _linhas;

        public decimal TotalDebitos => _linhas.Where(l => l.DeletadoEm == null).Sum(l => l.Debito);
        public decimal TotalCreditos => _linhas.Where(l => l.DeletadoEm == null).Sum(l => l.Credito);
        public bool Balanceado => TotalDebitos == TotalCreditos && TotalDebitos > 0m;

        protected LancamentoContabil() { } // EF Core

        public LancamentoContabil(Guid? periodoContabilId, string? numeroLancamento, DateTime data, string? historico, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PeriodoContabilId = periodoContabilId;
            NumeroLancamento = numeroLancamento;
            Data = data;
            Historico = historico;
            Estado = EEstadoLancamentoContabil.Rascunho;
            Validar();
        }

        public void AdicionarLinha(Guid contaContabilId, decimal debito, decimal credito, string? historico, string usuario)
        {
            if (Estado != EEstadoLancamentoContabil.Rascunho)
            {
                AddNotification(nameof(Estado), "Só é possível adicionar linhas em lançamento em rascunho.");
                return;
            }
            var linha = new LancamentoContabilLinha(Id, contaContabilId, debito, credito, historico, TenantId, usuario);
            _linhas.Add(linha);
            if (!linha.IsValid) AddNotifications(linha);
        }

        /// <summary>Confirma o lançamento (Rascunho → Confirmado). Valida balanceamento (CGL-VAL-015/016).</summary>
        public void Confirmar(string usuario)
        {
            if (Estado != EEstadoLancamentoContabil.Rascunho)
            {
                AddNotification(nameof(Estado), "Somente lançamento em rascunho pode ser confirmado.");
                return;
            }
            ValidarBalanceamento();
            if (!IsValid) return;
            Estado = EEstadoLancamentoContabil.Confirmado;
            MarcarAlterado(usuario);
        }

        /// <summary>Estorna um lançamento confirmado (Confirmado → Estornado). Não faz exclusão física.</summary>
        public void Estornar(string usuario)
        {
            if (Estado != EEstadoLancamentoContabil.Confirmado)
            {
                AddNotification(nameof(Estado), "Somente lançamento confirmado pode ser estornado.");
                return;
            }
            Estado = EEstadoLancamentoContabil.Estornado;
            MarcarAlterado(usuario);
        }

        /// <summary>Cancela um lançamento em rascunho (Rascunho → Cancelado).</summary>
        public void Cancelar(string usuario)
        {
            if (Estado != EEstadoLancamentoContabil.Rascunho)
            {
                AddNotification(nameof(Estado), "Somente lançamento em rascunho pode ser cancelado.");
                return;
            }
            Estado = EEstadoLancamentoContabil.Cancelado;
            MarcarAlterado(usuario);
        }

        public void ValidarBalanceamento()
        {
            Clear();
            if (TotalDebitos <= 0m && TotalCreditos <= 0m)
                AddNotification(nameof(Linhas), "Lançamento deve possuir débito ou crédito maior que zero.");
            else if (TotalDebitos != TotalCreditos)
                AddNotification(nameof(Linhas), "Total de débitos deve ser igual ao total de créditos.");
        }

        public void Validar()
        {
            AddNotifications(new Contract<LancamentoContabil>()
                .Requires()
            );
        }
    }
}
