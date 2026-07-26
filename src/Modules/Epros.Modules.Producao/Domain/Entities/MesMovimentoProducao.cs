using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-MES — Par coordenado de entrada de produto acabado e consumo de materiais (prd_mes_movimento_producao).
    /// Fiel ao EF §17. MES-REG-014/015/016: entrada de produto acabado e baixa de materiais coordenadas.
    /// </summary>
    public class MesMovimentoProducao : EntidadeSaaSBase
    {
        public Guid OrdemId { get; private set; }
        public Guid? MovimentoPaiId { get; private set; }
        public ETipoMovimentoMes TipoMovimento { get; private set; }
        public EStatusMovimentoMes Status { get; private set; } = EStatusMovimentoMes.Pendente;
        public Guid ProdutoId { get; private set; }
        public Guid? VariacaoId { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal? ValorUnitario { get; private set; }
        public decimal? ValorTotal { get; private set; }
        public Guid LocalEstoqueId { get; private set; }
        public DateTime? ConfirmadoEm { get; private set; }

        protected MesMovimentoProducao() { } // EF Core

        public MesMovimentoProducao(
            Guid ordemId,
            ETipoMovimentoMes tipoMovimento,
            Guid produtoId,
            decimal quantidade,
            Guid localEstoqueId,
            string tenantId,
            string criadoPor,
            Guid? movimentoPaiId = null,
            Guid? variacaoId = null,
            decimal? valorUnitario = null,
            decimal? valorTotal = null)
            : base(tenantId, criadoPor)
        {
            OrdemId = ordemId;
            TipoMovimento = tipoMovimento;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            LocalEstoqueId = localEstoqueId;
            MovimentoPaiId = movimentoPaiId;
            VariacaoId = variacaoId;
            ValorUnitario = valorUnitario;
            ValorTotal = valorTotal;
            Status = EStatusMovimentoMes.Pendente;

            AddNotifications(new Contract<MesMovimentoProducao>()
                .Requires()
                .AreNotEquals(ordemId, Guid.Empty, nameof(OrdemId), "O movimento deve referenciar a ordem [Origem: MesMovimentoProducao].")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O movimento deve referenciar o produto [Origem: MesMovimentoProducao].")
                .AreNotEquals(localEstoqueId, Guid.Empty, nameof(LocalEstoqueId), "O movimento deve referenciar o local de estoque [Origem: MesMovimentoProducao].")
                .IsGreaterThan(quantidade, 0m, nameof(Quantidade), "A quantidade movimentada deve ser maior que zero [Origem: MesMovimentoProducao].")
            );
        }

        /// <summary>MES-REG-016: confirma o movimento de forma coordenada com o par.</summary>
        public void Confirmar(string alteradoPor)
        {
            Status = EStatusMovimentoMes.Confirmado;
            ConfirmadoEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }
    }
}
