using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Extensão de papel Contador (CAD-PEM 12.7). 1:1 com Pessoa via PessoaId.</summary>
    public class PessoaContador : EntidadeSaaSBase
    {
        public Guid PessoaId { get; private set; }
        public string? Crc { get; private set; }

        protected PessoaContador() { } // EF Core

        public PessoaContador(Guid pessoaId, string? crc, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PessoaId = pessoaId;
            Crc = crc;
            Validar();
        }

        public void Alterar(string? crc, string alteradoPor)
        {
            Crc = crc;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PessoaContador>()
                .Requires()
                .AreNotEquals(PessoaId, Guid.Empty, nameof(PessoaId), "PessoaId é obrigatório [Origem: PessoaContador]")
                .HasMaxLen(Crc ?? string.Empty, 15, nameof(Crc), "Crc deve ter no máximo 15 caracteres [Origem: PessoaContador]")
            );
        }
    }
}
