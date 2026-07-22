using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public enum TipoSubdivisao
    {
        Estado = 1,
        Provincia = 2,
        Condado = 3,
        Regiao = 4
    }

    public class Subdivisao : EntidadeSaaSBase, IGlobalEntity
    {
        public Guid PaisId { get; private set; }
        public string CodigoISO31662 { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public TipoSubdivisao Tipo { get; private set; }
        public Guid? TerritorioPaiId { get; private set; }

        // Navigation Properties
        public Pais Pais { get; private set; } = null!;
        public Subdivisao? TerritorioPai { get; private set; }

        protected Subdivisao() { } // EF Core

        public Subdivisao(
            Guid paisId,
            string codigoISO31662,
            string nome,
            TipoSubdivisao tipo,
            Guid? territorioPaiId,
            string criadoPor)
            : base("system", criadoPor)
        {
            AddNotifications(new Contract<Subdivisao>()
                .Requires()
                .AreNotEquals(paisId, Guid.Empty, nameof(PaisId), "O ID do país é obrigatório.")
                .IsNotNullOrEmpty(codigoISO31662, nameof(CodigoISO31662), "O Código ISO 3166-2 é obrigatório.")
                .HasMaxLen(codigoISO31662, 10, nameof(CodigoISO31662), "O Código ISO 3166-2 deve ter no máximo 10 caracteres.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O Nome da subdivisão é obrigatório.")
                .HasMaxLen(nome, 60, nameof(Nome), "O Nome da subdivisão deve ter no máximo 60 caracteres.")
            );

            PaisId = paisId;
            CodigoISO31662 = codigoISO31662.ToUpperInvariant();
            Nome = nome;
            Tipo = tipo;
            TerritorioPaiId = territorioPaiId;
        }
    }
}
