using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Contracts;
using Epros.Modules.Aplicativo.Domain.Entities.Plataforma.Conectores;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Conectores
{
    /// <summary>
    /// PLT · CONECTORES/WEBHOOKS (ED-08) — framework geral: registro de endpoint, fan-out por evento,
    /// entrega com retry/backoff/dead-letter (TC-03) e assinatura HMAC. Segredo no cofre (T5);
    /// eventos validados contra o catálogo central (T2). HTTP real = ambiente.
    /// </summary>
    public class RegistrarEndpointWebhookCommandHandler : ICommandHandler<RegistrarEndpointWebhookCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        private readonly ISegredoCofreService _cofre;

        public RegistrarEndpointWebhookCommandHandler(ContextAplicativo context, ITenantProvider tenant,
            ICurrentUser user, ISegredoCofreService cofre)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
            _cofre = cofre;
        }

        public async Task<CommandResult> Handle(RegistrarEndpointWebhookCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var desconhecidos = request.Eventos.Where(e => !CatalogoEventosIntegracao.EhEventoConhecido(e)).ToList();
            if (desconhecidos.Any())
                return CommandResult.Falha($"Eventos não reconhecidos no catálogo (T2): {string.Join(", ", desconhecidos)}");

            string? segredoCifrado = null;
            if (!string.IsNullOrWhiteSpace(request.Segredo))
                segredoCifrado = await _cofre.CriptografarAsync(request.Segredo); // T5 — nunca em claro

            var endpoint = new EndpointWebhook(request.Nome, request.Url,
                JsonSerializer.Serialize(request.Eventos), segredoCifrado, request.HeadersJson,
                request.MaxTentativas <= 0 ? 5 : request.MaxTentativas, tenantId, usuario);
            if (!endpoint.IsValid) return CommandResult.Falha(endpoint.Notifications.Select(n => n.Message));

            _context.EndpointsWebhook.Add(endpoint);
            _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                CatalogoEventosIntegracao.Plataforma.ConectorEndpointRegistrado,
                JsonSerializer.Serialize(new { endpoint.Id, endpoint.Url })));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Endpoint de webhook registrado.", new { endpoint.Id });
        }
    }

    public class AtualizarEndpointWebhookCommandHandler : ICommandHandler<AtualizarEndpointWebhookCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;

        public AtualizarEndpointWebhookCommandHandler(ContextAplicativo context, ITenantProvider tenant, ICurrentUser user)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
        }

        public async Task<CommandResult> Handle(AtualizarEndpointWebhookCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var endpoint = await _context.EndpointsWebhook.FirstOrDefaultAsync(e => e.Id == request.EndpointId, ct);
            if (endpoint == null) return CommandResult.Falha("Endpoint não encontrado.");

            if (request.Eventos != null)
            {
                var desconhecidos = request.Eventos.Where(e => !CatalogoEventosIntegracao.EhEventoConhecido(e)).ToList();
                if (desconhecidos.Any())
                    return CommandResult.Falha($"Eventos não reconhecidos: {string.Join(", ", desconhecidos)}");
                endpoint.Atualizar(JsonSerializer.Serialize(request.Eventos), request.HeadersJson, request.MaxTentativas, usuario);
            }
            else
            {
                endpoint.Atualizar(null, request.HeadersJson, request.MaxTentativas, usuario);
            }

            if (request.Ativo == true) endpoint.Ativar(usuario);
            else if (request.Ativo == false) endpoint.Desativar(usuario);

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Endpoint atualizado.");
        }
    }

    public class PublicarEventoWebhookCommandHandler : ICommandHandler<PublicarEventoWebhookCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        private readonly ISegredoCofreService _cofre;

        public PublicarEventoWebhookCommandHandler(ContextAplicativo context, ITenantProvider tenant,
            ICurrentUser user, ISegredoCofreService cofre)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
            _cofre = cofre;
        }

        public async Task<CommandResult> Handle(PublicarEventoWebhookCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var endpoints = await _context.EndpointsWebhook.Where(e => e.Ativo).ToListAsync(ct);
            var inscritos = endpoints.Where(e =>
            {
                var eventos = JsonSerializer.Deserialize<List<string>>(e.EventosInscritosJson) ?? new();
                return eventos.Contains(request.EventType);
            }).ToList();

            var criadas = 0;
            foreach (var endpoint in inscritos)
            {
                string? assinatura = null;
                if (!string.IsNullOrWhiteSpace(endpoint.SegredoCifrado))
                {
                    var segredo = await _cofre.DescriptografarAsync(endpoint.SegredoCifrado);
                    assinatura = AssinarHmac(request.Payload, segredo);
                }
                _context.EntregasWebhook.Add(new EntregaWebhook(endpoint.Id, request.EventType, request.Payload,
                    assinatura, tenantId, usuario));
                criadas++;
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"{criadas} entrega(s) enfileirada(s).", new { Entregas = criadas });
        }

        internal static string AssinarHmac(string payload, string segredo)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(segredo));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        }
    }

    public class ProcessarEntregaWebhookCommandHandler : ICommandHandler<ProcessarEntregaWebhookCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        private readonly IWebhookDispatchService _dispatch;

        public ProcessarEntregaWebhookCommandHandler(ContextAplicativo context, ITenantProvider tenant,
            ICurrentUser user, IWebhookDispatchService dispatch)
        {
            _context = context;
            _tenant = tenant;
            _user = user;
            _dispatch = dispatch;
        }

        public async Task<CommandResult> Handle(ProcessarEntregaWebhookCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var usuario = _user.GetUserId() ?? "system";

            var entrega = await _context.EntregasWebhook.FirstOrDefaultAsync(e => e.Id == request.EntregaId, ct);
            if (entrega == null) return CommandResult.Falha("Entrega não encontrada.");
            if (!entrega.Pendente) return CommandResult.Falha($"Entrega não está pendente (status {entrega.Status}).");

            var endpoint = await _context.EndpointsWebhook.FirstOrDefaultAsync(e => e.Id == entrega.EndpointId, ct);
            if (endpoint == null) return CommandResult.Falha("Endpoint da entrega não encontrado.");

            IReadOnlyDictionary<string, string>? headers = string.IsNullOrWhiteSpace(endpoint.HeadersJson)
                ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(endpoint.HeadersJson);

            var resultado = await _dispatch.EnviarAsync(endpoint.Url, entrega.Payload, entrega.AssinaturaHmac, headers, ct);

            if (!resultado.Configurado)
                return CommandResult.Ok("Dispatcher de webhook não configurado — entrega permanece pendente (valida-ambiente).",
                    new { Pendente = true });

            if (resultado.Sucesso)
            {
                entrega.RegistrarSucesso(resultado.CodigoHttp, usuario);
                _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                    CatalogoEventosIntegracao.Plataforma.ConectorEntregaConcluida,
                    JsonSerializer.Serialize(new { entrega.Id, entrega.EndpointId })));
            }
            else
            {
                entrega.RegistrarFalha(endpoint.MaxTentativas, resultado.CodigoHttp, resultado.Erro ?? "erro", usuario);
                if (entrega.Status == "DeadLetter")
                    _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                        CatalogoEventosIntegracao.Plataforma.ConectorEntregaFalhou,
                        JsonSerializer.Serialize(new { entrega.Id, entrega.EndpointId, entrega.Tentativas })));
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"Entrega processada: {entrega.Status}.", new { entrega.Status, entrega.Tentativas });
        }
    }

    // ===================== Queries =====================

    public class ObterEndpointsWebhookQueryHandler : IQueryHandler<ObterEndpointsWebhookQuery, IReadOnlyList<EndpointWebhookDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterEndpointsWebhookQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<EndpointWebhookDto>> Handle(ObterEndpointsWebhookQuery request, CancellationToken ct)
        {
            var q = _context.EndpointsWebhook.AsNoTracking().AsQueryable();
            if (request.ApenasAtivos) q = q.Where(e => e.Ativo);
            var lista = await q.OrderBy(e => e.Nome).ToListAsync(ct);
            return lista.Select(e => new EndpointWebhookDto(e.Id, e.Nome, e.Url,
                JsonSerializer.Deserialize<List<string>>(e.EventosInscritosJson) ?? new(),
                !string.IsNullOrWhiteSpace(e.SegredoCifrado), e.MaxTentativas, e.Ativo, e.CriadoEm)).ToList();
        }
    }

    public class ObterEntregasWebhookQueryHandler : IQueryHandler<ObterEntregasWebhookQuery, IReadOnlyList<EntregaWebhookDto>>
    {
        private readonly ContextAplicativo _context;
        public ObterEntregasWebhookQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IReadOnlyList<EntregaWebhookDto>> Handle(ObterEntregasWebhookQuery request, CancellationToken ct)
        {
            var q = _context.EntregasWebhook.AsNoTracking().AsQueryable();
            if (request.EndpointId.HasValue) q = q.Where(e => e.EndpointId == request.EndpointId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status)) q = q.Where(e => e.Status == request.Status);
            return await q.OrderByDescending(e => e.CriadoEm)
                .Select(e => new EntregaWebhookDto(e.Id, e.EndpointId, e.EventType, e.Status, e.Tentativas,
                    e.ProximaTentativaEm, e.CodigoRespostaHttp, e.UltimoErro, e.CriadoEm)).ToListAsync(ct);
        }
    }

    public class ObterEventosPublicaveisQueryHandler : IQueryHandler<ObterEventosPublicaveisQuery, IReadOnlyList<string>>
    {
        public Task<IReadOnlyList<string>> Handle(ObterEventosPublicaveisQuery request, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<string>>(CatalogoEventosIntegracao.Todos.OrderBy(x => x).ToList());
    }
}
