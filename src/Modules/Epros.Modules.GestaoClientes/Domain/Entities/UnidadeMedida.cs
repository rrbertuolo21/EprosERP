using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class UnidadeMedida : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? CodigoUNECE { get; private set; }

        protected UnidadeMedida() { } // EF Core

        public UnidadeMedida(string nome, string? codigoUNECE, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<UnidadeMedida>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome da unidade de medida é obrigatório.")
                .HasMaxLen(nome, 50, nameof(Nome), "Nome deve ter no máximo 50 caracteres.")
            );

            if (codigoUNECE != null)
            {
                AddNotifications(new Contract<UnidadeMedida>()
                    .Requires()
                    .HasMaxLen(codigoUNECE, 10, nameof(CodigoUNECE), "Código UNECE deve ter no máximo 10 caracteres.")
                );
            }

            Nome = nome;
            CodigoUNECE = codigoUNECE;
        }

        public void Atualizar(string nome, string? codigoUNECE, string alteradoPor)
        {
            AddNotifications(new Contract<UnidadeMedida>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome da unidade de medida é obrigatório.")
                .HasMaxLen(nome, 50, nameof(Nome), "Nome deve ter no máximo 50 caracteres.")
            );

            if (codigoUNECE != null)
            {
                AddNotifications(new Contract<UnidadeMedida>()
                    .Requires()
                    .HasMaxLen(codigoUNECE, 10, nameof(CodigoUNECE), "Código UNECE deve ter no máximo 10 caracteres.")
                );
            }

            if (IsValid)
            {
                Nome = nome;
                CodigoUNECE = codigoUNECE;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
