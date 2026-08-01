using System;
using System.Linq;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    // ─────────────────────────────────────────────────────
    // Listar / Obter ContasAPagar (agregado fiel)
    // ─────────────────────────────────────────────────────
    public record ListarContasAPagarAgregadoQuery(
        ESituacao? Situacao = null,
        Guid? PessoaId = null,
        DateTime? VencimentoDe = null,
        DateTime? VencimentoAte = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IRequest<CommandResult>;

    public class ListarContasAPagarAgregadoQueryHandler : IRequestHandler<ListarContasAPagarAgregadoQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ListarContasAPagarAgregadoQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarContasAPagarAgregadoQuery r, CancellationToken ct)
        {
            var query = _context.ContasAPagarAgregado.AsNoTracking().AsQueryable();

            if (r.Situacao.HasValue) query = query.Where(c => c.Situacao == r.Situacao.Value);
            if (r.PessoaId.HasValue) query = query.Where(c => c.PessoaId == r.PessoaId.Value);
            if (r.VencimentoDe.HasValue) query = query.Where(c => c.DataVencimento >= r.VencimentoDe.Value);
            if (r.VencimentoAte.HasValue) query = query.Where(c => c.DataVencimento <= r.VencimentoAte.Value);

            var total = await query.CountAsync(ct);
            var itens = await query
                .OrderBy(c => c.DataVencimento)
                .Skip((r.Pagina - 1) * r.TamanhoPagina)
                .Take(r.TamanhoPagina)
                .Select(c => new
                {
                    c.Id,
                    c.PessoaId,
                    c.NomePessoa,
                    c.Documento,
                    c.Situacao,
                    c.DataVencimento,
                    c.DataEmissao,
                    c.DataBaixa,
                    c.ValorTitulo,
                    c.ValorTotalAPagarTitulo,
                    c.ValorTotalPago,
                    c.NumeroParcela
                })
                .ToListAsync(ct);

            return CommandResult.Ok("OK", new { Total = total, r.Pagina, Itens = itens });
        }
    }

    public record ObterContasAPagarAgregadoQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterContasAPagarAgregadoQueryHandler : IRequestHandler<ObterContasAPagarAgregadoQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ObterContasAPagarAgregadoQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterContasAPagarAgregadoQuery r, CancellationToken ct)
        {
            var conta = await _context.ContasAPagarAgregado.AsNoTracking()
                .Include(c => c.ContasAPagarItens)
                .FirstOrDefaultAsync(c => c.Id == r.Id, ct);

            if (conta is null)
                return CommandResult.Falha("Título a pagar não encontrado.");

            return CommandResult.Ok("OK", conta);
        }
    }

    // ─────────────────────────────────────────────────────
    // Listar / Obter ContasAReceber (agregado fiel)
    // ─────────────────────────────────────────────────────
    public record ListarContasAReceberAgregadoQuery(
        ESituacao? Situacao = null,
        Guid? PessoaId = null,
        DateTime? VencimentoDe = null,
        DateTime? VencimentoAte = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IRequest<CommandResult>;

    public class ListarContasAReceberAgregadoQueryHandler : IRequestHandler<ListarContasAReceberAgregadoQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ListarContasAReceberAgregadoQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarContasAReceberAgregadoQuery r, CancellationToken ct)
        {
            var query = _context.ContasAReceberAgregado.AsNoTracking().AsQueryable();

            if (r.Situacao.HasValue) query = query.Where(c => c.Situacao == r.Situacao.Value);
            if (r.PessoaId.HasValue) query = query.Where(c => c.PessoaId == r.PessoaId.Value);
            if (r.VencimentoDe.HasValue) query = query.Where(c => c.DataVencimento >= r.VencimentoDe.Value);
            if (r.VencimentoAte.HasValue) query = query.Where(c => c.DataVencimento <= r.VencimentoAte.Value);

            var total = await query.CountAsync(ct);
            var itens = await query
                .OrderBy(c => c.DataVencimento)
                .Skip((r.Pagina - 1) * r.TamanhoPagina)
                .Take(r.TamanhoPagina)
                .Select(c => new
                {
                    c.Id,
                    c.PessoaId,
                    c.NomePessoa,
                    c.Documento,
                    c.Situacao,
                    c.DataVencimento,
                    c.DataEmissao,
                    c.DataBaixa,
                    c.ValorTitulo,
                    c.ValorTotalAReceberTitulo,
                    c.ValorTotalRecebido,
                    c.NumeroParcela
                })
                .ToListAsync(ct);

            return CommandResult.Ok("OK", new { Total = total, r.Pagina, Itens = itens });
        }
    }

    public record ObterContasAReceberAgregadoQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterContasAReceberAgregadoQueryHandler : IRequestHandler<ObterContasAReceberAgregadoQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ObterContasAReceberAgregadoQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterContasAReceberAgregadoQuery r, CancellationToken ct)
        {
            var conta = await _context.ContasAReceberAgregado.AsNoTracking()
                .Include(c => c.ContasAReceberItens)
                .FirstOrDefaultAsync(c => c.Id == r.Id, ct);

            if (conta is null)
                return CommandResult.Falha("Título a receber não encontrado.");

            return CommandResult.Ok("OK", conta);
        }
    }

    // ─────────────────────────────────────────────────────
    // Importação OFX
    // ─────────────────────────────────────────────────────
    public record ListarImportacoesOfxQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;

    public class ListarImportacoesOfxQueryHandler : IRequestHandler<ListarImportacoesOfxQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ListarImportacoesOfxQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarImportacoesOfxQuery r, CancellationToken ct)
        {
            var query = _context.ImportacoesArquivoOfx.AsNoTracking().AsQueryable();
            var total = await query.CountAsync(ct);
            var itens = await query
                .OrderByDescending(o => o.DataFimExtrato)
                .Skip((r.Pagina - 1) * r.TamanhoPagina)
                .Take(r.TamanhoPagina)
                .Select(o => new { o.Id, o.CodigoBanco, o.NumeroConta, o.TipoConta, o.DataInicioExtrato, o.DataFimExtrato })
                .ToListAsync(ct);

            return CommandResult.Ok("OK", new { Total = total, r.Pagina, Itens = itens });
        }
    }

    public record ObterImportacaoOfxComTransacoesQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterImportacaoOfxComTransacoesQueryHandler : IRequestHandler<ObterImportacaoOfxComTransacoesQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ObterImportacaoOfxComTransacoesQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterImportacaoOfxComTransacoesQuery r, CancellationToken ct)
        {
            var ofx = await _context.ImportacoesArquivoOfx.AsNoTracking()
                .Include(o => o.Transacoes)
                .FirstOrDefaultAsync(o => o.Id == r.Id, ct);

            if (ofx is null)
                return CommandResult.Falha("Importação OFX não encontrada.");

            return CommandResult.Ok("OK", ofx);
        }
    }
}
