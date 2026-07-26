using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Projetos.Application.Queries;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    // ===== PRJ-ENC =====
    public class ObterEncerramentosQueryHandler : IQueryHandler<ObterEncerramentosQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterEncerramentosQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterEncerramentosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Encerramentos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(e => e.Status.ToString() == request.Status);

            var dados = await query
                .OrderByDescending(e => e.DataCriacao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Encerramentos listados com sucesso!", dados);
        }
    }

    public class ObterEncerramentoPorIdQueryHandler : IQueryHandler<ObterEncerramentoPorIdQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterEncerramentoPorIdQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterEncerramentoPorIdQuery request, CancellationToken cancellationToken)
        {
            var enc = await _context.Encerramentos
                .Include(e => e.Itens)
                .Include(e => e.Anexos)
                .Include(e => e.Historicos)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            if (enc == null) return CommandResult.Falha("Encerramento nao encontrado.");
            return CommandResult.Ok("Encerramento carregado com sucesso!", enc);
        }
    }

    public class ObterEncerramentosPorProjetoQueryHandler : IQueryHandler<ObterEncerramentosPorProjetoQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterEncerramentosPorProjetoQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterEncerramentosPorProjetoQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.Encerramentos
                .Where(e => e.ProjetoId == request.ProjetoId)
                .OrderByDescending(e => e.DataCriacao)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Encerramentos do projeto listados com sucesso!", dados);
        }
    }

    // ===== PRJ-RSK =====
    public class ObterRiscosPorProjetoQueryHandler : IQueryHandler<ObterRiscosPorProjetoQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterRiscosPorProjetoQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterRiscosPorProjetoQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Riscos.Where(r => r.ProjetoId == request.ProjetoId);
            if (!string.IsNullOrWhiteSpace(request.Prioridade))
                query = query.Where(r => r.Prioridade.ToString() == request.Prioridade);

            var dados = await query.OrderByDescending(r => r.CriadoEm).ToListAsync(cancellationToken);
            return CommandResult.Ok("Riscos listados com sucesso!", dados);
        }
    }

    public class ObterRiscoPorIdQueryHandler : IQueryHandler<ObterRiscoPorIdQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterRiscoPorIdQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterRiscoPorIdQuery request, CancellationToken cancellationToken)
        {
            var risco = await _context.Riscos
                .Include(r => r.Responsaveis)
                .Include(r => r.Comentarios)
                .Include(r => r.Historicos)
                .Include(r => r.Anexos)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            if (risco == null) return CommandResult.Falha("Risco nao encontrado.");
            return CommandResult.Ok("Risco carregado com sucesso!", risco);
        }
    }

    public class ObterEstagiosRiscoQueryHandler : IQueryHandler<ObterEstagiosRiscoQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterEstagiosRiscoQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterEstagiosRiscoQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.EstagiosRisco.OrderBy(e => e.Ordem).ToListAsync(cancellationToken);
            return CommandResult.Ok("Estagios listados com sucesso!", dados);
        }
    }

    // ===== PRJ-PRT =====
    public class ObterPortfoliosQueryHandler : IQueryHandler<ObterPortfoliosQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterPortfoliosQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterPortfoliosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Portfolios.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(p => p.Status.ToString() == request.Status);

            var dados = await query
                .OrderByDescending(p => p.DataCriacao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Portfolios listados com sucesso!", dados);
        }
    }

    public class ObterPortfolioPorIdQueryHandler : IQueryHandler<ObterPortfolioPorIdQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterPortfolioPorIdQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterPortfolioPorIdQuery request, CancellationToken cancellationToken)
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.Itens)
                .Include(p => p.Anexos)
                .Include(p => p.Historicos)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (portfolio == null) return CommandResult.Falha("Portfolio nao encontrado.");
            return CommandResult.Ok("Portfolio carregado com sucesso!", portfolio);
        }
    }
}
