using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    /// <summary>Papel da identidade global DENTRO de um tenant (nível de vínculo, não de empresa).</summary>
    public enum PapelAcessoTenant
    {
        /// <summary>Dono do tenant (admin primário — normalmente quem fez o self-register).</summary>
        Proprietario = 1,
        /// <summary>Membro comum da organização do tenant.</summary>
        Membro = 2,
        /// <summary>Parceiro externo com acesso concedido (ex.: contador que atende vários clientes).</summary>
        ContadorParceiro = 3
    }

    /// <summary>
    /// Vínculo N:N entre uma identidade global (<see cref="Usuario"/>, chaveada por e-mail único global)
    /// e um tenant. É a "membership": uma pessoa pode ter vários registros — um por tenant a que tem
    /// acesso (ex.: contador parceiro cadastrado por mais de um cliente). O <c>TenantId</c> herdado de
    /// <see cref="EntidadeSaaSBase"/> é o TENANT CONCEDIDO por este vínculo; <see cref="UsuarioId"/> aponta
    /// para a identidade global. Após o login (global), lista-se os tenants ativos e emite-se um token
    /// escopado ao tenant escolhido. Índice único (UsuarioId, TenantId) impede vínculo duplicado.
    /// </summary>
    public class AcessoUsuarioTenant : EntidadeSaaSBase
    {
        /// <summary>Identidade global vinculada (Usuario.Id — e-mail é único global).</summary>
        public Guid UsuarioId { get; private set; }

        /// <summary>Papel do usuário neste tenant (nível de membership).</summary>
        public PapelAcessoTenant Papel { get; private set; }

        /// <summary>Vínculo ativo? Vínculo inativo não habilita seleção/entrada no tenant.</summary>
        public bool Ativo { get; private set; }

        protected AcessoUsuarioTenant() { } // EF Core

        public AcessoUsuarioTenant(
            string tenantId,
            Guid usuarioId,
            PapelAcessoTenant papel,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
                AddNotification(nameof(TenantId), "O tenant do vínculo é obrigatório.");
            if (usuarioId == Guid.Empty)
                AddNotification(nameof(UsuarioId), "O ID do usuário é obrigatório.");

            UsuarioId = usuarioId;
            Papel = papel;
            Ativo = true;
        }

        public void Ativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }

        public void Desativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void AlterarPapel(PapelAcessoTenant novoPapel, string alteradoPor)
        {
            Papel = novoPapel;
            MarcarAlterado(alteradoPor);
        }
    }
}
