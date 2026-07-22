using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Municipio : EntidadeSaaSBase, IGlobalEntity
    {
        public Guid PaisId { get; private set; }
        public Guid SubdivisaoId { get; private set; }
        // Legado: Municipio.Estado (EEstado) — UF direta (sigla) além do vínculo por Subdivisao.
        public string? Uf { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public long CodigoIbge { get; private set; }
        public decimal? Latitude { get; private set; }
        public decimal? Longitude { get; private set; }
        public bool Ativo { get; private set; }

        // Navigation Properties
        public Pais Pais { get; private set; } = null!;
        public Subdivisao Subdivisao { get; private set; } = null!;

        protected Municipio() { } // EF Core

        public Municipio(
            Guid paisId,
            Guid subdivisaoId,
            string nome,
            long codigoIbge,
            decimal? latitude,
            decimal? longitude,
            string criadoPor,
            string? uf = null)
            : base(new Guid($"00000000-0000-0000-0000-{codigoIbge:D12}"), Guid.NewGuid(), "system", criadoPor, DateTime.UtcNow)
        {
            AddNotifications(new Contract<Municipio>()
                .Requires()
                .AreNotEquals(paisId, Guid.Empty, nameof(PaisId), "O ID do país é obrigatório.")
                .AreNotEquals(subdivisaoId, Guid.Empty, nameof(SubdivisaoId), "O ID da subdivisão/estado é obrigatório.")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O Nome do município é obrigatório.")
                .HasMaxLen(nome, 150, nameof(Nome), "O Nome do município deve ter no máximo 150 caracteres.")
                .IsGreaterThan(codigoIbge, 0, nameof(CodigoIbge), "O Código IBGE deve ser maior que zero.")
                .IsTrue(codigoIbge.ToString().Length == 7, nameof(CodigoIbge), "O Código IBGE do município brasileiro deve ter exatamente 7 dígitos.")
            );

            PaisId = paisId;
            SubdivisaoId = subdivisaoId;
            Uf = uf;
            Nome = nome;
            CodigoIbge = codigoIbge;
            Latitude = latitude;
            Longitude = longitude;
            Ativo = true;
        }

        public void DefinirUf(string? uf, string alteradoPor)
        {
            Uf = uf;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }
    }
}
