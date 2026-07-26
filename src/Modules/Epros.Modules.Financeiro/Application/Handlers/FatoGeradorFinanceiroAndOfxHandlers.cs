using System.Linq;
using Epros.Modules.Financeiro.Application.Commands;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Handlers
{
    // ─────────────────────────────────────────────────────
    // FatoGeradorFinanceiro
    // ─────────────────────────────────────────────────────
    public class CriarFatoGeradorFinanceiroCommandHandler : IRequestHandler<CriarFatoGeradorFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarFatoGeradorFinanceiroCommandHandler(ContextFinanceiro context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarFatoGeradorFinanceiroCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var fato = new FatoGeradorFinanceiro(r.Origem, r.VendaId, r.CompraId, r.Descricao, tenantId, userId);
            _context.FatosGeradoresFinanceiros.Add(fato);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Fato gerador criado com sucesso.", new { fato.Id });
        }
    }

    public class AtualizarFatoGeradorFinanceiroCommandHandler : IRequestHandler<AtualizarFatoGeradorFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarFatoGeradorFinanceiroCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarFatoGeradorFinanceiroCommand r, CancellationToken ct)
        {
            var fato = await _context.FatosGeradoresFinanceiros.FirstOrDefaultAsync(f => f.Id == r.Id, ct);
            if (fato is null)
                return CommandResult.Falha("Fato gerador não encontrado.");

            fato.Alterar(r.Origem, r.VendaId, r.CompraId, r.Descricao, _currentUser.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Fato gerador atualizado com sucesso.", new { fato.Id });
        }
    }

    public class DeletarFatoGeradorFinanceiroCommandHandler : IRequestHandler<DeletarFatoGeradorFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarFatoGeradorFinanceiroCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarFatoGeradorFinanceiroCommand r, CancellationToken ct)
        {
            var fato = await _context.FatosGeradoresFinanceiros.FirstOrDefaultAsync(f => f.Id == r.Id, ct);
            if (fato is null)
                return CommandResult.Falha("Fato gerador não encontrado.");

            fato.Deletar(_currentUser.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Fato gerador removido com sucesso.");
        }
    }

    // ─────────────────────────────────────────────────────
    // Importação OFX
    // ─────────────────────────────────────────────────────
    public class CriarImportacaoArquivoOfxCommandHandler : IRequestHandler<CriarImportacaoArquivoOfxCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarImportacaoArquivoOfxCommandHandler(ContextFinanceiro context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarImportacaoArquivoOfxCommand r, CancellationToken ct)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var ofx = new ImportacacaoArquivoOfx(r.CodigoBanco, r.NumeroConta, r.TipoConta, r.DataInicioExtrato,
                r.DataFimExtrato, tenantId, userId);

            if (!ofx.IsValid)
                return CommandResult.Falha(ofx.Notifications.Select(n => n.Message));

            _context.ImportacoesArquivoOfx.Add(ofx);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Importação OFX criada com sucesso.", new { ofx.Id });
        }
    }

    public class AdicionarTransacaoOfxCommandHandler : IRequestHandler<AdicionarTransacaoOfxCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarTransacaoOfxCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarTransacaoOfxCommand r, CancellationToken ct)
        {
            var ofx = await _context.ImportacoesArquivoOfx
                .Include(o => o.Transacoes)
                .FirstOrDefaultAsync(o => o.Id == r.ImportacacaoArquivoOfxId, ct);

            if (ofx is null)
                return CommandResult.Falha("Importação OFX não encontrada.");

            ofx.AdicionarTransacao(r.IdentificadorTransacao, r.Data, r.Valor, r.Tipo, r.Descricao,
                _currentUser.GetUserId() ?? "system");

            if (!ofx.IsValid)
                return CommandResult.Falha(ofx.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Transação OFX adicionada com sucesso.");
        }
    }

    public class DeletarImportacaoArquivoOfxCommandHandler : IRequestHandler<DeletarImportacaoArquivoOfxCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarImportacaoArquivoOfxCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarImportacaoArquivoOfxCommand r, CancellationToken ct)
        {
            var ofx = await _context.ImportacoesArquivoOfx
                .Include(o => o.Transacoes)
                .FirstOrDefaultAsync(o => o.Id == r.Id, ct);

            if (ofx is null)
                return CommandResult.Falha("Importação OFX não encontrada.");

            ofx.DeletarComTransacoes(_currentUser.GetUserId() ?? "system");
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Importação OFX removida com sucesso.");
        }
    }

    public class ConciliarTransacaoOfxContasAReceberCommandHandler : IRequestHandler<ConciliarTransacaoOfxContasAReceberCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ConciliarTransacaoOfxContasAReceberCommandHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ConciliarTransacaoOfxContasAReceberCommand r, CancellationToken ct)
        {
            var ofx = await _context.ImportacoesArquivoOfx
                .Include(o => o.Transacoes)
                .FirstOrDefaultAsync(o => o.Id == r.ImportacacaoArquivoOfxId, ct);

            if (ofx is null)
                return CommandResult.Falha("Importação OFX não encontrada.");

            ofx.AdicionarConciliacaoCR(r.TransacaoId, r.ContasAReceberId);
            if (!ofx.IsValid)
                return CommandResult.Falha(ofx.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Transação conciliada com o título a receber.");
        }
    }

    public class ConciliarTransacaoOfxContasAPagarCommandHandler : IRequestHandler<ConciliarTransacaoOfxContasAPagarCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public ConciliarTransacaoOfxContasAPagarCommandHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ConciliarTransacaoOfxContasAPagarCommand r, CancellationToken ct)
        {
            var ofx = await _context.ImportacoesArquivoOfx
                .Include(o => o.Transacoes)
                .FirstOrDefaultAsync(o => o.Id == r.ImportacacaoArquivoOfxId, ct);

            if (ofx is null)
                return CommandResult.Falha("Importação OFX não encontrada.");

            ofx.AdicionarConciliacaoCP(r.TransacaoId, r.ContasAPagarId);
            if (!ofx.IsValid)
                return CommandResult.Falha(ofx.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Transação conciliada com o título a pagar.");
        }
    }

    public class EstornarConciliacaoTransacaoOfxCReceberCommandHandler : IRequestHandler<EstornarConciliacaoTransacaoOfxCReceberCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public EstornarConciliacaoTransacaoOfxCReceberCommandHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(EstornarConciliacaoTransacaoOfxCReceberCommand r, CancellationToken ct)
        {
            var transacao = await _context.ImportacoesArquivoOfxTransacoes.FirstOrDefaultAsync(t => t.Id == r.TransacaoId, ct);
            if (transacao is null)
                return CommandResult.Falha("Transação OFX não encontrada.");

            transacao.EstornarConciliacaoCR();
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conciliação estornada.");
        }
    }

    public class EstornarConciliacaoTransacaoOfxCPagarCommandHandler : IRequestHandler<EstornarConciliacaoTransacaoOfxCPagarCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;

        public EstornarConciliacaoTransacaoOfxCPagarCommandHandler(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(EstornarConciliacaoTransacaoOfxCPagarCommand r, CancellationToken ct)
        {
            var transacao = await _context.ImportacoesArquivoOfxTransacoes.FirstOrDefaultAsync(t => t.Id == r.TransacaoId, ct);
            if (transacao is null)
                return CommandResult.Falha("Transação OFX não encontrada.");

            transacao.EstornarConciliacaoCP();
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Conciliação estornada.");
        }
    }
}
