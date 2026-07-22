using System.Linq;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    public class CriarContasAReceberCommandHandler : IRequestHandler<CriarContasAReceberCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarContasAReceberCommandHandler(ContextFinanceiro context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarContasAReceberCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var conta = new ContasAReceber(r.PessoaId, r.PlanoDeContasFinanceiroItemId, r.FatoGeradorFinanceiroId,
                r.NomePessoa, r.Situacao, r.DataVencimento, r.DataEmissao, r.DataBaixa, r.Documento, r.ValorTitulo,
                r.ValorInicialDesconto, r.ValorInicialMulta, r.ValorInicialJuros, r.ValorInicialAcrescimo,
                r.NumeroParcela, r.Detalhamento, r.JustificativaCancelamento, tenantId, userId);

            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            _context.ContasAReceberAgregado.Add(conta);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a receber criado com sucesso.", new { conta.Id });
        }
    }

    public class AtualizarContasAReceberCommandHandler : IRequestHandler<AtualizarContasAReceberCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarContasAReceberCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarContasAReceberCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAReceberAgregado
                .Include(c => c.ContasAReceberItens)
                .FirstOrDefaultAsync(c => c.Id == r.Id, ct);

            if (conta is null)
                return CommandResult.Falha("Título a receber não encontrado.");

            conta.Alterar(r.PessoaId, r.PlanoDeContasFinanceiroItemId, r.NomePessoa, r.Situacao, r.DataVencimento,
                r.DataEmissao, r.DataBaixa, r.Documento, r.ValorTitulo, r.ValorInicialDesconto, r.ValorInicialMulta,
                r.ValorInicialJuros, r.ValorInicialAcrescimo, r.NumeroParcela, r.Detalhamento,
                r.JustificativaCancelamento, _currentUser.GetUserId() ?? "system");

            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a receber atualizado com sucesso.", new { conta.Id });
        }
    }

    public class DeletarContasAReceberCommandHandler : IRequestHandler<DeletarContasAReceberCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarContasAReceberCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarContasAReceberCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAReceberAgregado
                .Include(c => c.ContasAReceberItens)
                .FirstOrDefaultAsync(c => c.Id == r.Id, ct);

            if (conta is null)
                return CommandResult.Falha("Título a receber não encontrado.");

            conta.DeletarComItens(_currentUser.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a receber removido com sucesso.");
        }
    }

    public class CancelarContasAReceberCommandHandler : IRequestHandler<CancelarContasAReceberCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public CancelarContasAReceberCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CancelarContasAReceberCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAReceberAgregado.FirstOrDefaultAsync(c => c.Id == r.Id, ct);
            if (conta is null)
                return CommandResult.Falha("Título a receber não encontrado.");

            conta.Cancelar(r.JustificativaCancelamento, _currentUser.GetUserId() ?? "system");
            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a receber cancelado.", new { conta.Situacao });
        }
    }

    public class BaixarContasAReceberConciliadoCommandHandler : IRequestHandler<BaixarContasAReceberConciliadoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public BaixarContasAReceberConciliadoCommandHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(BaixarContasAReceberConciliadoCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAReceberAgregado.FirstOrDefaultAsync(c => c.Id == r.Id, ct);
            if (conta is null)
                return CommandResult.Falha("Título a receber não encontrado.");

            conta.BaixarTituloConciliado(r.ValorRecebido);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a receber baixado (conciliado).", new { conta.Situacao });
        }
    }

    public class EstornarContasAReceberConciliadoCommandHandler : IRequestHandler<EstornarContasAReceberConciliadoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public EstornarContasAReceberConciliadoCommandHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(EstornarContasAReceberConciliadoCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAReceberAgregado.FirstOrDefaultAsync(c => c.Id == r.Id, ct);
            if (conta is null)
                return CommandResult.Falha("Título a receber não encontrado.");

            conta.EstornarTituloConciliado();
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a receber estornado.", new { conta.Situacao });
        }
    }

    public class IncluirContasAReceberItemCommandHandler : IRequestHandler<IncluirContasAReceberItemCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public IncluirContasAReceberItemCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(IncluirContasAReceberItemCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAReceberAgregado
                .Include(c => c.ContasAReceberItens)
                .FirstOrDefaultAsync(c => c.Id == r.ContasAReceberId, ct);

            if (conta is null)
                return CommandResult.Falha("Título a receber não encontrado.");

            conta.IncluirContasAReceberItem(r.PlanoDeContasFinanceiroItemId, r.ContaBancariaId, r.TipoPagamento,
                r.ValorParcela, r.ValorPago, r.ValorDesconto, r.ValorMulta, r.ValorJuros, r.ValorAcrescimo,
                r.DataRecebimento, _currentUser.GetUserId() ?? "system");

            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Item incluído com sucesso.");
        }
    }

    public class AlterarContasAReceberItemCommandHandler : IRequestHandler<AlterarContasAReceberItemCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AlterarContasAReceberItemCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AlterarContasAReceberItemCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAReceberAgregado
                .Include(c => c.ContasAReceberItens)
                .FirstOrDefaultAsync(c => c.Id == r.ContasAReceberId, ct);

            if (conta is null)
                return CommandResult.Falha("Título a receber não encontrado.");

            conta.AlterarContasAReceberItem(r.ContasAReceberItemId, r.PlanoDeContasFinanceiroItemId, r.ContaBancariaId,
                r.TipoPagamento, r.ValorParcela, r.ValorPago, r.ValorDesconto, r.ValorMulta, r.ValorJuros,
                r.ValorAcrescimo, r.DataRecebimento, _currentUser.GetUserId() ?? "system");

            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Item alterado com sucesso.");
        }
    }

    public class DeletarContasAReceberItemCommandHandler : IRequestHandler<DeletarContasAReceberItemCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarContasAReceberItemCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarContasAReceberItemCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAReceberAgregado
                .Include(c => c.ContasAReceberItens)
                .FirstOrDefaultAsync(c => c.Id == r.ContasAReceberId, ct);

            if (conta is null)
                return CommandResult.Falha("Título a receber não encontrado.");

            conta.DeletarContasAReceberItem(r.ContasAReceberItemId, _currentUser.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Item removido com sucesso.");
        }
    }
}
