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
    // ===== Registrar parada =====
    public record RegistrarParadaCommand(
        string Codigo,
        string Descricao,
        Guid ResponsavelId,
        ETipoParada TipoParada,
        DateTime DataHoraInicio,
        Guid UsuarioRegistroId,
        Guid? EquipamentoId,
        Guid? LinhaId,
        Guid? CelulaId,
        Guid? MotivoParadaId,
        string? Observacao) : ICommand;

    public class RegistrarParadaCommandValidator : AbstractValidator<RegistrarParadaCommand>
    {
        public RegistrarParadaCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30);
            RuleFor(c => c.Descricao).NotEmpty().MaximumLength(500);
            RuleFor(c => c.ResponsavelId).NotEmpty();
            RuleFor(c => c.UsuarioRegistroId).NotEmpty();
        }
    }

    public class RegistrarParadaCommandHandler : ICommandHandler<RegistrarParadaCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarParadaCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarParadaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var existe = await _context.Paradas.AnyAsync(p => p.Codigo == request.Codigo, cancellationToken);
            if (existe)
                return CommandResult.Falha("Ja existe parada com este codigo para a empresa/tenant.");

            var parada = new Parada(request.Codigo, request.Descricao, request.ResponsavelId, request.TipoParada, request.DataHoraInicio,
                request.UsuarioRegistroId, request.EquipamentoId, request.LinhaId, request.CelulaId, request.MotivoParadaId, request.Observacao, tenantId, usuario);
            if (!parada.IsValid)
                return CommandResult.Falha(parada.Notifications.Select(n => n.Message));

            _context.Paradas.Add(parada);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Parada registrada.", new { parada.Id });
        }
    }

    // ===== Finalizar parada (calcula duracao) =====
    public record FinalizarParadaCommand(Guid ParadaId, DateTime DataHoraFim) : ICommand;

    public class FinalizarParadaCommandHandler : ICommandHandler<FinalizarParadaCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ICurrentUser _currentUser;

        public FinalizarParadaCommandHandler(ContextManutencao context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(FinalizarParadaCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var parada = await _context.Paradas.FirstOrDefaultAsync(p => p.Id == request.ParadaId, cancellationToken);
            if (parada == null)
                return CommandResult.Falha("Parada nao encontrada.");

            parada.Finalizar(request.DataHoraFim, usuario);
            if (!parada.IsValid)
                return CommandResult.Falha(parada.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Parada finalizada.", new { parada.Id, parada.DuracaoMinutos });
        }
    }

    // ===== Cadastrar motivo de parada =====
    public record CriarMotivoParadaCommand(
        string Codigo,
        string Descricao,
        Guid? MotivoPaiId,
        string? TipoParadaAplicavel,
        bool ExigeObservacao,
        bool ExigeAnexo) : ICommand;

    public class CriarMotivoParadaCommandHandler : ICommandHandler<CriarMotivoParadaCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarMotivoParadaCommandHandler(ContextManutencao context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarMotivoParadaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var existe = await _context.MotivosParada.AnyAsync(m => m.Codigo == request.Codigo, cancellationToken);
            if (existe)
                return CommandResult.Falha("Ja existe motivo de parada com este codigo para a empresa/tenant.");

            var motivo = new MotivoParada(request.Codigo, request.Descricao, request.MotivoPaiId, request.TipoParadaAplicavel, request.ExigeObservacao, request.ExigeAnexo, tenantId, usuario);
            if (!motivo.IsValid)
                return CommandResult.Falha(motivo.Notifications.Select(n => n.Message));

            _context.MotivosParada.Add(motivo);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Motivo de parada cadastrado.", new { motivo.Id });
        }
    }
}
