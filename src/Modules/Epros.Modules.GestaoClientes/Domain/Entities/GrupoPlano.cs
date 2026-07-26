using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class GrupoPlano : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;
        public bool Ativo { get; private set; } = true;

        protected GrupoPlano() { } // EF Core

        public GrupoPlano(string descricao, string tenantId, string criadoPor, bool ativo = true)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<GrupoPlano>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "Descrição do grupo é obrigatória")
            );

            Descricao = descricao;
            Ativo = ativo;
        }

        public void Atualizar(string descricao, string alteradoPor, bool? ativo = null)
        {
            AddNotifications(new Contract<GrupoPlano>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "Descrição do grupo é obrigatória")
            );

            if (IsValid)
            {
                Descricao = descricao;
                if (ativo.HasValue)
                {
                    Ativo = ativo.Value;
                }
                MarcarAlterado(alteradoPor);
            }
        }

        public void DefinirAtivo(bool ativo, string alteradoPor)
        {
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }
    }
}
