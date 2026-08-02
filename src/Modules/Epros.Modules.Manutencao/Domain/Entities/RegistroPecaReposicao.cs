using System;
using System.Collections.Generic;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Manutencao.Domain.Entities
{
    /// <summary>MAN-PEC — Registro de pecas de reposicao (agregado raiz). EF 11.1.</summary>
    public class RegistroPecaReposicao : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public EStatusRegistroManutencao Status { get; private set; } = EStatusRegistroManutencao.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public Guid? OrdemServicoId { get; private set; }
        public Guid? PlanoPreventivoId { get; private set; }
        public int Versao { get; private set; } = 1;

        public List<ItemPecaReposicao> Itens { get; private set; } = new();

        protected RegistroPecaReposicao() { }

        public RegistroPecaReposicao(
            string codigo,
            string descricao,
            Guid responsavelId,
            Guid? ordemServicoId,
            Guid? planoPreventivoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            ResponsavelId = responsavelId;
            OrdemServicoId = ordemServicoId;
            PlanoPreventivoId = planoPreventivoId;
            Status = EStatusRegistroManutencao.Rascunho;
            Versao = 1;
            Validar();
        }

        public void AdicionarItem(ItemPecaReposicao item, string usuario)
        {
            if (!item.IsValid) { AddNotifications(item.Notifications); return; }
            Itens.Add(item);
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Ativar(string usuario)
        {
            if (Status != EStatusRegistroManutencao.Rascunho && Status != EStatusRegistroManutencao.EmAnalise && Status != EStatusRegistroManutencao.Suspenso)
            {
                AddNotification(nameof(Status), "Somente registros em rascunho, analise ou suspensos podem ser ativados.");
                return;
            }
            Status = EStatusRegistroManutencao.Ativo;
            Versao++;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<RegistroPecaReposicao>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O codigo do registro e obrigatorio [Origem: RegistroPecaReposicao].")
                .IsLowerOrEqualsThan(Codigo?.Length ?? 0, 30, nameof(Codigo), "O codigo deve ter no maximo 30 caracteres.")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descricao e obrigatoria [Origem: RegistroPecaReposicao].")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsavel e obrigatorio."));
        }
    }

    /// <summary>MAN-PEC — Item de peca. EF 11.2.</summary>
    public class ItemPecaReposicao : EntidadeSaaSBase
    {
        public Guid RegistroId { get; private set; }
        public Guid? OrdemServicoId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid? GradeId { get; private set; }
        public int Sequencia { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal QuantidadeEntregue { get; private set; }
        public Guid? TabelaPrecoId { get; private set; }
        public decimal ValorProduto { get; private set; }
        public decimal ValorVenda { get; private set; }
        public decimal ValorCusto { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public decimal ValorSubtotal { get; private set; }
        public decimal TaxaDesconto { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorTotal { get; private set; }
        public ETipoSaidaItem? TipoSaida { get; private set; }
        public string? Informacao { get; private set; }
        public string? Observacao { get; private set; }
        public EStatusItemPeca StatusItem { get; private set; } = EStatusItemPeca.Rascunho;

        public List<ReservaPeca> Reservas { get; private set; } = new();
        public List<MovimentoPeca> Movimentos { get; private set; } = new();

        protected ItemPecaReposicao() { }

        public ItemPecaReposicao(
            Guid registroId,
            Guid produtoId,
            int sequencia,
            decimal quantidade,
            Guid? ordemServicoId,
            Guid? gradeId,
            decimal valorUnitario,
            decimal taxaDesconto,
            ETipoSaidaItem? tipoSaida,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            RegistroId = registroId;
            ProdutoId = produtoId;
            Sequencia = sequencia;
            Quantidade = quantidade;
            QuantidadeEntregue = 0m; // RN: valor inicial zero.
            OrdemServicoId = ordemServicoId;
            GradeId = gradeId;
            ValorUnitario = valorUnitario;
            TaxaDesconto = taxaDesconto;
            TipoSaida = tipoSaida;
            StatusItem = EStatusItemPeca.Rascunho;
            RecalcularTotais();

            AddNotifications(new Contract<ItemPecaReposicao>()
                .Requires()
                .AreNotEquals(registroId, Guid.Empty, nameof(RegistroId), "O registro e obrigatorio.")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O produto e obrigatorio.")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A quantidade deve ser maior que zero."));
        }

        private void RecalcularTotais()
        {
            ValorSubtotal = Quantidade * ValorUnitario;
            ValorDesconto = ValorSubtotal * (TaxaDesconto / 100m);
            ValorTotal = ValorSubtotal - ValorDesconto;
        }

        public void RegistrarEntrega(decimal quantidadeEntregue, string usuario)
        {
            if (quantidadeEntregue <= 0)
            {
                AddNotification(nameof(QuantidadeEntregue), "A quantidade entregue deve ser maior que zero.");
                return;
            }
            if (QuantidadeEntregue + quantidadeEntregue > Quantidade)
            {
                AddNotification(nameof(QuantidadeEntregue), "A quantidade entregue nao pode exceder a quantidade solicitada.");
                return;
            }
            QuantidadeEntregue += quantidadeEntregue;
            StatusItem = QuantidadeEntregue >= Quantidade ? EStatusItemPeca.EntregueTotal : EStatusItemPeca.EntregueParcial;
            MarcarAlterado(usuario);
        }

        // MAN-PEC RN-PEC-007/008: devolucao correlacionada reduz o entregue (movimento inverso).
        public void RegistrarDevolucao(decimal quantidadeDevolvida, string usuario)
        {
            if (quantidadeDevolvida <= 0)
            {
                AddNotification(nameof(QuantidadeEntregue), "A quantidade devolvida deve ser maior que zero.");
                return;
            }
            if (quantidadeDevolvida > QuantidadeEntregue)
            {
                AddNotification(nameof(QuantidadeEntregue), "A devolucao nao pode exceder a quantidade entregue.");
                return;
            }
            QuantidadeEntregue -= quantidadeDevolvida;
            StatusItem = QuantidadeEntregue <= 0m
                ? EStatusItemPeca.Devolvido
                : EStatusItemPeca.EntregueParcial;
            MarcarAlterado(usuario);
        }

        // MAN-PEC: marca o item como reservado (ciclo reservar -> consumir).
        public void MarcarReservado(string usuario)
        {
            if (StatusItem == EStatusItemPeca.Rascunho)
                StatusItem = EStatusItemPeca.Reservado;
            MarcarAlterado(usuario);
        }

        public decimal SaldoAConsumir() => Quantidade - QuantidadeEntregue;
    }

    /// <summary>MAN-PEC — Reserva de peca. EF 11.3.</summary>
    public class ReservaPeca : EntidadeSaaSBase
    {
        public Guid ItemId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid? GradeId { get; private set; }
        public Guid? LocalEstoqueId { get; private set; }
        public decimal QuantidadeReservada { get; private set; }
        public EStatusReservaPeca StatusReserva { get; private set; } = EStatusReservaPeca.Pendente;
        public DateTime DataReserva { get; private set; }
        public Guid UsuarioId { get; private set; }

        protected ReservaPeca() { }

        public ReservaPeca(Guid itemId, Guid produtoId, decimal quantidadeReservada, Guid usuarioId, Guid? gradeId, Guid? localEstoqueId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ItemId = itemId;
            ProdutoId = produtoId;
            QuantidadeReservada = quantidadeReservada;
            UsuarioId = usuarioId;
            GradeId = gradeId;
            LocalEstoqueId = localEstoqueId;
            StatusReserva = EStatusReservaPeca.Pendente;
            DataReserva = DateTime.UtcNow;

            AddNotifications(new Contract<ReservaPeca>()
                .Requires()
                .AreNotEquals(itemId, Guid.Empty, nameof(ItemId), "O item e obrigatorio.")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O produto e obrigatorio.")
                .IsGreaterThan(quantidadeReservada, 0, nameof(QuantidadeReservada), "A quantidade reservada deve ser maior que zero."));
        }

        public void Reservar(string usuario) { StatusReserva = EStatusReservaPeca.Reservada; MarcarAlterado(usuario); }
        public void Liberar(string usuario) { StatusReserva = EStatusReservaPeca.Liberada; MarcarAlterado(usuario); }
        public void Cancelar(string usuario) { StatusReserva = EStatusReservaPeca.Cancelada; MarcarAlterado(usuario); }
    }

    /// <summary>MAN-PEC — Movimento de peca (baixa/devolucao). EF 11.4.</summary>
    public class MovimentoPeca : EntidadeSaaSBase
    {
        public Guid ItemId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid? GradeId { get; private set; }
        public ETipoMovimentoPeca TipoMovimento { get; private set; }
        public decimal Quantidade { get; private set; }
        public Guid? MovimentoEstoqueId { get; private set; }
        public EStatusMovimentoPeca StatusMovimento { get; private set; } = EStatusMovimentoPeca.Pendente;
        public string? InformacaoMovimento { get; private set; }
        public DateTime DataMovimento { get; private set; }
        public string? Erro { get; private set; }

        protected MovimentoPeca() { }

        public MovimentoPeca(Guid itemId, Guid produtoId, ETipoMovimentoPeca tipoMovimento, decimal quantidade, string? informacaoMovimento, Guid? gradeId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ItemId = itemId;
            ProdutoId = produtoId;
            TipoMovimento = tipoMovimento;
            Quantidade = quantidade;
            InformacaoMovimento = informacaoMovimento;
            GradeId = gradeId;
            StatusMovimento = EStatusMovimentoPeca.Pendente;
            DataMovimento = DateTime.UtcNow;

            AddNotifications(new Contract<MovimentoPeca>()
                .Requires()
                .AreNotEquals(itemId, Guid.Empty, nameof(ItemId), "O item e obrigatorio.")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O produto e obrigatorio.")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A quantidade movimentada deve ser maior que zero."));
        }

        public void Confirmar(Guid? movimentoEstoqueId, string usuario)
        {
            StatusMovimento = EStatusMovimentoPeca.Confirmado;
            MovimentoEstoqueId = movimentoEstoqueId;
            MarcarAlterado(usuario);
        }

        public void RegistrarFalha(string erro, string usuario)
        {
            StatusMovimento = EStatusMovimentoPeca.Falhou;
            Erro = erro;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>MAN-PEC — Politica de reposicao (min/max/ponto de pedido). EF 11.5.</summary>
    public class PoliticaReposicao : EntidadeSaaSBase
    {
        public Guid ProdutoId { get; private set; }
        public Guid? GradeId { get; private set; }
        public Guid? LocalEstoqueId { get; private set; }
        public decimal? EstoqueMinimo { get; private set; }
        public decimal? EstoqueMaximo { get; private set; }
        public decimal? PontoPedido { get; private set; }
        public int? LeadTimeDias { get; private set; }
        public string? Criticidade { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected PoliticaReposicao() { }

        public PoliticaReposicao(Guid produtoId, decimal? estoqueMinimo, decimal? estoqueMaximo, decimal? pontoPedido, int? leadTimeDias, string? criticidade, Guid? gradeId, Guid? localEstoqueId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProdutoId = produtoId;
            EstoqueMinimo = estoqueMinimo;
            EstoqueMaximo = estoqueMaximo;
            PontoPedido = pontoPedido;
            LeadTimeDias = leadTimeDias;
            Criticidade = criticidade;
            GradeId = gradeId;
            LocalEstoqueId = localEstoqueId;
            Ativo = true;

            AddNotifications(new Contract<PoliticaReposicao>()
                .Requires()
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O produto e obrigatorio.")
                .IsTrue(
                    !estoqueMinimo.HasValue || !estoqueMaximo.HasValue || estoqueMaximo.Value >= estoqueMinimo.Value,
                    nameof(EstoqueMaximo),
                    "O estoque maximo deve ser maior ou igual ao minimo."));
        }
    }

    /// <summary>MAN-PEC — Kit preventivo (pecas previstas por plano). EF 11.6.</summary>
    public class KitPreventivo : EntidadeSaaSBase
    {
        public Guid PlanoPreventivoId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid? GradeId { get; private set; }
        public decimal QuantidadePrevista { get; private set; }
        public int? Sequencia { get; private set; }
        public string? Observacao { get; private set; }
        public bool Ativo { get; private set; } = true;

        protected KitPreventivo() { }

        public KitPreventivo(Guid planoPreventivoId, Guid produtoId, decimal quantidadePrevista, int? sequencia, string? observacao, Guid? gradeId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PlanoPreventivoId = planoPreventivoId;
            ProdutoId = produtoId;
            QuantidadePrevista = quantidadePrevista;
            Sequencia = sequencia;
            Observacao = observacao;
            GradeId = gradeId;
            Ativo = true;

            AddNotifications(new Contract<KitPreventivo>()
                .Requires()
                .AreNotEquals(planoPreventivoId, Guid.Empty, nameof(PlanoPreventivoId), "O plano preventivo e obrigatorio.")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O produto e obrigatorio.")
                .IsGreaterThan(quantidadePrevista, 0, nameof(QuantidadePrevista), "A quantidade prevista deve ser maior que zero."));
        }
    }
}
