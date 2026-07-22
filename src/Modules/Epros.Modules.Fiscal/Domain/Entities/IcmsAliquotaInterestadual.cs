using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class IcmsAliquotaInterestadual : EntidadeSaaSBase, IGlobalEntity
    {
        public EEstado UfOrigem { get; private set; }
        public EEstado UfDestino { get; private set; }
        public decimal ValorAliquota { get; private set; }

        protected IcmsAliquotaInterestadual() { } // EF Core

        public IcmsAliquotaInterestadual(EEstado ufOrigem, EEstado ufDestino, decimal valorAliquota, string criadoPor)
            : base("system", criadoPor)
        {
            UfOrigem = ufOrigem;
            UfDestino = ufDestino;
            ValorAliquota = valorAliquota;
            Validar();
        }

        public void Alterar(EEstado ufOrigem, EEstado ufDestino, decimal valorAliquota, string alteradoPor)
        {
            UfOrigem = ufOrigem;
            UfDestino = ufDestino;
            ValorAliquota = valorAliquota;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<IcmsAliquotaInterestadual>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(EEstado), UfOrigem), nameof(UfOrigem), "UfOrigem não consta na lista [Origem: IcmsAliquotaInterestadual]")
                .IsTrue(Enum.IsDefined(typeof(EEstado), UfDestino), nameof(UfDestino), "UfDestino não consta na lista [Origem: IcmsAliquotaInterestadual]")
                .IsGreaterOrEqualsThan(ValorAliquota, 0.0m, nameof(ValorAliquota), "O campo ValorAliquota deve ser maior ou igual a Zero [Origem: IcmsAliquotaInterestadual]")
            );
        }
    }
}
