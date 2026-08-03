using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Application.Services;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    // ===== Moeda =====
    public class CriarMoedaCommandHandler : IRequestHandler<CriarMoedaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarMoedaCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarMoedaCommand request, CancellationToken ct)
        {
            var moeda = new Moeda(request.CodigoIso, request.Simbolo, request.Nome, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!moeda.IsValid) return CommandResult.Falha(moeda.Notifications.Select(n => n.Message));
            _context.Moedas.Add(moeda);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Moeda cadastrada com sucesso.", new { moeda.Id });
        }
    }

    public class AtualizarMoedaCommandHandler : IRequestHandler<AtualizarMoedaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtualizarMoedaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtualizarMoedaCommand request, CancellationToken ct)
        {
            var moeda = await _context.Moedas.FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            if (moeda == null) return CommandResult.Falha("Moeda não encontrada.");
            moeda.Alterar(request.CodigoIso, request.Simbolo, request.Nome, _user.GetUserId() ?? "system");
            if (!moeda.IsValid) return CommandResult.Falha(moeda.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Moeda atualizada com sucesso.", new { moeda.Id });
        }
    }

    public class DeletarMoedaCommandHandler : IRequestHandler<DeletarMoedaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public DeletarMoedaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(DeletarMoedaCommand request, CancellationToken ct)
        {
            var moeda = await _context.Moedas.FirstOrDefaultAsync(m => m.Id == request.Id, ct);
            if (moeda == null) return CommandResult.Falha("Moeda não encontrada.");
            moeda.Deletar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Moeda removida (exclusão lógica).", new { moeda.Id });
        }
    }

    // ===== Taxa de Câmbio =====
    public class RegistrarTaxaCambioCommandHandler : IRequestHandler<RegistrarTaxaCambioCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarTaxaCambioCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarTaxaCambioCommand request, CancellationToken ct)
        {
            var moedaExiste = await _context.Moedas.AnyAsync(m => m.Id == request.MoedaId, ct);
            if (!moedaExiste) return CommandResult.Falha("Moeda informada não encontrada.");
            var taxa = new TaxaCambio(request.MoedaId, request.DataTaxa, request.TaxaCompra, request.TaxaVenda,
                request.OrigemTaxa, request.Observacao, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!taxa.IsValid) return CommandResult.Falha(taxa.Notifications.Select(n => n.Message));
            _context.TaxasCambio.Add(taxa);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Taxa de câmbio registrada.", new { taxa.Id });
        }
    }

    // ===== Exposição Cambial =====
    public class CriarExposicaoCambialCommandHandler : IRequestHandler<CriarExposicaoCambialCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarExposicaoCambialCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarExposicaoCambialCommand request, CancellationToken ct)
        {
            var exp = new ExposicaoCambial(request.MoedaId, request.ValorExposto, request.DataReferencia, request.OrigemExposicao,
                request.EntidadeOrigemTipo, request.EntidadeOrigemId, request.TaxaReferenciaId, request.ValorMoedaBase,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!exp.IsValid) return CommandResult.Falha(exp.Notifications.Select(n => n.Message));
            _context.ExposicoesCambiais.Add(exp);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Exposição cambial registrada.", new { exp.Id });
        }
    }

    public class MarcarExposicaoHedgeadaCommandHandler : IRequestHandler<MarcarExposicaoHedgeadaCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public MarcarExposicaoHedgeadaCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(MarcarExposicaoHedgeadaCommand request, CancellationToken ct)
        {
            var exp = await _context.ExposicoesCambiais.FirstOrDefaultAsync(e => e.Id == request.Id, ct);
            if (exp == null) return CommandResult.Falha("Exposição cambial não encontrada.");
            exp.MarcarHedgeada(_user.GetUserId() ?? "system");
            if (!exp.IsValid) return CommandResult.Falha(exp.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Exposição marcada como hedgeada.", new { exp.Id });
        }
    }

    public class EncerrarExposicaoCambialCommandHandler : IRequestHandler<EncerrarExposicaoCambialCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public EncerrarExposicaoCambialCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(EncerrarExposicaoCambialCommand request, CancellationToken ct)
        {
            var exp = await _context.ExposicoesCambiais.FirstOrDefaultAsync(e => e.Id == request.Id, ct);
            if (exp == null) return CommandResult.Falha("Exposição cambial não encontrada.");
            exp.Encerrar(_user.GetUserId() ?? "system");
            if (!exp.IsValid) return CommandResult.Falha(exp.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Exposição cambial encerrada.", new { exp.Id });
        }
    }

    // ===== Reavaliação de Títulos =====
    public class CriarReavaliacaoTituloCommandHandler : IRequestHandler<CriarReavaliacaoTituloCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarReavaliacaoTituloCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarReavaliacaoTituloCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            var reav = new ReavaliacaoTitulo(request.DataReavaliacao, request.Observacao, tenantId, userId);
            if (request.Itens != null)
                foreach (var i in request.Itens)
                    reav.AdicionarItem(i.MoedaId, i.TituloTipo, i.TituloId, i.TaxaCambioId, i.ValorOriginalMoeda, i.ValorReavaliadoBase, tenantId, userId);
            if (!reav.IsValid) return CommandResult.Falha(reav.Notifications.Select(n => n.Message));
            _context.ReavaliacoesTitulo.Add(reav);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Reavaliação cambial criada (rascunho).", new { reav.Id, reav.TotalVariacao });
        }
    }

    public class AprovarReavaliacaoTituloCommandHandler : IRequestHandler<AprovarReavaliacaoTituloCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AprovarReavaliacaoTituloCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AprovarReavaliacaoTituloCommand request, CancellationToken ct)
        {
            var reav = await _context.ReavaliacoesTitulo.FirstOrDefaultAsync(r => r.Id == request.Id, ct);
            if (reav == null) return CommandResult.Falha("Reavaliação não encontrada.");
            reav.Aprovar(_user.GetUserId() ?? "system");
            if (!reav.IsValid) return CommandResult.Falha(reav.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Reavaliação aprovada.", new { reav.Id });
        }
    }

    public class ContabilizarReavaliacaoTituloCommandHandler : IRequestHandler<ContabilizarReavaliacaoTituloCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public ContabilizarReavaliacaoTituloCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(ContabilizarReavaliacaoTituloCommand request, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var reav = await _context.ReavaliacoesTitulo.FirstOrDefaultAsync(r => r.Id == request.Id, ct);
            if (reav == null) return CommandResult.Falha("Reavaliação não encontrada.");
            reav.Contabilizar(userId);
            if (!reav.IsValid) return CommandResult.Falha(reav.Notifications.Select(n => n.Message));

            // Wiring evento→ledger (TEC-8): variação cambial (ganho/perda) × título reavaliado
            // (de-para = valida-contador). Só contabiliza quando há variação líquida.
            var valorVariacao = Math.Abs(reav.TotalVariacao);
            if (valorVariacao > 0m)
                await ContabilizacaoEventoService.GerarLancamentoAsync(
                    _context, _tenant.GetTenantId(), userId,
                    CatalogoEventosIntegracao.Financeiro.VariacaoCambialContabilizada, reav.Id, valorVariacao,
                    $"Variação cambial da reavaliação {reav.Id}", ct);

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Reavaliação contabilizada.", new { reav.Id });
        }
    }

    public class CancelarReavaliacaoTituloCommandHandler : IRequestHandler<CancelarReavaliacaoTituloCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public CancelarReavaliacaoTituloCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(CancelarReavaliacaoTituloCommand request, CancellationToken ct)
        {
            var reav = await _context.ReavaliacoesTitulo.FirstOrDefaultAsync(r => r.Id == request.Id, ct);
            if (reav == null) return CommandResult.Falha("Reavaliação não encontrada.");
            reav.Cancelar(_user.GetUserId() ?? "system");
            if (!reav.IsValid) return CommandResult.Falha(reav.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Reavaliação cancelada.", new { reav.Id });
        }
    }
}
