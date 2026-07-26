using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PessoaPrestadorServico : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public string? Cei { get; private set; }

        protected PessoaPrestadorServico() { } // EF Core

        public PessoaPrestadorServico(
            Guid pessoaId,
            string? cei,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PessoaPrestadorServico>()
                .Requires()
                .HasMaxLen(cei ?? string.Empty, 12, nameof(Cei), "O campo Cei deve ter no máximo 12 caracteres [Origem: PessoaPrestadorServico]")
            );

            PessoaId = pessoaId;
            Cei = cei;
        }
    }
}
