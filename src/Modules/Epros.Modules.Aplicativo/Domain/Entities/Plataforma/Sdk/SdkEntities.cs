using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Sdk
{
    /// <summary>
    /// PLT · SDK/EXTENSÕES (PD-09, recorte) — chave de API pública com escopos. Recorte do submódulo:
    /// chaves + escopos (não o gateway inteiro, que é TC-10). O SEGREDO nunca é persistido: guarda-se
    /// apenas o hash (SHA-256) e um prefixo visível para identificação. Revogável e com expiração.
    /// </summary>
    public class ChaveApiPublica : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        /// <summary>Prefixo visível (ex.: "epk_ab12cd34") para identificar a chave sem revelar o segredo.</summary>
        public string Prefixo { get; private set; } = string.Empty;
        /// <summary>Hash SHA-256 (hex) do segredo completo. O segredo em claro NUNCA é armazenado.</summary>
        public string HashChave { get; private set; } = string.Empty;
        /// <summary>Escopos concedidos (JSON de strings do catálogo de escopos).</summary>
        public string EscoposJson { get; private set; } = "[]";
        public bool Ativa { get; private set; } = true;
        public DateTime? ExpiraEm { get; private set; }
        public DateTime? UltimoUsoEm { get; private set; }
        public DateTime? RevogadaEm { get; private set; }

        protected ChaveApiPublica() { }

        public ChaveApiPublica(string nome, string prefixo, string hashChave, string escoposJson,
            DateTime? expiraEm, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ChaveApiPublica>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da chave é obrigatório.")
                .IsNotNullOrEmpty(prefixo, nameof(Prefixo), "O prefixo da chave é obrigatório.")
                .IsNotNullOrEmpty(hashChave, nameof(HashChave), "O hash da chave é obrigatório."));

            Nome = nome;
            Prefixo = prefixo;
            HashChave = hashChave;
            EscoposJson = string.IsNullOrWhiteSpace(escoposJson) ? "[]" : escoposJson;
            ExpiraEm = expiraEm;
            Ativa = true;
        }

        public bool Expirada => ExpiraEm.HasValue && ExpiraEm.Value < DateTime.UtcNow;
        public bool Valida => Ativa && RevogadaEm == null && !Expirada;

        public void RegistrarUso() => UltimoUsoEm = DateTime.UtcNow;

        public void Revogar(string usuario)
        {
            Ativa = false;
            RevogadaEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }

    /// <summary>Catálogo de escopos concedíveis a uma chave de API pública (recorte da plataforma).</summary>
    public static class EscoposApi
    {
        public static readonly IReadOnlyList<string> Disponiveis = new[]
        {
            "ged:ler", "ged:escrever",
            "assinatura:ler", "assinatura:solicitar",
            "analytics:ler", "analytics:escrever",
            "conectores:publicar",
            "iot:ingerir", "iot:ler",
            "wizards:ler", "wizards:executar"
        };

        public static bool EhValido(string escopo) => System.Linq.Enumerable.Contains(Disponiveis, escopo);
    }
}
