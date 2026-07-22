using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PessoaCliente : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public bool EhConsumidorFinal { get; private set; }
        public ETipoContribuinte TipoContribuinte { get; private set; }

        protected PessoaCliente() { } // EF Core

        public PessoaCliente(
            Guid pessoaId,
            bool ehConsumidorFinal,
            ETipoContribuinte tipoContribuinte,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PessoaCliente>()
                .Requires()
                .IsTrue(Enum.IsDefined(typeof(ETipoContribuinte), tipoContribuinte), nameof(TipoContribuinte), "TipoContribuinte não consta na lista [Origem: PessoaCliente]")
            );

            PessoaId = pessoaId;
            EhConsumidorFinal = ehConsumidorFinal;
            TipoContribuinte = tipoContribuinte;
        }
    }
}
