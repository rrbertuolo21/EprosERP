using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GRC.Application.Handlers
{
    /// <summary>D-CIA-02 — define amostra do teste (justificativa obrigatória).</summary>
    public class DefinirAmostraAuditoriaCommandHandler : ICommandHandler<DefinirAmostraAuditoriaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirAmostraAuditoriaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirAmostraAuditoriaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var amostra = new AmostraAuditoria(
                request.TesteControleId, request.PlanoAuditoriaId, request.Metodo,
                request.Tamanho, request.Criterio, request.Justificativa, tenantId, usuario);
            if (!amostra.IsValid)
                return CommandResult.Falha(amostra.Notifications.Select(n => n.Message));

            _context.AmostrasAuditoria.Add(amostra);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Amostra de auditoria definida.", new { AmostraId = amostra.Id, amostra.Metodo, amostra.Tamanho });
        }
    }

    /// <summary>D-CIA-04 — emite token de acesso externo com expiração obrigatória parametrizada.</summary>
    public class EmitirTokenAcessoAuditoriaCommandHandler : ICommandHandler<EmitirTokenAcessoAuditoriaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public EmitirTokenAcessoAuditoriaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EmitirTokenAcessoAuditoriaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var planoExiste = await _context.PlanosAuditoria.AnyAsync(p => p.Id == request.PlanoAuditoriaId, cancellationToken);
            if (!planoExiste)
                return CommandResult.Falha("Plano de auditoria nao encontrado.");

            var horas = request.ExpiracaoHoras ?? await ObterExpiracaoPadraoAsync(cancellationToken);
            if (horas <= 0)
                return CommandResult.Falha("A expiracao do token deve ser positiva (horas).");

            var tokenClaro = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
            var tokenHash = Hash(tokenClaro);

            var token = new TokenAcessoAuditoria(
                request.PlanoAuditoriaId, tokenHash, request.AuditorId, request.Escopo,
                DateTime.UtcNow.AddHours(horas), tenantId, usuario);
            if (!token.IsValid)
                return CommandResult.Falha(token.Notifications.Select(n => n.Message));

            _context.TokensAcessoAuditoria.Add(token);
            await _context.SaveChangesAsync(cancellationToken);

            // Token em claro retornado UMA vez; persistimos só o hash.
            return CommandResult.Ok("Token de acesso externo emitido.",
                new { TokenId = token.Id, Token = tokenClaro, token.ExpiraEm });
        }

        private async Task<int> ObterExpiracaoPadraoAsync(CancellationToken ct)
        {
            var param = await _context.ControleParametros
                .FirstOrDefaultAsync(p => p.Chave == "CIA_TOKEN_EXPIRACAO_HORAS" && p.Ativo, ct);
            if (param != null && int.TryParse(param.ValorJson.Trim('"'), out var horas) && horas > 0)
                return horas;
            return 72; // default seguro
        }

        private static string Hash(string valor)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(valor)));
        }
    }

    /// <summary>D-CIA-04 — revoga token de acesso externo.</summary>
    public class RevogarTokenAcessoAuditoriaCommandHandler : ICommandHandler<RevogarTokenAcessoAuditoriaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ICurrentUser _currentUser;

        public RevogarTokenAcessoAuditoriaCommandHandler(ContextGRC context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RevogarTokenAcessoAuditoriaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var token = await _context.TokensAcessoAuditoria.FirstOrDefaultAsync(t => t.Id == request.TokenId, cancellationToken);
            if (token == null)
                return CommandResult.Falha("Token de acesso nao encontrado.");

            token.Revogar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Token de acesso revogado.", new { token.Id, token.Revogado });
        }
    }

    /// <summary>RN-CIA-014 — registra evidência.</summary>
    public class RegistrarEvidenciaAuditoriaCommandHandler : ICommandHandler<RegistrarEvidenciaAuditoriaCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarEvidenciaAuditoriaCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarEvidenciaAuditoriaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var evidencia = new EvidenciaAuditoria(
                request.AchadoId, request.TesteControleId, request.ArquivoId, request.Descricao, tenantId, usuario);
            if (!evidencia.IsValid)
                return CommandResult.Falha(evidencia.Notifications.Select(n => n.Message));

            _context.EvidenciasAuditoria.Add(evidencia);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Evidencia de auditoria registrada.", new { EvidenciaId = evidencia.Id });
        }
    }

    /// <summary>RN-CIA-013 — aprova achado (emite alerta se crítico).</summary>
    public class AprovarAchadoCommandHandler : ICommandHandler<AprovarAchadoCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AprovarAchadoCommandHandler(ContextGRC context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarAchadoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var achado = await _context.Achados.FirstOrDefaultAsync(a => a.Id == request.AchadoId, cancellationToken);
            if (achado == null)
                return CommandResult.Falha("Achado nao encontrado.");

            achado.Aprovar(request.AprovadorId, usuario);
            if (!achado.IsValid)
                return CommandResult.Falha(achado.Notifications.Select(n => n.Message));

            if (achado.EhCritico())
                _context.OutboxMessages.Add(new OutboxMessage(tenantId,
                    CatalogoEventosIntegracao.Grc.CiaAchadoCritico,
                    JsonSerializer.Serialize(new { achado.Id, achado.Severidade, request.AprovadorId })));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Achado aprovado.", new { achado.Id, achado.Aprovado });
        }
    }

    /// <summary>RN-CIA-013/014 — encerra achado exigindo evidência e (se crítico) aprovação.</summary>
    public class EncerrarAchadoCommandHandler : ICommandHandler<EncerrarAchadoCommand>
    {
        private readonly ContextGRC _context;
        private readonly ICurrentUser _currentUser;

        public EncerrarAchadoCommandHandler(ContextGRC context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(EncerrarAchadoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var achado = await _context.Achados.FirstOrDefaultAsync(a => a.Id == request.AchadoId, cancellationToken);
            if (achado == null)
                return CommandResult.Falha("Achado nao encontrado.");

            var possuiEvidencia = await _context.EvidenciasAuditoria.AnyAsync(e => e.AchadoId == achado.Id, cancellationToken);

            achado.Encerrar(request.Motivo, possuiEvidencia, usuario);
            if (!achado.IsValid)
                return CommandResult.Falha(achado.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Achado encerrado.", new { achado.Id, achado.Status });
        }
    }
}
