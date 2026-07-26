using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class FcpAliquotaUf : EntidadeSaaSBase, IGlobalEntity
    {
        public EEstado Uf { get; private set; }
        public decimal ValorAliquota { get; private set; }
        public string? Observacao { get; private set; }

        protected FcpAliquotaUf() { } // EF Core

        public FcpAliquotaUf(EEstado uf, decimal valorAliquota, string? observacao, string criadoPor)
            : base("system", criadoPor)
        {
            Uf = uf;
            ValorAliquota = valorAliquota;
            Observacao = observacao;
            Validar();
        }

        public void Alterar(EEstado uf, decimal valorAliquota, string? observacao, string alteradoPor)
        {
            Uf = uf;
            ValorAliquota = valorAliquota;
            Observacao = observacao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<FcpAliquotaUf>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(EEstado), Uf), nameof(Uf), "Uf não consta na lista [Origem: FcpAliquotaUf]")
                .IsGreaterOrEqualsThan(ValorAliquota, 0.0m, nameof(ValorAliquota), "O campo ValorAliquota deve ser maior ou igual a Zero [Origem: FcpAliquotaUf]")
            );
        }
    }
}
