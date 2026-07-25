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
    /// <summary>
    /// Lista os CFOPs adequados a um tipo de operação (0 = Entrada, 1 = Saída) para as telas de
    /// Venda/Compra (<c>obter-cfops</c>). Saída = CFOPs iniciados em 5/6/7; Entrada = 1/2/3.
    /// </summary>
    public record ListarCfopsPorTipoOperacaoQuery(int TipoOperacao) : IQuery<CommandResult>;

    public class ListarCfopsPorTipoOperacaoQueryHandler : IRequestHandler<ListarCfopsPorTipoOperacaoQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ListarCfopsPorTipoOperacaoQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarCfopsPorTipoOperacaoQuery request, CancellationToken cancellationToken)
        {
            // Saída (1) -> primeiro dígito 5,6,7 ; Entrada (0) -> 1,2,3.
            var saida = request.TipoOperacao == 1;
            var min = saida ? 5000 : 1000;
            var max = saida ? 7999 : 3999;

            var itens = await _context.Cfops
                .AsNoTracking()
                .Where(c => c.DeletadoEm == null && c.CfopCodigo >= min && c.CfopCodigo <= max)
                .OrderBy(c => c.CfopCodigo)
                .Select(c => new
                {
                    c.Id,
                    c.CfopCodigo,
                    c.Descricao,
                    c.NaturezaOperacao,
                    c.CfopCorrelacao,
                    c.IndicadorCombustivel,
                    c.IntegraFaturamento
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = itens.Count, Itens = itens });
        }
    }
}
