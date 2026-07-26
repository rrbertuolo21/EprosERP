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
    public record ListarPlanosContratoQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ListarContratosFinanceirosQuery(EStatusContratoFinanceiro? Status, Guid? PessoaId, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ObterContratoFinanceiroPorIdQuery(Guid Id) : IRequest<CommandResult>;
    public record ListarFaturasRecorrentesQuery(Guid ContratoId) : IRequest<CommandResult>;
    public record ListarReajustesContratoQuery(Guid ContratoId) : IRequest<CommandResult>;

    public class ContratosFinanceirosQueryHandlers :
        IRequestHandler<ListarPlanosContratoQuery, CommandResult>,
        IRequestHandler<ListarContratosFinanceirosQuery, CommandResult>,
        IRequestHandler<ObterContratoFinanceiroPorIdQuery, CommandResult>,
        IRequestHandler<ListarFaturasRecorrentesQuery, CommandResult>,
        IRequestHandler<ListarReajustesContratoQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ContratosFinanceirosQueryHandlers(ContextFinanceiro context) => _context = context;

        private static (int pagina, int tamanho) Normaliza(int pagina, int tamanho)
            => (pagina <= 0 ? 1 : pagina, tamanho is <= 0 or > 100 ? 20 : tamanho);

        public async Task<CommandResult> Handle(ListarPlanosContratoQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.PlanosContrato.AsNoTracking();
            var total = await query.CountAsync(ct);
            var itens = await query.OrderBy(p => p.Descricao).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(p => new { p.Id, p.Descricao, p.Valor, p.Periodicidade, p.Status }).ToListAsync(ct);
            return CommandResult.Ok("Planos de contrato listados.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ListarContratosFinanceirosQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.ContratosFinanceiros.AsNoTracking().AsQueryable();
            if (request.Status.HasValue) query = query.Where(c => c.Status == request.Status.Value);
            if (request.PessoaId.HasValue) query = query.Where(c => c.PessoaId == request.PessoaId.Value);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderByDescending(c => c.DataInicio).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(c => new { c.Id, c.PlanoId, c.PessoaId, c.DataInicio, c.DataFim, c.Status }).ToListAsync(ct);
            return CommandResult.Ok("Contratos financeiros listados.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ObterContratoFinanceiroPorIdQuery request, CancellationToken ct)
        {
            var c = await _context.ContratosFinanceiros.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            return c == null ? CommandResult.Falha("Contrato financeiro não encontrado.") : CommandResult.Ok("Contrato encontrado.", c);
        }

        public async Task<CommandResult> Handle(ListarFaturasRecorrentesQuery request, CancellationToken ct)
        {
            var itens = await _context.FaturasRecorrentes.AsNoTracking().Where(f => f.ContratoId == request.ContratoId)
                .OrderByDescending(f => f.Competencia)
                .Select(f => new { f.Id, f.Competencia, f.Valor, f.Status, f.TituloContasReceberId }).ToListAsync(ct);
            return CommandResult.Ok("Faturas recorrentes listadas.", itens);
        }

        public async Task<CommandResult> Handle(ListarReajustesContratoQuery request, CancellationToken ct)
        {
            var itens = await _context.ReajustesContrato.AsNoTracking().Where(r => r.ContratoId == request.ContratoId)
                .OrderByDescending(r => r.DataReajuste)
                .Select(r => new { r.Id, r.DataReajuste, r.ValorAnterior, r.ValorNovo, r.Motivo }).ToListAsync(ct);
            return CommandResult.Ok("Reajustes do contrato listados.", itens);
        }
    }
}
