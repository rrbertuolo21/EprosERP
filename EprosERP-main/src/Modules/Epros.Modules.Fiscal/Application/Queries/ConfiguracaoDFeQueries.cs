using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Queries
{
    public record ListarConfiguracoesDFeQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterConfiguracaoDFePorIdQuery(Guid Id) : IQuery<CommandResult>;
    public record ObterConfiguracaoDFePorEmpresaQuery(Guid EmpresaId) : IQuery<CommandResult>;

    public class ListarConfiguracoesDFeQueryHandler : IRequestHandler<ListarConfiguracoesDFeQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarConfiguracoesDFeQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarConfiguracoesDFeQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ConfiguracoesDFe.AsNoTracking().Where(c => c.DeletadoEm == null).AsQueryable();

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(c => c.EmpresaId)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new { c.Id, c.EmpresaId, c.NFeSerieProducao, c.NfceSerieProducao })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterConfiguracaoDFePorIdQueryHandler : IRequestHandler<ObterConfiguracaoDFePorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterConfiguracaoDFePorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterConfiguracaoDFePorIdQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.ConfiguracoesDFe.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (c == null) return CommandResult.Falha("Configuração DF-e não encontrada.");
            return CommandResult.Ok("OK", ProjetarDto(c));
        }

        internal static object ProjetarDto(Domain.Entities.ConfiguracaoDFe c) => new
        {
            c.Id,
            c.EmpresaId,
            c.NFeSerieProducao, c.NFeUltimoNrProducao, c.NFeSerieHomologacao, c.NFeUltimoNrHomologacao,
            c.NfceCscProducao, c.NfceIdCscProducao, c.NfceSerieProducao, c.NfceUltimoNrProducao,
            c.NfceCscHomologacao, c.NfceIdCscHomologacao, c.NfceSerieHomologacao, c.NfceUltimoNrHomologacao
        };
    }

    public class ObterConfiguracaoDFePorEmpresaQueryHandler : IRequestHandler<ObterConfiguracaoDFePorEmpresaQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterConfiguracaoDFePorEmpresaQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterConfiguracaoDFePorEmpresaQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.ConfiguracoesDFe.AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmpresaId == request.EmpresaId && x.DeletadoEm == null, cancellationToken);
            if (c == null) return CommandResult.Falha("Configuração DF-e não encontrada.");
            return CommandResult.Ok("OK", ObterConfiguracaoDFePorIdQueryHandler.ProjetarDto(c));
        }
    }
}
