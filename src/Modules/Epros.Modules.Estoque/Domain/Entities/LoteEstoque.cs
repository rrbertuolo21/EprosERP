using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Lote rastreável de produto (EF Rastreabilidade §7.2 `rlt_lote`). [DECIDIDO D10] o controle de lote é
    /// configurável por tipo de produto. RLT-003: código de lote obrigatório quando o produto controla lote.
    /// Os saldos por lote (recebida/disponível/bloqueada/consumida — §7.3 `rlt_lote_saldo`) estão consolidados
    /// aqui por local/empresa enquanto a granularidade saldo-por-posição (D2) não é materializada — decisão de
    /// produto registrada em DECISOES-PENDENTES-RAFAEL.md.
    /// </summary>
    public class LoteEstoque : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid? LocalId { get; private set; }
        public Guid? FichaEntradaId { get; private set; }
        public string CodigoLote { get; private set; } = string.Empty;
        public DateTime? DataFabricacao { get; private set; }
        public DateTime? DataValidade { get; private set; }
        public DateTime? DataRecebimento { get; private set; }
        public EOrigemLote Origem { get; private set; }
        public EStatusLoteRastreabilidade Status { get; private set; } = EStatusLoteRastreabilidade.Ativo;
        public string? Observacao { get; private set; }

        // Saldos por lote (§7.3 folded)
        public decimal QuantidadeRecebida { get; private set; }
        public decimal QuantidadeDisponivel { get; private set; }
        public decimal QuantidadeBloqueada { get; private set; }
        public decimal QuantidadeConsumida { get; private set; }

        protected LoteEstoque() { } // EF Core

        public LoteEstoque(Guid empresaId, Guid produtoId, string codigoLote, decimal quantidadeRecebida,
            Guid? localId, Guid? fichaEntradaId, DateTime? dataFabricacao, DateTime? dataValidade, EOrigemLote origem,
            string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            ProdutoId = produtoId;
            CodigoLote = codigoLote ?? string.Empty;
            QuantidadeRecebida = quantidadeRecebida;
            QuantidadeDisponivel = quantidadeRecebida;
            QuantidadeBloqueada = 0m;
            QuantidadeConsumida = 0m;
            LocalId = localId;
            FichaEntradaId = fichaEntradaId;
            DataFabricacao = dataFabricacao;
            DataValidade = dataValidade;
            DataRecebimento = origem == EOrigemLote.Compra ? DateTime.UtcNow : (DateTime?)null;
            Origem = origem;
            Status = EStatusLoteRastreabilidade.Ativo;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LoteEstoque>()
                .Requires()
                .IsNotEmpty(ProdutoId, nameof(ProdutoId), "O produto do lote é obrigatório [Origem: LoteEstoque]")
                .IsNotNullOrEmpty(CodigoLote, nameof(CodigoLote), "O código de lote é obrigatório quando o produto controla lote [RLT-003] [Origem: LoteEstoque]")
                .IsGreaterThan(QuantidadeRecebida, 0m, nameof(QuantidadeRecebida), "A quantidade recebida do lote deve ser maior que zero [Origem: LoteEstoque]"));
        }

        /// <summary>True se o lote pode ser consumido na saída (RLT-007/008): ativo e não bloqueado/recall/vencido.</summary>
        public bool PodeConsumir() => Status == EStatusLoteRastreabilidade.Ativo && QuantidadeDisponivel > 0m;

        /// <summary>RLT-017: a quantidade consumida não pode exceder a disponível.</summary>
        public void Consumir(decimal quantidade, string usuario)
        {
            if (quantidade <= 0m) throw new InvalidOperationException("Quantidade de consumo inválida.");
            if (Status != EStatusLoteRastreabilidade.Ativo) throw new InvalidOperationException("Lote não está ativo para consumo [RLT-007].");
            if (quantidade > QuantidadeDisponivel) throw new InvalidOperationException("Quantidade consumida excede a disponível do lote [RLT-017].");
            QuantidadeDisponivel -= quantidade;
            QuantidadeConsumida += quantidade;
            if (QuantidadeDisponivel == 0m && QuantidadeBloqueada == 0m) Status = EStatusLoteRastreabilidade.Consumido;
            MarcarAlterado(usuario);
        }

        /// <summary>RLT-012/018: bloqueia (parte do) saldo do lote, exigindo motivo (no handler).</summary>
        public void Bloquear(decimal? quantidade, string usuario)
        {
            var qtd = quantidade ?? QuantidadeDisponivel;
            if (qtd > QuantidadeDisponivel) throw new InvalidOperationException("Quantidade bloqueada excede o saldo do lote [RLT-018].");
            QuantidadeDisponivel -= qtd;
            QuantidadeBloqueada += qtd;
            Status = EStatusLoteRastreabilidade.Bloqueado;
            MarcarAlterado(usuario);
        }

        public void Desbloquear(string usuario)
        {
            QuantidadeDisponivel += QuantidadeBloqueada;
            QuantidadeBloqueada = 0m;
            Status = QuantidadeDisponivel > 0m ? EStatusLoteRastreabilidade.Ativo : EStatusLoteRastreabilidade.Consumido;
            MarcarAlterado(usuario);
        }

        public void MarcarVencido(string usuario)
        {
            Status = EStatusLoteRastreabilidade.Vencido;
            MarcarAlterado(usuario);
        }
    }
}
