using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    // ----- BANCO QUERIES -----
    public record ListarBancosQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 50
    ) : IRequest<CommandResult>;

    public class ListarBancosQueryHandler : IRequestHandler<ListarBancosQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ListarBancosQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarBancosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Bancos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                query = query.Where(b => b.Codigo.Contains(request.Localizar) || b.Descricao.Contains(request.Localizar));
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(b => b.Codigo)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(b => new
                {
                    b.Id,
                    b.Codigo,
                    b.Descricao
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public record ObterBancoPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterBancoPorIdQueryHandler : IRequestHandler<ObterBancoPorIdQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ObterBancoPorIdQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterBancoPorIdQuery request, CancellationToken cancellationToken)
        {
            var banco = await _context.Bancos
                .AsNoTracking()
                .Select(b => new
                {
                    b.Id,
                    b.Codigo,
                    b.Descricao
                })
                .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

            if (banco == null) return CommandResult.Falha("Banco não encontrado.");
            return CommandResult.Ok("OK", banco);
        }
    }


    // ----- CONTA BANCARIA QUERIES -----
    public record ListarContasBancariasQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IRequest<CommandResult>;

    public class ListarContasBancariasQueryHandler : IRequestHandler<ListarContasBancariasQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ListarContasBancariasQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarContasBancariasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ContasBancarias.AsNoTracking().Include(c => c.Banco).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                query = query.Where(c => c.Apelido.Contains(request.Localizar) || c.Conta.Contains(request.Localizar));
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(c => c.Apelido)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new
                {
                    c.Id,
                    c.BancoId,
                    BancoNome = c.Banco.Descricao,
                    BancoCodigo = c.Banco.Codigo,
                    c.TipoContaBancaria,
                    c.Apelido,
                    c.Titular,
                    c.Agencia,
                    c.Conta,
                    c.Gerente,
                    c.FoneGerente,
                    c.Detalhe,
                    c.DigitoAgencia,
                    c.DataEncerramento
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public record ObterContaBancariaPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterContaBancariaPorIdQueryHandler : IRequestHandler<ObterContaBancariaPorIdQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ObterContaBancariaPorIdQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterContaBancariaPorIdQuery request, CancellationToken cancellationToken)
        {
            var conta = await _context.ContasBancarias
                .AsNoTracking()
                .Include(c => c.Banco)
                .Select(c => new
                {
                    c.Id,
                    c.BancoId,
                    BancoNome = c.Banco.Descricao,
                    BancoCodigo = c.Banco.Codigo,
                    c.TipoContaBancaria,
                    c.Apelido,
                    c.Titular,
                    c.Agencia,
                    c.Conta,
                    c.Gerente,
                    c.FoneGerente,
                    c.Detalhe,
                    c.DigitoAgencia,
                    c.DataEncerramento
                })
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (conta == null) return CommandResult.Falha("Conta bancária não encontrada.");
            return CommandResult.Ok("OK", conta);
        }
    }


    // ----- CARTAO DE CREDITO QUERIES -----
    public record ListarCartoesDeCreditoQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IRequest<CommandResult>;

    public class ListarCartoesDeCreditoQueryHandler : IRequestHandler<ListarCartoesDeCreditoQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ListarCartoesDeCreditoQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarCartoesDeCreditoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CartoesDeCredito.AsNoTracking().Include(c => c.ContaBancaria).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                query = query.Where(c => c.Apelido.Contains(request.Localizar) || c.Titular.Contains(request.Localizar));
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(c => c.Apelido)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new
                {
                    c.Id,
                    c.ContaBancariaId,
                    ContaApelido = c.ContaBancaria.Apelido,
                    c.Apelido,
                    c.Titular,
                    c.BandeiraCartao,
                    c.Observacao
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public record ObterCartaoDeCreditoPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterCartaoDeCreditoPorIdQueryHandler : IRequestHandler<ObterCartaoDeCreditoPorIdQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ObterCartaoDeCreditoPorIdQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterCartaoDeCreditoPorIdQuery request, CancellationToken cancellationToken)
        {
            var cartao = await _context.CartoesDeCredito
                .AsNoTracking()
                .Include(c => c.ContaBancaria)
                .Select(c => new
                {
                    c.Id,
                    c.ContaBancariaId,
                    ContaApelido = c.ContaBancaria.Apelido,
                    c.Apelido,
                    c.Titular,
                    c.BandeiraCartao,
                    c.Observacao
                })
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (cartao == null) return CommandResult.Falha("Cartão de crédito não encontrado.");
            return CommandResult.Ok("OK", cartao);
        }
    }


    // ----- CARTAO DE CREDITO FATURA QUERIES -----
    public record ListarFaturasCartaoQuery(
        Guid CartaoDeCreditoId,
        int Pagina = 1,
        int TamanhoPagina = 50
    ) : IRequest<CommandResult>;

    public class ListarFaturasCartaoQueryHandler : IRequestHandler<ListarFaturasCartaoQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ListarFaturasCartaoQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarFaturasCartaoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CartoesDeCreditoFaturas
                .AsNoTracking()
                .Where(f => f.CartaoDeCreditoId == request.CartaoDeCreditoId)
                .AsQueryable();

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(f => f.DataVencimento)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(f => new
                {
                    f.Id,
                    f.CartaoDeCreditoId,
                    f.DataLancamento,
                    f.DataVencimento,
                    f.Valor,
                    f.Pago
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }
}
