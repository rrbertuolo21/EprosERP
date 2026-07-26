using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>
    /// Configuração de um gateway de pagamento (ex.: Mercado Pago) da plataforma.
    /// É <see cref="IGlobalEntity"/>: não sofre filtro automático por tenant, pois convive
    /// a configuração global (<see cref="TenantAlvo"/> == null) com overrides por tenant
    /// (<see cref="TenantAlvo"/> preenchido).
    /// Segredos (<see cref="AccessToken"/>, <see cref="WebhookSecret"/>) são persistidos já
    /// cifrados (ciphertext do <c>ISegredoCofreService</c>) — a cifragem é responsabilidade do handler.
    /// </summary>
    public class ConfiguracaoGatewayPagamento : EntidadeSaaSBase, IGlobalEntity
    {
        public EProvedorGateway Provedor { get; private set; } = EProvedorGateway.MercadoPago;
        public EAmbienteGateway Ambiente { get; private set; } = EAmbienteGateway.Sandbox;

        /// <summary>Access token do gateway. Armazenado cifrado (ciphertext do cofre).</summary>
        public string AccessToken { get; private set; } = string.Empty;

        /// <summary>Public key do gateway (não sigilosa).</summary>
        public string? PublicKey { get; private set; }

        /// <summary>Segredo de validação do webhook. Armazenado cifrado (ciphertext do cofre).</summary>
        public string? WebhookSecret { get; private set; }

        public string Moeda { get; private set; } = "BRL";
        public string? NotificationUrl { get; private set; }

        /// <summary>
        /// Escopo da configuração (o "TenantId" da especificação):
        /// <c>null</c> = configuração global da plataforma; preenchido = override para o tenant informado.
        /// Não confundir com <see cref="EntidadeSaaSBase.TenantId"/> (sempre "system" aqui, pois é entidade global).
        /// </summary>
        public string? TenantAlvo { get; private set; }

        public bool Ativo { get; private set; } = true;

        protected ConfiguracaoGatewayPagamento() { } // EF Core

        public ConfiguracaoGatewayPagamento(
            EProvedorGateway provedor,
            EAmbienteGateway ambiente,
            string accessTokenCifrado,
            string? publicKey,
            string? webhookSecretCifrado,
            string? moeda,
            string? notificationUrl,
            string? tenantAlvo,
            bool ativo,
            string criadoPor)
            : base("system", criadoPor)
        {
            Provedor = provedor;
            Ambiente = ambiente;
            AccessToken = accessTokenCifrado;
            PublicKey = publicKey;
            WebhookSecret = webhookSecretCifrado;
            Moeda = string.IsNullOrWhiteSpace(moeda) ? "BRL" : moeda!.Trim().ToUpperInvariant();
            NotificationUrl = notificationUrl;
            TenantAlvo = string.IsNullOrWhiteSpace(tenantAlvo) ? null : tenantAlvo!.Trim();
            Ativo = ativo;

            Validar();
        }

        /// <summary>
        /// Atualiza a configuração. Se <paramref name="accessTokenCifrado"/> ou
        /// <paramref name="webhookSecretCifrado"/> vierem null, o segredo atual é preservado
        /// (permite editar sem reenviar o segredo).
        /// </summary>
        public void Atualizar(
            EProvedorGateway provedor,
            EAmbienteGateway ambiente,
            string? accessTokenCifrado,
            string? publicKey,
            string? webhookSecretCifrado,
            string? moeda,
            string? notificationUrl,
            string? tenantAlvo,
            bool ativo,
            string alteradoPor)
        {
            Provedor = provedor;
            Ambiente = ambiente;
            if (!string.IsNullOrWhiteSpace(accessTokenCifrado))
                AccessToken = accessTokenCifrado!;
            PublicKey = publicKey;
            if (!string.IsNullOrWhiteSpace(webhookSecretCifrado))
                WebhookSecret = webhookSecretCifrado;
            Moeda = string.IsNullOrWhiteSpace(moeda) ? "BRL" : moeda!.Trim().ToUpperInvariant();
            NotificationUrl = notificationUrl;
            TenantAlvo = string.IsNullOrWhiteSpace(tenantAlvo) ? null : tenantAlvo!.Trim();
            Ativo = ativo;

            Validar();
            if (IsValid) MarcarAlterado(alteradoPor);
        }

        public void DefinirAtivo(bool ativo, string alteradoPor)
        {
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
        }

        private void Validar()
        {
            Clear();
            AddNotifications(new Contract<ConfiguracaoGatewayPagamento>()
                .Requires()
                .IsNotNullOrEmpty(AccessToken, nameof(AccessToken), "AccessToken é obrigatório")
                .IsNotNullOrEmpty(Moeda, nameof(Moeda), "Moeda é obrigatória")
                .HasMaxLen(Moeda, 3, nameof(Moeda), "Moeda deve ter no máximo 3 caracteres (ISO 4217)")
            );
        }
    }
}
