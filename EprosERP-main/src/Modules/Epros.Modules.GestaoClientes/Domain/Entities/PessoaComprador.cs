using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Extensão de papel Comprador (CAD-PEM 12.7). 1:1 com Pessoa via PessoaId.</summary>
    public class PessoaComprador : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }

        protected PessoaComprador() { } // EF Core

        public PessoaComprador(Guid pessoaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PessoaComprador>()
                .Requires()
                .AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId), "PessoaId é obrigatório [Origem: PessoaComprador]")
            );
        }
    }
}
