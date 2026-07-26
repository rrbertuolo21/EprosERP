using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Commands
{
    // ===== Enriquecer configuracao tecnica do equipamento (MAN-IND) =====
    public record ConfigurarEquipamentoCommand(
        Guid EquipamentoId,
        string? Descricao,
        Guid? TipoEquipamentoId,
        Guid? MarcaId,
        string? NumeroSerie,
        string? FuncaoOperacional,
        Guid? EstadoConservacaoId,
        Guid? ResponsavelId,
        Guid? LocalId) : ICommand;

    public class ConfigurarEquipamentoCommandHandler : ICommandHandler<ConfigurarEquipamentoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ICurrentUser _currentUser;

        public ConfigurarEquipamentoCommandHandler(ContextManutencao context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(ConfigurarEquipamentoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var equipamento = await _context.Equipamentos.FirstOrDefaultAsync(e => e.Id == request.EquipamentoId, cancellationToken);
            if (equipamento == null)
                return CommandResult.Falha("Equipamento nao encontrado.");

            equipamento.EnriquecerConfiguracao(request.Descricao, request.TipoEquipamentoId, request.MarcaId, request.NumeroSerie,
                request.FuncaoOperacional, request.EstadoConservacaoId, request.ResponsavelId, request.LocalId, usuario);
            if (!equipamento.IsValid)
                return CommandResult.Falha(equipamento.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuracao do equipamento atualizada.", new { equipamento.Id });
        }
    }

    // ===== Iniciar inducao =====
    public record IniciarInducaoCommand(Guid EquipamentoId, DateTime? DataInicio, Guid? ResponsavelId, string? ChecklistJson, string? Observacao) : ICommand;

    public class IniciarInducaoCommandHandler : ICommandHandler<IniciarInducaoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public IniciarInducaoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(IniciarInducaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var equipamento = await _context.Equipamentos.FirstOrDefaultAsync(e => e.Id == request.EquipamentoId, cancellationToken);
            if (equipamento == null)
                return CommandResult.Falha("Equipamento nao encontrado.");

            var inducao = new EquipamentoInducao(request.EquipamentoId, request.DataInicio, request.ResponsavelId, request.ChecklistJson, request.Observacao, tenantId, usuario);
            if (!inducao.IsValid)
                return CommandResult.Falha(inducao.Notifications.Select(n => n.Message));

            _context.EquipamentoInducoes.Add(inducao);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Inducao iniciada.", new { inducao.Id });
        }
    }

    // ===== Aprovar inducao =====
    public record AprovarInducaoCommand(Guid InducaoId) : ICommand;

    public class AprovarInducaoCommandHandler : ICommandHandler<AprovarInducaoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ICurrentUser _currentUser;

        public AprovarInducaoCommandHandler(ContextManutencao context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarInducaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var inducao = await _context.EquipamentoInducoes.FirstOrDefaultAsync(i => i.Id == request.InducaoId, cancellationToken);
            if (inducao == null)
                return CommandResult.Falha("Inducao nao encontrada.");

            inducao.Aprovar(usuario);
            if (!inducao.IsValid)
                return CommandResult.Falha(inducao.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Inducao aprovada.", new { inducao.Id, Status = inducao.StatusInducao.ToString() });
        }
    }
}
