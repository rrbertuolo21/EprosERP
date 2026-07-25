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
    // ===== PRJ-ORC =====
    public class ObterOrcamentosPorProjetoQueryHandler : IQueryHandler<ObterOrcamentosPorProjetoQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterOrcamentosPorProjetoQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterOrcamentosPorProjetoQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.Orcamentos
                .Where(o => o.ProjetoId == request.ProjetoId)
                .OrderByDescending(o => o.CriadoEm)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Orcamentos listados com sucesso!", dados);
        }
    }

    public class ObterOrcamentoPorIdQueryHandler : IQueryHandler<ObterOrcamentoPorIdQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterOrcamentoPorIdQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterOrcamentoPorIdQuery request, CancellationToken cancellationToken)
        {
            var orcamento = await _context.Orcamentos
                .Include(o => o.Marcos)
                .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
            if (orcamento == null)
                return CommandResult.Falha("Orcamento nao encontrado.");
            return CommandResult.Ok("Orcamento carregado com sucesso!", orcamento);
        }
    }

    // ===== PRJ-REC =====
    public class ObterApontamentosQueryHandler : IQueryHandler<ObterApontamentosQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterApontamentosQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterApontamentosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.RecursoTimesheets.AsQueryable();
            if (request.ProjetoId.HasValue)
                query = query.Where(t => t.ProjetoId == request.ProjetoId);
            if (request.UsuarioId.HasValue)
                query = query.Where(t => t.UsuarioId == request.UsuarioId);

            var dados = await query.OrderByDescending(t => t.Data).ToListAsync(cancellationToken);
            return CommandResult.Ok("Apontamentos listados com sucesso!", dados);
        }
    }

    public class ObterAlocacoesRecursoQueryHandler : IQueryHandler<ObterAlocacoesRecursoQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterAlocacoesRecursoQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterAlocacoesRecursoQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.RecursoAlocacoes
                .Where(a => a.ProjetoId == request.ProjetoId)
                .OrderByDescending(a => a.CriadoEm)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Alocacoes listadas com sucesso!", dados);
        }
    }

    // ===== PRJ-RST =====
    public class ObterTarefasPorProjetoQueryHandler : IQueryHandler<ObterTarefasPorProjetoQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterTarefasPorProjetoQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterTarefasPorProjetoQuery request, CancellationToken cancellationToken)
        {
            var dados = await _context.TarefasProjeto
                .Where(t => t.ProjetoId == request.ProjetoId && t.Ativo)
                .OrderBy(t => t.Ordem)
                .ToListAsync(cancellationToken);
            return CommandResult.Ok("Tarefas listadas com sucesso!", dados);
        }
    }

    public class ObterTarefaPorIdQueryHandler : IQueryHandler<ObterTarefaPorIdQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterTarefaPorIdQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterTarefaPorIdQuery request, CancellationToken cancellationToken)
        {
            var tarefa = await _context.TarefasProjeto.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
            if (tarefa == null)
                return CommandResult.Falha("Tarefa nao encontrada.");
            return CommandResult.Ok("Tarefa carregada com sucesso!", tarefa);
        }
    }

    // ===== PRJ-FAT =====
    public class ObterFaturamentosQueryHandler : IQueryHandler<ObterFaturamentosQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterFaturamentosQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterFaturamentosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Faturamentos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(f => f.Status.ToString() == request.Status);

            var dados = await query
                .OrderByDescending(f => f.DataCriacao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Faturamentos listados com sucesso!", dados);
        }
    }

    public class ObterFaturamentoPorIdQueryHandler : IQueryHandler<ObterFaturamentoPorIdQuery, CommandResult>
    {
        private readonly ContextProjetos _context;
        public ObterFaturamentoPorIdQueryHandler(ContextProjetos context) => _context = context;

        public async Task<CommandResult> Handle(ObterFaturamentoPorIdQuery request, CancellationToken cancellationToken)
        {
            var faturamento = await _context.Faturamentos
                .Include(f => f.Itens)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
            if (faturamento == null)
                return CommandResult.Falha("Faturamento nao encontrado.");
            return CommandResult.Ok("Faturamento carregado com sucesso!", faturamento);
        }
    }
}
