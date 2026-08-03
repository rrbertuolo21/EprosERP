using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Commands
{
    // ===================================================================================
    // MAN-IND — D4/D5: caminhos de ESCRITA que existiam so como tabela (sem command):
    // vinculo patrimonial (snapshot), servico-por-equipamento e atributo livre.
    // ===================================================================================

    // ===== Vincular patrimonio (snapshot; depreciacao = valida-contador D32-F) =====
    public record VincularPatrimonioEquipamentoCommand(
        Guid EquipamentoId,
        Guid AtivoPatrimonialId,
        string? NumeroPatrimonial,
        string? NomeAtivo) : ICommand;

    public class VincularPatrimonioEquipamentoCommandValidator : AbstractValidator<VincularPatrimonioEquipamentoCommand>
    {
        public VincularPatrimonioEquipamentoCommandValidator()
        {
            RuleFor(c => c.EquipamentoId).NotEmpty();
            RuleFor(c => c.AtivoPatrimonialId).NotEmpty();
        }
    }

    public class VincularPatrimonioEquipamentoCommandHandler : ICommandHandler<VincularPatrimonioEquipamentoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public VincularPatrimonioEquipamentoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(VincularPatrimonioEquipamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var equipamento = await _context.Equipamentos.FirstOrDefaultAsync(e => e.Id == request.EquipamentoId, cancellationToken);
            if (equipamento == null)
                return CommandResult.Falha("Equipamento nao encontrado.");

            // MANUTENCAO registra apenas o snapshot factual; o calculo de depreciacao e fronteira contabil
            // (Ativo Fixo/Financeiro) — valida-contador D32-F. Nao contabiliza de memoria.
            var vinculo = new EquipamentoVinculoPatrimonial(request.EquipamentoId, request.AtivoPatrimonialId, request.NumeroPatrimonial, request.NomeAtivo, tenantId, usuario);
            if (!vinculo.IsValid)
                return CommandResult.Falha(vinculo.Notifications.Select(n => n.Message));

            _context.EquipamentoVinculosPatrimoniais.Add(vinculo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Vinculo patrimonial registrado (snapshot). Depreciacao: valida-contador.", new { vinculo.Id });
        }
    }

    // ===== Servico por equipamento =====
    public record AdicionarServicoEquipamentoCommand(
        Guid ServicoId,
        Guid EquipamentoId,
        Guid? AtivoPatrimonialId,
        DateTime? VigenciaInicio,
        DateTime? VigenciaFim) : ICommand;

    public class AdicionarServicoEquipamentoCommandValidator : AbstractValidator<AdicionarServicoEquipamentoCommand>
    {
        public AdicionarServicoEquipamentoCommandValidator()
        {
            RuleFor(c => c.ServicoId).NotEmpty();
            RuleFor(c => c.EquipamentoId).NotEmpty();
        }
    }

    public class AdicionarServicoEquipamentoCommandHandler : ICommandHandler<AdicionarServicoEquipamentoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarServicoEquipamentoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarServicoEquipamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var equipamento = await _context.Equipamentos.FirstOrDefaultAsync(e => e.Id == request.EquipamentoId, cancellationToken);
            if (equipamento == null)
                return CommandResult.Falha("Equipamento nao encontrado.");

            var jaExiste = await _context.EquipamentoServicos.AnyAsync(s => s.EquipamentoId == request.EquipamentoId && s.ServicoId == request.ServicoId, cancellationToken);
            if (jaExiste)
                return CommandResult.Falha("Este servico ja esta vinculado ao equipamento.");

            var servico = new EquipamentoServico(request.ServicoId, request.EquipamentoId, request.AtivoPatrimonialId, request.VigenciaInicio, request.VigenciaFim, tenantId, usuario);
            if (!servico.IsValid)
                return CommandResult.Falha(servico.Notifications.Select(n => n.Message));

            _context.EquipamentoServicos.Add(servico);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Servico vinculado ao equipamento.", new { servico.Id });
        }
    }

    // ===== Atributo livre (upsert por equipamento+chave) =====
    public record DefinirAtributoEquipamentoCommand(
        Guid EquipamentoId,
        string Chave,
        string? Valor,
        string? OrigemFuncional) : ICommand;

    public class DefinirAtributoEquipamentoCommandValidator : AbstractValidator<DefinirAtributoEquipamentoCommand>
    {
        public DefinirAtributoEquipamentoCommandValidator()
        {
            RuleFor(c => c.EquipamentoId).NotEmpty();
            RuleFor(c => c.Chave).NotEmpty().MaximumLength(120);
        }
    }

    public class DefinirAtributoEquipamentoCommandHandler : ICommandHandler<DefinirAtributoEquipamentoCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DefinirAtributoEquipamentoCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DefinirAtributoEquipamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var equipamento = await _context.Equipamentos.FirstOrDefaultAsync(e => e.Id == request.EquipamentoId, cancellationToken);
            if (equipamento == null)
                return CommandResult.Falha("Equipamento nao encontrado.");

            var atributo = new EquipamentoAtributo(request.EquipamentoId, request.Chave, request.Valor, request.OrigemFuncional, tenantId, usuario);
            if (!atributo.IsValid)
                return CommandResult.Falha(atributo.Notifications.Select(n => n.Message));

            _context.EquipamentoAtributos.Add(atributo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Atributo do equipamento definido.", new { atributo.Id });
        }
    }
}
