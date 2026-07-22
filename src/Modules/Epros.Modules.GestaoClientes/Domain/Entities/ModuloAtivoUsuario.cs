using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Módulo ativo (avulso/override) por usuário/contexto (APP-CAT 11.8). Tenant-scoped.</summary>
    public class ModuloAtivoUsuario : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public string Modulo { get; private set; } = string.Empty;
        public EOrigemModuloAtivo Origem { get; private set; }

        protected ModuloAtivoUsuario() { } // EF Core

        public ModuloAtivoUsuario(
            Guid usuarioId,
            string modulo,
            EOrigemModuloAtivo origem,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioId = usuarioId;
            Modulo = modulo;
            Origem = origem;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ModuloAtivoUsuario>()
                .Requires()
                .AreNotEquals(UsuarioId, Guid.Empty, nameof(UsuarioId), "UsuarioId é obrigatório [Origem: ModuloAtivoUsuario]")
                .IsNotNullOrEmpty(Modulo, nameof(Modulo), "Modulo é obrigatório [Origem: ModuloAtivoUsuario]")
                .HasMaxLen(Modulo ?? string.Empty, 100, nameof(Modulo), "Modulo deve ter no máximo 100 caracteres [Origem: ModuloAtivoUsuario]")
                .IsTrue(Enum.IsDefined(typeof(EOrigemModuloAtivo), Origem), nameof(Origem), "Origem não consta na lista [Origem: ModuloAtivoUsuario]")
            );
        }
    }
}
