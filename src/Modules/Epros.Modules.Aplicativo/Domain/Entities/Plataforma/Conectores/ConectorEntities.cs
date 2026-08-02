using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Conectores
{
    /// <summary>
    /// PLT · CONECTORES/WEBHOOKS (ED-08) — endpoint de webhook genérico registrado por um tenant.
    /// Framework geral (hoje só existe gateway de pagamento). O segredo de assinatura HMAC é cifrado
    /// no cofre (T5) — nunca em claro. A entrega usa a abstração de dispatch (HTTP real = ambiente).
    /// </summary>
    public class EndpointWebhook : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Url { get; private set; } = string.Empty;
        /// <summary>Lista de event types inscritos (JSON de strings do catálogo central T2).</summary>
        public string EventosInscritosJson { get; private set; } = "[]";
        /// <summary>Segredo HMAC CIFRADO no cofre (T5). Nunca persistido em claro.</summary>
        public string? SegredoCifrado { get; private set; }
        public string? HeadersJson { get; private set; }
        public int MaxTentativas { get; private set; } = 5;
        public bool Ativo { get; private set; } = true;

        protected EndpointWebhook() { }

        public EndpointWebhook(string nome, string url, string eventosInscritosJson, string? segredoCifrado,
            string? headersJson, int maxTentativas, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EndpointWebhook>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do endpoint é obrigatório.")
                .IsNotNullOrEmpty(url, nameof(Url), "A URL do endpoint é obrigatória.")
                .IsTrue(Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == "https" || u.Scheme == "http"),
                    nameof(Url), "A URL deve ser http(s) absoluta.")
                .IsGreaterOrEqualsThan(maxTentativas, 1, nameof(MaxTentativas), "MaxTentativas deve ser >= 1."));

            Nome = nome;
            Url = url;
            EventosInscritosJson = string.IsNullOrWhiteSpace(eventosInscritosJson) ? "[]" : eventosInscritosJson;
            SegredoCifrado = segredoCifrado;
            HeadersJson = headersJson;
            MaxTentativas = maxTentativas;
            Ativo = true;
        }

        public void Atualizar(string? eventosInscritosJson, string? headersJson, int? maxTentativas, string usuario)
        {
            if (!string.IsNullOrWhiteSpace(eventosInscritosJson)) EventosInscritosJson = eventosInscritosJson;
            if (headersJson != null) HeadersJson = headersJson;
            if (maxTentativas is >= 1) MaxTentativas = maxTentativas.Value;
            MarcarAlterado(usuario);
        }

        public void DefinirSegredo(string? segredoCifrado, string usuario)
        {
            SegredoCifrado = segredoCifrado;
            MarcarAlterado(usuario);
        }

        public void Ativar(string usuario) { Ativo = true; MarcarAlterado(usuario); }
        public void Desativar(string usuario) { Ativo = false; MarcarAlterado(usuario); }
    }

    /// <summary>
    /// Tentativa de entrega de um evento a um endpoint. Estado + retry com backoff + dead-letter (TC-03).
    /// </summary>
    public class EntregaWebhook : EntidadeSaaSBase
    {
        public Guid EndpointId { get; private set; }
        public string EventType { get; private set; } = string.Empty;
        public string Payload { get; private set; } = string.Empty;
        public string? AssinaturaHmac { get; private set; }
        /// <summary>Pendente, Entregue, Falha, DeadLetter.</summary>
        public string Status { get; private set; } = "Pendente";
        public int Tentativas { get; private set; }
        public DateTime? ProximaTentativaEm { get; private set; }
        public int? CodigoRespostaHttp { get; private set; }
        public string? UltimoErro { get; private set; }

        protected EntregaWebhook() { }

        public EntregaWebhook(Guid endpointId, string eventType, string payload, string? assinaturaHmac,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EntregaWebhook>()
                .Requires()
                .IsTrue(endpointId != Guid.Empty, nameof(EndpointId), "O endpoint é obrigatório.")
                .IsNotNullOrEmpty(eventType, nameof(EventType), "O tipo de evento é obrigatório."));

            EndpointId = endpointId;
            EventType = eventType;
            Payload = payload;
            AssinaturaHmac = assinaturaHmac;
            Status = "Pendente";
            Tentativas = 0;
            ProximaTentativaEm = DateTime.UtcNow;
        }

        public bool Pendente => Status == "Pendente";

        public void RegistrarSucesso(int? codigoHttp, string usuario)
        {
            Tentativas++;
            Status = "Entregue";
            CodigoRespostaHttp = codigoHttp;
            ProximaTentativaEm = null;
            UltimoErro = null;
            MarcarAlterado(usuario);
        }

        /// <summary>Falha: incrementa tentativa; agenda backoff exponencial ou vai a dead-letter.</summary>
        public void RegistrarFalha(int maxTentativas, int? codigoHttp, string erro, string usuario)
        {
            Tentativas++;
            CodigoRespostaHttp = codigoHttp;
            UltimoErro = erro;
            if (Tentativas >= maxTentativas)
            {
                Status = "DeadLetter";
                ProximaTentativaEm = null;
            }
            else
            {
                Status = "Pendente";
                var minutos = Math.Min(60, (int)Math.Pow(2, Tentativas)); // 2,4,8,16,32,60...
                ProximaTentativaEm = DateTime.UtcNow.AddMinutes(minutos);
            }
            MarcarAlterado(usuario);
        }
    }
}
