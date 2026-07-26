using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    public record ListarContasFinanceirasQuery(bool? ApenasAbertas, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ObterSaldoContaFinanceiraQuery(Guid ContaFinanceiraId) : IRequest<CommandResult>;
    public record ListarTransacoesContaQuery(Guid ContaFinanceiraId, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ListarMovimentosFinanceirosQuery(bool? apenasNaoConciliados, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ListarChequesQuery(ESituacaoCheque? Situacao, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ListarCaixasOperacionaisQuery(EStatusCaixaOperacional? Status) : IRequest<CommandResult>;

    public class TesourariaQueryHandlers :
        IRequestHandler<ListarContasFinanceirasQuery, CommandResult>,
        IRequestHandler<ObterSaldoContaFinanceiraQuery, CommandResult>,
        IRequestHandler<ListarTransacoesContaQuery, CommandResult>,
        IRequestHandler<ListarMovimentosFinanceirosQuery, CommandResult>,
        IRequestHandler<ListarChequesQuery, CommandResult>,
        IRequestHandler<ListarCaixasOperacionaisQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public TesourariaQueryHandlers(ContextFinanceiro context) => _context = context;

        private static (int pagina, int tamanho) Normaliza(int pagina, int tamanho)
            => (pagina <= 0 ? 1 : pagina, tamanho is <= 0 or > 100 ? 20 : tamanho);

        public async Task<CommandResult> Handle(ListarContasFinanceirasQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.ContasFinanceiras.AsNoTracking().AsQueryable();
            if (request.ApenasAbertas == true) query = query.Where(c => !c.Fechada);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderBy(c => c.Nome).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(c => new { c.Id, c.Nome, c.NumeroConta, c.Fechada, c.SaldoAbertura }).ToListAsync(ct);
            return CommandResult.Ok("Contas financeiras listadas.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ObterSaldoContaFinanceiraQuery request, CancellationToken ct)
        {
            var conta = await _context.ContasFinanceiras.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ContaFinanceiraId, ct);
            if (conta == null) return CommandResult.Falha("Conta financeira não encontrada.");
            var creditos = await _context.TransacoesContaFinanceira.AsNoTracking()
                .Where(t => t.ContaFinanceiraId == request.ContaFinanceiraId && t.Tipo == ETipoTransacaoConta.Credito).SumAsync(t => (decimal?)t.Valor, ct) ?? 0m;
            var debitos = await _context.TransacoesContaFinanceira.AsNoTracking()
                .Where(t => t.ContaFinanceiraId == request.ContaFinanceiraId && t.Tipo == ETipoTransacaoConta.Debito).SumAsync(t => (decimal?)t.Valor, ct) ?? 0m;
            var saldo = (conta.SaldoAbertura ?? 0m) + creditos - debitos;
            return CommandResult.Ok("Saldo da conta financeira apurado.", new { conta.Id, conta.SaldoAbertura, creditos, debitos, saldo });
        }

        public async Task<CommandResult> Handle(ListarTransacoesContaQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.TransacoesContaFinanceira.AsNoTracking().Where(t => t.ContaFinanceiraId == request.ContaFinanceiraId);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderByDescending(t => t.DataOperacao).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(t => new { t.Id, t.Valor, t.Tipo, t.Subtipo, t.DataOperacao, t.Nota }).ToListAsync(ct);
            return CommandResult.Ok("Transações da conta listadas.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ListarMovimentosFinanceirosQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.MovimentosFinanceiros.AsNoTracking().AsQueryable();
            if (request.apenasNaoConciliados == true) query = query.Where(m => !m.Conciliado);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderByDescending(m => m.Emissao).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(m => new { m.Id, m.Emissao, m.ContaId, m.Credito, m.Debito, m.Conciliado, m.Planejamento }).ToListAsync(ct);
            return CommandResult.Ok("Movimentos financeiros listados.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ListarChequesQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.Cheques.AsNoTracking().AsQueryable();
            if (request.Situacao.HasValue) query = query.Where(c => c.Situacao == request.Situacao.Value);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderByDescending(c => c.Vencimento).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(c => new { c.Id, c.Tipo, c.PessoaId, c.Emissao, c.Vencimento, c.Valor, c.Situacao }).ToListAsync(ct);
            return CommandResult.Ok("Cheques listados.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ListarCaixasOperacionaisQuery request, CancellationToken ct)
        {
            var query = _context.CaixasOperacionais.AsNoTracking().AsQueryable();
            if (request.Status.HasValue) query = query.Where(c => c.Status == request.Status.Value);
            var itens = await query.OrderByDescending(c => c.CriadoEm)
                .Select(c => new { c.Id, c.Status, c.UsuarioId, c.ValorInicial, c.ValorFechamento, c.DataFechamento }).ToListAsync(ct);
            return CommandResult.Ok("Caixas operacionais listados.", itens);
        }
    }
}
