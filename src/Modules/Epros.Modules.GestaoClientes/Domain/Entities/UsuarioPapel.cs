using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Papel atribuído a um usuário (RBAC) - APP-TEN-003 11.6.
    /// 1.09: a atribuição passa a ser (usuário, empresa, papel). <see cref="EmpresaId"/> nulo significa
    /// "vale para todas as empresas do tenant" (usado pelo papel de sistema Administrador); um EmpresaId
    /// específico restringe o papel àquela empresa (o mesmo usuário pode ser Admin na A e Vendedor na B).</summary>
    public class UsuarioPapel : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public Guid PapelId { get; private set; }
        /// <summary>Empresa à qual o papel se aplica; nulo = todas as empresas do tenant (1.09).</summary>
        public Guid? EmpresaId { get; private set; }
        public string? ModelType { get; private set; }

        protected UsuarioPapel() { } // EF Core

        public UsuarioPapel(Guid usuarioId, Guid papelId, string? modelType, string tenantId, string criadoPor, Guid? empresaId = null)
            : base(tenantId, criadoPor)
        {
            UsuarioId = usuarioId;
            PapelId = papelId;
            EmpresaId = empresaId;
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
