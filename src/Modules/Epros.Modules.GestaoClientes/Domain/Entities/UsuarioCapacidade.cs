using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>
    /// Grant/deny direto de uma capacidade a um usuário (RBAC) - APP-TEN-003 §11.8 (usuario_capacidade).
    /// REG-040: permissão direta pode conceder ou negar capacidade específica.
    /// REG-041: negação explícita no usuário prevalece sobre papel.
    /// </summary>
    public class UsuarioCapacidade : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public Guid CapacidadeId { get; private set; }
        public bool Granted { get; private set; }

        protected UsuarioCapacidade() { } // EF Core

        public UsuarioCapacidade(Guid usuarioId, Guid capacidadeId, bool granted, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioId = usuarioId;
            CapacidadeId = capacidadeId;
            Granted = granted;
            Validar();
        }

        /// <summary>Altera a concessão (grant/deny). REG-041: deny prevalece sobre papel.</summary>
        public void AlterarConcessao(bool granted, string alteradoPor)
        {
            Granted = granted;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<UsuarioCapacidade>()
                .Requires()
                .AreNotEquals(UsuarioId, Guid.Empty, nameof(UsuarioId), "UsuarioId é obrigatório [Origem: UsuarioCapacidade]")
                .AreNotEquals(CapacidadeId, Guid.Empty, nameof(CapacidadeId), "CapacidadeId é obrigatório [Origem: UsuarioCapacidade]")
            );
        }
    }
}
