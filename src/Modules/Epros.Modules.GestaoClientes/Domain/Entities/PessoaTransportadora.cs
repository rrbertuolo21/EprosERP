using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PessoaTransportadora : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public string? Ciot { get; private set; }
        public string? Rntrc { get; private set; }

        protected PessoaTransportadora() { } // EF Core

        public PessoaTransportadora(
            Guid pessoaId,
            string? ciot,
            string? rntrc,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PessoaTransportadora>()
                .Requires()
                .HasMaxLen(ciot ?? string.Empty, 16, nameof(Ciot), "O campo Ciot deve ter no máximo 16 caracteres [Origem: PessoaTransportadora]")
                .HasMaxLen(rntrc ?? string.Empty, 14, nameof(Rntrc), "O campo Rntrc deve ter no máximo 14 caracteres [Origem: PessoaTransportadora]")
            );

            PessoaId = pessoaId;
            Ciot = ciot;
            Rntrc = rntrc;
        }
    }
}
