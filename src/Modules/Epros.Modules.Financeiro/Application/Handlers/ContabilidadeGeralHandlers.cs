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
    // ===== Conta Contábil =====
    public class CriarContaContabilCommandHandler : IRequestHandler<CriarContaContabilCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarContaContabilCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarContaContabilCommand request, CancellationToken ct)
        {
            var conta = new ContaContabil(request.CodigoConta, request.NomeConta, request.ContaPaiId, request.Nivel,
                request.TipoConta, request.AceitaLancamento, request.ParticipaContabilidadeGeral,
                request.ParticipaOrcamento, request.ParticipaDepreciacao, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!conta.IsValid) return CommandResult.Falha(conta.Notifications.Select(n => n.Message));
            _context.ContasContabeis.Add(conta);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conta contábil cadastrada com sucesso.", new { conta.Id });
        }
    }

    public class AtualizarContaContabilCommandHandler : IRequestHandler<AtualizarContaContabilCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtualizarContaContabilCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtualizarContaContabilCommand request, CancellationToken ct)
        {
            var conta = await _context.ContasContabeis.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (conta == null) return CommandResult.Falha("Conta contábil não encontrada.");
            conta.Alterar(request.CodigoConta, request.NomeConta, request.ContaPaiId, request.Nivel, request.TipoConta,
                request.AceitaLancamento, request.ParticipaContabilidadeGeral, request.ParticipaOrcamento,
                request.ParticipaDepreciacao, _user.GetUserId() ?? "system");
            if (!conta.IsValid) return CommandResult.Falha(conta.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conta contábil atualizada com sucesso.", new { conta.Id });
        }
    }

    public class DeletarContaContabilCommandHandler : IRequestHandler<DeletarContaContabilCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public DeletarContaContabilCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(DeletarContaContabilCommand request, CancellationToken ct)
        {
            var conta = await _context.ContasContabeis.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (conta == null) return CommandResult.Falha("Conta contábil não encontrada.");
            // EF §5.8: contas com filhos ativos não devem ser excluídas sem regra de controle.
            var temFilhos = await _context.ContasContabeis.AnyAsync(c => c.ContaPaiId == request.Id, ct);
            if (temFilhos) return CommandResult.Falha("Conta possui contas filhas e não pode ser excluída.");
            conta.Deletar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conta contábil removida (exclusão lógica).", new { conta.Id });
        }
    }

    // ===== Período Contábil =====
    public class CriarPeriodoContabilCommandHandler : IRequestHandler<CriarPeriodoContabilCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarPeriodoContabilCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarPeriodoContabilCommand request, CancellationToken ct)
        {
            var periodo = new PeriodoContabil(request.AnoFiscal, request.DataInicio, request.DataFim, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!periodo.IsValid) return CommandResult.Falha(periodo.Notifications.Select(n => n.Message));
            _context.PeriodosContabeis.Add(periodo);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Período contábil criado com sucesso.", new { periodo.Id });
        }
    }

    public class IniciarFechamentoPeriodoCommandHandler : IRequestHandler<IniciarFechamentoPeriodoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public IniciarFechamentoPeriodoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(IniciarFechamentoPeriodoCommand request, CancellationToken ct)
        {
            var periodo = await _context.PeriodosContabeis.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
            if (periodo == null) return CommandResult.Falha("Período contábil não encontrado.");
            periodo.IniciarFechamento(_user.GetUserId() ?? "system");
            if (!periodo.IsValid) return CommandResult.Falha(periodo.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Período em fechamento.", new { periodo.Id });
        }
    }

    public class FecharPeriodoContabilCommandHandler : IRequestHandler<FecharPeriodoContabilCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public FecharPeriodoContabilCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(FecharPeriodoContabilCommand request, CancellationToken ct)
        {
            var periodo = await _context.PeriodosContabeis.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
            if (periodo == null) return CommandResult.Falha("Período contábil não encontrado.");
            periodo.Fechar(request.UsuarioFechamentoId, request.DataFechamento, _user.GetUserId() ?? "system");
            if (!periodo.IsValid) return CommandResult.Falha(periodo.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Período fechado.", new { periodo.Id });
        }
    }

    public class ReabrirPeriodoContabilCommandHandler : IRequestHandler<ReabrirPeriodoContabilCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public ReabrirPeriodoContabilCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(ReabrirPeriodoContabilCommand request, CancellationToken ct)
        {
            var periodo = await _context.PeriodosContabeis.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
            if (periodo == null) return CommandResult.Falha("Período contábil não encontrado.");
            periodo.Reabrir(request.UsuarioReaberturaId, request.Motivo, _user.GetUserId() ?? "system");
            if (!periodo.IsValid) return CommandResult.Falha(periodo.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Período reaberto (exceção auditada).", new { periodo.Id });
        }
    }

    // ===== Lançamento Contábil =====
    public class CriarLancamentoContabilCommandHandler : IRequestHandler<CriarLancamentoContabilCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarLancamentoContabilCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarLancamentoContabilCommand request, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            // Bloqueia lançamento em período fechado/em fechamento.
            if (request.PeriodoContabilId.HasValue)
            {
                var periodo = await _context.PeriodosContabeis.FirstOrDefaultAsync(p => p.Id == request.PeriodoContabilId.Value, ct);
                if (periodo != null && periodo.BloqueiaLancamento)
                    return CommandResult.Falha("Período contábil fechado — não aceita lançamentos.");
            }
            var lancamento = new LancamentoContabil(request.PeriodoContabilId, request.NumeroLancamento, request.Data, request.Historico, _tenant.GetTenantId(), userId);
            if (request.Linhas != null)
                foreach (var l in request.Linhas)
                    lancamento.AdicionarLinha(l.ContaContabilId, l.Debito, l.Credito, l.Historico, userId);
            if (!lancamento.IsValid) return CommandResult.Falha(lancamento.Notifications.Select(n => n.Message));
            _context.LancamentosContabeis.Add(lancamento);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Lançamento contábil criado (rascunho).", new { lancamento.Id });
        }
    }

    public class ConfirmarLancamentoContabilCommandHandler : IRequestHandler<ConfirmarLancamentoContabilCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public ConfirmarLancamentoContabilCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(ConfirmarLancamentoContabilCommand request, CancellationToken ct)
        {
            var lancamento = await _context.LancamentosContabeis.Include(l => l.Linhas).FirstOrDefaultAsync(l => l.Id == request.Id, ct);
            if (lancamento == null) return CommandResult.Falha("Lançamento contábil não encontrado.");
            lancamento.Confirmar(_user.GetUserId() ?? "system");
            if (!lancamento.IsValid) return CommandResult.Falha(lancamento.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Lançamento confirmado.", new { lancamento.Id });
        }
    }

    public class EstornarLancamentoContabilCommandHandler : IRequestHandler<EstornarLancamentoContabilCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public EstornarLancamentoContabilCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(EstornarLancamentoContabilCommand request, CancellationToken ct)
        {
            var lancamento = await _context.LancamentosContabeis.FirstOrDefaultAsync(l => l.Id == request.Id, ct);
            if (lancamento == null) return CommandResult.Falha("Lançamento contábil não encontrado.");
            lancamento.Estornar(_user.GetUserId() ?? "system");
            if (!lancamento.IsValid) return CommandResult.Falha(lancamento.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Lançamento estornado.", new { lancamento.Id });
        }
    }

    public class CancelarLancamentoContabilCommandHandler : IRequestHandler<CancelarLancamentoContabilCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public CancelarLancamentoContabilCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(CancelarLancamentoContabilCommand request, CancellationToken ct)
        {
            var lancamento = await _context.LancamentosContabeis.FirstOrDefaultAsync(l => l.Id == request.Id, ct);
            if (lancamento == null) return CommandResult.Falha("Lançamento contábil não encontrado.");
            lancamento.Cancelar(_user.GetUserId() ?? "system");
            if (!lancamento.IsValid) return CommandResult.Falha(lancamento.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Lançamento cancelado.", new { lancamento.Id });
        }
    }

    // ===== Saldo de Abertura =====
    public class RegistrarSaldoAberturaCommandHandler : IRequestHandler<RegistrarSaldoAberturaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarSaldoAberturaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarSaldoAberturaCommand request, CancellationToken ct)
        {
            var saldo = new SaldoAbertura(request.Numero, request.Data, request.ContaContabilId, request.CodigoConta,
                request.TipoSaldo, request.Valor, request.Historico, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!saldo.IsValid) return CommandResult.Falha(saldo.Notifications.Select(n => n.Message));
            _context.SaldosAbertura.Add(saldo);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Saldo de abertura registrado.", new { saldo.Id });
        }
    }
}
