using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class LogAuditoriaConfiguracao : EntidadeSaaSBase
    {
        public string Entidade { get; private set; } = string.Empty;
        public Guid RegistroId { get; private set; }
        public string Campo { get; private set; } = string.Empty;
        public string? ValorAnterior { get; private set; }
        public string? ValorNovo { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime DataHora { get; private set; }
        public string? Justificativa { get; private set; }

        protected LogAuditoriaConfiguracao() { } // EF Core

        public LogAuditoriaConfiguracao(
            string entidade,
            Guid registroId,
            string campo,
            string? valorAnterior,
            string? valorNovo,
            string usuarioId,
            string? justificativa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<LogAuditoriaConfiguracao>()
                .Requires()
                .IsNotNullOrEmpty(entidade, nameof(Entidade), "Nome da entidade é obrigatório.")
                .AreNotEquals(registroId, Guid.Empty, nameof(RegistroId), "RegistroId inválido.")
                .IsNotNullOrEmpty(campo, nameof(Campo), "Nome do campo é obrigatório.")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "Usuário associado é obrigatório.")
            );

            Entidade = entidade;
            RegistroId = registroId;
            Campo = campo;
            ValorAnterior = valorAnterior;
            ValorNovo = valorNovo;
            UsuarioId = usuarioId;
            DataHora = DateTime.UtcNow;
            Justificativa = justificativa;
        }
    }
}
