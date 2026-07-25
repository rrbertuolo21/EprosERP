using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class NcmTributacaoFundoCombatePobreza : EntidadeSaaSBase
    {
        public Guid NcmTributacaoId { get; private set; }
        public string Uf { get; private set; } = string.Empty;
        public decimal ValorPercentual { get; private set; }

        protected NcmTributacaoFundoCombatePobreza() { } // EF Core

        public NcmTributacaoFundoCombatePobreza(
            Guid ncmTributacaoId,
            string uf,
            decimal valorPercentual,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            NcmTributacaoId = ncmTributacaoId;
            Uf = uf;
            ValorPercentual = valorPercentual;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<NcmTributacaoFundoCombatePobreza>()
                .Requires()
                .IsLowerOrEqualsThan((Uf ?? "").Length, 2, nameof(Uf), "O campo Uf deve ter no máximo 2 caracteres [Origem: NcmTributacaoFundoCombatePobreza]")
            );
        }

        public void Alterar(string uf, decimal valorPercentual, string alteradoPor)
        {
            Uf = uf;
            ValorPercentual = valorPercentual;
            MarcarAlterado(alteradoPor);
            Validar();
        }
    }
}
