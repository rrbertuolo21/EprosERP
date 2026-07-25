using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-MES — Material consumido pela ordem a partir da estrutura de produto (prd_mes_consumo_material).
    /// Fiel ao EF §17. Gerado na finalização (MES-REG-013).
    /// </summary>
    public class MesConsumoMaterial : EntidadeSaaSBase
    {
        public Guid OrdemId { get; private set; }
        public Guid EstruturaId { get; private set; }
        public Guid ComponenteId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid? VariacaoId { get; private set; }
        public decimal QuantidadePrevista { get; private set; }
        public decimal QuantidadeConsumida { get; private set; }
        public decimal? PercentualDesperdicio { get; private set; }
        public Guid? UnidadeId { get; private set; }
        public decimal? CustoConsumo { get; private set; }
        public EStatusConsumoMes Status { get; private set; } = EStatusConsumoMes.Previsto;

        protected MesConsumoMaterial() { } // EF Core

        public MesConsumoMaterial(
            Guid ordemId,
            Guid estruturaId,
            Guid componenteId,
            Guid produtoId,
            string tenantId,
            string criadoPor,
            decimal quantidadePrevista = 0m,
            decimal quantidadeConsumida = 0m,
            Guid? variacaoId = null,
            decimal? percentualDesperdicio = null,
            Guid? unidadeId = null,
            decimal? custoConsumo = null)
            : base(tenantId, criadoPor)
        {
            OrdemId = ordemId;
            EstruturaId = estruturaId;
            ComponenteId = componenteId;
            ProdutoId = produtoId;
            QuantidadePrevista = quantidadePrevista;
            QuantidadeConsumida = quantidadeConsumida;
            VariacaoId = variacaoId;
            PercentualDesperdicio = percentualDesperdicio;
            UnidadeId = unidadeId;
            CustoConsumo = custoConsumo;
            Status = EStatusConsumoMes.Previsto;

            AddNotifications(new Contract<MesConsumoMaterial>()
                .Requires()
                .AreNotEquals(ordemId, Guid.Empty, nameof(OrdemId), "O consumo deve referenciar a ordem [Origem: MesConsumoMaterial].")
                .AreNotEquals(estruturaId, Guid.Empty, nameof(EstruturaId), "O consumo deve referenciar a estrutura [Origem: MesConsumoMaterial]. (MES-REG-013)")
                .AreNotEquals(componenteId, Guid.Empty, nameof(ComponenteId), "O consumo deve referenciar o componente [Origem: MesConsumoMaterial]. (MES-REG-013)")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O consumo deve referenciar o produto [Origem: MesConsumoMaterial].")
            );
        }

        /// <summary>MES-REG-015/016: confirma o consumo do material em estoque.</summary>
        public void Confirmar(decimal quantidadeConsumida, string alteradoPor)
        {
            QuantidadeConsumida = quantidadeConsumida;
            Status = EStatusConsumoMes.Consumido;
            MarcarAlterado(alteradoPor);
        }
    }
}
