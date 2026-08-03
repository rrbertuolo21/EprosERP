using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Historico de reajuste do aluguel (ID7/NF-02). O indice (IGP-M/IPCA/INPC) e PARAMETRIZAVEL e
    /// o reajuste vem DESLIGADO por padrao. O percentual/fonte/arredondamento do indice sao
    /// valida-contador — a IMOBILIARIA registra o fato e o valor ratificado, sem inventar o numero.
    /// </summary>
    public class LocacaoReajuste : EntidadeSaaSBase
    {
        public Guid LocacaoId { get; private set; }
        /// <summary>Nome do indice contratual (parametrico). Vazio quando reajuste desligado.</summary>
        public string? Indice { get; private set; }
        public DateTime DataBase { get; private set; }
        public decimal ValorAnterior { get; private set; }
        public decimal ValorNovo { get; private set; }
        /// <summary>Percentual aplicado (informado/ratificado). Nao calculado pela IMOBILIARIA.</summary>
        public decimal? PercentualAplicado { get; private set; }

        protected LocacaoReajuste() { } // EF Core

        public LocacaoReajuste(
            Guid locacaoId,
            string? indice,
            DateTime dataBase,
            decimal valorAnterior,
            decimal valorNovo,
            decimal? percentualAplicado,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            LocacaoId = locacaoId;
            Indice = indice;
            DataBase = dataBase.Date;
            ValorAnterior = valorAnterior;
            ValorNovo = valorNovo;
            PercentualAplicado = percentualAplicado;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LocacaoReajuste>()
                .Requires()
                .AreNotEquals(LocacaoId, Guid.Empty, nameof(LocacaoId),
                    "O reajuste exige locacao. [Origem: LocacaoReajuste] (NF-02)")
                .IsGreaterThan(ValorNovo, 0, nameof(ValorNovo),
                    "O valor reajustado deve ser positivo. [Origem: LocacaoReajuste] (NF-02)")
                .IsGreaterThan(ValorAnterior, 0, nameof(ValorAnterior),
                    "O valor anterior deve ser positivo. [Origem: LocacaoReajuste]"));
        }
    }
}
