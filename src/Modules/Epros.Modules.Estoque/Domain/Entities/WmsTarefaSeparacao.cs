using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// D5 (WMS operacional) — tarefa de separação/conferência que MOVE o grão fino (EstoqueSaldoLocal).
    /// Ciclo: Pendente (reserva a posição de origem) → Separada → Conferida (baixa a origem e, se houver,
    /// credita a posição de destino) | Cancelada (libera a reserva). O saldo agregado (EstoqueProduto) NÃO é
    /// tocado por movimento interno de armazém — apenas a posição física muda; o total do produto na empresa
    /// permanece constante, preservando a verdade da suíte.
    /// A orquestração sobre EstoqueSaldoLocal é feita em <c>WmsSeparacaoService</c>.
    /// </summary>
    public class WmsTarefaSeparacao : EntidadeSaaSBase
    {
        public Guid ArmazemId { get; private set; }
        public Guid EmpresaId { get; private set; }
        public Guid ProdutoId { get; private set; }

        public Guid LocalOrigemId { get; private set; }
        public Guid? LocalDestinoId { get; private set; }
        public string CodigoLote { get; private set; } = string.Empty;
        public string NumeroSerie { get; private set; } = string.Empty;

        public decimal QuantidadeSolicitada { get; private set; }
        public decimal QuantidadeConferida { get; private set; }
        public EStatusTarefaSeparacao Status { get; private set; } = EStatusTarefaSeparacao.Pendente;
        public string? Observacao { get; private set; }

        protected WmsTarefaSeparacao() { } // EF Core

        public WmsTarefaSeparacao(Guid armazemId, Guid empresaId, Guid produtoId, Guid localOrigemId, Guid? localDestinoId,
            string? codigoLote, string? numeroSerie, decimal quantidadeSolicitada, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ArmazemId = armazemId;
            EmpresaId = empresaId;
            ProdutoId = produtoId;
            LocalOrigemId = localOrigemId;
            LocalDestinoId = localDestinoId;
            CodigoLote = EstoqueSaldoLocal.Normalizar(codigoLote);
            NumeroSerie = EstoqueSaldoLocal.Normalizar(numeroSerie);
            QuantidadeSolicitada = quantidadeSolicitada;
            Observacao = observacao;
            Status = EStatusTarefaSeparacao.Pendente;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<WmsTarefaSeparacao>()
                .Requires()
                .IsNotEmpty(ProdutoId, nameof(ProdutoId), "O produto da tarefa é obrigatório [Origem: WmsTarefaSeparacao]")
                .IsNotEmpty(LocalOrigemId, nameof(LocalOrigemId), "O local de origem da separação é obrigatório [Origem: WmsTarefaSeparacao]")
                .IsGreaterThan(QuantidadeSolicitada, 0m, nameof(QuantidadeSolicitada), "A quantidade a separar deve ser maior que zero [Origem: WmsTarefaSeparacao]"));
        }

        public void MarcarSeparada(string usuario)
        {
            if (Status != EStatusTarefaSeparacao.Pendente)
                throw new InvalidOperationException("Só uma tarefa Pendente pode ser marcada como Separada.");
            Status = EStatusTarefaSeparacao.Separada;
            MarcarAlterado(usuario);
        }

        /// <summary>Registra a conferência (quantidade efetivamente separada) e conclui a tarefa.</summary>
        public void MarcarConferida(decimal quantidadeConferida, string usuario)
        {
            if (Status == EStatusTarefaSeparacao.Conferida || Status == EStatusTarefaSeparacao.Cancelada)
                throw new InvalidOperationException("Tarefa já encerrada.");
            if (quantidadeConferida <= 0m || quantidadeConferida > QuantidadeSolicitada)
                throw new InvalidOperationException("Quantidade conferida inválida (deve ser >0 e ≤ solicitada).");
            QuantidadeConferida = quantidadeConferida;
            Status = EStatusTarefaSeparacao.Conferida;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string usuario)
        {
            if (Status == EStatusTarefaSeparacao.Conferida)
                throw new InvalidOperationException("Tarefa já conferida não pode ser cancelada.");
            Status = EStatusTarefaSeparacao.Cancelada;
            MarcarAlterado(usuario);
        }
    }
}
