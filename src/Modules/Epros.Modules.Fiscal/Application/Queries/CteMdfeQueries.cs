using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Queries
{
    /// <summary>Lista os CT-e emitidos (histórico), paginado, com filtro opcional por status.</summary>
    public record ListarCtesQuery(string? Status = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    /// <summary>Lista os MDF-e emitidos (histórico), paginado, com filtro opcional por status.</summary>
    public record ListarMdfesQuery(string? Status = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarCtesQueryHandler : IRequestHandler<ListarCtesQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarCtesQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarCtesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ConhecimentosTransporteEletronicos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(c => c.Status == request.Status);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(c => c.DataEmissao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new
                {
                    c.Id,
                    c.Serie,
                    c.Numero,
                    c.Status,
                    c.ChaveAcesso,
                    c.Protocolo,
                    c.RemetenteDocumento,
                    c.DestinatarioDocumento,
                    c.ValorTotal,
                    c.DataEmissao,
                    c.DataAutorizacao
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ListarMdfesQueryHandler : IRequestHandler<ListarMdfesQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarMdfesQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarMdfesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ManifestosEletronicosDocumentosFiscais.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(m => m.Status == request.Status);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(m => m.DataEmissao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(m => new
                {
                    m.Id,
                    m.Serie,
                    m.Numero,
                    m.Status,
                    m.ChaveAcesso,
                    m.Protocolo,
                    m.UfInicio,
                    m.UfFim,
                    m.ValorCarga,
                    m.DataEmissao,
                    m.DataAutorizacao,
                    m.DataEncerramento
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }
}
