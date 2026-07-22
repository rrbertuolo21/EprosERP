using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class FormatoCodigoPostal : EntidadeSaaSBase, IGlobalEntity
    {
        public Guid PaisId { get; private set; }
        public string Regex { get; private set; } = string.Empty;
        public string Mascara { get; private set; } = string.Empty;
        public string? Exemplo { get; private set; }

        // Navigation Properties
        public Pais Pais { get; private set; } = null!;

        protected FormatoCodigoPostal() { } // EF Core

        public FormatoCodigoPostal(
            Guid paisId,
            string regex,
            string mascara,
            string? exemplo,
            string criadoPor)
            : base("system", criadoPor)
        {
            AddNotifications(new Contract<FormatoCodigoPostal>()
                .Requires()
                .AreNotEquals(paisId, Guid.Empty, nameof(PaisId), "O ID do país é obrigatório.")
                .IsNotNullOrEmpty(regex, nameof(Regex), "O padrão Regex é obrigatório.")
                .HasMaxLen(regex, 100, nameof(Regex), "O padrão Regex deve ter no máximo 100 caracteres.")
                .IsNotNullOrEmpty(mascara, nameof(Mascara), "A Máscara de exibição é obrigatória.")
                .HasMaxLen(mascara, 30, nameof(Mascara), "A Máscara de exibição deve ter no máximo 30 caracteres.")
            );

            PaisId = paisId;
            Regex = regex;
            Mascara = mascara;
            Exemplo = exemplo;
        }
    }
}
