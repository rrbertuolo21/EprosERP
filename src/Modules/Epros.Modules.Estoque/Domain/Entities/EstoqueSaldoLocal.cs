using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// D2 (ESTOQUE) — saldo de estoque no GRÃO FINO: por Empresa + Produto + Local + Lote + Nº de Série.
    /// É a materialização da posição física real (WMS). Vive EM PARALELO ao agregado
    /// <see cref="EstoqueProduto"/> (chave Empresa+Produto), que permanece a verdade de saldo/custo já
    /// coberta pela suíte: para um mesmo (Empresa, Produto), a soma das quantidades das linhas
    /// EstoqueSaldoLocal reconcilia com <c>EstoqueProduto.QuantidadeSaldoEstoque</c>.
    ///
    /// Convenção de bucket (mesma disciplina de <c>MotorMovimentacaoEstoque.EmpresaPadrao</c>):
    /// dimensões não informadas usam valor "vazio" determinístico para manter a UNICIDADE do grão sob
    /// PostgreSQL (onde NULL não colide em índice único): LocalId = <see cref="System.Guid.Empty"/>,
    /// CodigoLote = "" e NumeroSerie = "". Assim "sem local/sem lote" é UMA linha estável, não N linhas.
    /// Backfill do histórico pode começar vazio (documentado): o grão passa a ser alimentado a partir das
    /// próximas movimentações; o agregado nunca deixa de ser correto.
    /// </summary>
    public class EstoqueSaldoLocal : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public Guid ProdutoId { get; private set; }

        /// <summary>Local/armazém/endereço operacional (WMS). <see cref="System.Guid.Empty"/> = local padrão (não informado).</summary>
        public Guid LocalId { get; private set; }

        /// <summary>Código do lote. "" quando o produto não controla lote (D10).</summary>
        public string CodigoLote { get; private set; } = string.Empty;

        /// <summary>Número de série. "" quando o produto não é serializado (D10).</summary>
        public string NumeroSerie { get; private set; } = string.Empty;

        public decimal QuantidadeSaldo { get; private set; }
        public decimal QuantidadeReservada { get; private set; }
        public decimal ValorSaldo { get; private set; }
        public decimal ValorCustoMedio { get; private set; }

        /// <summary>Validade da posição — insumo do FEFO (menor validade sai primeiro). Null = sem validade.</summary>
        public DateTime? DataValidade { get; private set; }

        // Navegação intra-módulo (o agregado é referenciado por chave lógica, sem FK física para não acoplar).
        public Produto? Produto { get; private set; }

        protected EstoqueSaldoLocal() { } // EF Core

        public EstoqueSaldoLocal(
            Guid empresaId, Guid produtoId, Guid? localId, string? codigoLote, string? numeroSerie,
            DateTime? dataValidade, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            ProdutoId = produtoId;
            LocalId = localId ?? Guid.Empty;
            CodigoLote = Normalizar(codigoLote);
            NumeroSerie = Normalizar(numeroSerie);
            DataValidade = dataValidade;
        }

        /// <summary>Normaliza dimensão textual: null/whitespace vira "" (bucket estável do grão).</summary>
        public static string Normalizar(string? valor) => string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();

        /// <summary>
        /// Credita quantidade e valor na posição e recalcula o custo médio móvel desta linha (espelha a
        /// regra do agregado: entrada sobre saldo zero assume o custo desta entrada; nunca divide por zero).
        /// </summary>
        public void Creditar(decimal quantidade, decimal valorTotal, DateTime? dataValidade, string usuario)
        {
            QuantidadeSaldo += quantidade;
            ValorSaldo += valorTotal;
            if (dataValidade.HasValue && (DataValidade == null || dataValidade.Value < DataValidade.Value))
                DataValidade = dataValidade; // guarda a menor validade conhecida da posição (FEFO)
            AtualizarValorCustoMedio();
            MarcarAlterado(usuario);
        }

        /// <summary>Debita quantidade e baixa o valor pelo custo informado (custo médio vigente do agregado).</summary>
        public void Debitar(decimal quantidade, decimal valorBaixado, string usuario)
        {
            QuantidadeSaldo -= quantidade;
            ValorSaldo -= valorBaixado;
            if (QuantidadeSaldo <= 0m) ValorSaldo = 0m; // posição zerada/negativa → valor zero (custo médio preservado)
            AtualizarValorCustoMedio();
            MarcarAlterado(usuario);
        }

        public void SomarReservado(decimal quantidade, string usuario)
        {
            QuantidadeReservada += quantidade;
            MarcarAlterado(usuario);
        }

        public void DiminuirReservado(decimal quantidade, string usuario)
        {
            QuantidadeReservada -= quantidade;
            if (QuantidadeReservada < 0m) QuantidadeReservada = 0m;
            MarcarAlterado(usuario);
        }

        /// <summary>Saldo livre para separação = disponível − reservado.</summary>
        public decimal QuantidadeDisponivel() => QuantidadeSaldo - QuantidadeReservada;

        private void AtualizarValorCustoMedio()
        {
            if (QuantidadeSaldo <= 0m) return; // D13: preserva último custo, nunca divide por zero
            ValorCustoMedio = ValorSaldo / QuantidadeSaldo;
        }
    }
}
