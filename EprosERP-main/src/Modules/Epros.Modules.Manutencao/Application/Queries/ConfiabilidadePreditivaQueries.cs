using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Queries
{
    // ===================== MAN-CRV =====================
    public record ListarRevisoesConfiabilidadeQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterRevisaoConfiabilidadePorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarRevisoesConfiabilidadeQueryHandler : IQueryHandler<ListarRevisoesConfiabilidadeQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ListarRevisoesConfiabilidadeQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ListarRevisoesConfiabilidadeQuery request, CancellationToken cancellationToken)
        {
            var query = _context.RevisoesConfiabilidade.AsNoTracking().OrderBy(r => r.Codigo);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Revisoes de confiabilidade listadas.", new { total, itens });
        }
    }

    public class ObterRevisaoConfiabilidadePorIdQueryHandler : IQueryHandler<ObterRevisaoConfiabilidadePorIdQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ObterRevisaoConfiabilidadePorIdQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ObterRevisaoConfiabilidadePorIdQuery request, CancellationToken cancellationToken)
        {
            var revisao = await _context.RevisoesConfiabilidade.AsNoTracking()
                .Include(r => r.ModosFalha)
                .Include(r => r.Indicadores)
                .Include(r => r.Recomendacoes)
                .Include(r => r.Anexos)
                .Include(r => r.Historicos)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            return revisao == null ? CommandResult.Falha("Revisao de confiabilidade nao encontrada.") : CommandResult.Ok("Revisao encontrada.", revisao);
        }
    }

    // ===================== MAN-PDT =====================
    public record ListarMonitoramentosPreditivosQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterMonitoramentoPreditivoPorIdQuery(Guid Id) : IQuery<CommandResult>;
    public record ListarAlarmesPreditivosQuery(Guid? MonitoramentoId, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarMonitoramentosPreditivosQueryHandler : IQueryHandler<ListarMonitoramentosPreditivosQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ListarMonitoramentosPreditivosQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ListarMonitoramentosPreditivosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.MonitoramentosPreditivos.AsNoTracking().OrderBy(m => m.Codigo);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Monitoramentos preditivos listados.", new { total, itens });
        }
    }

    public class ObterMonitoramentoPreditivoPorIdQueryHandler : IQueryHandler<ObterMonitoramentoPreditivoPorIdQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ObterMonitoramentoPreditivoPorIdQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ObterMonitoramentoPreditivoPorIdQuery request, CancellationToken cancellationToken)
        {
            var monitoramento = await _context.MonitoramentosPreditivos.AsNoTracking()
                .Include(m => m.Itens)
                .Include(m => m.PontosMedicao)
                .Include(m => m.Alarmes)
                .Include(m => m.Anexos)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            return monitoramento == null ? CommandResult.Falha("Monitoramento preditivo nao encontrado.") : CommandResult.Ok("Monitoramento encontrado.", monitoramento);
        }
    }

    public class ListarAlarmesPreditivosQueryHandler : IQueryHandler<ListarAlarmesPreditivosQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ListarAlarmesPreditivosQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ListarAlarmesPreditivosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.AlarmesPreditivos.AsNoTracking();
            if (request.MonitoramentoId.HasValue)
                query = query.Where(a => a.MonitoramentoId == request.MonitoramentoId.Value);

            var ordenada = query.OrderByDescending(a => a.DataHoraDisparo);
            var total = await ordenada.CountAsync(cancellationToken);
            var itens = await ordenada
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Alarmes preditivos listados.", new { total, itens });
        }
    }
}
