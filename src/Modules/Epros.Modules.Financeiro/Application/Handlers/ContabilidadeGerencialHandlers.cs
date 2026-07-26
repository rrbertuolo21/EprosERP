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
    // ===== Centro de Custo =====
    public class CriarCentroCustoCommandHandler : IRequestHandler<CriarCentroCustoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarCentroCustoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarCentroCustoCommand request, CancellationToken ct)
        {
            var cc = new CentroCusto(request.Codigo, request.Descricao, request.PaiId, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!cc.IsValid) return CommandResult.Falha(cc.Notifications.Select(n => n.Message));
            _context.CentrosCusto.Add(cc);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Centro de custo cadastrado.", new { cc.Id });
        }
    }

    public class AtualizarCentroCustoCommandHandler : IRequestHandler<AtualizarCentroCustoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtualizarCentroCustoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtualizarCentroCustoCommand request, CancellationToken ct)
        {
            var cc = await _context.CentrosCusto.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (cc == null) return CommandResult.Falha("Centro de custo não encontrado.");
            cc.Alterar(request.Codigo, request.Descricao, request.PaiId, _user.GetUserId() ?? "system");
            if (!cc.IsValid) return CommandResult.Falha(cc.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Centro de custo atualizado.", new { cc.Id });
        }
    }

    public class DefinirEstadoCentroCustoCommandHandler : IRequestHandler<DefinirEstadoCentroCustoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public DefinirEstadoCentroCustoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(DefinirEstadoCentroCustoCommand request, CancellationToken ct)
        {
            var cc = await _context.CentrosCusto.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (cc == null) return CommandResult.Falha("Centro de custo não encontrado.");
            cc.DefinirEstado(request.Estado, _user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Estado do centro de custo atualizado.", new { cc.Id, cc.Estado });
        }
    }

    public class DeletarCentroCustoCommandHandler : IRequestHandler<DeletarCentroCustoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public DeletarCentroCustoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(DeletarCentroCustoCommand request, CancellationToken ct)
        {
            var cc = await _context.CentrosCusto.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            if (cc == null) return CommandResult.Falha("Centro de custo não encontrado.");
            var temFilhos = await _context.CentrosCusto.AnyAsync(c => c.PaiId == request.Id, ct);
            if (temFilhos) return CommandResult.Falha("Centro de custo possui filhos e não pode ser excluído.");
            cc.Deletar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Centro de custo removido (exclusão lógica).", new { cc.Id });
        }
    }

    // ===== Alocação a Centro de Custo =====
    public class AlocarTituloCentroCustoCommandHandler : IRequestHandler<AlocarTituloCentroCustoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AlocarTituloCentroCustoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AlocarTituloCentroCustoCommand request, CancellationToken ct)
        {
            var ccExiste = await _context.CentrosCusto.AnyAsync(c => c.Id == request.CentroCustoId, ct);
            if (!ccExiste) return CommandResult.Falha("Centro de custo não encontrado.");
            // EF §6: a soma dos percentuais de rateio de um título não pode ultrapassar 100%.
            var somaExistente = await _context.AlocacoesCentroCusto.Where(a => a.TituloId == request.TituloId).SumAsync(a => (decimal?)a.Percentual, ct) ?? 0m;
            if (somaExistente + request.Percentual > 100m)
                return CommandResult.Falha("A soma dos percentuais de rateio do título ultrapassa 100%.");
            var aloc = new AlocacaoCentroCusto(request.TituloId, request.TipoTitulo, request.CentroCustoId, request.Percentual,
                request.ValorRateado, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!aloc.IsValid) return CommandResult.Falha(aloc.Notifications.Select(n => n.Message));
            _context.AlocacoesCentroCusto.Add(aloc);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Título alocado ao centro de custo.", new { aloc.Id });
        }
    }

    public class RemoverAlocacaoCentroCustoCommandHandler : IRequestHandler<RemoverAlocacaoCentroCustoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public RemoverAlocacaoCentroCustoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(RemoverAlocacaoCentroCustoCommand request, CancellationToken ct)
        {
            var aloc = await _context.AlocacoesCentroCusto.FirstOrDefaultAsync(a => a.Id == request.Id, ct);
            if (aloc == null) return CommandResult.Falha("Alocação não encontrada.");
            aloc.Deletar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Alocação removida (exclusão lógica).", new { aloc.Id });
        }
    }

    // ===== Dimensão Analítica =====
    public class CriarDimensaoAnaliticaCommandHandler : IRequestHandler<CriarDimensaoAnaliticaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarDimensaoAnaliticaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarDimensaoAnaliticaCommand request, CancellationToken ct)
        {
            var dim = new DimensaoAnalitica(request.Tipo, request.Valor, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!dim.IsValid) return CommandResult.Falha(dim.Notifications.Select(n => n.Message));
            _context.DimensoesAnaliticas.Add(dim);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Dimensão analítica cadastrada.", new { dim.Id });
        }
    }

    public class VincularAlocacaoDimensaoCommandHandler : IRequestHandler<VincularAlocacaoDimensaoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public VincularAlocacaoDimensaoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(VincularAlocacaoDimensaoCommand request, CancellationToken ct)
        {
            var vinculo = new AlocacaoDimensao(request.AlocacaoCentroCustoId, request.DimensaoAnaliticaId, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!vinculo.IsValid) return CommandResult.Falha(vinculo.Notifications.Select(n => n.Message));
            _context.AlocacoesDimensao.Add(vinculo);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Dimensão vinculada à alocação.", new { vinculo.Id });
        }
    }
}
