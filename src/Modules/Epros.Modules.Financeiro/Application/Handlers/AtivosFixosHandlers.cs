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
    // ===== Grupo de Bem =====
    public class CriarGrupoBemCommandHandler : IRequestHandler<CriarGrupoBemCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarGrupoBemCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarGrupoBemCommand request, CancellationToken ct)
        {
            var grupo = new GrupoBem(request.Codigo, request.Nome, request.ContaAtivoId, request.ContaDepreciacaoId, request.ContaBaixaId,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!grupo.IsValid) return CommandResult.Falha(grupo.Notifications.Select(n => n.Message));
            _context.GruposBem.Add(grupo);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Grupo de bem cadastrado.", new { grupo.Id });
        }
    }

    public class AtualizarGrupoBemCommandHandler : IRequestHandler<AtualizarGrupoBemCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtualizarGrupoBemCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtualizarGrupoBemCommand request, CancellationToken ct)
        {
            var grupo = await _context.GruposBem.FirstOrDefaultAsync(g => g.Id == request.Id, ct);
            if (grupo == null) return CommandResult.Falha("Grupo de bem não encontrado.");
            grupo.Alterar(request.Codigo, request.Nome, request.ContaAtivoId, request.ContaDepreciacaoId, request.ContaBaixaId, _user.GetUserId() ?? "system");
            if (!grupo.IsValid) return CommandResult.Falha(grupo.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Grupo de bem atualizado.", new { grupo.Id });
        }
    }

    // ===== Ativo Fixo =====
    public class CriarAtivoFixoCommandHandler : IRequestHandler<CriarAtivoFixoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public CriarAtivoFixoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(CriarAtivoFixoCommand request, CancellationToken ct)
        {
            var ativo = new AtivoFixo(request.Descricao, request.ValorCompra, request.DataAquisicao,
                request.GrupoBemId, request.TipoAquisicaoId, request.EstadoConservacaoId, request.SetorId, request.FornecedorId, request.ColaboradorId,
                request.NumeroNb, request.Nome, request.NumeroSerie, request.Funcao, request.NumeroNotaFiscal, request.ChaveNfe,
                request.ValorOriginal, request.VencimentoGarantia, request.Deprecia, request.TipoDepreciacao, request.TaxaAnual, request.TaxaMensal,
                _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!ativo.IsValid) return CommandResult.Falha(ativo.Notifications.Select(n => n.Message));
            _context.AtivosFixos.Add(ativo);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Ativo fixo cadastrado.", new { ativo.Id });
        }
    }

    public class AtualizarAtivoFixoCommandHandler : IRequestHandler<AtualizarAtivoFixoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _user;
        public AtualizarAtivoFixoCommandHandler(ContextFinanceiro context, ICurrentUser user) { _context = context; _user = user; }

        public async Task<CommandResult> Handle(AtualizarAtivoFixoCommand request, CancellationToken ct)
        {
            var ativo = await _context.AtivosFixos.FirstOrDefaultAsync(a => a.Id == request.Id, ct);
            if (ativo == null) return CommandResult.Falha("Ativo fixo não encontrado.");
            ativo.Alterar(request.Descricao, request.ValorCompra, request.DataAquisicao, request.GrupoBemId, request.SetorId,
                request.FornecedorId, request.ColaboradorId, request.NumeroNb, request.Nome, request.NumeroSerie, request.Funcao,
                request.Deprecia, request.TipoDepreciacao, request.TaxaAnual, request.TaxaMensal, _user.GetUserId() ?? "system");
            if (!ativo.IsValid) return CommandResult.Falha(ativo.Notifications.Select(n => n.Message));
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Ativo fixo atualizado.", new { ativo.Id });
        }
    }

    public class BaixarAtivoFixoCommandHandler : IRequestHandler<BaixarAtivoFixoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public BaixarAtivoFixoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(BaixarAtivoFixoCommand request, CancellationToken ct)
        {
            var userId = _user.GetUserId() ?? "system";
            var ativo = await _context.AtivosFixos.FirstOrDefaultAsync(a => a.Id == request.Id, ct);
            if (ativo == null) return CommandResult.Falha("Ativo fixo não encontrado.");
            ativo.Baixar(request.DataBaixa, request.ValorBaixa, userId);
            if (!ativo.IsValid) return CommandResult.Falha(ativo.Notifications.Select(n => n.Message));
            var mov = new MovimentacaoAtivo(ativo.Id, Domain.Enums.ETipoMovimentacaoAtivo.Baixa, request.DataBaixa,
                request.ValorBaixa, request.Observacao, null, _tenant.GetTenantId(), userId);
            _context.MovimentacoesAtivo.Add(mov);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Ativo fixo baixado.", new { ativo.Id });
        }
    }

    // ===== Depreciação Mensal =====
    public class RegistrarDepreciacaoMensalCommandHandler : IRequestHandler<RegistrarDepreciacaoMensalCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarDepreciacaoMensalCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarDepreciacaoMensalCommand request, CancellationToken ct)
        {
            var tenantId = _tenant.GetTenantId();
            var userId = _user.GetUserId() ?? "system";
            var ativo = await _context.AtivosFixos.FirstOrDefaultAsync(a => a.Id == request.AtivoId, ct);
            if (ativo == null) return CommandResult.Falha("Ativo fixo não encontrado.");
            // EF §7.3: não duplicar depreciação para a mesma competência.
            var jaExiste = await _context.DepreciacoesMensais.AnyAsync(d => d.AtivoId == request.AtivoId && d.Competencia == request.Competencia, ct);
            if (jaExiste) return CommandResult.Falha("Já existe depreciação para o ativo nesta competência.");
            // Valor <= 0 ⇒ o motor calcula a cota do mês pela taxa informada no ativo (Linear/SaldoDecrescente).
            // Fórmula universal (Negocio-acumulado/contabil); taxa é fato legal RFB já informado → valida-contador.
            var valor = request.Valor > 0m ? request.Valor : ativo.CalcularCotaDepreciacaoMensal();
            if (valor <= 0m) return CommandResult.Falha("Não há cota de depreciação a registrar (informe o valor ou configure taxa/depreciação no ativo).");
            var dep = new DepreciacaoMensal(request.AtivoId, request.Competencia, valor, request.MetodoDepreciacao, request.TaxaAplicada, tenantId, userId);
            if (!dep.IsValid) return CommandResult.Falha(dep.Notifications.Select(n => n.Message));
            ativo.AplicarDepreciacao(valor, System.DateTime.UtcNow, userId);
            if (!ativo.IsValid) return CommandResult.Falha(ativo.Notifications.Select(n => n.Message));
            _context.DepreciacoesMensais.Add(dep);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Depreciação mensal registrada.", new { dep.Id, ativo.ValorAtualizado, ativo.Status });
        }
    }

    // ===== Movimentação =====
    public class RegistrarMovimentacaoAtivoCommandHandler : IRequestHandler<RegistrarMovimentacaoAtivoCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenant;
        private readonly ICurrentUser _user;
        public RegistrarMovimentacaoAtivoCommandHandler(ContextFinanceiro context, ITenantProvider tenant, ICurrentUser user)
        { _context = context; _tenant = tenant; _user = user; }

        public async Task<CommandResult> Handle(RegistrarMovimentacaoAtivoCommand request, CancellationToken ct)
        {
            var ativoExiste = await _context.AtivosFixos.AnyAsync(a => a.Id == request.AtivoId, ct);
            if (!ativoExiste) return CommandResult.Falha("Ativo fixo não encontrado.");
            var mov = new MovimentacaoAtivo(request.AtivoId, request.TipoMovimentacao, request.DataMovimentacao,
                request.Valor, request.Observacao, request.UsuarioId, _tenant.GetTenantId(), _user.GetUserId() ?? "system");
            if (!mov.IsValid) return CommandResult.Falha(mov.Notifications.Select(n => n.Message));
            _context.MovimentacoesAtivo.Add(mov);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Movimentação registrada.", new { mov.Id });
        }
    }
}
