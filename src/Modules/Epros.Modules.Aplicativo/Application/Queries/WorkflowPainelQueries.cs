using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    // ---------- DTOs ----------

    public record WfAgendamentoDto(Guid Id, string Nome, string ExpressaoIntervalar, bool Ativo, DateTime? ProximaExecucaoEm);

    public record WfJobDto(Guid Id, Guid AgendamentoId, string Nome, EWfJobStatus Status, int TentativaAtual, DateTime PrevistoPara, DateTime? IniciadoEm, DateTime? FinalizadoEm, string? Log);

    public record WfJobTentativaDto(Guid Id, Guid JobId, int NumeroTentativa, EWfJobTentativaStatus Status, string? Mensagem, DateTime? IniciadoEm, DateTime? FinalizadoEm);

    /// <summary>Consolidação por status para o painel gestor / relatório de posição geral (§7.6.5-6).</summary>
    public record WfPainelDto(Dictionary<string, int> InstanciasPorStatus, Dictionary<string, int> JobsPorStatus, int AgendamentosAtivos);

    // ---------- Queries ----------

    public record ListarWfAgendamentosQuery(bool? Ativo) : IQuery<IEnumerable<WfAgendamentoDto>>;

    public record ListarWfJobsQuery(Guid? AgendamentoId, EWfJobStatus? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<IEnumerable<WfJobDto>>;

    public record ListarWfJobTentativasQuery(Guid JobId) : IQuery<IEnumerable<WfJobTentativaDto>>;

    public record PainelWorkflowQuery() : IQuery<WfPainelDto>;

    // ---------- Handlers ----------

    public class ListarWfAgendamentosQueryHandler : IQueryHandler<ListarWfAgendamentosQuery, IEnumerable<WfAgendamentoDto>>
    {
        private readonly ContextAplicativo _context;
        public ListarWfAgendamentosQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IEnumerable<WfAgendamentoDto>> Handle(ListarWfAgendamentosQuery request, CancellationToken ct)
        {
            var q = _context.WfAgendamentos.AsNoTracking().Where(a => a.DeletadoEm == null);
            if (request.Ativo.HasValue) q = q.Where(a => a.Ativo == request.Ativo.Value);

            return await q.OrderBy(a => a.Nome)
                .Select(a => new WfAgendamentoDto(a.Id, a.Nome, a.ExpressaoIntervalar, a.Ativo, a.ProximaExecucaoEm))
                .ToListAsync(ct);
        }
    }

    public class ListarWfJobsQueryHandler : IQueryHandler<ListarWfJobsQuery, IEnumerable<WfJobDto>>
    {
        private readonly ContextAplicativo _context;
        public ListarWfJobsQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IEnumerable<WfJobDto>> Handle(ListarWfJobsQuery request, CancellationToken ct)
        {
            var q = _context.WfJobs.AsNoTracking().Where(j => j.DeletadoEm == null);
            if (request.AgendamentoId.HasValue) q = q.Where(j => j.AgendamentoId == request.AgendamentoId);
            if (request.Status.HasValue) q = q.Where(j => j.Status == request.Status.Value);

            var pagina = request.Pagina < 1 ? 1 : request.Pagina;
            var tamanho = request.TamanhoPagina < 1 ? 20 : request.TamanhoPagina;

            return await q.OrderByDescending(j => j.PrevistoPara)
                .Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(j => new WfJobDto(j.Id, j.AgendamentoId, j.Nome, j.Status, j.TentativaAtual, j.PrevistoPara, j.IniciadoEm, j.FinalizadoEm, j.Log))
                .ToListAsync(ct);
        }
    }

    public class ListarWfJobTentativasQueryHandler : IQueryHandler<ListarWfJobTentativasQuery, IEnumerable<WfJobTentativaDto>>
    {
        private readonly ContextAplicativo _context;
        public ListarWfJobTentativasQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<IEnumerable<WfJobTentativaDto>> Handle(ListarWfJobTentativasQuery request, CancellationToken ct)
        {
            return await _context.WfJobTentativas.AsNoTracking()
                .Where(t => t.JobId == request.JobId && t.DeletadoEm == null)
                .OrderBy(t => t.NumeroTentativa)
                .Select(t => new WfJobTentativaDto(t.Id, t.JobId, t.NumeroTentativa, t.Status, t.Mensagem, t.IniciadoEm, t.FinalizadoEm))
                .ToListAsync(ct);
        }
    }

    public class PainelWorkflowQueryHandler : IQueryHandler<PainelWorkflowQuery, WfPainelDto>
    {
        private readonly ContextAplicativo _context;
        public PainelWorkflowQueryHandler(ContextAplicativo context) => _context = context;

        public async Task<WfPainelDto> Handle(PainelWorkflowQuery request, CancellationToken ct)
        {
            var instancias = await _context.WfInstancias.AsNoTracking().Where(i => i.DeletadoEm == null)
                .GroupBy(i => i.Status).Select(g => new { g.Key, Total = g.Count() }).ToListAsync(ct);

            var jobs = await _context.WfJobs.AsNoTracking().Where(j => j.DeletadoEm == null)
                .GroupBy(j => j.Status).Select(g => new { g.Key, Total = g.Count() }).ToListAsync(ct);

            var agendasAtivas = await _context.WfAgendamentos.AsNoTracking().CountAsync(a => a.DeletadoEm == null && a.Ativo, ct);

            return new WfPainelDto(
                instancias.ToDictionary(x => x.Key.ToString(), x => x.Total),
                jobs.ToDictionary(x => x.Key.ToString(), x => x.Total),
                agendasAtivas);
        }
    }
}
