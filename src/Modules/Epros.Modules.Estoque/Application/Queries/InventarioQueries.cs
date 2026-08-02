using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    public record ListarInventariosQuery(ESituacaoInventario? Situacao = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterInventarioPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public record ListarReajustesEstoqueQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarInventariosQueryHandler : IRequestHandler<ListarInventariosQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarInventariosQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarInventariosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Inventarios.AsNoTracking().Where(i => i.DeletadoEm == null);
            if (request.Situacao.HasValue)
                query = query.Where(i => i.Situacao == request.Situacao.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(i => i.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(i => new { i.Id, i.EmpresaId, i.DataContagem, i.TipoInventario, i.Situacao, i.Acuracidade, i.EstoqueAtualizado, i.CriadoEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }

    public class ObterInventarioPorIdQueryHandler : IRequestHandler<ObterInventarioPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ObterInventarioPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterInventarioPorIdQuery request, CancellationToken cancellationToken)
        {
            var inv = await _context.Inventarios.AsNoTracking().FirstOrDefaultAsync(i => i.Id == request.Id && i.DeletadoEm == null, cancellationToken);
            if (inv == null) return CommandResult.Falha("Inventário não encontrado.");

            var itens = await _context.InventarioItens.AsNoTracking()
                .Where(it => it.InventarioId == inv.Id && it.DeletadoEm == null)
                .Select(it => new { it.Id, it.ProdutoId, it.LocalId, it.Lote, it.QuantidadeSistema, it.Contagem01, it.Contagem02, it.Contagem03, it.QuantidadeContada, it.Divergencia, it.FechadoContagem })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new
            {
                inv.Id, inv.EmpresaId, inv.DataContagem, inv.TipoInventario, inv.Situacao,
                inv.Acuracidade, inv.EstoqueAtualizado, inv.Observacao, inv.CriadoEm, Itens = itens
            });
        }
    }

    public class ListarReajustesEstoqueQueryHandler : IRequestHandler<ListarReajustesEstoqueQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarReajustesEstoqueQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarReajustesEstoqueQuery request, CancellationToken cancellationToken)
        {
            var query = _context.InventarioReajustes.AsNoTracking().Where(r => r.DeletadoEm == null);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(r => r.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(r => new { r.Id, r.ColaboradorId, r.DataReajuste, r.Porcentagem, r.TipoReajuste, r.Situacao, r.CriadoEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }
}
