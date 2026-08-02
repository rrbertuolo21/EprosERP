using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Domain.Entities.Orcamento;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    public class CriarOrcamentoProjetoCommandHandler : ICommandHandler<CriarOrcamentoProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarOrcamentoProjetoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarOrcamentoProjetoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var orcamento = new OrcamentoProjeto(
                request.ProjetoId,
                request.Budget,
                request.BillingType,
                request.BillingRate,
                request.EstimatedHours,
                request.CostsEstimate,
                tenantId,
                usuario);

            if (!orcamento.IsValid)
                return CommandResult.Falha(orcamento.Notifications.Select(n => n.Message));

            _context.Orcamentos.Add(orcamento);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Orcamento criado com sucesso!", new { orcamento.Id });
        }
    }

    public class AdicionarMarcoOrcamentarioCommandHandler : ICommandHandler<AdicionarMarcoOrcamentarioCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarMarcoOrcamentarioCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarMarcoOrcamentarioCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var orcamento = await _context.Orcamentos
                .Include(o => o.Marcos)
                .FirstOrDefaultAsync(o => o.Id == request.OrcamentoProjetoId, cancellationToken);

            if (orcamento == null)
                return CommandResult.Falha("Orcamento nao encontrado.");

            orcamento.AdicionarMarco(request.Titulo, request.Custo, request.DataInicio, request.DataFim, request.Resumo, usuario);
            if (!orcamento.IsValid)
                return CommandResult.Falha(orcamento.Notifications.Select(n => n.Message));

            var novoMarco = orcamento.Marcos.Last();
            _context.Entry(novoMarco).State = EntityState.Added;
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Marco adicionado com sucesso!", new { MarcoId = novoMarco.Id });
        }
    }

    public class AtualizarProgressoMarcoCommandHandler : ICommandHandler<AtualizarProgressoMarcoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarProgressoMarcoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarProgressoMarcoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var marco = await _context.MarcosOrcamentarios
                .FirstOrDefaultAsync(m => m.Id == request.MarcoId && m.OrcamentoProjetoId == request.OrcamentoProjetoId, cancellationToken);

            if (marco == null)
                return CommandResult.Falha("Marco nao encontrado.");

            marco.AtualizarProgresso(request.Progresso, request.Status, usuario);
            if (!marco.IsValid)
                return CommandResult.Falha(marco.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Progresso do marco atualizado com sucesso!", new { marco.Id, marco.Progresso });
        }
    }

    public class SubmeterOrcamentoProjetoCommandHandler : ICommandHandler<SubmeterOrcamentoProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public SubmeterOrcamentoProjetoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SubmeterOrcamentoProjetoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var orcamento = await _context.Orcamentos.FirstOrDefaultAsync(o => o.Id == request.OrcamentoProjetoId, cancellationToken);
            if (orcamento == null)
                return CommandResult.Falha("Orcamento nao encontrado.");

            orcamento.Submeter(usuario);
            if (!orcamento.IsValid)
                return CommandResult.Falha(orcamento.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Orcamento submetido para aprovacao.", new { orcamento.Id });
        }
    }

    public class AprovarOrcamentoProjetoCommandHandler : ICommandHandler<AprovarOrcamentoProjetoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AprovarOrcamentoProjetoCommandHandler(ContextProjetos context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarOrcamentoProjetoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";
            var orcamento = await _context.Orcamentos.FirstOrDefaultAsync(o => o.Id == request.OrcamentoProjetoId, cancellationToken);
            if (orcamento == null)
                return CommandResult.Falha("Orcamento nao encontrado.");

            orcamento.Aprovar(usuario);
            if (!orcamento.IsValid)
                return CommandResult.Falha(orcamento.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Orcamento aprovado com sucesso!", new { orcamento.Id });
        }
    }

    /// <summary>DP-ORC-002: congela baseline imutável do orçamento aprovado e publica evento no Outbox.</summary>
    public class CongelarBaselineOrcamentoCommandHandler : ICommandHandler<CongelarBaselineOrcamentoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CongelarBaselineOrcamentoCommandHandler(ContextProjetos context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CongelarBaselineOrcamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var orcamento = await _context.Orcamentos
                .Include(o => o.Marcos)
                .FirstOrDefaultAsync(o => o.Id == request.OrcamentoProjetoId, cancellationToken);

            if (orcamento == null)
                return CommandResult.Falha("Orcamento nao encontrado.");

            var marcosSnapshot = orcamento.Marcos
                .Select(m => new { m.Id, m.Titulo, m.Custo, m.DataInicio, m.DataFim, m.Progresso, Status = m.Status.ToString() })
                .ToList();
            var marcosJson = JsonSerializer.Serialize(marcosSnapshot);

            var baseline = orcamento.CongelarBaseline(marcosJson, request.Motivo, usuario);
            if (!orcamento.IsValid || baseline == null)
                return CommandResult.Falha(orcamento.Notifications.Select(n => n.Message));

            _context.BaselinesOrcamento.Add(baseline);

            var payload = JsonSerializer.Serialize(new
            {
                baseline.OrcamentoProjetoId,
                baseline.ProjetoId,
                baseline.NumeroBaseline,
                baseline.BudgetSnapshot,
                baseline.CustoMarcosTotal
            });
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Projetos.OrcamentoBaselineCongelada, payload));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Baseline congelada com sucesso!", new
            {
                BaselineId = baseline.Id,
                baseline.NumeroBaseline,
                baseline.BudgetSnapshot,
                baseline.CustoMarcosTotal
            });
        }
    }
}
