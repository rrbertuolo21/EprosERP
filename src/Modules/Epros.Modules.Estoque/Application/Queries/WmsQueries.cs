using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    /// <summary>
    /// Consultas do submódulo Gestão de Armazém WMS (EST-WMS). WMS-005/006/007: filtros por nome, cidade e
    /// status. WMS-008/009: ordenação/paginação. Tenant é aplicado pelo filtro global do ContextBase.
    /// </summary>
    public record ListarWmsArmazensQuery(string? Nome = null, string? Cidade = null, bool? Ativo = null, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;

    public record ObterWmsArmazemPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ListarWmsArmazensQueryHandler : IRequestHandler<ListarWmsArmazensQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarWmsArmazensQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarWmsArmazensQuery request, CancellationToken cancellationToken)
        {
            var query = _context.WmsArmazens.AsNoTracking().Where(a => a.DeletadoEm == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Nome))
                query = query.Where(a => a.Nome.ToLower().Contains(request.Nome.ToLower()));
            if (!string.IsNullOrWhiteSpace(request.Cidade))
                query = query.Where(a => a.Cidade.ToLower().Contains(request.Cidade.ToLower()));
            if (request.Ativo.HasValue)
                query = query.Where(a => a.Ativo == request.Ativo.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(a => a.Nome)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(a => new { a.Id, a.Nome, a.Endereco, a.Cidade, a.Cep, a.Telefone, a.Email, a.Ativo, a.UsuarioDonoId, a.CriadoEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterWmsArmazemPorIdQueryHandler : IRequestHandler<ObterWmsArmazemPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterWmsArmazemPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterWmsArmazemPorIdQuery request, CancellationToken cancellationToken)
        {
            var a = await _context.WmsArmazens.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (a == null)
                return CommandResult.Falha("Armazém não encontrado.");

            return CommandResult.Ok("OK", new { a.Id, a.Nome, a.Endereco, a.Cidade, a.Cep, a.Telefone, a.Email, a.Ativo, a.UsuarioDonoId, a.CriadoEm });
        }
    }
}
