using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    public class CriarDemandaPrevisaoCommandHandler : ICommandHandler<CriarDemandaPrevisaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarDemandaPrevisaoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarDemandaPrevisaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var previsao = new DemandaPrevisao(request.EmpresaId, request.Codigo, request.Nome, request.PeriodoInicio, request.PeriodoFim, request.Observacoes, tenantId, usuario);
            if (!previsao.IsValid) return CommandResult.Falha(previsao.Notifications.Select(n => n.Message), "Dados da previsão inválidos.");
            _context.DemandaPrevisoes.Add(previsao);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Previsão de demanda criada com sucesso!", new { previsao.Id, Status = previsao.Status.ToString() });
        }
    }

    public class AdicionarDemandaItemCommandHandler : ICommandHandler<AdicionarDemandaItemCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AdicionarDemandaItemCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarDemandaItemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var previsao = await _context.DemandaPrevisoes.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.PrevisaoId, cancellationToken);
            if (previsao == null) return CommandResult.Falha("Previsão não encontrada.");

            var item = new DemandaItem(request.PrevisaoId, request.CenarioId, request.VersaoId, request.ProdutoId, request.Periodo,
                request.QuantidadeHistorica, request.QuantidadePrevista, request.UnidadeMedidaId, request.Observacoes, tenantId, usuario);
            if (!item.IsValid) return CommandResult.Falha(item.Notifications.Select(n => n.Message), "Dados do item inválidos.");
            _context.DemandaItens.Add(item);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item de previsão adicionado.", new { item.Id });
        }
    }

    public class AjustarDemandaItemCommandHandler : ICommandHandler<AjustarDemandaItemCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AjustarDemandaItemCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AjustarDemandaItemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var item = await _context.DemandaItens.FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == request.ItemId, cancellationToken);
            if (item == null) return CommandResult.Falha("Item de previsão não encontrado.");
            item.Ajustar(request.QuantidadeAjustada, usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item ajustado.", new { item.Id, item.QuantidadeAjustada });
        }
    }

    public class CriarDemandaCenarioCommandHandler : ICommandHandler<CriarDemandaCenarioCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarDemandaCenarioCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarDemandaCenarioCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var previsao = await _context.DemandaPrevisoes.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.PrevisaoId, cancellationToken);
            if (previsao == null) return CommandResult.Falha("Previsão não encontrada.");
            var cenario = new DemandaCenario(request.PrevisaoId, request.Nome, request.Tipo, request.Descricao, tenantId, usuario);
            if (!cenario.IsValid) return CommandResult.Falha(cenario.Notifications.Select(n => n.Message), "Dados do cenário inválidos.");
            _context.DemandaCenarios.Add(cenario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Cenário criado.", new { cenario.Id });
        }
    }

    public class AprovarDemandaPrevisaoCommandHandler : ICommandHandler<AprovarDemandaPrevisaoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AprovarDemandaPrevisaoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AprovarDemandaPrevisaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var previsao = await _context.DemandaPrevisoes.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.PrevisaoId, cancellationToken);
            if (previsao == null) return CommandResult.Falha("Previsão não encontrada.");

            // §16: publicar demanda exige previsão aprovada com itens (produto + quantidade).
            var temItens = await _context.DemandaItens.AnyAsync(i => i.TenantId == tenantId && i.PrevisaoId == previsao.Id, cancellationToken);
            if (!temItens) return CommandResult.Falha("A previsão deve possuir itens antes de ser aprovada.");

            previsao.Aprovar(request.CenarioVigenteId, request.VersaoVigenteId, usuario);
            _context.DemandaConsensos.Add(new DemandaConsenso(previsao.Id, request.CenarioVigenteId, request.VersaoVigenteId, "Aprovado", null, System.DateTime.UtcNow, request.Observacoes, tenantId, usuario));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Previsão aprovada.", new { previsao.Id, Status = previsao.Status.ToString() });
        }
    }

    public class PublicarDemandaCommandHandler : ICommandHandler<PublicarDemandaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public PublicarDemandaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context; _tenantProvider = tenantProvider; _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(PublicarDemandaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";
            var previsao = await _context.DemandaPrevisoes.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == request.PrevisaoId, cancellationToken);
            if (previsao == null) return CommandResult.Falha("Previsão não encontrada.");

            // §16 / critério 8: bloqueia publicação de previsão não aprovada.
            if (previsao.Status != EDemandaStatus.Aprovado)
                return CommandResult.Falha("Somente previsões aprovadas podem ser publicadas para estoque/produção.");

            // Integração via Outbox (EF §11): estoque e produção consomem demanda planejada aprovada.
            _context.DemandaIntegracoes.Add(new DemandaIntegracao(previsao.Id, EDemandaIntegracaoDestino.Estoque, EDemandaIntegracaoDirecao.Saida, tenantId, usuario));
            _context.DemandaIntegracoes.Add(new DemandaIntegracao(previsao.Id, EDemandaIntegracaoDestino.Producao, EDemandaIntegracaoDirecao.Saida, tenantId, usuario));
            var payload = JsonSerializer.Serialize(new { previsao.Id, previsao.PeriodoInicio, previsao.PeriodoFim });
            _context.OutboxMessages.Add(new OutboxMessage(tenantId, "DemandaPlanejadaPublicada", payload));
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Demanda publicada para estoque e produção.", new { previsao.Id });
        }
    }
}
