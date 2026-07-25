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
    // ===== Plano de Contrato =====
    public class CriarPlanoContratoCommandHandler : IRequestHandler<CriarPlanoContratoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarPlanoContratoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarPlanoContratoCommand request, CancellationToken ct)
        {
            var plano = new PlanoContrato(request.Descricao, request.Valor, request.Periodicidade, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!plano.IsValid) return CommandResult.Falha(plano.Notifications.Select(n => n.Message));
            _context.PlanosContrato.Add(plano);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Plano de contrato cadastrado.", new { plano.Id });
        }
    }

    public class AtualizarPlanoContratoCommandHandler : IRequestHandler<AtualizarPlanoContratoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtualizarPlanoContratoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtualizarPlanoContratoCommand request, CancellationToken ct)
        {
            var plano = await _context.PlanosContrato.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
            if (plano == null) return CommandResult.Falha("Plano de contrato não encontrado.");
            plano.Alterar(request.Descricao, request.Valor, request.Periodicidade, _user.GetUserId() ?? "system");
            if (!plano.IsValid) return CommandResult.Falha(plano.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Plano de contrato atualizado.", new { plano.Id });
        }
    }

    // ===== Contrato Financeiro =====
    public class CriarContratoFinanceiroCommandHandler : IRequestHandler<CriarContratoFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarContratoFinanceiroCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarContratoFinanceiroCommand request, CancellationToken ct)
        {
            var planoExiste = await _context.PlanosContrato.AnyAsync(p => p.Id == request.PlanoId, ct);
            if (!planoExiste) return CommandResult.Falha("Plano de contrato não encontrado.");
            var contrato = new ContratoFinanceiro(request.PlanoId, request.PessoaId, request.DataInicio, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!contrato.IsValid) return CommandResult.Falha(contrato.Notifications.Select(n => n.Message));
            _context.ContratosFinanceiros.Add(contrato);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Contrato financeiro cadastrado.", new { contrato.Id });
        }
    }

    public class SuspenderContratoFinanceiroCommandHandler : IRequestHandler<SuspenderContratoFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public SuspenderContratoFinanceiroCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(SuspenderContratoFinanceiroCommand request, CancellationToken ct)
        {
            var contrato = await _context.ContratosFinanceiros.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (contrato == null) return CommandResult.Falha("Contrato financeiro não encontrado.");
            contrato.Suspender(_user.GetUserId() ?? "system");
            if (!contrato.IsValid) return CommandResult.Falha(contrato.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Contrato suspenso.", new { contrato.Id });
        }
    }

    public class ReativarContratoFinanceiroCommandHandler : IRequestHandler<ReativarContratoFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public ReativarContratoFinanceiroCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(ReativarContratoFinanceiroCommand request, CancellationToken ct)
        {
            var contrato = await _context.ContratosFinanceiros.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (contrato == null) return CommandResult.Falha("Contrato financeiro não encontrado.");
            contrato.Reativar(_user.GetUserId() ?? "system");
            if (!contrato.IsValid) return CommandResult.Falha(contrato.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Contrato reativado.", new { contrato.Id });
        }
    }

    public class CancelarContratoFinanceiroCommandHandler : IRequestHandler<CancelarContratoFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public CancelarContratoFinanceiroCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(CancelarContratoFinanceiroCommand request, CancellationToken ct)
        {
            var contrato = await _context.ContratosFinanceiros.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (contrato == null) return CommandResult.Falha("Contrato financeiro não encontrado.");
            contrato.Cancelar(request.Motivo, request.DataFim, _user.GetUserId() ?? "system");
            if (!contrato.IsValid) return CommandResult.Falha(contrato.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Contrato cancelado.", new { contrato.Id });
        }
    }

    public class ReajustarContratoFinanceiroCommandHandler : IRequestHandler<ReajustarContratoFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public ReajustarContratoFinanceiroCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(ReajustarContratoFinanceiroCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            var contrato = await _context.ContratosFinanceiros.Include(c => c.Plano).FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (contrato == null) return CommandResult.Falha("Contrato financeiro não encontrado.");
            var valorAnterior = contrato.Plano?.Valor;
            var reajuste = new ReajusteContrato(contrato.Id, request.DataReajuste, valorAnterior, request.ValorNovo, request.Motivo, request.AprovacaoId, tenantId, userId);
            if (!reajuste.IsValid) return CommandResult.Falha(reajuste.Notifications.Select(n => n.Message));
            _context.ReajustesContrato.Add(reajuste);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Reajuste registrado.", new { reajuste.Id });
        }
    }

    // ===== Fatura Recorrente =====
    public class GerarFaturaRecorrenteCommandHandler : IRequestHandler<GerarFaturaRecorrenteCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public GerarFaturaRecorrenteCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(GerarFaturaRecorrenteCommand request, CancellationToken ct)
        {
            var contrato = await _context.ContratosFinanceiros.FirstOrDefaultAsync(c => c.Id == request.ContratoId, ct);
            if (contrato == null) return CommandResult.Falha("Contrato financeiro não encontrado.");
            if (contrato.Status != Domain.Enums.EStatusContratoFinanceiro.Ativo)
                return CommandResult.Falha("Somente contrato ativo gera fatura recorrente.");
            // EF §13.3: não duplicar competência para o mesmo contrato.
            var jaExiste = await _context.FaturasRecorrentes.AnyAsync(f => f.ContratoId == request.ContratoId && f.Competencia == request.Competencia, ct);
            if (jaExiste) return CommandResult.Falha("Já existe fatura recorrente para o contrato nesta competência.");
            var fatura = new FaturaRecorrente(request.ContratoId, request.Competencia, request.Valor, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!fatura.IsValid) return CommandResult.Falha(fatura.Notifications.Select(n => n.Message));
            _context.FaturasRecorrentes.Add(fatura);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Fatura recorrente gerada.", new { fatura.Id });
        }
    }

    public class CancelarFaturaRecorrenteCommandHandler : IRequestHandler<CancelarFaturaRecorrenteCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public CancelarFaturaRecorrenteCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(CancelarFaturaRecorrenteCommand request, CancellationToken ct)
        {
            var fatura = await _context.FaturasRecorrentes.FirstOrDefaultAsync(f => f.Id == request.Id, ct);
            if (fatura == null) return CommandResult.Falha("Fatura recorrente não encontrada.");
            fatura.Cancelar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Fatura recorrente cancelada.", new { fatura.Id });
        }
    }
}
