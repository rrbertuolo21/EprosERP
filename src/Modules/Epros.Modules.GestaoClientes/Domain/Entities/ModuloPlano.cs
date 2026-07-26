using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class ModuloPlano : EntidadeSaaSBase
    {
        public string NomeModulo { get; private set; } = string.Empty;
        public Guid PlanoId { get; private set; }

        protected ModuloPlano() { } // EF Core

        public ModuloPlano(string nomeModulo, Guid planoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ModuloPlano>()
                .Requires()
                .IsNotNullOrEmpty(nomeModulo, nameof(NomeModulo), "Nome do módulo é obrigatório")
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "PlanoId inválido")
            );

            NomeModulo = nomeModulo;
            PlanoId = planoId;
        }
    }
}
