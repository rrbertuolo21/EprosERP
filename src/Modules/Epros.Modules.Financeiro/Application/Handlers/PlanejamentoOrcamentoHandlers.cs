using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    // ===== Versão Orçamentária =====
    public class CriarVersaoOrcamentariaCommandHandler : IRequestHandler<CriarVersaoOrcamentariaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarVersaoOrcamentariaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarVersaoOrcamentariaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            var versao = new VersaoOrcamentaria(request.Nome, request.PeriodoInicio, request.PeriodoFim, tenantId, userId);
            if (request.Linhas != null)
                foreach (var l in request.Linhas)
                    versao.AdicionarLinha(l.ContaId, l.CentroCustoId, l.Periodo, l.ValorOrcado, tenantId, userId);
            if (!versao.IsValid) return CommandResult.Falha(versao.Notifications.Select(n => n.Message));
            _context.VersoesOrcamentarias.Add(versao);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Versão orçamentária criada (rascunho).", new { versao.Id });
        }
    }

    public class AprovarVersaoOrcamentariaCommandHandler : IRequestHandler<AprovarVersaoOrcamentariaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AprovarVersaoOrcamentariaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AprovarVersaoOrcamentariaCommand request, CancellationToken ct)
        {
            var v = await _context.VersoesOrcamentarias.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (v == null) return CommandResult.Falha("Versão orçamentária não encontrada.");
            v.Aprovar(_user.GetUserId() ?? "system");
            if (!v.IsValid) return CommandResult.Falha(v.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Versão aprovada.", new { v.Id });
        }
    }

    public class AtivarVersaoOrcamentariaCommandHandler : IRequestHandler<AtivarVersaoOrcamentariaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtivarVersaoOrcamentariaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AtivarVersaoOrcamentariaCommand request, CancellationToken ct)
        {
            var v = await _context.VersoesOrcamentarias.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (v == null) return CommandResult.Falha("Versão orçamentária não encontrada.");
            v.Ativar(_user.GetUserId() ?? "system");
            if (!v.IsValid) return CommandResult.Falha(v.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Versão ativada.", new { v.Id });
        }
    }

    public class EncerrarVersaoOrcamentariaCommandHandler : IRequestHandler<EncerrarVersaoOrcamentariaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public EncerrarVersaoOrcamentariaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(EncerrarVersaoOrcamentariaCommand request, CancellationToken ct)
        {
            var v = await _context.VersoesOrcamentarias.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (v == null) return CommandResult.Falha("Versão orçamentária não encontrada.");
            v.Encerrar(_user.GetUserId() ?? "system");
            if (!v.IsValid) return CommandResult.Falha(v.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Versão encerrada.", new { v.Id });
        }
    }

    public class RegistrarRealizadoLinhaCommandHandler : IRequestHandler<RegistrarRealizadoLinhaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public RegistrarRealizadoLinhaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(RegistrarRealizadoLinhaCommand request, CancellationToken ct)
        {
            var linha = await _context.LinhasOrcamento.FirstOrDefaultAsync(l => l.Id == request.LinhaId, ct);
            if (linha == null) return CommandResult.Falha("Linha de orçamento não encontrada.");
            linha.RegistrarRealizado(request.ValorRealizado, _user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Realizado registrado.", new { linha.Id, linha.VariacaoValor, linha.VariacaoPercentual });
        }
    }

    // ===== Período Orçamentário =====
    public class CriarPeriodoOrcamentarioCommandHandler : IRequestHandler<CriarPeriodoOrcamentarioCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarPeriodoOrcamentarioCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarPeriodoOrcamentarioCommand request, CancellationToken ct)
        {
            var periodo = new PeriodoOrcamentario(request.DataInicio, request.DataFim, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!periodo.IsValid) return CommandResult.Falha(periodo.Notifications.Select(n => n.Message));
            _context.PeriodosOrcamentarios.Add(periodo);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Período orçamentário criado.", new { periodo.Id });
        }
    }

    public class AprovarPeriodoOrcamentarioCommandHandler : IRequestHandler<AprovarPeriodoOrcamentarioCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AprovarPeriodoOrcamentarioCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AprovarPeriodoOrcamentarioCommand request, CancellationToken ct)
        {
            var p = await _context.PeriodosOrcamentarios.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (p == null) return CommandResult.Falha("Período orçamentário não encontrado.");
            p.Aprovar(_user.GetUserId() ?? "system");
            if (!p.IsValid) return CommandResult.Falha(p.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Período aprovado.", new { p.Id });
        }
    }

    public class AtivarPeriodoOrcamentarioCommandHandler : IRequestHandler<AtivarPeriodoOrcamentarioCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtivarPeriodoOrcamentarioCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AtivarPeriodoOrcamentarioCommand request, CancellationToken ct)
        {
            var p = await _context.PeriodosOrcamentarios.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (p == null) return CommandResult.Falha("Período orçamentário não encontrado.");
            p.Ativar(_user.GetUserId() ?? "system");
            if (!p.IsValid) return CommandResult.Falha(p.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Período ativado.", new { p.Id });
        }
    }

    public class EncerrarPeriodoOrcamentarioCommandHandler : IRequestHandler<EncerrarPeriodoOrcamentarioCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public EncerrarPeriodoOrcamentarioCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(EncerrarPeriodoOrcamentarioCommand request, CancellationToken ct)
        {
            var usuario = _user.GetUserId() ?? "system";
            var p = await _context.PeriodosOrcamentarios.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (p == null) return CommandResult.Falha("Período orçamentário não encontrado.");
            p.Encerrar(usuario);
            if (!p.IsValid) return CommandResult.Falha(p.Notifications.Select(n => n.Message));

            // EF FIN-PO §6.4/§15: encerrar período encerra em cascata os budgets filhos ativos.
            var budgetsAtivos = await _context.Budgets
                .Where(b => b.PeriodoId == p.Id && b.Status == Domain.Enums.EStatusOrcamento.Ativo)
                .ToListAsync(ct);
            var encerrados = 0;
            foreach (var b in budgetsAtivos)
            {
                b.Encerrar(usuario);
                if (b.IsValid) encerrados++;
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Período encerrado.", new { p.Id, budgetsEncerrados = encerrados });
        }
    }

    // ===== Budget =====
    public class CriarBudgetCommandHandler : IRequestHandler<CriarBudgetCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarBudgetCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarBudgetCommand request, CancellationToken ct)
        {
            var periodoExiste = await _context.PeriodosOrcamentarios.AnyAsync(p => p.Id == request.PeriodoId, ct);
            if (!periodoExiste) return CommandResult.Falha("Período orçamentário não encontrado.");
            var budget = new Budget(request.PeriodoId, request.Tipo, request.Valor, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!budget.IsValid) return CommandResult.Falha(budget.Notifications.Select(n => n.Message));
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Budget criado (rascunho).", new { budget.Id });
        }
    }

    public class AprovarBudgetCommandHandler : IRequestHandler<AprovarBudgetCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AprovarBudgetCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AprovarBudgetCommand request, CancellationToken ct)
        {
            var b = await _context.Budgets.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (b == null) return CommandResult.Falha("Budget não encontrado.");
            b.Aprovar(_user.GetUserId() ?? "system");
            if (!b.IsValid) return CommandResult.Falha(b.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Budget aprovado.", new { b.Id });
        }
    }

    public class AtivarBudgetCommandHandler : IRequestHandler<AtivarBudgetCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtivarBudgetCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AtivarBudgetCommand request, CancellationToken ct)
        {
            var b = await _context.Budgets.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (b == null) return CommandResult.Falha("Budget não encontrado.");
            b.Ativar(_user.GetUserId() ?? "system");
            if (!b.IsValid) return CommandResult.Falha(b.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Budget ativado.", new { b.Id });
        }
    }

    public class EncerrarBudgetCommandHandler : IRequestHandler<EncerrarBudgetCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public EncerrarBudgetCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(EncerrarBudgetCommand request, CancellationToken ct)
        {
            var b = await _context.Budgets.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (b == null) return CommandResult.Falha("Budget não encontrado.");
            b.Encerrar(_user.GetUserId() ?? "system");
            if (!b.IsValid) return CommandResult.Falha(b.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Budget encerrado.", new { b.Id });
        }
    }

    public class ExcluirBudgetCommandHandler : IRequestHandler<ExcluirBudgetCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public ExcluirBudgetCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(ExcluirBudgetCommand request, CancellationToken ct)
        {
            var b = await _context.Budgets.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (b == null) return CommandResult.Falha("Budget não encontrado.");
            b.Excluir(_user.GetUserId() ?? "system");
            if (!b.IsValid) return CommandResult.Falha(b.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Budget excluído.", new { b.Id });
        }
    }

    public class AlocarBudgetCommandHandler : IRequestHandler<AlocarBudgetCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AlocarBudgetCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(AlocarBudgetCommand request, CancellationToken ct)
        {
            var budgetExiste = await _context.Budgets.AnyAsync(b => b.Id == request.BudgetId, ct);
            if (!budgetExiste) return CommandResult.Falha("Budget não encontrado.");
            var aloc = new BudgetAlocacao(request.BudgetId, request.ContaId, request.ValorAlocado, request.AutoAprovar, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!aloc.IsValid) return CommandResult.Falha(aloc.Notifications.Select(n => n.Message));
            _context.BudgetAlocacoes.Add(aloc);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Budget alocado à conta.", new { aloc.Id });
        }
    }

    // ===== Metas =====
    public class CriarMetaCategoriaCommandHandler : IRequestHandler<CriarMetaCategoriaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarMetaCategoriaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarMetaCategoriaCommand request, CancellationToken ct)
        {
            var cat = new MetaCategoria(request.Nome, request.Codigo, request.Escopo, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!cat.IsValid) return CommandResult.Falha(cat.Notifications.Select(n => n.Message));
            _context.MetaCategorias.Add(cat);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Categoria de meta cadastrada.", new { cat.Id });
        }
    }

    public class CriarMetaCommandHandler : IRequestHandler<CriarMetaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarMetaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarMetaCommand request, CancellationToken ct)
        {
            var categoriaExiste = await _context.MetaCategorias.AnyAsync(c => c.Id == request.CategoriaId, ct);
            if (!categoriaExiste) return CommandResult.Falha("Categoria de meta não encontrada.");
            var meta = new Meta(request.CategoriaId, request.Tipo, request.Prioridade, request.DataInicio, request.DataAlvo, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!meta.IsValid) return CommandResult.Falha(meta.Notifications.Select(n => n.Message));
            _context.Metas.Add(meta);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Meta criada (rascunho).", new { meta.Id });
        }
    }

    public class AtivarMetaCommandHandler : IRequestHandler<AtivarMetaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtivarMetaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(AtivarMetaCommand request, CancellationToken ct)
        {
            var meta = await _context.Metas.FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            if (meta == null) return CommandResult.Falha("Meta não encontrada.");
            meta.Ativar(_user.GetUserId() ?? "system");
            if (!meta.IsValid) return CommandResult.Falha(meta.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Meta ativada.", new { meta.Id });
        }
    }

    public class RegistrarContribuicaoMetaCommandHandler : IRequestHandler<RegistrarContribuicaoMetaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarContribuicaoMetaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(RegistrarContribuicaoMetaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            var meta = await _context.Metas.FirstOrDefaultAsync(m => m.Id == request.MetaId, ct);
            if (meta == null) return CommandResult.Falha("Meta não encontrada.");
            if (meta.Status != Domain.Enums.EStatusMeta.Ativa) return CommandResult.Falha("Somente meta ativa aceita contribuições.");
            var contrib = new MetaContribuicao(request.MetaId, request.Valor, request.Tipo, request.Data, tenantId, userId);
            if (!contrib.IsValid) return CommandResult.Falha(contrib.Notifications.Select(n => n.Message));
            _context.MetaContribuicoes.Add(contrib);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Contribuição registrada.", new { contrib.Id });
        }
    }

    public class RegistrarTrackingMetaCommandHandler : IRequestHandler<RegistrarTrackingMetaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarTrackingMetaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(RegistrarTrackingMetaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            var meta = await _context.Metas.FirstOrDefaultAsync(m => m.Id == request.MetaId, ct);
            if (meta == null) return CommandResult.Falha("Meta não encontrada.");
            var tracking = new MetaTracking(request.MetaId, request.Percentual, request.StatusProgresso, request.Data, tenantId, userId);
            if (!tracking.IsValid) return CommandResult.Falha(tracking.Notifications.Select(n => n.Message));
            _context.MetaTrackings.Add(tracking);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Tracking registrado.", new { tracking.Id });
        }
    }

    // ===== Milestones de meta (EF FIN-PO §6.7/§8.6) =====
    public class CriarMilestoneMetaCommandHandler : IRequestHandler<CriarMilestoneMetaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarMilestoneMetaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarMilestoneMetaCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            var meta = await _context.Metas.FirstOrDefaultAsync(m => m.Id == request.MetaId, ct);
            if (meta == null) return CommandResult.Falha("Meta não encontrada.");
            // §6.6: milestones só em metas ativas.
            if (meta.Status != Domain.Enums.EStatusMeta.Ativa) return CommandResult.Falha("Somente meta ativa aceita milestones.");
            var milestone = meta.AdicionarMilestone(request.Descricao, tenantId, userId);
            _context.MetaMilestones.Add(milestone);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Milestone criado (pendente).", new { milestone.Id });
        }
    }

    public class ConcluirMilestoneMetaCommandHandler : IRequestHandler<ConcluirMilestoneMetaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public ConcluirMilestoneMetaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(ConcluirMilestoneMetaCommand request, CancellationToken ct)
        {
            var milestone = await _context.MetaMilestones.FirstOrDefaultAsync(m => m.Id == request.MilestoneId, ct);
            if (milestone == null) return CommandResult.Falha("Milestone não encontrado.");
            milestone.Concluir(_user.GetUserId() ?? "system");
            if (!milestone.IsValid) return CommandResult.Falha(milestone.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Milestone concluído.", new { milestone.Id });
        }
    }

    // ===== Orçamento comercial (EF FIN-PO §6.2/§8.2/§12.3-12.4) =====
    public class CriarOrcamentoComercialCommandHandler : IRequestHandler<CriarOrcamentoComercialCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarOrcamentoComercialCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(CriarOrcamentoComercialCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            var orc = new OrcamentoComercial(
                request.VendedorId, request.TransportadoraId, request.ClienteId, request.CondicaoPagamentoId,
                request.Tipo, request.Codigo, request.TipoFrete, request.Observacao, request.StatusPedido,
                request.DataCadastro, request.DataEntrega, request.Validade,
                request.ValorFrete, request.TaxaComissao, request.TaxaDesconto, tenantId, userId);
            foreach (var i in request.Itens ?? new List<OrcamentoComercialItemInput>())
                orc.AdicionarItem(i.ProdutoId, i.Quantidade, i.ValorUnitario, i.TaxaDesconto, tenantId, userId);
            orc.CalcularTotais();
            orc.Validar();
            if (!orc.IsValid) return CommandResult.Falha(orc.Notifications.Select(n => n.Message));
            _context.OrcamentosComerciais.Add(orc);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Orçamento comercial criado.", new { orc.Id, orc.ValorSubtotal, orc.ValorTotal });
        }
    }

    public class AlterarOrcamentoComercialCommandHandler : IRequestHandler<AlterarOrcamentoComercialCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AlterarOrcamentoComercialCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }
        public async Task<CommandResult> Handle(AlterarOrcamentoComercialCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            var orc = await _context.OrcamentosComerciais.Include(o => o.Itens).FirstOrDefaultAsync(o => o.Id == request.Id, ct);
            if (orc == null) return CommandResult.Falha("Orçamento comercial não encontrado.");
            orc.AlterarCabecalho(
                request.VendedorId, request.TransportadoraId, request.ClienteId, request.CondicaoPagamentoId,
                request.Tipo, request.Codigo, request.TipoFrete, request.Observacao, request.StatusPedido,
                request.DataEntrega, request.Validade,
                request.ValorFrete, request.TaxaComissao, request.TaxaDesconto, userId);
            // Substitui os itens: remove os antigos explicitamente e recria a partir do input.
            // Como o cabeçalho já existe (grafo rastreado), os novos itens precisam ser marcados
            // como Added explicitamente — senão, por terem PK Guid já preenchida, o EF os trata como
            // Modified e tenta UPDATE de linhas inexistentes.
            _context.OrcamentoComercialItens.RemoveRange(orc.Itens.ToList());
            orc.LimparItens();
            foreach (var i in request.Itens ?? new List<OrcamentoComercialItemInput>())
            {
                var item = orc.AdicionarItem(i.ProdutoId, i.Quantidade, i.ValorUnitario, i.TaxaDesconto, tenantId, userId);
                _context.OrcamentoComercialItens.Add(item);
            }
            orc.CalcularTotais();
            orc.Validar();
            if (!orc.IsValid) return CommandResult.Falha(orc.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Orçamento comercial alterado.", new { orc.Id, orc.ValorSubtotal, orc.ValorTotal });
        }
    }

    public class ExcluirOrcamentoComercialCommandHandler : IRequestHandler<ExcluirOrcamentoComercialCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public ExcluirOrcamentoComercialCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }
        public async Task<CommandResult> Handle(ExcluirOrcamentoComercialCommand request, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var orc = await _context.OrcamentosComerciais.Include(o => o.Itens).FirstOrDefaultAsync(o => o.Id == request.Id, ct);
            if (orc == null) return CommandResult.Falha("Orçamento comercial não encontrado.");
            foreach (var item in orc.Itens) item.Deletar(userId);
            orc.Deletar(userId);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Orçamento comercial excluído.", new { orc.Id });
        }
    }
}
