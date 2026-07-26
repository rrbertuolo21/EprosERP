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
    // ===== Grupo de Consolidação =====
    public class CriarGrupoConsolidacaoCommandHandler : IRequestHandler<CriarGrupoConsolidacaoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarGrupoConsolidacaoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarGrupoConsolidacaoCommand request, CancellationToken ct)
        {
            var grupo = new GrupoConsolidacao(request.Codigo, request.Nome, request.Descricao, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!grupo.IsValid) return CommandResult.Falha(grupo.Notifications.Select(n => n.Message));
            _context.GruposConsolidacao.Add(grupo);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Grupo de consolidação cadastrado.", new { grupo.Id });
        }
    }

    public class AtualizarGrupoConsolidacaoCommandHandler : IRequestHandler<AtualizarGrupoConsolidacaoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtualizarGrupoConsolidacaoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtualizarGrupoConsolidacaoCommand request, CancellationToken ct)
        {
            var grupo = await _context.GruposConsolidacao.FirstOrDefaultAsync(g => g.Id == request.Id, ct);
            if (grupo == null) return CommandResult.Falha("Grupo de consolidação não encontrado.");
            grupo.Alterar(request.Codigo, request.Nome, request.Descricao, _user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Grupo de consolidação atualizado.", new { grupo.Id });
        }
    }

    public class AdicionarEmpresaGrupoCommandHandler : IRequestHandler<AdicionarEmpresaGrupoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public AdicionarEmpresaGrupoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(AdicionarEmpresaGrupoCommand request, CancellationToken ct)
        {
            var grupoExiste = await _context.GruposConsolidacao.AnyAsync(g => g.Id == request.GrupoConsolidacaoId, ct);
            if (!grupoExiste) return CommandResult.Falha("Grupo de consolidação não encontrado.");
            var jaVinculada = await _context.GruposEmpresa.AnyAsync(e => e.GrupoConsolidacaoId == request.GrupoConsolidacaoId && e.EmpresaId == request.EmpresaId, ct);
            if (jaVinculada) return CommandResult.Falha("Empresa já vinculada ao grupo.");
            var vinculo = new GrupoEmpresa(request.GrupoConsolidacaoId, request.EmpresaId, request.DataInicio, request.DataFim, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!vinculo.IsValid) return CommandResult.Falha(vinculo.Notifications.Select(n => n.Message));
            _context.GruposEmpresa.Add(vinculo);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Empresa adicionada ao grupo.", new { vinculo.Id });
        }
    }

    public class RemoverEmpresaGrupoCommandHandler : IRequestHandler<RemoverEmpresaGrupoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public RemoverEmpresaGrupoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(RemoverEmpresaGrupoCommand request, CancellationToken ct)
        {
            var vinculo = await _context.GruposEmpresa.FirstOrDefaultAsync(e => e.Id == request.Id, ct);
            if (vinculo == null) return CommandResult.Falha("Vínculo não encontrado.");
            vinculo.Deletar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Empresa removida do grupo (exclusão lógica).", new { vinculo.Id });
        }
    }

    // ===== Demonstrativo =====
    public class CriarDemonstrativoCommandHandler : IRequestHandler<CriarDemonstrativoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarDemonstrativoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarDemonstrativoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            var grupoExiste = await _context.GruposConsolidacao.AnyAsync(g => g.Id == request.GrupoConsolidacaoId, ct);
            if (!grupoExiste) return CommandResult.Falha("Grupo de consolidação não encontrado.");
            var demo = new Demonstrativo(request.GrupoConsolidacaoId, request.Periodo, tenantId, userId);
            if (request.Linhas != null)
                foreach (var l in request.Linhas)
                    demo.AdicionarLinha(l.Ordem, l.CodigoLinha, l.DescricaoLinha, l.Valor, l.TipoLinha, tenantId, userId);
            if (!demo.IsValid) return CommandResult.Falha(demo.Notifications.Select(n => n.Message));
            _context.Demonstrativos.Add(demo);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Demonstrativo consolidado criado (provisório).", new { demo.Id });
        }
    }

    public class PublicarDemonstrativoCommandHandler : IRequestHandler<PublicarDemonstrativoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public PublicarDemonstrativoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(PublicarDemonstrativoCommand request, CancellationToken ct)
        {
            var demo = await _context.Demonstrativos.FirstOrDefaultAsync(d => d.Id == request.Id, ct);
            if (demo == null) return CommandResult.Falha("Demonstrativo não encontrado.");
            demo.Publicar(request.UsuarioPublicacaoId, _user.GetUserId() ?? "system");
            if (!demo.IsValid) return CommandResult.Falha(demo.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Demonstrativo publicado.", new { demo.Id });
        }
    }

    // ===== Balancete Consolidado =====
    public class CriarBalanceteConsolidadoCommandHandler : IRequestHandler<CriarBalanceteConsolidadoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarBalanceteConsolidadoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarBalanceteConsolidadoCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            var grupoExiste = await _context.GruposConsolidacao.AnyAsync(g => g.Id == request.GrupoConsolidacaoId, ct);
            if (!grupoExiste) return CommandResult.Falha("Grupo de consolidação não encontrado.");
            var bal = new BalanceteConsolidado(request.GrupoConsolidacaoId, request.Periodo, tenantId, userId);
            decimal totalDeb = 0, totalCred = 0;
            if (request.Linhas != null)
                foreach (var l in request.Linhas)
                {
                    bal.AdicionarLinha(l.ContaId, l.CodigoConta, l.NomeConta, l.SaldoAnterior, l.Debito, l.Credito, l.SaldoFinal, tenantId, userId);
                    totalDeb += l.Debito ?? 0;
                    totalCred += l.Credito ?? 0;
                }
            bal.AtualizarTotais(totalDeb, totalCred, totalDeb - totalCred, userId);
            if (!bal.IsValid) return CommandResult.Falha(bal.Notifications.Select(n => n.Message));
            _context.BalancetesConsolidados.Add(bal);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Balancete consolidado criado (provisório).", new { bal.Id, bal.TotalDebito, bal.TotalCredito });
        }
    }

    public class PublicarBalanceteConsolidadoCommandHandler : IRequestHandler<PublicarBalanceteConsolidadoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public PublicarBalanceteConsolidadoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(PublicarBalanceteConsolidadoCommand request, CancellationToken ct)
        {
            var bal = await _context.BalancetesConsolidados.FirstOrDefaultAsync(b => b.Id == request.Id, ct);
            if (bal == null) return CommandResult.Falha("Balancete consolidado não encontrado.");
            bal.Publicar(_user.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Balancete consolidado publicado.", new { bal.Id });
        }
    }

    // ===== Eliminação Intercompany =====
    public class RegistrarEliminacaoIntercompanyCommandHandler : IRequestHandler<RegistrarEliminacaoIntercompanyCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarEliminacaoIntercompanyCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarEliminacaoIntercompanyCommand request, CancellationToken ct)
        {
            var grupoExiste = await _context.GruposConsolidacao.AnyAsync(g => g.Id == request.GrupoConsolidacaoId, ct);
            if (!grupoExiste) return CommandResult.Falha("Grupo de consolidação não encontrado.");
            var elim = new EliminacaoIntercompany(request.GrupoConsolidacaoId, request.Periodo, request.EmpresaOrigemId, request.EmpresaDestinoId,
                request.Valor, request.Descricao, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!elim.IsValid) return CommandResult.Falha(elim.Notifications.Select(n => n.Message));
            _context.EliminacoesIntercompany.Add(elim);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Eliminação intercompany registrada.", new { elim.Id });
        }
    }
}
