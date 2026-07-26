using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Rateio de um título (CP/CR) a um centro de custo por percentual (EF FIN-CMG §11.2 cmg_alocacao_centro_custo).
    /// Título e centro de custo referenciados por Guid FK.
    /// </summary>
    public class AlocacaoCentroCusto : EntidadeSaaSBase
    {
        public Guid TituloId { get; private set; }
        public ETipoTituloAlocacao? TipoTitulo { get; private set; }
        public Guid CentroCustoId { get; private set; }
        public decimal Percentual { get; private set; }
        public decimal? ValorRateado { get; private set; }

        protected AlocacaoCentroCusto() { } // EF Core

        public AlocacaoCentroCusto(Guid tituloId, ETipoTituloAlocacao? tipoTitulo, Guid centroCustoId, decimal percentual,
            decimal? valorRateado, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TituloId = tituloId;
            TipoTitulo = tipoTitulo;
            CentroCustoId = centroCustoId;
            Percentual = percentual;
            ValorRateado = valorRateado;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<AlocacaoCentroCusto>()
                .Requires()
                .IsNotEmpty(TituloId, nameof(TituloId), "O título é obrigatório [Origem: AlocacaoCentroCusto]")
                .IsNotEmpty(CentroCustoId, nameof(CentroCustoId), "O centro de custo é obrigatório [Origem: AlocacaoCentroCusto]")
                .IsGreaterThan(Percentual, 0, nameof(Percentual), "O percentual de rateio deve ser maior que zero [Origem: AlocacaoCentroCusto]")
                .IsLowerOrEqualsThan(Percentual, 100, nameof(Percentual), "O percentual de rateio não pode exceder 100% [Origem: AlocacaoCentroCusto]")
            );
        }
    }
}
