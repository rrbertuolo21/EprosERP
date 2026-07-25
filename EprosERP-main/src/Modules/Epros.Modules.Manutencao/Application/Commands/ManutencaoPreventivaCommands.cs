using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Commands
{
    // ===== Criar plano preventivo =====
    public record CriarPlanoPreventivoCommand(
        string Codigo,
        string Descricao,
        Guid ResponsavelId,
        string? AlvoTipo,
        Guid? AlvoId,
        string? Observacao) : ICommand;

    public class CriarPlanoPreventivoCommandValidator : AbstractValidator<CriarPlanoPreventivoCommand>
    {
        public CriarPlanoPreventivoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30);
            RuleFor(c => c.Descricao).NotEmpty().MaximumLength(500);
            RuleFor(c => c.ResponsavelId).NotEmpty();
        }
    }

    public class CriarPlanoPreventivoCommandHandler : ICommandHandler<CriarPlanoPreventivoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPlanoPreventivoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPlanoPreventivoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // RN-PRV-001: codigo unico por tenant.
            var existe = await _context.PlanosPreventivos
                .AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);
            if (existe)
                return CommandResult.Falha("Ja existe plano preventivo com este codigo para a empresa/tenant.");

            var plano = new PlanoPreventivo(request.Codigo, request.Descricao, request.ResponsavelId, request.AlvoTipo, request.AlvoId, request.Observacao, tenantId, usuario);
            if (!plano.IsValid)
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            _context.PlanosPreventivos.Add(plano);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Plano preventivo criado com sucesso.", new { plano.Id });
        }
    }

    // ===== Adicionar periodicidade =====
    public record AdicionarPeriodicidadePlanoCommand(
        Guid PlanoId,
        ETipoPeriodicidade TipoPeriodicidade,
        DateTime? DataInicio,
        int? Intervalo,
        string? UnidadeIntervalo,
        string? ContadorTipo,
        decimal? ContadorBase,
        decimal? ContadorProximo,
        string? Tolerancia,
        DateTime? ProximaExecucao) : ICommand;

    public class AdicionarPeriodicidadePlanoCommandHandler : ICommandHandler<AdicionarPeriodicidadePlanoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarPeriodicidadePlanoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarPeriodicidadePlanoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var plano = await _context.PlanosPreventivos
                .Include(p => p.Periodicidades)
                .FirstOrDefaultAsync(p => p.Id == request.PlanoId, cancellationToken);
            if (plano == null)
                return CommandResult.Falha("Plano preventivo nao encontrado.");

            var periodicidade = new PlanoPreventivoPeriodicidade(
                plano.Id, request.TipoPeriodicidade, request.DataInicio, request.Intervalo, request.UnidadeIntervalo,
                request.ContadorTipo, request.ContadorBase, request.ContadorProximo, request.Tolerancia, request.ProximaExecucao,
                tenantId, usuario);

            plano.AdicionarPeriodicidade(periodicidade, usuario);
            if (!plano.IsValid)
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Periodicidade adicionada ao plano.", new { periodicidade.Id });
        }
    }

    // ===== Adicionar kit de peca =====
    public record AdicionarKitPecaPlanoCommand(Guid PlanoId, Guid PecaId, decimal Quantidade, string? Unidade, bool Obrigatoria, string? Observacao) : ICommand;

    public class AdicionarKitPecaPlanoCommandHandler : ICommandHandler<AdicionarKitPecaPlanoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarKitPecaPlanoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarKitPecaPlanoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var plano = await _context.PlanosPreventivos
                .Include(p => p.KitPecas)
                .FirstOrDefaultAsync(p => p.Id == request.PlanoId, cancellationToken);
            if (plano == null)
                return CommandResult.Falha("Plano preventivo nao encontrado.");

            var item = new PlanoPreventivoKitPeca(plano.Id, request.PecaId, request.Quantidade, request.Unidade, request.Obrigatoria, request.Observacao, tenantId, usuario);
            plano.AdicionarKitPeca(item, usuario);
            if (!plano.IsValid)
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Kit de peca adicionado ao plano.", new { item.Id });
        }
    }

    // ===== Ativar plano (RN-PRV-008/009) =====
    public record AtivarPlanoPreventivoCommand(Guid PlanoId) : ICommand;

    public class AtivarPlanoPreventivoCommandHandler : ICommandHandler<AtivarPlanoPreventivoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ICurrentUser _currentUser;

        public AtivarPlanoPreventivoCommandHandler(ContextManutencao context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtivarPlanoPreventivoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var plano = await _context.PlanosPreventivos
                .Include(p => p.Periodicidades)
                .FirstOrDefaultAsync(p => p.Id == request.PlanoId, cancellationToken);
            if (plano == null)
                return CommandResult.Falha("Plano preventivo nao encontrado.");

            plano.Ativar(usuario);
            if (!plano.IsValid)
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Plano preventivo ativado.", new { plano.Id, Status = plano.Status.ToString() });
        }
    }
}
