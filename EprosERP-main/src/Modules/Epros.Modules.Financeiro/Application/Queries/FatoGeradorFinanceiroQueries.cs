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
    /// <summary>
    /// Consulta do FatoGeradorFinanceiro (origem/rastreabilidade de títulos). No legado a entidade
    /// não tinha controller próprio (só navegação a partir de ContasA*); aqui expomos leitura HTTP
    /// paginada e por Id, filtrável por Origem / VendaId / CompraId.
    /// </summary>
    public record ListarFatosGeradoresFinanceirosQuery(
        int? Origem = null,
        Guid? VendaId = null,
        Guid? CompraId = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IRequest<CommandResult>;

    public class ListarFatosGeradoresFinanceirosQueryHandler : IRequestHandler<ListarFatosGeradoresFinanceirosQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ListarFatosGeradoresFinanceirosQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarFatosGeradoresFinanceirosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.FatosGeradoresFinanceiros
                .AsNoTracking()
                .Where(f => f.DeletadoEm == null)
                .AsQueryable();

            if (request.Origem.HasValue)
                query = query.Where(f => (int)f.Origem == request.Origem.Value);

            if (request.VendaId.HasValue)
                query = query.Where(f => f.VendaId == request.VendaId.Value);

            if (request.CompraId.HasValue)
                query = query.Where(f => f.CompraId == request.CompraId.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(f => f.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(f => new
                {
                    f.Id,
                    Origem = (int)f.Origem,
                    f.VendaId,
                    f.CompraId,
                    f.Descricao,
                    f.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    /// <summary>Obtém um FatoGeradorFinanceiro por Id, com os títulos (contas a pagar/receber) vinculados.</summary>
    public record ObterFatoGeradorFinanceiroPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ObterFatoGeradorFinanceiroPorIdQueryHandler : IRequestHandler<ObterFatoGeradorFinanceiroPorIdQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ObterFatoGeradorFinanceiroPorIdQueryHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ObterFatoGeradorFinanceiroPorIdQuery request, CancellationToken cancellationToken)
        {
            var fato = await _context.FatosGeradoresFinanceiros
                .AsNoTracking()
                .Where(f => f.DeletadoEm == null && f.Id == request.Id)
                .Select(f => new
                {
                    f.Id,
                    Origem = (int)f.Origem,
                    f.VendaId,
                    f.CompraId,
                    f.Descricao,
                    f.CriadoEm,
                    ContasAReceber = f.ContasARecebers
                        .Where(c => c.DeletadoEm == null)
                        .Select(c => new { c.Id })
                        .ToList(),
                    ContasAPagar = f.ContasAPagars
                        .Where(c => c.DeletadoEm == null)
                        .Select(c => new { c.Id })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (fato == null) return CommandResult.Falha("Fato gerador financeiro não encontrado.");
            return CommandResult.Ok("OK", fato);
        }
    }
}
