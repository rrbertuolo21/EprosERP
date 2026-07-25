using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Definicao
{
    /// <summary>
    /// Vinculo N:N entre projeto e cliente. Origem: EF PRJ-DEF 11.2 (prj_def_projeto_cliente).
    /// RN-DEF-005: cliente deve existir.
    /// </summary>
    public class ProjetoCliente : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public Guid ClienteId { get; private set; }

        protected ProjetoCliente() { } // EF Core

        public ProjetoCliente(Guid projetoId, Guid clienteId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ProjetoCliente>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: ProjetoCliente]")
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "O cliente e obrigatorio. [Origem: ProjetoCliente]"));

            ProjetoId = projetoId;
            ClienteId = clienteId;
        }
    }
}
