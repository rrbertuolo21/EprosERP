using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Queries
{
    public record ListarFichasProducaoQuery(
        int? Situacao,
        Guid? PessoaId,
        Guid? VendaId,
        int Pagina = 1,
        int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterFichaProducaoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarFichasProducaoQueryHandler : IQueryHandler<ListarFichasProducaoQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ListarFichasProducaoQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ListarFichasProducaoQuery request, CancellationToken ct)
        {
            var query = _context.FichasProducao.AsNoTracking().AsQueryable();

            if (request.Situacao.HasValue)
                query = query.Where(f => f.Situacao == (ESituacaoFichaProducao)request.Situacao.Value);
            if (request.PessoaId.HasValue)
                query = query.Where(f => f.PessoaId == request.PessoaId.Value);
            if (request.VendaId.HasValue)
                query = query.Where(f => f.VendaId == request.VendaId.Value);

            var total = await query.CountAsync(ct);
            var itens = await query
                .OrderByDescending(f => f.Entrada)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(ct);

            return CommandResult.Ok("Fichas de produção listadas com sucesso.", new { total, itens });
        }
    }

    public class ObterFichaProducaoPorIdQueryHandler : IQueryHandler<ObterFichaProducaoPorIdQuery, CommandResult>
    {
        private readonly ContextProducao _context;
        public ObterFichaProducaoPorIdQueryHandler(ContextProducao context) => _context = context;

        public async Task<CommandResult> Handle(ObterFichaProducaoPorIdQuery request, CancellationToken ct)
        {
            var ficha = await _context.FichasProducao.AsNoTracking().FirstOrDefaultAsync(f => f.Id == request.Id, ct);
            if (ficha == null)
                return CommandResult.Falha("Ficha de produção não encontrada.");

            return CommandResult.Ok("Ficha de produção obtida com sucesso.", ficha);
        }
    }
}
