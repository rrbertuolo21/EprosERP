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
    public record ListarCfopsQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterCfopPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarCfopsQueryHandler : IRequestHandler<ListarCfopsQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ListarCfopsQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarCfopsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Cfops.AsNoTracking().Where(c => c.DeletadoEm == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                query = query.Where(c => c.Descricao.Contains(request.Localizar) || c.CfopCodigo.ToString().Contains(request.Localizar));
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(c => c.CfopCodigo)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new
                {
                    c.Id,
                    c.CfopCodigo,
                    c.Descricao,
                    c.NaturezaOperacao,
                    c.CfopCorrelacao,
                    c.IntegraFaturamento,
                    c.IndicadorNfe,
                    c.IndicadorComunicacao,
                    c.IndicadorTransporte,
                    c.IndicadorDevolucao,
                    c.IndicadorRetorno,
                    c.IndicadorAnulacao,
                    c.IndicadorRemessa,
                    c.IndicadorCombustivel,
                    c.IndicadorTransferencia,
                    c.IndicadorNfce,
                    c.IndicadorCiap,
                    c.IndicadorUsoConsumo,
                    c.IndicadorUsoSemOperacao,
                    c.IndicadorSt,
                    c.IndicadorMei,
                    c.IncidenciaSimples,
                    c.CfopDevolucao
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterCfopPorIdQueryHandler : IRequestHandler<ObterCfopPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterCfopPorIdQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterCfopPorIdQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.Cfops.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (c == null)
            {
                return CommandResult.Falha("CFOP não encontrado.");
            }

            var dto = new
            {
                c.Id,
                c.CfopCodigo,
                c.Descricao,
                c.NaturezaOperacao,
                c.CfopCorrelacao,
                c.IntegraFaturamento,
                c.IndicadorNfe,
                c.IndicadorComunicacao,
                c.IndicadorTransporte,
                c.IndicadorDevolucao,
                c.IndicadorRetorno,
                c.IndicadorAnulacao,
                c.IndicadorRemessa,
                c.IndicadorCombustivel,
                c.IndicadorTransferencia,
                c.IndicadorNfce,
                c.IndicadorCiap,
                c.IndicadorUsoConsumo,
                c.IndicadorUsoSemOperacao,
                c.IndicadorSt,
                c.IndicadorMei,
                c.IncidenciaSimples,
                c.CfopDevolucao
            };

            return CommandResult.Ok("OK", dto);
        }
    }
}
