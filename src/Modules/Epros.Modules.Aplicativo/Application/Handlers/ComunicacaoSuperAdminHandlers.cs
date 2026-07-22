using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    public class EnviarComunicacaoSuperAdminCommandHandler : ICommandHandler<EnviarComunicacaoSuperAdminCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EnviarComunicacaoSuperAdminCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EnviarComunicacaoSuperAdminCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var enviadoPor = _currentUser.GetUserId() ?? "system";

            Guid.TryParse(enviadoPor, out var enviadoPorGuid);

            var comunicacao = new ComunicacaoSuperAdmin(
                businessIds: request.BusinessIds,
                assunto: request.Subject,
                mensagem: request.Message,
                enviadoPor: enviadoPorGuid,
                status: "Pendente",
                tenantId: "system",
                criadoPor: enviadoPor,
                canais: request.Canais
            );

            if (!comunicacao.IsValid)
            {
                var erros = comunicacao.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao validar o envio da comunicação.");
            }

            _context.ComunicacoesSuperAdmin.Add(comunicacao);

            // Grava a mensagem na fila de Outbox do monólito para processamento assíncrono (REG-020)
            var outboxPayload = new
            {
                ComunicacaoId = comunicacao.Id,
                BusinessIds = comunicacao.BusinessIds,
                Canais = comunicacao.Canais,
                Assunto = comunicacao.Assunto,
                Mensagem = comunicacao.Mensagem,
                TenantId = "system"
            };

            var payloadJson = System.Text.Json.JsonSerializer.Serialize(outboxPayload, new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var outboxMessage = new Epros.Shared.Domain.Events.OutboxMessage("system", "ComunicacaoSuperAdminCriada", payloadJson);
            _context.OutboxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Comunicação registrada e enfileirada com sucesso!", new { ComunicacaoId = comunicacao.Id });
        }
    }

    public class AtualizarStatusComunicacaoCommandHandler : ICommandHandler<AtualizarStatusComunicacaoCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public class TesteInterno { } // Apenas aux

        public AtualizarStatusComunicacaoCommandHandler(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarStatusComunicacaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (tenantId != "system")
            {
                return CommandResult.Falha(new[] { "Acesso Proibido: Esta operação é restrita ao tenant do sistema (Siser)." });
            }

            var alteradoPor = _currentUser.GetUserId() ?? "system";

            var comunicacao = await _context.ComunicacoesSuperAdmin
                .FirstOrDefaultAsync(c => c.Id == request.ComunicacaoId && c.DeletadoEm == null, cancellationToken);

            if (comunicacao == null)
            {
                return CommandResult.Falha(new[] { "Comunicação não encontrada." });
            }

            comunicacao.AtualizarStatus(request.Status, alteradoPor);

            if (!comunicacao.IsValid)
            {
                var erros = comunicacao.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Erro ao atualizar o status da comunicação.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Status da comunicação atualizado com sucesso!");
        }
    }
}
