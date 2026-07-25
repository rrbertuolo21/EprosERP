using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>Dimensão analítica gerencial genérica tipo/valor (EF FIN-CMG §11.3 cmg_dimensao_analitica).</summary>
    public class DimensaoAnalitica : EntidadeSaaSBase
    {
        public string Tipo { get; private set; } = string.Empty;
        public string Valor { get; private set; } = string.Empty;

        protected DimensaoAnalitica() { } // EF Core

        public DimensaoAnalitica(string tipo, string valor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Tipo = tipo;
            Valor = valor;
            Validar();
        }

        public void Alterar(string tipo, string valor, string usuario)
        {
            Tipo = tipo;
            Valor = valor;
            MarcarAlterado(usuario);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<DimensaoAnalitica>()
                .Requires()
                .IsNotNullOrEmpty(Tipo, nameof(Tipo), "O tipo da dimensão é obrigatório [Origem: DimensaoAnalitica]")
                .IsNotNullOrEmpty(Valor, nameof(Valor), "O valor da dimensão é obrigatório [Origem: DimensaoAnalitica]")
            );
        }
    }

    /// <summary>Vínculo entre alocação de centro de custo e dimensão analítica (EF FIN-CMG §11.4 cmg_alocacao_dimensao).</summary>
    public class AlocacaoDimensao : EntidadeSaaSBase
    {
        public Guid AlocacaoCentroCustoId { get; private set; }
        public Guid DimensaoAnaliticaId { get; private set; }

        protected AlocacaoDimensao() { } // EF Core

        public AlocacaoDimensao(Guid alocacaoCentroCustoId, Guid dimensaoAnaliticaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AlocacaoCentroCustoId = alocacaoCentroCustoId;
            DimensaoAnaliticaId = dimensaoAnaliticaId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<AlocacaoDimensao>()
                .Requires()
                .IsNotEmpty(AlocacaoCentroCustoId, nameof(AlocacaoCentroCustoId), "A alocação é obrigatória [Origem: AlocacaoDimensao]")
                .IsNotEmpty(DimensaoAnaliticaId, nameof(DimensaoAnaliticaId), "A dimensão é obrigatória [Origem: AlocacaoDimensao]")
            );
        }
    }
}
