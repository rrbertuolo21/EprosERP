using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class ConfiguracaoEmail : EntidadeSaaSBase
    {
        public string? Host { get; private set; }
        public int? Port { get; private set; }
        public string? Username { get; private set; }
        public string? Password { get; private set; }
        public string? FromEmail { get; private set; }

        protected ConfiguracaoEmail() { } // EF Core

        public ConfiguracaoEmail(
            string? host,
            int? port,
            string? username,
            string? password,
            string? fromEmail,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (host != null)
                AddNotifications(new Contract<ConfiguracaoEmail>().Requires().HasMaxLen(host, 150, nameof(Host), "Host deve ter no máximo 150 caracteres."));
            if (username != null)
                AddNotifications(new Contract<ConfiguracaoEmail>().Requires().HasMaxLen(username, 150, nameof(Username), "Username deve ter no máximo 150 caracteres."));
            if (password != null)
                AddNotifications(new Contract<ConfiguracaoEmail>().Requires().HasMaxLen(password, 250, nameof(Password), "Senha deve ter no máximo 250 caracteres."));
            if (fromEmail != null)
                AddNotifications(new Contract<ConfiguracaoEmail>().Requires().HasMaxLen(fromEmail, 150, nameof(FromEmail), "E-mail do remetente deve ter no máximo 150 caracteres."));

            Host = host;
            Port = port;
            Username = username;
            Password = password;
            FromEmail = fromEmail;
        }

        public void Atualizar(
            string? host,
            int? port,
            string? username,
            string? password,
            string? fromEmail,
            string alteradoPor)
        {
            if (host != null)
                AddNotifications(new Contract<ConfiguracaoEmail>().Requires().HasMaxLen(host, 150, nameof(Host), "Host deve ter no máximo 150 caracteres."));
            if (username != null)
                AddNotifications(new Contract<ConfiguracaoEmail>().Requires().HasMaxLen(username, 150, nameof(Username), "Username deve ter no máximo 150 caracteres."));
            if (password != null)
                AddNotifications(new Contract<ConfiguracaoEmail>().Requires().HasMaxLen(password, 250, nameof(Password), "Senha deve ter no máximo 250 caracteres."));
            if (fromEmail != null)
                AddNotifications(new Contract<ConfiguracaoEmail>().Requires().HasMaxLen(fromEmail, 150, nameof(FromEmail), "E-mail do remetente deve ter no máximo 150 caracteres."));

            if (IsValid)
            {
                Host = host;
                Port = port;
                Username = username;
                if (!string.IsNullOrEmpty(password) && password != "••••••••")
                {
                    Password = password;
                }
                FromEmail = fromEmail;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
