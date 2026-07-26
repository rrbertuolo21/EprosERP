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
    /// Listagem/histórico paginado dos XMLs importados (NF-e de entrada/saída).
    /// Porte fiel do legado <c>ImportacaoXmlsController</c> (GET base com ordenarPor /
    /// ordemAscendente / paginação). Não retorna o campo Xml (payload pesado) na listagem.
    /// </summary>
    public record ListarImportacoesXmlQuery(
        string OrdenarPor = "data",
        bool OrdemAscendente = false,
        int Pagina = 1,
        int TamanhoPagina = 25
    ) : IQuery<CommandResult>;

    public class ListarImportacoesXmlQueryHandler : IRequestHandler<ListarImportacoesXmlQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarImportacoesXmlQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarImportacoesXmlQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ImportacoesXml
                .AsNoTracking()
                .Where(x => x.DeletadoEm == null)
                .AsQueryable();

            var asc = request.OrdemAscendente;
            query = (request.OrdenarPor?.ToLowerInvariant()) switch
            {
                "chave" => asc ? query.OrderBy(x => x.NfeId) : query.OrderByDescending(x => x.NfeId),
                "tipoxml" => asc ? query.OrderBy(x => x.TipoDeXml) : query.OrderByDescending(x => x.TipoDeXml),
                "tipoevento" => asc ? query.OrderBy(x => x.TipoEvento) : query.OrderByDescending(x => x.TipoEvento),
                _ => asc ? query.OrderBy(x => x.CriadoEm) : query.OrderByDescending(x => x.CriadoEm),
            };

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(x => new
                {
                    x.Id,
                    x.EmpresaId,
                    TipoDeXml = (int)x.TipoDeXml,
                    x.NfeId,
                    x.CodigoSefaz,
                    x.TipoEvento,
                    StatusImportacaoXml = (int)x.StatusImportacaoXml,
                    x.MensagemErroImportacaoXml,
                    StatusCadastro = (int)x.StatusCadastro,
                    x.MensagemErroCadastro,
                    StatusSalvarPdf = (int)x.StatusSalvarPdf,
                    x.MensagemErroSalvarPdf,
                    DataImportacao = x.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    /// <summary>Detalhe de uma importação XML por Id (legado <c>importacao-xmls/{id}</c>). Inclui o XML.</summary>
    public record ObterImportacaoXmlPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ObterImportacaoXmlPorIdQueryHandler : IRequestHandler<ObterImportacaoXmlPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterImportacaoXmlPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterImportacaoXmlPorIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _context.ImportacoesXml
                .AsNoTracking()
                .Where(x => x.DeletadoEm == null && x.Id == request.Id)
                .Select(x => new
                {
                    x.Id,
                    x.EmpresaId,
                    x.Xml,
                    TipoDeXml = (int)x.TipoDeXml,
                    x.NfeId,
                    x.CodigoSefaz,
                    x.TipoEvento,
                    StatusImportacaoXml = (int)x.StatusImportacaoXml,
                    x.MensagemErroImportacaoXml,
                    StatusCadastro = (int)x.StatusCadastro,
                    x.MensagemErroCadastro,
                    StatusSalvarPdf = (int)x.StatusSalvarPdf,
                    x.MensagemErroSalvarPdf,
                    DataImportacao = x.CriadoEm
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null) return CommandResult.Falha("Importação de XML não encontrada.");
            return CommandResult.Ok("OK", item);
        }
    }
}
