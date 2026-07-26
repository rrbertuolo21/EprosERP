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
    public record ListarCfopPadroesQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterCfopPadraoPorCodigoQuery(int CfopCodigo) : IQuery<CommandResult>;

    public class ListarCfopPadroesQueryHandler : IRequestHandler<ListarCfopPadroesQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ListarCfopPadroesQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarCfopPadroesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CfopPadroes.AsNoTracking().Where(c => c.DeletadoEm == null).AsQueryable();

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
                    c.DataInicioVigencia,
                    c.DataFimVigencia,
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

    public class ObterCfopPadraoPorCodigoQueryHandler : IRequestHandler<ObterCfopPadraoPorCodigoQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterCfopPadraoPorCodigoQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterCfopPadraoPorCodigoQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.CfopPadroes.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CfopCodigo == request.CfopCodigo && x.DeletadoEm == null, cancellationToken);

            if (c == null)
            {
                return CommandResult.Falha("CFOP Padrão não encontrado.");
            }

            var dto = new
            {
                c.Id,
                c.CfopCodigo,
                c.DataInicioVigencia,
                c.DataFimVigencia,
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
