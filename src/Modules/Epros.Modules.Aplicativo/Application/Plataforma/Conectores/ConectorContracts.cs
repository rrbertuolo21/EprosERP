using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Conectores
{
    // ===================== Commands =====================

    /// <summary>
    /// Registra um endpoint de webhook. Os eventos inscritos são validados contra o catálogo central
    /// (T2). O segredo HMAC é cifrado no cofre (T5) antes de persistir.
    /// </summary>
    public record RegistrarEndpointWebhookCommand(
        string Nome, string Url, List<string> Eventos, string? Segredo, string? HeadersJson, int MaxTentativas) : ICommand;

    public class RegistrarEndpointWebhookCommandValidator : AbstractValidator<RegistrarEndpointWebhookCommand>
    {
        public RegistrarEndpointWebhookCommandValidator()
        {
            RuleFor(c => c.Nome).NotEmpty();
            RuleFor(c => c.Url).NotEmpty();
            RuleFor(c => c.Eventos).NotEmpty().WithMessage("Inscreva ao menos um evento.");
        }
    }

    public record AtualizarEndpointWebhookCommand(
        Guid EndpointId, List<string>? Eventos, string? HeadersJson, int? MaxTentativas, bool? Ativo) : ICommand;

    /// <summary>Fan-out: enfileira uma entrega para cada endpoint ativo inscrito no tipo de evento.</summary>
    public record PublicarEventoWebhookCommand(string EventType, string Payload) : ICommand;

    /// <summary>Processa (tenta entregar) uma entrega pendente via o dispatcher (HTTP = ambiente).</summary>
    public record ProcessarEntregaWebhookCommand(Guid EntregaId) : ICommand;

    // ===================== Queries =====================

    public record ObterEndpointsWebhookQuery(bool ApenasAtivos = false) : IQuery<IReadOnlyList<EndpointWebhookDto>>;

    public record ObterEntregasWebhookQuery(Guid? EndpointId = null, string? Status = null) : IQuery<IReadOnlyList<EntregaWebhookDto>>;

    /// <summary>Catálogo de eventos publicáveis (T2) — os event types conhecidos.</summary>
    public record ObterEventosPublicaveisQuery() : IQuery<IReadOnlyList<string>>;

    // ===================== DTOs =====================

    public record EndpointWebhookDto(Guid Id, string Nome, string Url, IReadOnlyList<string> Eventos,
        bool TemSegredo, int MaxTentativas, bool Ativo, DateTime CriadoEm);

    public record EntregaWebhookDto(Guid Id, Guid EndpointId, string EventType, string Status, int Tentativas,
        DateTime? ProximaTentativaEm, int? CodigoRespostaHttp, string? UltimoErro, DateTime CriadoEm);
}
