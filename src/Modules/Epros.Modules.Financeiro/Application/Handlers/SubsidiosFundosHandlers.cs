using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    // ===== Programa de Subsídio =====
    public class CriarProgramaSubsidioCommandHandler : IRequestHandler<CriarProgramaSubsidioCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarProgramaSubsidioCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarProgramaSubsidioCommand request, CancellationToken ct)
        {
            var programa = new ProgramaSubsidio(request.Orgao, request.ValorTotal, request.VigenciaInicio, request.VigenciaFim,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!programa.IsValid) return CommandResult.Falha(programa.Notifications.Select(n => n.Message));
            _context.ProgramasSubsidio.Add(programa);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Programa de subsídio cadastrado.", new { programa.Id });
        }
    }

    public class AtualizarProgramaSubsidioCommandHandler : IRequestHandler<AtualizarProgramaSubsidioCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtualizarProgramaSubsidioCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtualizarProgramaSubsidioCommand request, CancellationToken ct)
        {
            var programa = await _context.ProgramasSubsidio.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
            if (programa == null) return CommandResult.Falha("Programa de subsídio não encontrado.");
            programa.Alterar(request.Orgao, request.ValorTotal, request.VigenciaInicio, request.VigenciaFim, _user.GetUserId() ?? "system");
            if (!programa.IsValid) return CommandResult.Falha(programa.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Programa de subsídio atualizado.", new { programa.Id });
        }
    }

    public class IniciarPrestacaoContasProgramaCommandHandler : IRequestHandler<IniciarPrestacaoContasProgramaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public IniciarPrestacaoContasProgramaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(IniciarPrestacaoContasProgramaCommand request, CancellationToken ct)
        {
            var programa = await _context.ProgramasSubsidio.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
            if (programa == null) return CommandResult.Falha("Programa de subsídio não encontrado.");
            programa.IniciarPrestacaoContas(_user.GetUserId() ?? "system");
            if (!programa.IsValid) return CommandResult.Falha(programa.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Prestação de contas iniciada.", new { programa.Id });
        }
    }

    public class EncerrarProgramaSubsidioCommandHandler : IRequestHandler<EncerrarProgramaSubsidioCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public EncerrarProgramaSubsidioCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(EncerrarProgramaSubsidioCommand request, CancellationToken ct)
        {
            var programa = await _context.ProgramasSubsidio.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
            if (programa == null) return CommandResult.Falha("Programa de subsídio não encontrado.");
            programa.Encerrar(_user.GetUserId() ?? "system");
            if (!programa.IsValid) return CommandResult.Falha(programa.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Programa de subsídio encerrado.", new { programa.Id });
        }
    }

    // ===== Utilização de Subsídio =====
    public class VincularDespesaElegivelCommandHandler : IRequestHandler<VincularDespesaElegivelCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public VincularDespesaElegivelCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(VincularDespesaElegivelCommand request, CancellationToken ct)
        {
            var programa = await _context.ProgramasSubsidio.FirstOrDefaultAsync(p => p.Id == request.ProgramaSubsidioId, ct);
            if (programa == null) return CommandResult.Falha("Programa de subsídio não encontrado.");
            if (programa.Estado == Domain.Enums.EEstadoProgramaSubsidio.Encerrado)
                return CommandResult.Falha("Programa encerrado não aceita novas utilizações.");
            // EF §8: a soma das utilizações não pode ultrapassar o valor total do programa.
            var utilizado = await _context.UtilizacoesSubsidio.Where(u => u.ProgramaSubsidioId == request.ProgramaSubsidioId)
                .SumAsync(u => (decimal?)u.ValorElegivel, ct) ?? 0m;
            if (utilizado + request.ValorElegivel > programa.ValorTotal)
                return CommandResult.Falha("A utilização ultrapassa o valor total do programa.");
            var utilizacao = new UtilizacaoSubsidio(request.ProgramaSubsidioId, request.TituloPagarId, request.ValorElegivel,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!utilizacao.IsValid) return CommandResult.Falha(utilizacao.Notifications.Select(n => n.Message));
            _context.UtilizacoesSubsidio.Add(utilizacao);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Despesa elegível vinculada ao programa.", new { utilizacao.Id, saldoRestante = programa.ValorTotal - utilizado - request.ValorElegivel });
        }
    }

    public class RemoverUtilizacaoSubsidioCommandHandler : IRequestHandler<RemoverUtilizacaoSubsidioCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public RemoverUtilizacaoSubsidioCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(RemoverUtilizacaoSubsidioCommand request, CancellationToken ct)
        {
            var utilizacao = await _context.UtilizacoesSubsidio.FirstOrDefaultAsync(u => u.Id == request.Id, ct);
            if (utilizacao == null) return CommandResult.Falha("Utilização não encontrada.");
            utilizacao.Deletar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Utilização removida (exclusão lógica).", new { utilizacao.Id });
        }
    }
}
