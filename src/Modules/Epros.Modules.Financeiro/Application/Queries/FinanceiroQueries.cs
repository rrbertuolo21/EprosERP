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
    // Mapeamento de status (string legada da API) → ESituacao (fiel)
    // Mantém compatibilidade dos filtros/rota expostos ao front.
    // ─────────────────────────────────────────────────────
    internal static class SituacaoFiltro
    {
        public static ESituacao? Mapear(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return null;
            return status.Trim().ToLowerInvariant() switch
            {
                "aberta" or "aberto" => ESituacao.Aberto,
                "paga" or "pago" or "recebida" or "recebido" => ESituacao.Pago,
                "pagoparcial" or "parcial" => ESituacao.PagoParcial,
                "cancelada" or "cancelado" => ESituacao.Cancelado,
                _ => null
            };
        }
    }

    // ─────────────────────────────────────────────────────
    // QUERY: Listar Contas a Pagar (modelo fiel)
    // ─────────────────────────────────────────────────────
    public record ListarContasPagarQuery(
        string? Status = null,
        DateTime? VencimentoDe = null,
        DateTime? VencimentoAte = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IRequest<CommandResult>;

    public class ListarContasPagarQueryHandler : IRequestHandler<ListarContasPagarQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ListarContasPagarQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarContasPagarQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ContasAPagarAgregado.AsNoTracking().AsQueryable();

            var situacao = SituacaoFiltro.Mapear(request.Status);
            if (situacao.HasValue)
                query = query.Where(c => c.Situacao == situacao.Value);

            if (request.VencimentoDe.HasValue)
                query = query.Where(c => c.DataVencimento >= request.VencimentoDe.Value);

            if (request.VencimentoAte.HasValue)
                query = query.Where(c => c.DataVencimento <= request.VencimentoAte.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(c => c.DataVencimento)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new
                {
                    c.Id,
                    Descricao = c.Detalhamento,
                    FornecedorId = c.PessoaId,
                    c.NomePessoa,
                    Valor = c.ValorTitulo,
                    c.ValorTotalAPagarTitulo,
                    c.ValorTotalPago,
                    c.DataVencimento,
                    c.DataBaixa,
                    c.Situacao,
                    c.Documento,
                    c.NumeroParcela
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    // ─────────────────────────────────────────────────────
    // QUERY: Listar Contas a Receber (modelo fiel)
    // ─────────────────────────────────────────────────────
    public record ListarContasReceberQuery(
        string? Status = null,
        DateTime? VencimentoDe = null,
        DateTime? VencimentoAte = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IRequest<CommandResult>;

    public class ListarContasReceberQueryHandler : IRequestHandler<ListarContasReceberQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ListarContasReceberQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarContasReceberQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ContasAReceberAgregado.AsNoTracking().AsQueryable();

            var situacao = SituacaoFiltro.Mapear(request.Status);
            if (situacao.HasValue)
                query = query.Where(c => c.Situacao == situacao.Value);

            if (request.VencimentoDe.HasValue)
                query = query.Where(c => c.DataVencimento >= request.VencimentoDe.Value);

            if (request.VencimentoAte.HasValue)
                query = query.Where(c => c.DataVencimento <= request.VencimentoAte.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(c => c.DataVencimento)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new
                {
                    c.Id,
                    Descricao = c.Detalhamento,
                    ClienteId = c.PessoaId,
                    c.NomePessoa,
                    Valor = c.ValorTitulo,
                    c.ValorTotalAReceberTitulo,
                    c.ValorTotalRecebido,
                    c.DataVencimento,
                    c.DataBaixa,
                    c.Situacao,
                    c.Documento,
                    c.NumeroParcela
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    // ─────────────────────────────────────────────────────
    // QUERY: Fluxo de Caixa Projetado (período) — modelo fiel
    // Considera o saldo em aberto (ValorTotalAPagarTitulo / ValorTotalAReceberTitulo)
    // dos títulos ainda não cancelados nem quitados.
    // ─────────────────────────────────────────────────────
    public record ObterFluxoCaixaQuery(
        DateTime De,
        DateTime Ate
    ) : IRequest<CommandResult>;

    public class ObterFluxoCaixaQueryHandler : IRequestHandler<ObterFluxoCaixaQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ObterFluxoCaixaQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterFluxoCaixaQuery request, CancellationToken cancellationToken)
        {
            var totalPagar = await _context.ContasAPagarAgregado
                .AsNoTracking()
                .Where(c => c.DataVencimento >= request.De
                         && c.DataVencimento <= request.Ate
                         && c.Situacao != ESituacao.Cancelado
                         && c.Situacao != ESituacao.Pago)
                .SumAsync(c => c.ValorTotalAPagarTitulo, cancellationToken);

            var totalReceber = await _context.ContasAReceberAgregado
                .AsNoTracking()
                .Where(c => c.DataVencimento >= request.De
                         && c.DataVencimento <= request.Ate
                         && c.Situacao != ESituacao.Cancelado
                         && c.Situacao != ESituacao.Pago)
                .SumAsync(c => c.ValorTotalAReceberTitulo, cancellationToken);

            return CommandResult.Ok("Fluxo de caixa calculado.", new
            {
                Periodo = new { De = request.De, Ate = request.Ate },
                TotalAPagar = totalPagar,
                TotalAReceber = totalReceber,
                SaldoProjetado = totalReceber - totalPagar
            });
        }
    }

    // ─────────────────────────────────────────────────────
    // QUERY: Totais Contas a Pagar (cards de resumo do dashboard)
    // Contrato front (/financeiro/contas-pagar/totais):
    //   valorVencendoHoje, valorVencido, valorAVencer
    // Campos extras (valorTotalAberto, valorTotalPago) para completude no período.
    // Base: saldo em aberto (ValorTotalAPagarTitulo) dos títulos não cancelados.
    // ─────────────────────────────────────────────────────
    public record ObterTotaisContasPagarQuery(
        DateTime? De = null,
        DateTime? Ate = null
    ) : IRequest<CommandResult>;

    public class ObterTotaisContasPagarQueryHandler : IRequestHandler<ObterTotaisContasPagarQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ObterTotaisContasPagarQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterTotaisContasPagarQuery request, CancellationToken cancellationToken)
        {
            var hoje = DateTime.UtcNow.Date;
            var amanha = hoje.AddDays(1);

            var query = _context.ContasAPagarAgregado
                .AsNoTracking()
                .Where(c => c.Situacao != ESituacao.Cancelado && c.Situacao != ESituacao.Pago);

            if (request.De.HasValue)
                query = query.Where(c => c.DataVencimento >= request.De.Value);
            if (request.Ate.HasValue)
                query = query.Where(c => c.DataVencimento <= request.Ate.Value);

            var valorVencendoHoje = await query
                .Where(c => c.DataVencimento >= hoje && c.DataVencimento < amanha)
                .SumAsync(c => (decimal?)c.ValorTotalAPagarTitulo, cancellationToken) ?? 0m;

            var valorVencido = await query
                .Where(c => c.DataVencimento < hoje)
                .SumAsync(c => (decimal?)c.ValorTotalAPagarTitulo, cancellationToken) ?? 0m;

            var valorAVencer = await query
                .Where(c => c.DataVencimento >= amanha)
                .SumAsync(c => (decimal?)c.ValorTotalAPagarTitulo, cancellationToken) ?? 0m;

            var valorTotalAberto = await query
                .SumAsync(c => (decimal?)c.ValorTotalAPagarTitulo, cancellationToken) ?? 0m;

            var pagosQuery = _context.ContasAPagarAgregado
                .AsNoTracking()
                .Where(c => c.Situacao == ESituacao.Pago);
            if (request.De.HasValue)
                pagosQuery = pagosQuery.Where(c => c.DataVencimento >= request.De.Value);
            if (request.Ate.HasValue)
                pagosQuery = pagosQuery.Where(c => c.DataVencimento <= request.Ate.Value);
            var valorTotalPago = await pagosQuery
                .SumAsync(c => (decimal?)c.ValorTotalPago, cancellationToken) ?? 0m;

            return CommandResult.Ok("OK", new
            {
                ValorVencendoHoje = valorVencendoHoje,
                ValorVencido = valorVencido,
                ValorAVencer = valorAVencer,
                ValorTotalAberto = valorTotalAberto,
                ValorTotalPago = valorTotalPago
            });
        }
    }

    // ─────────────────────────────────────────────────────
    // QUERY: Totais Contas a Receber (cards de resumo do dashboard)
    // Contrato front (/financeiro/contas-receber/totais):
    //   valorVencendoHoje, valorVencido, valorAVencer
    // Campos extras (valorTotalAberto, valorTotalRecebido) para completude.
    // Base: saldo em aberto (ValorTotalAReceberTitulo) dos títulos não cancelados.
    // ─────────────────────────────────────────────────────
    public record ObterTotaisContasReceberQuery(
        DateTime? De = null,
        DateTime? Ate = null
    ) : IRequest<CommandResult>;

    public class ObterTotaisContasReceberQueryHandler : IRequestHandler<ObterTotaisContasReceberQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ObterTotaisContasReceberQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterTotaisContasReceberQuery request, CancellationToken cancellationToken)
        {
            var hoje = DateTime.UtcNow.Date;
            var amanha = hoje.AddDays(1);

            var query = _context.ContasAReceberAgregado
                .AsNoTracking()
                .Where(c => c.Situacao != ESituacao.Cancelado && c.Situacao != ESituacao.Pago);

            if (request.De.HasValue)
                query = query.Where(c => c.DataVencimento >= request.De.Value);
            if (request.Ate.HasValue)
                query = query.Where(c => c.DataVencimento <= request.Ate.Value);

            var valorVencendoHoje = await query
                .Where(c => c.DataVencimento >= hoje && c.DataVencimento < amanha)
                .SumAsync(c => (decimal?)c.ValorTotalAReceberTitulo, cancellationToken) ?? 0m;

            var valorVencido = await query
                .Where(c => c.DataVencimento < hoje)
                .SumAsync(c => (decimal?)c.ValorTotalAReceberTitulo, cancellationToken) ?? 0m;

            var valorAVencer = await query
                .Where(c => c.DataVencimento >= amanha)
                .SumAsync(c => (decimal?)c.ValorTotalAReceberTitulo, cancellationToken) ?? 0m;

            var valorTotalAberto = await query
                .SumAsync(c => (decimal?)c.ValorTotalAReceberTitulo, cancellationToken) ?? 0m;

            var recebidosQuery = _context.ContasAReceberAgregado
                .AsNoTracking()
                .Where(c => c.Situacao == ESituacao.Pago);
            if (request.De.HasValue)
                recebidosQuery = recebidosQuery.Where(c => c.DataVencimento >= request.De.Value);
            if (request.Ate.HasValue)
                recebidosQuery = recebidosQuery.Where(c => c.DataVencimento <= request.Ate.Value);
            var valorTotalRecebido = await recebidosQuery
                .SumAsync(c => (decimal?)c.ValorTotalRecebido, cancellationToken) ?? 0m;

            return CommandResult.Ok("OK", new
            {
                ValorVencendoHoje = valorVencendoHoje,
                ValorVencido = valorVencido,
                ValorAVencer = valorAVencer,
                ValorTotalAberto = valorTotalAberto,
                ValorTotalRecebido = valorTotalRecebido
            });
        }
    }

    // ─────────────────────────────────────────────────────
    // QUERY: Obter Conta a Pagar por ID (modelo fiel, com itens/baixas)
    // ─────────────────────────────────────────────────────
    public record ObterContaPagarPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterContaPagarPorIdQueryHandler : IRequestHandler<ObterContaPagarPorIdQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ObterContaPagarPorIdQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterContaPagarPorIdQuery request, CancellationToken cancellationToken)
        {
            var conta = await _context.ContasAPagarAgregado
                .AsNoTracking()
                .Include(c => c.ContasAPagarItens)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (conta is null)
                return CommandResult.Falha("Conta a pagar não encontrada.");

            return CommandResult.Ok("OK", conta);
        }
    }

    // ─────────────────────────────────────────────────────
    // QUERY: Obter Conta a Receber por ID (modelo fiel, com itens/baixas)
    // ─────────────────────────────────────────────────────
    public record ObterContaReceberPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterContaReceberPorIdQueryHandler : IRequestHandler<ObterContaReceberPorIdQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ObterContaReceberPorIdQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterContaReceberPorIdQuery request, CancellationToken cancellationToken)
        {
            var conta = await _context.ContasAReceberAgregado
                .AsNoTracking()
                .Include(c => c.ContasAReceberItens)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (conta is null)
                return CommandResult.Falha("Conta a receber não encontrada.");

            return CommandResult.Ok("OK", conta);
        }
    }
}
