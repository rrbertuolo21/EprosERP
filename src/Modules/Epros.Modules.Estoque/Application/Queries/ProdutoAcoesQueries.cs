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
    /// <summary>
    /// Porte fiel de ProdutoController.Get (GET api/v1/produtos/localizar-produto).
    /// Localiza produtos por código exato, depois por descrição (contém) e por fim por EAN, aplicando
    /// filtro de ativo, ordenação (codigo/descricao/valor) e paginação.
    /// </summary>
    public record LocalizarProdutoQuery(
        string? Localizar = null,
        DateTime? DataAlteracao = null,
        bool Ativo = true,
        string OrdenarPor = "codigo",
        bool OrdemAscendente = true,
        int Pagina = 1,
        int TamanhoPagina = 50
    ) : IQuery<CommandResult>;

    /// <summary>
    /// Porte de ProdutoController.Get (GET api/v1/produtos/gtin/{valorGtin}).
    /// No legado a consulta ao GTIN era feita no webservice da SEFAZ (SVRS) usando o certificado digital
    /// da empresa. Essa integração SOAP/certificado pertence à camada DFe (Hercules/OpenAC) e não é
    /// reescrita aqui (regra de reuso). Este read-model consulta a base local do tenant pelo EAN/GTIN e
    /// retorna o produto correspondente quando existir.
    /// </summary>
    public record ConsultarProdutoPorGtinQuery(string ValorGtin) : IQuery<CommandResult>;

    public class LocalizarProdutoQueryHandler : IRequestHandler<LocalizarProdutoQuery, CommandResult>
    {
        private const int TotalRegistroPaginacao = 200;
        private readonly ContextEstoque _context;

        public LocalizarProdutoQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(LocalizarProdutoQuery request, CancellationToken cancellationToken)
        {
            var tamanhoPagina = request.TamanhoPagina > TotalRegistroPaginacao ? TotalRegistroPaginacao : request.TamanhoPagina;

            var query = _context.Produtos.AsNoTracking().Where(p => p.DeletadoEm == null).AsQueryable();

            if (request.Ativo)
                query = query.Where(p => p.Ativo);

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                var termo = request.Localizar;

                // Legado: código exato -> descrição (contém) -> EAN (exato), na ordem, usando o primeiro que retorna algo.
                var porCodigo = query.Where(p => p.Codigo == termo);
                if (await porCodigo.AnyAsync(cancellationToken))
                    query = porCodigo;
                else
                {
                    var porDescricao = query.Where(p => p.Descricao.ToLower().Contains(termo.ToLower()));
                    if (await porDescricao.AnyAsync(cancellationToken))
                        query = porDescricao;
                    else
                        query = query.Where(p => p.Ean == termo);
                }
            }

            if (request.DataAlteracao.HasValue)
            {
                var dataAlteracao = request.DataAlteracao.Value;
                query = query.Where(p => p.AlteradoEm >= dataAlteracao || p.CriadoEm >= dataAlteracao);
            }

            var totalRegistros = await query.CountAsync(cancellationToken);
            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina);

            query = (request.OrdenarPor?.ToLower()) switch
            {
                "descricao" => request.OrdemAscendente ? query.OrderBy(p => p.Descricao) : query.OrderByDescending(p => p.Descricao),
                "valor" => request.OrdemAscendente ? query.OrderBy(p => p.ValorVenda) : query.OrderByDescending(p => p.ValorVenda),
                _ => request.OrdemAscendente ? query.OrderBy(p => p.Codigo) : query.OrderByDescending(p => p.Codigo),
            };

            var itens = await query
                .Skip((request.Pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .Select(p => new
                {
                    p.Id,
                    p.Codigo,
                    Sku = p.Sku,
                    p.Descricao,
                    Nome = p.Nome,
                    p.Ean,
                    p.ValorVenda,
                    p.ValorVendaPrazo,
                    p.ValorCompra,
                    p.PrecoVenda,
                    p.SaldoEstoque,
                    p.CustoMedio,
                    p.CategoriaId,
                    p.MarcaProdutoId,
                    p.UnidadeMedidaComercialId,
                    p.NcmId,
                    p.CestId,
                    p.CodigoAnpId,
                    p.TipoProduto,
                    p.Ativo,
                    p.Imagem,
                    p.UtilizaBalanca,
                    p.CodigoProdutoBalanca,
                    p.BalancaId
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = totalRegistros, TotalPaginas = totalPaginas, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ConsultarProdutoPorGtinQueryHandler : IRequestHandler<ConsultarProdutoPorGtinQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ConsultarProdutoPorGtinQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ConsultarProdutoPorGtinQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ValorGtin))
                return CommandResult.Falha("O GTIN é obrigatório.");

            var produto = await _context.Produtos
                .AsNoTracking()
                .Where(p => p.DeletadoEm == null && p.Ean == request.ValorGtin)
                .Select(p => new
                {
                    p.Id,
                    p.Codigo,
                    p.Descricao,
                    Gtin = p.Ean,
                    p.NcmId,
                    p.CestId,
                    p.ValorVenda,
                    p.ValorCompra
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (produto == null)
                return CommandResult.Falha($"Nenhum produto localizado na base local para o GTIN {request.ValorGtin}. A consulta ao cadastro nacional (SEFAZ) é feita pela camada de emissão fiscal.");

            return CommandResult.Ok("OK", produto);
        }
    }
}
