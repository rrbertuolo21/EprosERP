using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    // ============================ CategoriaProduto ============================

    public record ListarCategoriasProdutoQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterCategoriaProdutoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarCategoriasProdutoQueryHandler : IRequestHandler<ListarCategoriasProdutoQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarCategoriasProdutoQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarCategoriasProdutoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Categorias.AsNoTracking().Where(c => c.DeletadoEm == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(c => c.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(c => c.Descricao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new { c.Id, c.Descricao, c.ProdutoGrupoId })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterCategoriaProdutoPorIdQueryHandler : IRequestHandler<ObterCategoriaProdutoPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterCategoriaProdutoPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterCategoriaProdutoPorIdQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (c == null)
                return CommandResult.Falha("Categoria não encontrada.");

            return CommandResult.Ok("OK", new { c.Id, c.Descricao, c.ProdutoGrupoId });
        }
    }

    // ============================ MarcaProduto ============================

    public record ListarMarcasProdutoQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterMarcaProdutoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarMarcasProdutoQueryHandler : IRequestHandler<ListarMarcasProdutoQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarMarcasProdutoQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarMarcasProdutoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Marcas.AsNoTracking().Where(m => m.DeletadoEm == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(m => m.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(m => m.Descricao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(m => new { m.Id, m.Descricao, m.ProdutoGrupoId })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterMarcaProdutoPorIdQueryHandler : IRequestHandler<ObterMarcaProdutoPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterMarcaProdutoPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterMarcaProdutoPorIdQuery request, CancellationToken cancellationToken)
        {
            var m = await _context.Marcas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (m == null)
                return CommandResult.Falha("Marca não encontrada.");

            return CommandResult.Ok("OK", new { m.Id, m.Descricao, m.ProdutoGrupoId });
        }
    }

    // ============================ UnidadeMedidaComercial ============================

    public record ListarUnidadesMedidaComercialQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterUnidadeMedidaComercialPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarUnidadesMedidaComercialQueryHandler : IRequestHandler<ListarUnidadesMedidaComercialQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarUnidadesMedidaComercialQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarUnidadesMedidaComercialQuery request, CancellationToken cancellationToken)
        {
            var query = _context.UnidadesMedida.AsNoTracking().Where(u => u.DeletadoEm == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(u => u.UnidadeMedida.Contains(request.Localizar) || u.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(u => u.UnidadeMedida)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(u => new { u.Id, u.UnidadeMedida, u.Descricao, u.Fator, u.ProdutoGrupoId })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterUnidadeMedidaComercialPorIdQueryHandler : IRequestHandler<ObterUnidadeMedidaComercialPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterUnidadeMedidaComercialPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterUnidadeMedidaComercialPorIdQuery request, CancellationToken cancellationToken)
        {
            var u = await _context.UnidadesMedida.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (u == null)
                return CommandResult.Falha("Unidade de medida não encontrada.");

            return CommandResult.Ok("OK", new { u.Id, u.UnidadeMedida, u.Descricao, u.Fator, u.ProdutoGrupoId });
        }
    }

    // ============================ Adicionais ============================

    public record ListarAdicionaisQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterAdicionaisPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarAdicionaisQueryHandler : IRequestHandler<ListarAdicionaisQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarAdicionaisQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarAdicionaisQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Adicionais.AsNoTracking().Where(a => a.DeletadoEm == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(a => a.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(a => a.Descricao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(a => new { a.Id, a.Descricao, a.ValorPreco })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterAdicionaisPorIdQueryHandler : IRequestHandler<ObterAdicionaisPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterAdicionaisPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterAdicionaisPorIdQuery request, CancellationToken cancellationToken)
        {
            var a = await _context.Adicionais.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (a == null)
                return CommandResult.Falha("Adicional não encontrado.");

            return CommandResult.Ok("OK", new { a.Id, a.Descricao, a.ValorPreco });
        }
    }

    // ============================ Balanca ============================

    public record ListarBalancasQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterBalancaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarBalancasQueryHandler : IRequestHandler<ListarBalancasQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarBalancasQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarBalancasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Balancas.AsNoTracking().Where(b => b.DeletadoEm == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(b => b.Nome.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(b => b.Nome)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(b => new
                {
                    b.Id,
                    b.Nome,
                    b.QntDigitoIdentificador,
                    b.QntDigitoCodigoProduto,
                    b.QntDigitoValorProduto,
                    b.QntCasaDecimal,
                    b.TipoValor
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterBalancaPorIdQueryHandler : IRequestHandler<ObterBalancaPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterBalancaPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterBalancaPorIdQuery request, CancellationToken cancellationToken)
        {
            var b = await _context.Balancas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (b == null)
                return CommandResult.Falha("Balança não encontrada.");

            return CommandResult.Ok("OK", new
            {
                b.Id,
                b.Nome,
                b.QntDigitoIdentificador,
                b.QntDigitoCodigoProduto,
                b.QntDigitoValorProduto,
                b.QntCasaDecimal,
                b.TipoValor
            });
        }
    }
}
