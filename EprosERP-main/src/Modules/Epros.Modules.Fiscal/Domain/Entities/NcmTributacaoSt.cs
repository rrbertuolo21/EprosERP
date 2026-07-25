using System;
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class NcmTributacaoSt : EntidadeSaaSBase
    {
        public Guid NcmTributacaoId { get; private set; }
        public string Uf { get; private set; } = string.Empty;
        public ETipoCalculo TipoCalculo { get; private set; }
        public decimal ValorAliquotaIcmsSt { get; private set; }
        public decimal ValorMva { get; private set; }
        public decimal ValorPercentualReducaoBcIcmsSt { get; private set; }
        public int TipoReducaoIcmsSt { get; private set; }
        public decimal ValorUnitarioSt { get; private set; }
        public decimal ValorPercentualFcpSt { get; private set; }

        protected NcmTributacaoSt() { } // EF Core

        public NcmTributacaoSt(
            Guid ncmTributacaoId,
            string uf,
            ETipoCalculo tipoCalculo,
            decimal valorAliquotaIcmsSt,
            decimal valorMva,
            decimal valorPercentualReducaoBcIcmsSt,
            int tipoReducaoIcmsSt,
            decimal valorUnitarioSt,
            decimal valorPercentualFcpSt,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            NcmTributacaoId = ncmTributacaoId;
            Uf = uf;
            TipoCalculo = tipoCalculo;
            ValorAliquotaIcmsSt = valorAliquotaIcmsSt;
            ValorMva = valorMva;
            ValorPercentualReducaoBcIcmsSt = valorPercentualReducaoBcIcmsSt;
            TipoReducaoIcmsSt = tipoReducaoIcmsSt;
            ValorUnitarioSt = valorUnitarioSt;
            ValorPercentualFcpSt = valorPercentualFcpSt;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<NcmTributacaoSt>()
                .Requires()
                .IsLowerOrEqualsThan((Uf ?? "").Length, 2, nameof(Uf), "O campo Uf deve ter no máximo 2 caracteres [Origem: NcmTributacaoSt]")
                .IsTrue(Enum.IsDefined(typeof(ETipoCalculo), TipoCalculo), nameof(TipoCalculo), "TipoCalculo não consta na lista [Origem: NcmTributacaoSt]")
            );
        }

        public void Alterar(
            string uf,
            ETipoCalculo tipoCalculo,
            decimal valorAliquotaIcmsSt,
            decimal valorMva,
            decimal valorPercentualReducaoBcIcmsSt,
            int tipoReducaoIcmsSt,
            decimal valorUnitarioSt,
            decimal valorPercentualFcpSt,
            string alteradoPor)
        {
            Uf = uf;
            TipoCalculo = tipoCalculo;
            ValorAliquotaIcmsSt = valorAliquotaIcmsSt;
            ValorMva = valorMva;
            ValorPercentualReducaoBcIcmsSt = valorPercentualReducaoBcIcmsSt;
            TipoReducaoIcmsSt = tipoReducaoIcmsSt;
            ValorUnitarioSt = valorUnitarioSt;
            ValorPercentualFcpSt = valorPercentualFcpSt;
            MarcarAlterado(alteradoPor);
            Validar();
        }
    }
}
