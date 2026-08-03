using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Infrastructure.Data;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    /// <summary>Governança de upgrade/versão do Super Admin (APP-TEN-010).</summary>
    public class GovernancaVersaoHandlers :
        ICommandHandler<SolicitarUpgradeVersaoCommand>,
        ICommandHandler<AprovarUpgradeVersaoCommand>,
        ICommandHandler<RejeitarUpgradeVersaoCommand>,
        ICommandHandler<ExecutarUpgradeVersaoCommand>,
        ICommandHandler<AplicarRollbackUpgradeCommand>
    {
        private readonly ContextAplicativo _context;
        private readonly ICurrentUser _currentUser;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITenantProvider _tenantProvider;

        public GovernancaVersaoHandlers(ContextAplicativo context, ICurrentUser currentUser, IServiceProvider serviceProvider, ITenantProvider tenantProvider)
        {
            _context = context;
            _currentUser = currentUser;
            _serviceProvider = serviceProvider;
            _tenantProvider = tenantProvider;
        }

        private string User() => _currentUser.GetUserId() ?? "system";

        // 1.11 fix #3 — defense-in-depth: solicitar/aprovar/rejeitar/executar/rollback de upgrade
        // roda MigrateAsync em todos os contextos. Exige operador interno (tenant="system"), fail-closed.
        private CommandResult? GuardaInterno()
            => string.Equals(_tenantProvider.GetTenantId(), "system", StringComparison.OrdinalIgnoreCase)
                ? null
                : CommandResult.Falha(new[] { "Acesso Proibido: governança de versão é restrita ao operador interno da Siser." });

        public async Task<CommandResult> Handle(SolicitarUpgradeVersaoCommand request, CancellationToken ct)
        {
            var guarda = GuardaInterno();
            if (guarda != null) return guarda;

            var user = User();

            // Bloqueia nova solicitação enquanto houver upgrade não finalizado.
            var pendente = await _context.SolicitacoesUpgradeVersao
                .IgnoreQueryFilters()
                .AnyAsync(s => s.DeletadoEm == null &&
                               (s.Status == EStatusUpgradeVersao.Solicitado ||
                                s.Status == EStatusUpgradeVersao.Aprovado ||
                                s.Status == EStatusUpgradeVersao.EmExecucao), ct);
            if (pendente)
            {
                return CommandResult.Falha("Já existe uma solicitação de upgrade em andamento. Conclua-a antes de abrir outra.");
            }

            var solicitacao = new SolicitacaoUpgradeVersao(request.VersaoAtual, request.VersaoAlvo, request.Motivo, user, request.RollbackDisponivel, "system", user);
            if (!solicitacao.IsValid) return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message));

            _context.SolicitacoesUpgradeVersao.Add(solicitacao);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Solicitação de upgrade registrada. Aguardando aprovação de outro Super Admin.", new { solicitacao.Id });
        }

        public async Task<CommandResult> Handle(AprovarUpgradeVersaoCommand request, CancellationToken ct)
        {
            var guarda = GuardaInterno();
            if (guarda != null) return guarda;

            var user = User();
            var solicitacao = await _context.SolicitacoesUpgradeVersao.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == request.SolicitacaoId && s.DeletadoEm == null, ct);
            if (solicitacao == null) return CommandResult.Falha("Solicitação de upgrade não encontrada.");

            solicitacao.Aprovar(user, request.Comentario, user);
            if (!solicitacao.IsValid) return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Upgrade aprovado. Pronto para execução.");
        }

        public async Task<CommandResult> Handle(RejeitarUpgradeVersaoCommand request, CancellationToken ct)
        {
            var guarda = GuardaInterno();
            if (guarda != null) return guarda;

            var user = User();
            var solicitacao = await _context.SolicitacoesUpgradeVersao.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == request.SolicitacaoId && s.DeletadoEm == null, ct);
            if (solicitacao == null) return CommandResult.Falha("Solicitação de upgrade não encontrada.");

            solicitacao.Rejeitar(user, request.Comentario, user);
            if (!solicitacao.IsValid) return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Upgrade rejeitado.");
        }

        public async Task<CommandResult> Handle(ExecutarUpgradeVersaoCommand request, CancellationToken ct)
        {
            var guarda = GuardaInterno();
            if (guarda != null) return guarda;

            var user = User();
            var solicitacao = await _context.SolicitacoesUpgradeVersao.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == request.SolicitacaoId && s.DeletadoEm == null, ct);
            if (solicitacao == null) return CommandResult.Falha("Solicitação de upgrade não encontrada.");

            solicitacao.IniciarExecucao(user);
            if (!solicitacao.IsValid) return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);

            try
            {
                var contextTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ContextBase)))
                    .ToList();

                foreach (var type in contextTypes)
                {
                    if (_serviceProvider.GetService(type) is ContextBase contextInstance)
                    {
                        if (contextInstance.Database.IsRelational())
                            await contextInstance.Database.MigrateAsync(ct);
                        else
                            await contextInstance.Database.EnsureCreatedAsync(ct);
                    }
                }

                solicitacao.ConcluirComSucesso("Migrações aplicadas com sucesso.", user);
                _context.UpdateLogs.Add(new UpdateLog(solicitacao.VersaoAlvo, user, true, "Upgrade governado aplicado com sucesso.", "system", user));
                await _context.SaveChangesAsync(ct);
                return CommandResult.Ok("Upgrade executado com sucesso.");
            }
            catch (Exception ex)
            {
                solicitacao.RegistrarFalha($"Erro: {ex.Message}", user);
                _context.UpdateLogs.Add(new UpdateLog(solicitacao.VersaoAlvo, user, false, $"Erro no upgrade: {ex}", "system", user));
                await _context.SaveChangesAsync(ct);
                return CommandResult.Falha($"Erro ao executar upgrade: {ex.Message}. Considere aplicar rollback.");
            }
        }

        public async Task<CommandResult> Handle(AplicarRollbackUpgradeCommand request, CancellationToken ct)
        {
            var guarda = GuardaInterno();
            if (guarda != null) return guarda;

            var user = User();
            var solicitacao = await _context.SolicitacoesUpgradeVersao.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == request.SolicitacaoId && s.DeletadoEm == null, ct);
            if (solicitacao == null) return CommandResult.Falha("Solicitação de upgrade não encontrada.");

            solicitacao.AplicarRollback(request.Log, user);
            if (!solicitacao.IsValid) return CommandResult.Falha(solicitacao.Notifications.Select(n => n.Message));

            _context.UpdateLogs.Add(new UpdateLog(solicitacao.VersaoAtual, user, true, $"Rollback aplicado: {request.Log}", "system", user));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Rollback aplicado e registrado.");
        }
    }

    public class ListarSolicitacoesUpgradeQueryHandler : IQueryHandler<ListarSolicitacoesUpgradeQuery, IEnumerable<SolicitacaoUpgradeVersaoDto>>
    {
        private readonly ContextAplicativo _context;
        public ListarSolicitacoesUpgradeQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IEnumerable<SolicitacaoUpgradeVersaoDto>> Handle(ListarSolicitacoesUpgradeQuery request, CancellationToken ct)
        {
            return await _context.SolicitacoesUpgradeVersao.AsNoTracking().IgnoreQueryFilters()
                .Where(s => s.DeletadoEm == null)
                .OrderByDescending(s => s.CriadoEm)
                .Select(s => new SolicitacaoUpgradeVersaoDto(s.Id, s.VersaoAtual, s.VersaoAlvo, s.Motivo, s.Status, s.SolicitadoPor, s.AprovadoPor, s.Comentario, s.AprovadoEm, s.ExecutadoEm, s.RollbackDisponivel, s.CriadoEm))
                .ToListAsync(ct);
        }
    }
}
