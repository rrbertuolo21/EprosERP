using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class ConfiguracaoEmpresa : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string? Telefone { get; private set; }
        // 1.07 — Tipo do telefone informado no cadastro (Fixo/Celular/Comercial/Whatsapp). Antes era
        // validado e descartado; agora é persistido junto do número (nulo honesto quando não informado).
        public string? TelefoneTipo { get; private set; }
        public string? Endereco { get; private set; }
        public int TimeZoneId { get; private set; }
        public string DateFormat { get; private set; } = "DD-MM-YYYY";
        public int CurrencyId { get; private set; }
        public decimal VatPercentage { get; private set; }
        public int VatType { get; private set; } // 1: Inclusive, 2: Exclusive
        public int CurrencyPosition { get; private set; } // 1: Left, 2: Right
        public string? FooterText { get; private set; }
        public string? Logo { get; private set; }
        public string? Favicon { get; private set; }

        protected ConfiguracaoEmpresa() { } // EF Core

        public ConfiguracaoEmpresa(
            string tenantId,
            Guid empresaId,
            string nome,
            string email,
            string? telefone,
            string? endereco,
            int timeZoneId,
            string dateFormat,
            int currencyId,
            decimal vatPercentage,
            int vatType,
            int currencyPosition,
            string? footerText,
            string? logo,
            string? favicon,
            string criadoPor,
            string? telefoneTipo = null)
            : base(tenantId, criadoPor)
        {
            if (empresaId == Guid.Empty)
                AddNotification(nameof(EmpresaId), "O ID da empresa é obrigatório.");
            if (string.IsNullOrWhiteSpace(nome))
                AddNotification(nameof(Nome), "O Nome da empresa é obrigatório.");
            if (string.IsNullOrWhiteSpace(email))
                AddNotification(nameof(Email), "O E-mail da empresa é obrigatório.");
            if (timeZoneId <= 0)
                AddNotification(nameof(TimeZoneId), "O TimeZoneId deve ser maior que zero.");
            if (currencyId <= 0)
                AddNotification(nameof(CurrencyId), "O CurrencyId deve ser maior que zero.");
            if (vatPercentage < 0)
                AddNotification(nameof(VatPercentage), "O percentual do imposto não pode ser negativo.");
            if (vatType != 1 && vatType != 2)
                AddNotification(nameof(VatType), "O tipo de imposto (VatType) deve ser 1 (Inclusivo) ou 2 (Exclusivo).");
            if (currencyPosition != 1 && currencyPosition != 2)
                AddNotification(nameof(CurrencyPosition), "A posição da moeda deve ser 1 (Esquerda) ou 2 (Direita).");

            EmpresaId = empresaId;
            Nome = nome;
            Email = email;
            Telefone = telefone;
            TelefoneTipo = telefoneTipo;
            Endereco = endereco;
            TimeZoneId = timeZoneId;
            DateFormat = dateFormat ?? "DD-MM-YYYY";
            CurrencyId = currencyId;
            VatPercentage = vatPercentage;
            VatType = vatType;
            CurrencyPosition = currencyPosition;
            FooterText = footerText;
            Logo = logo;
            Favicon = favicon;
        }

        public void Atualizar(
            string nome,
            string email,
            string? telefone,
            string? endereco,
            int timeZoneId,
            string dateFormat,
            int currencyId,
            decimal vatPercentage,
            int vatType,
            int currencyPosition,
            string? footerText,
            string? logo,
            string? favicon,
            string alteradoPor,
            string? telefoneTipo = null)
        {
            if (string.IsNullOrWhiteSpace(nome))
                AddNotification(nameof(Nome), "O Nome da empresa é obrigatório.");
            if (string.IsNullOrWhiteSpace(email))
                AddNotification(nameof(Email), "O E-mail da empresa é obrigatório.");
            if (timeZoneId <= 0)
                AddNotification(nameof(TimeZoneId), "O TimeZoneId deve ser maior que zero.");
            if (currencyId <= 0)
                AddNotification(nameof(CurrencyId), "O CurrencyId deve ser maior que zero.");
            if (vatPercentage < 0)
                AddNotification(nameof(VatPercentage), "O percentual do imposto não pode ser negativo.");
            if (vatType != 1 && vatType != 2)
                AddNotification(nameof(VatType), "O tipo de imposto (VatType) deve ser 1 (Inclusivo) ou 2 (Exclusivo).");
            if (currencyPosition != 1 && currencyPosition != 2)
                AddNotification(nameof(CurrencyPosition), "A posição da moeda deve ser 1 (Esquerda) ou 2 (Direita).");

            if (IsValid)
            {
                Nome = nome;
                Email = email;
                Telefone = telefone;
                // Preserva o tipo de telefone gravado no onboarding quando a atualização não o informa
                // (o comando de configuração geral não gerencia esse campo). Só sobrescreve se vier valor.
                if (telefoneTipo != null) TelefoneTipo = telefoneTipo;
                Endereco = endereco;
                TimeZoneId = timeZoneId;
                DateFormat = dateFormat;
                CurrencyId = currencyId;
                VatPercentage = vatPercentage;
                VatType = vatType;
                CurrencyPosition = currencyPosition;
                FooterText = footerText;
                Logo = logo;
                Favicon = favicon;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
