using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-MES — Item da ordem de produção (prd_mes_ordem_item). Fiel ao EF §17.
    /// MES-REG-004/005: exige produto e referência ao cabeçalho.
    /// </summary>
    public class MesOrdemItem : EntidadeSaaSBase
    {
        public Guid OrdemId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid? VariacaoId { get; private set; }
        public decimal QuantidadeProduzir { get; private set; }
        public decimal QuantidadeProduzida { get; private set; }
        public decimal QuantidadeEntregue { get; private set; }
        public decimal CustoPrevisto { get; private set; }
        public decimal CustoRealizado { get; private set; }
        public EStatusItemMes Status { get; private set; } = EStatusItemMes.Pendente;

        protected MesOrdemItem() { } // EF Core

        public MesOrdemItem(
            Guid ordemId,
            Guid produtoId,
            decimal quantidadeProduzir,
            string tenantId,
            string criadoPor,
            Guid? variacaoId = null,
            decimal custoPrevisto = 0m)
            : base(tenantId, criadoPor)
        {
            OrdemId = ordemId;
            ProdutoId = produtoId;
            QuantidadeProduzir = quantidadeProduzir;
            VariacaoId = variacaoId;
            CustoPrevisto = custoPrevisto;
            Status = EStatusItemMes.Pendente;

            AddNotifications(new Contract<MesOrdemItem>()
                .Requires()
                .AreNotEquals(ordemId, Guid.Empty, nameof(OrdemId), "O item deve referenciar o cabeçalho da ordem [Origem: MesOrdemItem]. (MES-REG-005)")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O item da ordem deve referenciar um produto [Origem: MesOrdemItem]. (MES-REG-004)")
            );
        }

        /// <summary>MES-REG-021: registra produção realizada, entregue e desperdício por item.</summary>
        public void RegistrarProducao(decimal quantidadeProduzida, decimal quantidadeEntregue, string alteradoPor, decimal custoRealizado = 0m)
        {
            if (quantidadeProduzida < 0m)
            {
                AddNotification(nameof(QuantidadeProduzida), "A quantidade produzida não pode ser negativa. [Origem: MesOrdemItem]");
                return;
            }
            if (quantidadeEntregue < 0m)
            {
                AddNotification(nameof(QuantidadeEntregue), "A quantidade entregue não pode ser negativa. [Origem: MesOrdemItem]");
                return;
            }
            QuantidadeProduzida = quantidadeProduzida;
            QuantidadeEntregue = quantidadeEntregue;
            CustoRealizado = custoRealizado;
            Status = quantidadeEntregue > 0m ? EStatusItemMes.Entregue
                : quantidadeProduzida > 0m ? EStatusItemMes.Produzido
                : EStatusItemMes.EmProducao;
            MarcarAlterado(alteradoPor);
        }
    }
}
