using System;
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
    public record ListarTiposOperacoesFiscaisQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterTipoOperacaoFiscalPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarTiposOperacoesFiscaisQueryHandler : IRequestHandler<ListarTiposOperacoesFiscaisQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ListarTiposOperacoesFiscaisQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarTiposOperacoesFiscaisQuery request, CancellationToken cancellationToken)
        {
            var query = _context.TiposOperacoesFiscais
                .AsNoTracking()
                .Include(t => t.CfopNfe)
                .Include(t => t.CfopNfce)
                .Where(t => t.DeletadoEm == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                query = query.Where(t => t.Descricao.Contains(request.Localizar));
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(t => t.Descricao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(t => new
                {
                    t.Id,
                    t.TributarioGrupoId,
                    t.CfopNfeId,
                    CfopNfeCodigo = t.CfopNfe != null ? (int?)t.CfopNfe.CfopCodigo : null,
                    CfopNfeDescricao = t.CfopNfe != null ? t.CfopNfe.Descricao : null,
                    t.CfopNfceId,
                    CfopNfceCodigo = t.CfopNfce != null ? (int?)t.CfopNfce.CfopCodigo : null,
                    CfopNfceDescricao = t.CfopNfce != null ? t.CfopNfce.Descricao : null,
                    t.Descricao,
                    t.SobescreveTributacaoNcm,
                    t.Finalidade,
                    t.Atendimento,
                    t.TipoFrete,
                    t.TipoMovimento
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterTipoOperacaoFiscalPorIdQueryHandler : IRequestHandler<ObterTipoOperacaoFiscalPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterTipoOperacaoFiscalPorIdQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterTipoOperacaoFiscalPorIdQuery request, CancellationToken cancellationToken)
        {
            var t = await _context.TiposOperacoesFiscais
                .AsNoTracking()
                .Include(x => x.CfopNfe)
                .Include(x => x.CfopNfce)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);

            if (t == null)
            {
                return CommandResult.Falha("Operação Fiscal não encontrada.");
            }

            var dto = new
            {
                t.Id,
                t.TributarioGrupoId,
                t.CfopNfeId,
                CfopNfeCodigo = t.CfopNfe != null ? (int?)t.CfopNfe.CfopCodigo : null,
                CfopNfeDescricao = t.CfopNfe != null ? t.CfopNfe.Descricao : null,
                t.CfopNfceId,
                CfopNfceCodigo = t.CfopNfce != null ? (int?)t.CfopNfce.CfopCodigo : null,
                CfopNfceDescricao = t.CfopNfce != null ? t.CfopNfce.Descricao : null,
                t.Descricao,
                t.SobescreveTributacaoNcm,
                t.Finalidade,
                t.Atendimento,
                t.TipoFrete,
                t.TipoMovimento
            };

            return CommandResult.Ok("OK", dto);
        }
    }
}
