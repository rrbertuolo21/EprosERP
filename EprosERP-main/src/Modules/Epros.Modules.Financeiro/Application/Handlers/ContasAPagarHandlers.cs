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
    public class CriarContasAPagarCommandHandler : IRequestHandler<CriarContasAPagarCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarContasAPagarCommandHandler(ContextFinanceiro context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarContasAPagarCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var conta = new ContasAPagar(r.PessoaId, r.PlanoDeContasFinanceiroItemId, r.FatoGeradorFinanceiroId,
                r.NomePessoa, r.Situacao, r.DataVencimento, r.DataEmissao, r.DataBaixa, r.Documento, r.ValorTitulo,
                r.ValorInicialDesconto, r.ValorInicialMulta, r.ValorInicialJuros, r.ValorInicialAcrescimo,
                r.NumeroParcela, r.Detalhamento, r.JustificativaCancelamento, tenantId, userId);

            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            _context.ContasAPagarAgregado.Add(conta);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a pagar criado com sucesso.", new { conta.Id });
        }
    }

    public class AtualizarContasAPagarCommandHandler : IRequestHandler<AtualizarContasAPagarCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarContasAPagarCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarContasAPagarCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAPagarAgregado
                .Include(c => c.ContasAPagarItens)
                .FirstOrDefaultAsync(c => c.Id == r.Id, ct);

            if (conta is null)
                return CommandResult.Falha("Título a pagar não encontrado.");

            conta.Alterar(r.PessoaId, r.PlanoDeContasFinanceiroItemId, r.NomePessoa, r.Situacao, r.DataVencimento,
                r.DataEmissao, r.DataBaixa, r.Documento, r.ValorTitulo, r.ValorInicialDesconto, r.ValorInicialMulta,
                r.ValorInicialJuros, r.ValorInicialAcrescimo, r.NumeroParcela, r.Detalhamento,
                r.JustificativaCancelamento, _currentUser.GetUserId() ?? "system");

            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a pagar atualizado com sucesso.", new { conta.Id });
        }
    }

    public class DeletarContasAPagarCommandHandler : IRequestHandler<DeletarContasAPagarCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarContasAPagarCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarContasAPagarCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAPagarAgregado
                .Include(c => c.ContasAPagarItens)
                .FirstOrDefaultAsync(c => c.Id == r.Id, ct);

            if (conta is null)
                return CommandResult.Falha("Título a pagar não encontrado.");

            conta.DeletarComItens(_currentUser.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a pagar removido com sucesso.");
        }
    }

    public class CancelarContasAPagarCommandHandler : IRequestHandler<CancelarContasAPagarCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public CancelarContasAPagarCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CancelarContasAPagarCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAPagarAgregado.FirstOrDefaultAsync(c => c.Id == r.Id, ct);
            if (conta is null)
                return CommandResult.Falha("Título a pagar não encontrado.");

            conta.Cancelar(r.JustificativaCancelamento, _currentUser.GetUserId() ?? "system");
            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a pagar cancelado.", new { conta.Situacao });
        }
    }

    public class BaixarContasAPagarConciliadoCommandHandler : IRequestHandler<BaixarContasAPagarConciliadoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public BaixarContasAPagarConciliadoCommandHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(BaixarContasAPagarConciliadoCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAPagarAgregado.FirstOrDefaultAsync(c => c.Id == r.Id, ct);
            if (conta is null)
                return CommandResult.Falha("Título a pagar não encontrado.");

            conta.BaixarTituloConciliado(r.ValorPago);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a pagar baixado (conciliado).", new { conta.Situacao });
        }
    }

    public class EstornarContasAPagarConciliadoCommandHandler : IRequestHandler<EstornarContasAPagarConciliadoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public EstornarContasAPagarConciliadoCommandHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(EstornarContasAPagarConciliadoCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAPagarAgregado.FirstOrDefaultAsync(c => c.Id == r.Id, ct);
            if (conta is null)
                return CommandResult.Falha("Título a pagar não encontrado.");

            conta.EstornarTituloConciliado();
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título a pagar estornado.", new { conta.Situacao });
        }
    }

    public class IncluirContasAPagarItemCommandHandler : IRequestHandler<IncluirContasAPagarItemCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public IncluirContasAPagarItemCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(IncluirContasAPagarItemCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAPagarAgregado
                .Include(c => c.ContasAPagarItens)
                .FirstOrDefaultAsync(c => c.Id == r.ContasAPagarId, ct);

            if (conta is null)
                return CommandResult.Falha("Título a pagar não encontrado.");

            conta.IncluirContasAPagarItem(r.PlanoDeContasFinanceiroItemId, r.ContaBancariaId, r.TipoPagamento,
                r.ValorParcela, r.ValorPago, r.ValorDesconto, r.ValorMulta, r.ValorJuros, r.ValorAcrescimo,
                r.DataPagamento, _currentUser.GetUserId() ?? "system");

            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Item incluído com sucesso.");
        }
    }

    public class AlterarContasAPagarItemCommandHandler : IRequestHandler<AlterarContasAPagarItemCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AlterarContasAPagarItemCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AlterarContasAPagarItemCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAPagarAgregado
                .Include(c => c.ContasAPagarItens)
                .FirstOrDefaultAsync(c => c.Id == r.ContasAPagarId, ct);

            if (conta is null)
                return CommandResult.Falha("Título a pagar não encontrado.");

            conta.AlterarContasAPagarItem(r.ContasAPagarItemId, r.PlanoDeContasFinanceiroItemId, r.ContaBancariaId,
                r.TipoPagamento, r.ValorParcela, r.ValorPago, r.ValorDesconto, r.ValorMulta, r.ValorJuros,
                r.ValorAcrescimo, r.DataPagamento, _currentUser.GetUserId() ?? "system");

            if (!conta.IsValid)
                return CommandResult.Falha(conta.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Item alterado com sucesso.");
        }
    }

    public class DeletarContasAPagarItemCommandHandler : IRequestHandler<DeletarContasAPagarItemCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarContasAPagarItemCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarContasAPagarItemCommand r, CancellationToken ct)
        {
            var conta = await _context.ContasAPagarAgregado
                .Include(c => c.ContasAPagarItens)
                .FirstOrDefaultAsync(c => c.Id == r.ContasAPagarId, ct);

            if (conta is null)
                return CommandResult.Falha("Título a pagar não encontrado.");

            conta.DeletarContasAPagarItem(r.ContasAPagarItemId, _currentUser.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Item removido com sucesso.");
        }
    }
}
