using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PerfilAcesso : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }
        public List<PerfilAcessoMenu> Acessos { get; private set; } = new();

        protected PerfilAcesso() { } // EF Core

        public PerfilAcesso(string descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PerfilAcesso>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição do perfil é obrigatória")
                .HasMaxLen(descricao, 100, nameof(Descricao), "A descrição deve ter no máximo 100 caracteres")
            );

            Descricao = descricao;
            Ativo = true;
        }

        public void Atualizar(string descricao, string alteradoPor)
        {
            AddNotifications(new Contract<PerfilAcesso>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição do perfil é obrigatória")
                .HasMaxLen(descricao, 100, nameof(Descricao), "A descrição deve ter no máximo 100 caracteres")
            );

            if (IsValid)
            {
                Descricao = descricao;
                MarcarAlterado(alteradoPor);
            }
        }

        public void SincronizarAcessos(List<PerfilAcessoMenu> novosAcessos, string alteradoPor)
        {
            // Limpa acessos antigos e adiciona novos
            Acessos.Clear();
            foreach (var acesso in novosAcessos)
            {
                if (acesso.IsValid)
                {
                    Acessos.Add(acesso);
                }
                else
                {
                    AddNotifications(acesso.Notifications);
                }
            }
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Ativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }
    }
}
