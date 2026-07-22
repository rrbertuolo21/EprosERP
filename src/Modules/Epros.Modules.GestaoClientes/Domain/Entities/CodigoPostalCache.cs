using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class CodigoPostalCache : EntidadeSaaSBase, IGlobalEntity, IHardDeletable
    {
        public Guid PaisId { get; private set; }
        public string CodigoPostal { get; private set; } = string.Empty;
        public string? Logradouro { get; private set; }
        public string? Bairro { get; private set; }
        public Guid? MunicipioId { get; private set; }
        public string? Uf { get; private set; }
        public string? Provedor { get; private set; }
        public DateTime ConsultadoEm { get; private set; }
        public bool Falhou { get; private set; }
        public string? MotivoFalha { get; private set; }

        // Navigation Properties
        public Pais Pais { get; private set; } = null!;
        public Municipio? Municipio { get; private set; }

        protected CodigoPostalCache() { } // EF Core

        public CodigoPostalCache(
            Guid paisId,
            string codigoPostal,
            string? logradouro,
            string? bairro,
            Guid? municipioId,
            string? uf,
            string? provedor,
            string criadoPor)
            : base("system", criadoPor)
        {
            AddNotifications(new Contract<CodigoPostalCache>()
                .Requires()
                .AreNotEquals(paisId, Guid.Empty, nameof(PaisId), "O ID do país é obrigatório.")
                .IsNotNullOrEmpty(codigoPostal, nameof(CodigoPostal), "O código postal é obrigatório.")
                .HasMaxLen(codigoPostal, 15, nameof(CodigoPostal), "O código postal deve ter no máximo 15 caracteres.")
            );

            PaisId = paisId;
            CodigoPostal = codigoPostal;
            Logradouro = logradouro;
            Bairro = bairro;
            MunicipioId = municipioId;
            Uf = uf;
            Provedor = provedor;
            ConsultadoEm = DateTime.UtcNow;
            Falhou = false;
        }

        public static CodigoPostalCache CriarFalha(
            Guid paisId,
            string codigoPostal,
            string provedor,
            string motivoFalha,
            string criadoPor)
        {
            var cache = new CodigoPostalCache(
                paisId,
                codigoPostal,
                null,
                null,
                null,
                null,
                provedor,
                criadoPor
            );
            cache.Falhou = true;
            cache.MotivoFalha = motivoFalha;
            
            cache.AddNotifications(new Contract<CodigoPostalCache>()
                .Requires()
                .AreNotEquals(paisId, Guid.Empty, nameof(PaisId), "O ID do país é obrigatório.")
                .IsNotNullOrEmpty(codigoPostal, nameof(CodigoPostal), "O código postal é obrigatório.")
            );
            return cache;
        }

        public static CodigoPostalCache CriarManual(
            Guid paisId,
            string codigoPostal,
            string logradouro,
            string bairro,
            Guid municipioId,
            string uf,
            string justificativa,
            string criadoPor)
        {
            var cache = new CodigoPostalCache(
                paisId,
                codigoPostal,
                logradouro,
                bairro,
                municipioId,
                uf,
                "Manual",
                criadoPor
            );
            cache.MotivoFalha = justificativa;
            
            cache.AddNotifications(new Contract<CodigoPostalCache>()
                .Requires()
                .AreNotEquals(paisId, Guid.Empty, nameof(PaisId), "O ID do país é obrigatório.")
                .IsNotNullOrEmpty(codigoPostal, nameof(CodigoPostal), "O código postal é obrigatório.")
                .IsNotNullOrEmpty(logradouro, nameof(Logradouro), "O logradouro é obrigatório para registro manual.")
                .IsNotNullOrEmpty(justificativa, nameof(MotivoFalha), "A justificativa é obrigatória para registro manual.")
            );
            return cache;
        }
    }
}
