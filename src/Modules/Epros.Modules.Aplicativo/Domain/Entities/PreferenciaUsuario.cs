using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class PreferenciaUsuario : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public string? Idioma { get; private set; }
        public string? Tema { get; private set; }
        public string? Avatar { get; private set; }
        public bool RecebeNotificacoes { get; private set; }
        public string? Timezone { get; private set; }
        public string? FormatoData { get; private set; }
        public string? FormatoHora { get; private set; }
        public string? FormatoNumero { get; private set; }
        public string? PreferenciasJson { get; private set; }

        protected PreferenciaUsuario() { } // EF Core

        public PreferenciaUsuario(
            string tenantId,
            Guid usuarioId,
            string? idioma,
            string? tema,
            string? avatar,
            bool recebeNotificacoes,
            string? timezone,
            string? formatoData,
            string? formatoHora,
            string? formatoNumero,
            string? preferenciasJson,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (usuarioId == Guid.Empty)
                AddNotification(nameof(UsuarioId), "O ID do usuário é obrigatório.");

            UsuarioId = usuarioId;
            Idioma = idioma;
            Tema = tema;
            Avatar = avatar;
            RecebeNotificacoes = recebeNotificacoes;
            Timezone = timezone;
            FormatoData = formatoData;
            FormatoHora = formatoHora;
            FormatoNumero = formatoNumero;
            PreferenciasJson = preferenciasJson;
        }

        public void Atualizar(
            string? idioma,
            string? tema,
            string? avatar,
            bool recebeNotificacoes,
            string? timezone,
            string? formatoData,
            string? formatoHora,
            string? formatoNumero,
            string? preferenciasJson,
            string alteradoPor)
        {
            Idioma = idioma;
            Tema = tema;
            Avatar = avatar;
            RecebeNotificacoes = recebeNotificacoes;
            Timezone = timezone;
            FormatoData = formatoData;
            FormatoHora = formatoHora;
            FormatoNumero = formatoNumero;
            PreferenciasJson = preferenciasJson;
            MarcarAlterado(alteradoPor);
        }
    }
}
