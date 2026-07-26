using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-BOM — Linha de componente da estrutura (prd_bom_componente).
    /// Fiel ao EF §17. Calcula quantidade final (desperdício) e custo da linha (BOM-REG-010/012).
    /// </summary>
    public class BomComponente : EntidadeSaaSBase
    {
        public Guid EstruturaId { get; private set; }
        public Guid VariacaoComponenteId { get; private set; }
        public decimal Quantidade { get; private set; }
        public Guid? SubUnidadeId { get; private set; }
        public decimal? MultiplicadorUnidade { get; private set; }
        public decimal? PercentualDesperdicio { get; private set; }
        public decimal? QuantidadeFinal { get; private set; }
        public Guid? GrupoComponenteId { get; private set; }
        public int? OrdemMontagem { get; private set; }
        public decimal? CustoUnitarioComImpostos { get; private set; }
        public decimal? CustoLinha { get; private set; }
        public EStatusComponenteBom Status { get; private set; } = EStatusComponenteBom.Rascunho;

        protected BomComponente() { } // EF Core

        public BomComponente(
            Guid estruturaId,
            Guid variacaoComponenteId,
            decimal quantidade,
            string tenantId,
            string criadoPor,
            Guid? subUnidadeId = null,
            decimal? multiplicadorUnidade = null,
            decimal? percentualDesperdicio = null,
            Guid? grupoComponenteId = null,
            int? ordemMontagem = null,
            decimal? custoUnitarioComImpostos = null)
            : base(tenantId, criadoPor)
        {
            EstruturaId = estruturaId;
            VariacaoComponenteId = variacaoComponenteId;
            Quantidade = quantidade;
            SubUnidadeId = subUnidadeId;
            MultiplicadorUnidade = multiplicadorUnidade;
            PercentualDesperdicio = percentualDesperdicio;
            GrupoComponenteId = grupoComponenteId;
            OrdemMontagem = ordemMontagem;
            CustoUnitarioComImpostos = custoUnitarioComImpostos;
            Status = EStatusComponenteBom.Rascunho;

            Recalcular();

            AddNotifications(new Contract<BomComponente>()
                .Requires()
                .AreNotEquals(estruturaId, Guid.Empty, nameof(EstruturaId), "A estrutura é obrigatória [Origem: BomComponente].")
                .AreNotEquals(variacaoComponenteId, Guid.Empty, nameof(VariacaoComponenteId), "A variação do componente é obrigatória [Origem: BomComponente]. (BOM-REG-006)")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A quantidade do componente deve ser maior que zero [Origem: BomComponente]. (BOM-REG-007)")
            );

            // BOM-REG-008: quantidade deve ser maior que a quantidade desperdiçada.
            if (QuantidadeFinal.HasValue && QuantidadeFinal.Value <= 0)
            {
                AddNotification(nameof(QuantidadeFinal), "O desperdício não pode consumir toda a quantidade do componente. (BOM-REG-008)");
            }
        }

        /// <summary>BOM-REG-010/011/012: quantidade final após desperdício e custo dinâmico da linha.</summary>
        private void Recalcular()
        {
            var multiplicador = MultiplicadorUnidade.GetValueOrDefault(1m);
            if (multiplicador <= 0) multiplicador = 1m;

            var perda = PercentualDesperdicio.GetValueOrDefault(0m);
            QuantidadeFinal = Quantidade * multiplicador * (1m - (perda / 100m));

            if (CustoUnitarioComImpostos.HasValue)
            {
                CustoLinha = CustoUnitarioComImpostos.Value * Quantidade * multiplicador;
            }
        }

        public void Ativar(string alteradoPor)
        {
            Status = EStatusComponenteBom.Ativo;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            Status = EStatusComponenteBom.Inativo;
            MarcarAlterado(alteradoPor);
        }
    }
}
