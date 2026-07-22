using System;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace Epros.Infrastructure.Services
{
    public class MockNotificacaoService : INotificacaoService
    {
        private readonly ILogger<MockNotificacaoService> _logger;

        public MockNotificacaoService(ILogger<MockNotificacaoService> logger)
        {
            _logger = logger;
        }

        public Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml)
        {
            _logger.LogInformation("[Mock Email] Iniciando envio para: {Destinatario}, Assunto: {Assunto}", destinatario, assunto);
            Console.WriteLine($"[Mock Email] Destinatário: {destinatario}");
            Console.WriteLine($"[Mock Email] Assunto: {assunto}");
            Console.WriteLine($"[Mock Email] Corpo HTML:\n{corpoHtml}");

            VerificarSimulacaoFalha(destinatario);

            _logger.LogInformation("[Mock Email] Enviado com sucesso para: {Destinatario}", destinatario);
            return Task.CompletedTask;
        }

        public Task EnviarSmsAsync(string telefone, string mensagem)
        {
            _logger.LogInformation("[Mock SMS] Iniciando envio para: {Telefone}", telefone);
            Console.WriteLine($"[Mock SMS] Destinatário: {telefone}");
            Console.WriteLine($"[Mock SMS] Mensagem (Max 160 chars): {(mensagem.Length > 160 ? mensagem.Substring(0, 160) : mensagem)}");

            VerificarSimulacaoFalha(telefone);

            _logger.LogInformation("[Mock SMS] Enviado com sucesso para: {Telefone}", telefone);
            return Task.CompletedTask;
        }

        public Task EnviarWhatsAppAsync(string telefone, string mensagem)
        {
            _logger.LogInformation("[Mock WhatsApp] Iniciando envio para: {Telefone}", telefone);
            Console.WriteLine($"[Mock WhatsApp] Destinatário: {telefone}");
            Console.WriteLine($"[Mock WhatsApp] Mensagem: {mensagem}");

            VerificarSimulacaoFalha(telefone);

            _logger.LogInformation("[Mock WhatsApp] Enviado com sucesso para: {Telefone}", telefone);
            return Task.CompletedTask;
        }

        private void VerificarSimulacaoFalha(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            if (input.Contains("retry", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("[Mock Notification] Falha simulada para retentativa com input: {Input}", input);
                throw new InvalidOperationException("Erro simulado para retentativa.");
            }

            if (input.Contains("falhar", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("[Mock Notification] Falha catastrófica simulada com input: {Input}", input);
                throw new InvalidOperationException("Falha catastrófica simulada.");
            }
        }
    }
}
