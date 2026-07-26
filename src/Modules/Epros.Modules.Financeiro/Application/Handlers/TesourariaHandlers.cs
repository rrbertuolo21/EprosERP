using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    // ===== Conta Financeira =====
    public class CriarContaFinanceiraCommandHandler : IRequestHandler<CriarContaFinanceiraCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarContaFinanceiraCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarContaFinanceiraCommand request, CancellationToken ct)
        {
            var conta = new ContaFinanceira(request.Nome, request.NumeroConta, request.TipoContaId, request.Nota, request.SaldoAbertura,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!conta.IsValid) return CommandResult.Falha(conta.Notifications.Select(n => n.Message));
            _context.ContasFinanceiras.Add(conta);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conta financeira cadastrada.", new { conta.Id });
        }
    }

    public class AtualizarContaFinanceiraCommandHandler : IRequestHandler<AtualizarContaFinanceiraCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtualizarContaFinanceiraCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtualizarContaFinanceiraCommand request, CancellationToken ct)
        {
            var conta = await _context.ContasFinanceiras.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (conta == null) return CommandResult.Falha("Conta financeira não encontrada.");
            conta.Alterar(request.Nome, request.NumeroConta, request.TipoContaId, request.Nota, _user.GetUserId() ?? "system");
            if (!conta.IsValid) return CommandResult.Falha(conta.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conta financeira atualizada.", new { conta.Id });
        }
    }

    public class FecharContaFinanceiraCommandHandler : IRequestHandler<FecharContaFinanceiraCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public FecharContaFinanceiraCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(FecharContaFinanceiraCommand request, CancellationToken ct)
        {
            var conta = await _context.ContasFinanceiras.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (conta == null) return CommandResult.Falha("Conta financeira não encontrada.");
            conta.Fechar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conta financeira fechada.", new { conta.Id });
        }
    }

    // ===== Transação de Conta =====
    public class RegistrarTransacaoContaCommandHandler : IRequestHandler<RegistrarTransacaoContaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarTransacaoContaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarTransacaoContaCommand request, CancellationToken ct)
        {
            var conta = await _context.ContasFinanceiras.FirstOrDefaultAsync(c => c.Id == request.ContaFinanceiraId, ct);
            if (conta == null) return CommandResult.Falha("Conta financeira não encontrada.");
            if (conta.Fechada) return CommandResult.Falha("Conta financeira fechada não aceita transações.");
            var tx = new TransacaoContaFinanceira(request.ContaFinanceiraId, request.Valor, request.Tipo, request.Subtipo,
                request.DataOperacao, request.Nota, request.TransacaoOrigemId, request.PagamentoTransacaoId, null, null,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!tx.IsValid) return CommandResult.Falha(tx.Notifications.Select(n => n.Message));
            _context.TransacoesContaFinanceira.Add(tx);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Transação registrada.", new { tx.Id });
        }
    }

    public class RegistrarTransferenciaCommandHandler : IRequestHandler<RegistrarTransferenciaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarTransferenciaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarTransferenciaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            if (request.ContaOrigemId == request.ContaDestinoId)
                return CommandResult.Falha("Conta de origem e destino devem ser diferentes.");
            var origem = await _context.ContasFinanceiras.FirstOrDefaultAsync(c => c.Id == request.ContaOrigemId, ct);
            var destino = await _context.ContasFinanceiras.FirstOrDefaultAsync(c => c.Id == request.ContaDestinoId, ct);
            if (origem == null || destino == null) return CommandResult.Falha("Conta de origem ou destino não encontrada.");
            var parId = Guid.NewGuid();
            var saida = new TransacaoContaFinanceira(request.ContaOrigemId, request.Valor, ETipoTransacaoConta.Debito, "fund_transfer",
                request.DataOperacao, request.Nota, null, null, parId, request.ContaDestinoId, tenantId, userId);
            var entrada = new TransacaoContaFinanceira(request.ContaDestinoId, request.Valor, ETipoTransacaoConta.Credito, "fund_transfer",
                request.DataOperacao, request.Nota, null, null, parId, request.ContaOrigemId, tenantId, userId);
            if (!saida.IsValid) return CommandResult.Falha(saida.Notifications.Select(n => n.Message));
            _context.TransacoesContaFinanceira.Add(saida);
            _context.TransacoesContaFinanceira.Add(entrada);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Transferência registrada.", new { saida = saida.Id, entrada = entrada.Id });
        }
    }

    // ===== Movimento Financeiro =====
    public class RegistrarMovimentoFinanceiroCommandHandler : IRequestHandler<RegistrarMovimentoFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarMovimentoFinanceiroCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarMovimentoFinanceiroCommand request, CancellationToken ct)
        {
            var mov = new MovimentoFinanceiro(request.EmpresaId, request.Emissao, request.CaixaId, request.ContaId, request.Credito, request.Debito,
                request.ChequeId, request.ContasPagarId, request.ContasReceberId, request.PagamentoId, request.Planejamento,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!mov.IsValid) return CommandResult.Falha(mov.Notifications.Select(n => n.Message));
            _context.MovimentosFinanceiros.Add(mov);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Movimento financeiro registrado.", new { mov.Id });
        }
    }

    public class ConciliarMovimentoCommandHandler : IRequestHandler<ConciliarMovimentoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public ConciliarMovimentoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(ConciliarMovimentoCommand request, CancellationToken ct)
        {
            var mov = await _context.MovimentosFinanceiros.FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            if (mov == null) return CommandResult.Falha("Movimento financeiro não encontrado.");
            mov.Conciliar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Movimento conciliado.", new { mov.Id });
        }
    }

    // ===== Cheque =====
    public class RegistrarChequeCommandHandler : IRequestHandler<RegistrarChequeCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarChequeCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarChequeCommand request, CancellationToken ct)
        {
            var cheque = new Cheque(request.EmpresaId, request.Tipo, request.TipoPessoa, request.PessoaId, request.Emissao, request.Vencimento,
                request.Valor, request.ContaId, request.CaixaId, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!cheque.IsValid) return CommandResult.Falha(cheque.Notifications.Select(n => n.Message));
            _context.Cheques.Add(cheque);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Cheque registrado.", new { cheque.Id });
        }
    }

    public class AtualizarSituacaoChequeCommandHandler : IRequestHandler<AtualizarSituacaoChequeCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtualizarSituacaoChequeCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtualizarSituacaoChequeCommand request, CancellationToken ct)
        {
            var cheque = await _context.Cheques.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (cheque == null) return CommandResult.Falha("Cheque não encontrado.");
            cheque.DefinirSituacao(request.Situacao, _user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Situação do cheque atualizada.", new { cheque.Id, cheque.Situacao });
        }
    }

    // ===== Caixa Operacional =====
    public class AbrirCaixaOperacionalCommandHandler : IRequestHandler<AbrirCaixaOperacionalCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AbrirCaixaOperacionalCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AbrirCaixaOperacionalCommand request, CancellationToken ct)
        {
            var caixa = new CaixaOperacional(request.LocalId, request.UsuarioId, request.ValorInicial, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!caixa.IsValid) return CommandResult.Falha(caixa.Notifications.Select(n => n.Message));
            _context.CaixasOperacionais.Add(caixa);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Caixa operacional aberto.", new { caixa.Id });
        }
    }

    public class FecharCaixaOperacionalCommandHandler : IRequestHandler<FecharCaixaOperacionalCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public FecharCaixaOperacionalCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(FecharCaixaOperacionalCommand request, CancellationToken ct)
        {
            var caixa = await _context.CaixasOperacionais.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (caixa == null) return CommandResult.Falha("Caixa operacional não encontrado.");
            caixa.Fechar(request.ValorFechamento, request.TotalComprovantesCartao, request.TotalCheques, request.Observacao, _user.GetUserId() ?? "system");
            if (!caixa.IsValid) return CommandResult.Falha(caixa.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Caixa operacional fechado.", new { caixa.Id });
        }
    }
}
