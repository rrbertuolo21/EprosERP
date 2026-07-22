using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class UsuarioPermissao : EntidadeSaaSBase
    {
        public Guid PerfilColaboradorId { get; private set; }
        public string Recurso { get; private set; } = string.Empty;
        public string Acao { get; private set; } = string.Empty;
        public bool Permitido { get; private set; }

        protected UsuarioPermissao() { } // EF Core

        public UsuarioPermissao(Guid perfilColaboradorId, string recurso, string acao, bool permitido, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<UsuarioPermissao>()
                .Requires()
                .AreNotEquals(perfilColaboradorId, Guid.Empty, nameof(PerfilColaboradorId), "PerfilColaboradorId inválido")
                .IsNotNullOrEmpty(recurso, nameof(Recurso), "Recurso é obrigatório")
                .IsNotNullOrEmpty(acao, nameof(Acao), "Ação é obrigatória")
            );

            PerfilColaboradorId = perfilColaboradorId;
            Recurso = recurso;
            Acao = acao;
            Permitido = permitido;
        }

        public void AlterarPermissao(bool permitido, string alteradoPor)
        {
            Permitido = permitido;
            MarcarAlterado(alteradoPor);
        }
    }
}
