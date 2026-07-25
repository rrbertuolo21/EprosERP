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
    public record ListarServicosQuery(
        string? Localizar = null,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterServicoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarServicosQueryHandler : IRequestHandler<ListarServicosQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ListarServicosQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarServicosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Servicos
                .AsNoTracking()
                .Include(s => s.CodigoServicoSefaz)
                .Where(s => s.DeletadoEm == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                query = query.Where(s => s.Codigo.Contains(request.Localizar) || s.Descricao.Contains(request.Localizar));
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(s => s.Codigo)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(s => new
                {
                    s.Id,
                    s.UnidadeMedidaId,
                    s.CodigoServicoSefazId,
                    CodigoServicoSefazDescricao = s.CodigoServicoSefaz.Descricao,
                    CodigoServicoSefazCodigo = s.CodigoServicoSefaz.Codigo,
                    s.Codigo,
                    s.Descricao,
                    s.Valor,
                    s.InformacaoAdicional,
                    s.ServicoAtivo,
                    s.Cnae,
                    s.CodigoNbs,
                    s.IndicadorIss,
                    s.IndicadorIncentivo,
                    s.CstIbsCbs,
                    s.CClassTrib,
                    s.AliquotaIss,
                    s.AliquotaIssRetido,
                    s.AliquotaIrrfRetido,
                    s.AliquotaInss,
                    s.CstPisCofins,
                    s.AliquotaPis,
                    s.AliquotaCofins,
                    s.CalcularRetencao,
                    s.AnexoSimplesNacional
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterServicoPorIdQueryHandler : IRequestHandler<ObterServicoPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;

        public ObterServicoPorIdQueryHandler(ContextFiscal context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterServicoPorIdQuery request, CancellationToken cancellationToken)
        {
            var s = await _context.Servicos
                .AsNoTracking()
                .Include(x => x.CodigoServicoSefaz)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);

            if (s == null)
            {
                return CommandResult.Falha("Serviço não encontrado.");
            }

            var dto = new
            {
                s.Id,
                s.UnidadeMedidaId,
                s.CodigoServicoSefazId,
                CodigoServicoSefazDescricao = s.CodigoServicoSefaz.Descricao,
                CodigoServicoSefazCodigo = s.CodigoServicoSefaz.Codigo,
                s.Codigo,
                s.Descricao,
                s.Valor,
                s.InformacaoAdicional,
                s.ServicoAtivo,
                s.Cnae,
                s.CodigoNbs,
                s.IndicadorIss,
                s.IndicadorIncentivo,
                s.CstIbsCbs,
                s.CClassTrib,
                s.AliquotaIss,
                s.AliquotaIssRetido,
                s.AliquotaIrrfRetido,
                s.AliquotaInss,
                s.CstPisCofins,
                s.AliquotaPis,
                s.AliquotaCofins,
                s.CalcularRetencao,
                s.AnexoSimplesNacional
            };

            return CommandResult.Ok("OK", dto);
        }
    }
}
