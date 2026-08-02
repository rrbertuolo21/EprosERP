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
    // Todas as consultas do fornecedor são ISOLADAS por FornecedorId (PFO-002 / VAL-PFO-001).
    public record ListarCotacoesFornecedorQuery(Guid FornecedorId, EStatusCotacaoPublicada? Status = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ListarPreAvisosFornecedorQuery(Guid FornecedorId, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ListarDocumentosFornecedorQuery(Guid FornecedorId, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ListarConvitesFornecedorQuery(EStatusConviteFornecedor? Status = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarCotacoesFornecedorQueryHandler : IRequestHandler<ListarCotacoesFornecedorQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarCotacoesFornecedorQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarCotacoesFornecedorQuery request, CancellationToken cancellationToken)
        {
            if (request.FornecedorId == Guid.Empty) return CommandResult.Falha("Fornecedor é obrigatório [PFO-002].");
            var query = _context.PfoCotacoesPublicadas.AsNoTracking()
                .Where(c => c.DeletadoEm == null && c.FornecedorId == request.FornecedorId);
            if (request.Status.HasValue) query = query.Where(c => c.Status == request.Status.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderByDescending(c => c.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina).Take(request.TamanhoPagina)
                .Select(c => new { c.Id, c.CotacaoOrigemId, c.Status, c.PrazoResposta, c.CriadoEm })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }

    public class ListarPreAvisosFornecedorQueryHandler : IRequestHandler<ListarPreAvisosFornecedorQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarPreAvisosFornecedorQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarPreAvisosFornecedorQuery request, CancellationToken cancellationToken)
        {
            if (request.FornecedorId == Guid.Empty) return CommandResult.Falha("Fornecedor é obrigatório [PFO-002].");
            var query = _context.PfoPreAvisosEmbarque.AsNoTracking()
                .Where(p => p.DeletadoEm == null && p.FornecedorId == request.FornecedorId);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderByDescending(p => p.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina).Take(request.TamanhoPagina)
                .Select(p => new { p.Id, p.PedidoCompraId, p.Status, p.DataPrevistaEntrega, p.CriadoEm })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }

    public class ListarDocumentosFornecedorQueryHandler : IRequestHandler<ListarDocumentosFornecedorQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarDocumentosFornecedorQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarDocumentosFornecedorQuery request, CancellationToken cancellationToken)
        {
            if (request.FornecedorId == Guid.Empty) return CommandResult.Falha("Fornecedor é obrigatório [PFO-002].");
            var query = _context.PfoDocumentosFornecedor.AsNoTracking()
                .Where(d => d.DeletadoEm == null && d.FornecedorId == request.FornecedorId);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderByDescending(d => d.EnviadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina).Take(request.TamanhoPagina)
                .Select(d => new { d.Id, d.ReferenciaTipo, d.ReferenciaId, d.TipoDocumento, d.ArquivoId, d.Status, d.EnviadoEm })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }

    public class ListarConvitesFornecedorQueryHandler : IRequestHandler<ListarConvitesFornecedorQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarConvitesFornecedorQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarConvitesFornecedorQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PfoConvitesFornecedor.AsNoTracking().Where(c => c.DeletadoEm == null);
            if (request.Status.HasValue) query = query.Where(c => c.Status == request.Status.Value);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderByDescending(c => c.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina).Take(request.TamanhoPagina)
                .Select(c => new { c.Id, c.FornecedorId, c.EmailConvite, c.Status, c.DataEnvio, c.DataExpiracao })
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }
}
