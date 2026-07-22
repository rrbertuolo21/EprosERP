using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Papel atribuído diretamente a um usuário (RBAC) - APP-TEN-003 11.6.</summary>
    public class UsuarioPapel : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public Guid PapelId { get; private set; }
        public string? ModelType { get; private set; }

        protected UsuarioPapel() { } // EF Core

        public UsuarioPapel(Guid usuarioId, Guid papelId, string? modelType, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioId = usuarioId;
            PapelId = papelId;
            ModelType = modelType;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<UsuarioPapel>()
                .Requires()
                .AreNotEquals(UsuarioId, Guid.Empty, nameof(UsuarioId), "UsuarioId é obrigatório [Origem: UsuarioPapel]")
                .AreNotEquals(PapelId, Guid.Empty, nameof(PapelId), "PapelId é obrigatório [Origem: UsuarioPapel]")
                .HasMaxLen(ModelType ?? string.Empty, 100, nameof(ModelType), "ModelType deve ter no máximo 100 caracteres [Origem: UsuarioPapel]")
            );
        }
    }
}
