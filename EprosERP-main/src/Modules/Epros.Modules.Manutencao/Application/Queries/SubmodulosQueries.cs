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
    // ===================== MAN-PRV =====================
    public record ListarPlanosPreventivosQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterPlanoPreventivoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarPlanosPreventivosQueryHandler : IQueryHandler<ListarPlanosPreventivosQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ListarPlanosPreventivosQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ListarPlanosPreventivosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PlanosPreventivos.AsNoTracking().OrderBy(p => p.Codigo);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Planos preventivos listados.", new { total, itens });
        }
    }

    public class ObterPlanoPreventivoPorIdQueryHandler : IQueryHandler<ObterPlanoPreventivoPorIdQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ObterPlanoPreventivoPorIdQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ObterPlanoPreventivoPorIdQuery request, CancellationToken cancellationToken)
        {
            var plano = await _context.PlanosPreventivos.AsNoTracking()
                .Include(p => p.Periodicidades)
                .Include(p => p.ChecklistItens)
                .Include(p => p.KitPecas)
                .Include(p => p.Execucoes)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            return plano == null ? CommandResult.Falha("Plano preventivo nao encontrado.") : CommandResult.Ok("Plano encontrado.", plano);
        }
    }

    // ===================== MAN-TRB =====================
    public record ListarOrdensServicoQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterOrdemServicoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarOrdensServicoQueryHandler : IQueryHandler<ListarOrdensServicoQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ListarOrdensServicoQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ListarOrdensServicoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.OrdensServico.AsNoTracking().OrderByDescending(o => o.Data);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Ordens de servico listadas.", new { total, itens });
        }
    }

    public class ObterOrdemServicoPorIdQueryHandler : IQueryHandler<ObterOrdemServicoPorIdQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ObterOrdemServicoPorIdQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ObterOrdemServicoPorIdQuery request, CancellationToken cancellationToken)
        {
            var os = await _context.OrdensServico.AsNoTracking()
                .Include(o => o.Itens)
                .Include(o => o.Evolucoes)
                .Include(o => o.Equipamentos)
                .Include(o => o.VinculosFinanceiros)
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
            return os == null ? CommandResult.Falha("Ordem de servico nao encontrada.") : CommandResult.Ok("OS encontrada.", os);
        }
    }

    // ===================== MAN-PEC =====================
    public record ListarRegistrosPecaReposicaoQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterRegistroPecaReposicaoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarRegistrosPecaReposicaoQueryHandler : IQueryHandler<ListarRegistrosPecaReposicaoQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ListarRegistrosPecaReposicaoQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ListarRegistrosPecaReposicaoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.RegistrosPecaReposicao.AsNoTracking().OrderBy(r => r.Codigo);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Registros de pecas listados.", new { total, itens });
        }
    }

    public class ObterRegistroPecaReposicaoPorIdQueryHandler : IQueryHandler<ObterRegistroPecaReposicaoPorIdQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ObterRegistroPecaReposicaoPorIdQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ObterRegistroPecaReposicaoPorIdQuery request, CancellationToken cancellationToken)
        {
            var registro = await _context.RegistrosPecaReposicao.AsNoTracking()
                .Include(r => r.Itens)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            return registro == null ? CommandResult.Falha("Registro de pecas nao encontrado.") : CommandResult.Ok("Registro encontrado.", registro);
        }
    }

    // ===================== MAN-PAR =====================
    public record ListarParadasQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterParadaPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarParadasQueryHandler : IQueryHandler<ListarParadasQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ListarParadasQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ListarParadasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Paradas.AsNoTracking().OrderByDescending(p => p.DataHoraInicio);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Paradas listadas.", new { total, itens });
        }
    }

    public class ObterParadaPorIdQueryHandler : IQueryHandler<ObterParadaPorIdQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ObterParadaPorIdQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ObterParadaPorIdQuery request, CancellationToken cancellationToken)
        {
            var parada = await _context.Paradas.AsNoTracking()
                .Include(p => p.Itens)
                .Include(p => p.VinculosOs)
                .Include(p => p.Indicadores)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            return parada == null ? CommandResult.Falha("Parada nao encontrada.") : CommandResult.Ok("Parada encontrada.", parada);
        }
    }

    // ===================== MAN-IND =====================
    public record ListarInducoesQuery(Guid? EquipamentoId, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public class ListarInducoesQueryHandler : IQueryHandler<ListarInducoesQuery, CommandResult>
    {
        private readonly ContextManutencao _context;
        public ListarInducoesQueryHandler(ContextManutencao context) => _context = context;

        public async Task<CommandResult> Handle(ListarInducoesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.EquipamentoInducoes.AsNoTracking();
            if (request.EquipamentoId.HasValue)
                query = query.Where(i => i.EquipamentoId == request.EquipamentoId.Value);

            var ordenada = query.OrderByDescending(i => i.CriadoEm);
            var total = await ordenada.CountAsync(cancellationToken);
            var itens = await ordenada
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Inducoes listadas.", new { total, itens });
        }
    }
}
