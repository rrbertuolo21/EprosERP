using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epros.Modules.GestaoClientes.Infrastructure.Services
{
    /// <summary>
    /// Canal de e-mail transacional REAL (auditoria APP C3): envia via SMTP lendo a
    /// <see cref="ConfiguracaoEmail"/> do tenant (host/porta/credenciais; senha decifrada no cofre).
    /// Antes o único provedor registrado era o Mock (no-op) e reset-de-senha/boas-vindas/cobrança
    /// eram gerados mas nunca chegavam. Aqui: quando NÃO há configuração, loga <b>WARNING</b> explícito
    /// (não some em silêncio) — o operador vê que o e-mail não saiu e por quê.
    /// </summary>
    public class SmtpNotificacaoService : INotificacaoService
    {
        private readonly ContextGestaoClientes _context;
        private readonly ISegredoCofreService _cofre;
        private readonly ILogger<SmtpNotificacaoService> _logger;

        public SmtpNotificacaoService(
            ContextGestaoClientes context,
            ISegredoCofreService cofre,
            ILogger<SmtpNotificacaoService> logger)
        {
            _context = context;
            _cofre = cofre;
            _logger = logger;
        }

        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml)
        {
            // ConfiguracaoEmail é por-tenant (QueryFilter aplica o tenant corrente).
            var cfg = await _context.Set<ConfiguracaoEmail>().AsNoTracking().FirstOrDefaultAsync();
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.Host) || cfg.Port is null)
            {
                _logger.LogWarning(
                    "[Email] SMTP NÃO configurado para o tenant — e-mail \"{Assunto}\" para {Destinatario} NÃO foi enviado. Configure em Parâmetros › E-mail.",
                    assunto, destinatario);
                return;
            }

            var senha = cfg.Password;
            if (!string.IsNullOrEmpty(senha) && (senha.StartsWith("vault:v1:") || senha.StartsWith("local:v1:")))
                senha = await _cofre.DescriptografarAsync(senha);

            var mensagem = new MailMessage
            {
                From = new MailAddress(cfg.FromEmail ?? cfg.Username ?? "noreply@epros.com"),
                Subject = assunto,
                Body = corpoHtml,
                IsBodyHtml = true
            };
            mensagem.To.Add(destinatario);

            try
            {
                using var smtp = new SmtpClient(cfg.Host)
                {
                    Port = cfg.Port.Value,
                    Credentials = new NetworkCredential(cfg.Username, senha),
                    EnableSsl = true,
                    Timeout = 15000
                };
                await smtp.SendMailAsync(mensagem);
                _logger.LogInformation("[Email] enviado para {Destinatario} — \"{Assunto}\".", destinatario, assunto);
            }
            catch (Exception ex)
            {
                // Propaga para o Outbox reprocessar (retry/backoff), em vez de engolir.
                _logger.LogError(ex, "[Email] FALHA ao enviar para {Destinatario} — \"{Assunto}\".", destinatario, assunto);
                throw;
            }
        }

        public Task EnviarSmsAsync(string telefone, string mensagem)
        {
            _logger.LogWarning("[SMS] provedor não configurado — SMS para {Telefone} NÃO enviado.", telefone);
            return Task.CompletedTask;
        }

        public Task EnviarWhatsAppAsync(string telefone, string mensagem)
        {
            _logger.LogWarning("[WhatsApp] provedor não configurado — mensagem para {Telefone} NÃO enviada.", telefone);
            return Task.CompletedTask;
        }
    }
}
