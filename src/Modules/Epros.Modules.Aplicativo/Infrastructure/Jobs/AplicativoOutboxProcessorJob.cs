using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Aplicativo.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class AplicativoOutboxProcessorJob : IJob
    {
        private readonly ContextAplicativo _context;
        private readonly ContextGestaoClientes _gestaoClientesContext;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificacaoService _notificacaoService;

        public AplicativoOutboxProcessorJob(
            ContextAplicativo context,
            ContextGestaoClientes gestaoClientesContext,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor,
            INotificacaoService notificacaoService)
        {
            _context = context;
            _gestaoClientesContext = gestaoClientesContext;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
            _notificacaoService = notificacaoService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando AplicativoOutboxProcessorJob...");

            var messages = await _context.OutboxMessages
                .IgnoreQueryFilters()
                .Where(m => (m.EventType == "UsuarioCriado" ||
                             m.EventType == "UsuarioAtualizado" ||
                             m.EventType == "UsuarioDeletado" ||
                             m.EventType == "ImpersonacaoIniciada" ||
                             m.EventType == "AcessoSuporteIniciado" ||
                             m.EventType == "ComunicacaoSuperAdminCriada") &&
                             m.ProcessadoEm == null && 
                             m.Tentativas < 5)
                .OrderBy(m => m.CriadoEm)
                .ToListAsync();

            if (!messages.Any())
            {
                Console.WriteLine("[Quartz] Nenhuma mensagem do outbox de Aplicativo para processar.");
                return;
            }

            foreach (var message in messages)
            {
                // Configura o HttpContext com o tenant da mensagem para garantir isolamento
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = message.TenantId;
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    if (message.EventType == "UsuarioCriado")
                    {
                        var payload = JsonSerializer.Deserialize<UsuarioCriadoPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var notification = new UsuarioCriadoEventNotification(
                                UsuarioId: payload.UsuarioId,
                                Nome: payload.Nome,
                                Email: payload.Email,
                                TenantId: payload.TenantId,
                                PerfilUsuarioId: payload.PerfilUsuarioId,
                                Cargo: payload.Cargo,
                                Departamento: payload.Departamento,
                                LimiteDesconto: payload.LimiteDesconto,
                                CriadoPor: payload.CriadoPor
                            );
                            await _mediator.Publish(notification);
                        }
                    }
                    else if (message.EventType == "UsuarioAtualizado")
                    {
                        var payload = JsonSerializer.Deserialize<UsuarioAtualizadoPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var notification = new UsuarioAtualizadoEventNotification(
                                UsuarioId: payload.UsuarioId,
                                Nome: payload.Nome,
                                Email: payload.Email,
                                TenantId: payload.TenantId,
                                PerfilUsuarioId: payload.PerfilUsuarioId,
                                Cargo: payload.Cargo,
                                Departamento: payload.Departamento,
                                LimiteDesconto: payload.LimiteDesconto,
                                AlteradoPor: payload.AlteradoPor
                            );
                            await _mediator.Publish(notification);
                        }
                    }
                    else if (message.EventType == "UsuarioDeletado")
                    {
                        var payload = JsonSerializer.Deserialize<UsuarioDeletadoPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var notification = new UsuarioDeletadoEventNotification(
                                UsuarioId: payload.UsuarioId,
                                AlteradoPor: payload.AlteradoPor
                            );
                            await _mediator.Publish(notification);
                        }
                    }
                    else if (message.EventType == "ImpersonacaoIniciada")
                    {
                        var payload = JsonSerializer.Deserialize<ImpersonacaoIniciadaPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var notification = new ImpersonacaoIniciadaEventNotification(
                                SessaoImpersonacaoId: payload.SessaoImpersonacaoId,
                                UsuarioOriginalId: payload.UsuarioOriginalId,
                                UsuarioAlvoId: payload.UsuarioAlvoId,
                                EmpresaId: payload.EmpresaId,
                                Motivo: payload.Motivo,
                                CriadoPor: payload.CriadoPor,
                                TenantId: payload.TenantId
                            );
                            await _mediator.Publish(notification);
                        }
                    }
                    else if (message.EventType == "AcessoSuporteIniciado")
                    {
                        var payload = JsonSerializer.Deserialize<AcessoSuporteIniciadoPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var notification = new AcessoSuporteIniciadoEventNotification(
                                SessaoImpersonacaoId: payload.SessaoImpersonacaoId,
                                UsuarioOriginalId: payload.UsuarioOriginalId,
                                UsuarioAlvoId: payload.UsuarioAlvoId,
                                EmpresaId: payload.EmpresaId,
                                Motivo: payload.Motivo,
                                CriadoPor: payload.CriadoPor,
                                TenantAlvo: payload.TenantAlvo,
                                PerfilSuporte: payload.PerfilSuporte
                            );
                            await _mediator.Publish(notification);
                        }
                    }
                    else if (message.EventType == "ComunicacaoSuperAdminCriada")
                    {
                        var payload = JsonSerializer.Deserialize<ComunicacaoSuperAdminCriadaPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var clientes = await _gestaoClientesContext.Clientes
                                .IgnoreQueryFilters()
                                .Where(c => payload.BusinessIds.Contains(c.TenantId) && c.DeletadoEm == null)
                                .Select(c => new { c.TenantId, c.RazaoSocial, c.Email, c.Telefone })
                                .ToListAsync();

                            var comunicacao = await _context.ComunicacoesSuperAdmin
                                .IgnoreQueryFilters()
                                .FirstOrDefaultAsync(c => c.Id == payload.ComunicacaoId);

                            if (comunicacao == null)
                            {
                                throw new InvalidOperationException($"Comunicação {payload.ComunicacaoId} não encontrada no banco de dados.");
                            }

                            var mensagemCorpo = !string.IsNullOrEmpty(payload.Mensagem) ? payload.Mensagem : payload.Message;

                            foreach (var cliente in clientes)
                            {
                                foreach (var canal in payload.Canais)
                                {
                                    var msgCorpo = (mensagemCorpo ?? string.Empty)
                                        .Replace("{Nome}", cliente.RazaoSocial)
                                        .Replace("{Assunto}", payload.Assunto ?? string.Empty);

                                    if (canal == "Email")
                                    {
                                        await _notificacaoService.EnviarEmailAsync(cliente.Email, payload.Assunto ?? string.Empty, msgCorpo);
                                    }
                                    else if (canal == "SMS" && !string.IsNullOrEmpty(cliente.Telefone))
                                    {
                                        await _notificacaoService.EnviarSmsAsync(cliente.Telefone, msgCorpo);
                                    }
                                    else if (canal == "WhatsApp" && !string.IsNullOrEmpty(cliente.Telefone))
                                    {
                                        await _notificacaoService.EnviarWhatsAppAsync(cliente.Telefone, msgCorpo);
                                    }
                                }
                            }

                            comunicacao.AtualizarStatus("Sucesso", "OutboxProcessor");
                            _context.ComunicacoesSuperAdmin.Update(comunicacao);
                        }
                    }

                    message.MarcarProcessado();
                }
                catch (Exception ex)
                {
                    message.RegistrarFalha(ex.Message);
                    Console.WriteLine($"[Quartz] Erro ao processar mensagem do Aplicativo {message.Id}: {ex.Message}");

                    if (message.EventType == "ComunicacaoSuperAdminCriada" && message.Tentativas >= 5)
                    {
                        try
                        {
                            var payload = JsonSerializer.Deserialize<ComunicacaoSuperAdminCriadaPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (payload != null)
                            {
                                var comunicacao = await _context.ComunicacoesSuperAdmin
                                    .IgnoreQueryFilters()
                                    .FirstOrDefaultAsync(c => c.Id == payload.ComunicacaoId);
                                
                                if (comunicacao != null)
                                {
                                    comunicacao.AtualizarStatus("Falha", "OutboxProcessor");
                                    _context.ComunicacoesSuperAdmin.Update(comunicacao);
                                }
                            }
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"[Quartz] Erro ao atualizar status para Falha na comunicação: {innerEx.Message}");
                        }
                    }
                }

                _context.OutboxMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] AplicativoOutboxProcessorJob concluído.");
        }

        private class UsuarioCriadoPayload
        {
            public Guid UsuarioId { get; set; }
            public string Nome { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string TenantId { get; set; } = string.Empty;
            public Guid? PerfilUsuarioId { get; set; }
            public string Cargo { get; set; } = string.Empty;
            public string Departamento { get; set; } = string.Empty;
            public decimal LimiteDesconto { get; set; }
            public string CriadoPor { get; set; } = string.Empty;
        }

        private class UsuarioAtualizadoPayload
        {
            public Guid UsuarioId { get; set; }
            public string Nome { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string TenantId { get; set; } = string.Empty;
            public Guid? PerfilUsuarioId { get; set; }
            public string Cargo { get; set; } = string.Empty;
            public string Departamento { get; set; } = string.Empty;
            public decimal LimiteDesconto { get; set; }
            public string AlteradoPor { get; set; } = string.Empty;
        }

        private class UsuarioDeletadoPayload
        {
            public Guid UsuarioId { get; set; }
            public string AlteradoPor { get; set; } = string.Empty;
        }

        private class ImpersonacaoIniciadaPayload
        {
            public Guid SessaoImpersonacaoId { get; set; }
            public Guid UsuarioOriginalId { get; set; }
            public Guid UsuarioAlvoId { get; set; }
            public Guid? EmpresaId { get; set; }
            public string Motivo { get; set; } = string.Empty;
            public string CriadoPor { get; set; } = string.Empty;
            public string TenantId { get; set; } = string.Empty;
        }

        private class AcessoSuporteIniciadoPayload
        {
            public Guid SessaoImpersonacaoId { get; set; }
            public Guid UsuarioOriginalId { get; set; }
            public Guid UsuarioAlvoId { get; set; }
            public Guid? EmpresaId { get; set; }
            public string Motivo { get; set; } = string.Empty;
            public string CriadoPor { get; set; } = string.Empty;
            public string TenantAlvo { get; set; } = string.Empty;
            public string PerfilSuporte { get; set; } = string.Empty;
        }

        private class ComunicacaoSuperAdminCriadaPayload
        {
            public Guid ComunicacaoId { get; set; }
            public List<string> BusinessIds { get; set; } = new();
            public List<string> Canais { get; set; } = new();
            public string Assunto { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty; // Mapeia para Mensagem ou Message do Command
            public string Mensagem { get; set; } = string.Empty;
            public string TenantId { get; set; } = string.Empty;
        }
    }
}
