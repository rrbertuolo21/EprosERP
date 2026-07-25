using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    /// <summary>
    /// Base com a resolução da configuração de imposto da empresa (EF §11.3/§11.4) e a montagem de linhas.
    ///
    /// FISCAL / RN (negócio nunca de memória — CLAUDE.md Regra #0):
    /// A EF de Gestão de Serviços trata "imposto sobre valor agregado" como imposto por dentro/por fora
    /// configurado por empresa (IVA), não como emissão de documento fiscal (EF §4.2 tira o motor fiscal do
    /// escopo). O tributo de serviço no Brasil (ISSQN e, na Reforma, IBS/CBS) é regido por
    /// Negocio-acumulado/fiscal/nfse (RN01–RN03 da NT 004 SE/CGNFS-e v2.0 e LC 214/2025): a apuração/
    /// recolhimento do ISSQN ocorre no MAN/DNA e a NFS-e encapsula os grupos IBSCBS — nada disso é emitido
    /// aqui. Esta fatura apenas registra o gatilho financeiro; o cálculo tributário oficial e a emissão de
    /// NFS-e pertencem ao módulo fiscal/plataforma. O percentual/tipo efetivo deve vir da configuração
    /// fiscal da empresa; enquanto o lookup dessa configuração não existir no Context de Vendas, assume-se
    /// 0% Exclusivo (sem inventar alíquota — ver PENDÊNCIA no relatório).
    /// </summary>
    public abstract class ServicoFaturaHandlerBase
    {
        protected readonly ContextVendas _context;
        protected readonly ITenantProvider _tenantProvider;
        protected readonly ICurrentUser _currentUser;

        protected ServicoFaturaHandlerBase(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Resolve o percentual e o tipo de imposto configurados para a empresa (EF §11.3/§11.4).
        /// Sem inventar alíquota: default conservador 0% Exclusivo até existir o lookup de configuração
        /// fiscal da empresa no Context de Vendas.
        /// </summary>
        protected virtual (decimal percentual, ETipoImpostoServico tipo) ResolverConfiguracaoImposto(Guid empresaId)
            => (0m, ETipoImpostoServico.Exclusivo);

        protected ServicoFaturaLinha MontarLinha(ServicoFaturaLinhaInput input, string tenantId, string usuario)
            => new ServicoFaturaLinha(
                input.ServicoId, input.NomeServico, input.Descricao,
                input.Quantidade, input.PrecoUnitario, input.DescontoPercentual,
                tenantId, usuario);
    }

    public class CriarServicoFaturaCommandHandler : ServicoFaturaHandlerBase, ICommandHandler<CriarServicoFaturaCommand>
    {
        public CriarServicoFaturaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
            : base(context, tenantProvider, currentUser) { }

        public async Task<CommandResult> Handle(CriarServicoFaturaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var fatura = new ServicoFatura(
                request.EmpresaId, request.ClienteId, request.FuncionarioId, request.ContaPagamentoId,
                request.UsuarioId, string.Empty, request.DataFatura, request.DescontoCabecalho,
                request.CustoEnvioEntrega, request.ValorPago, request.Detalhes, tenantId, usuario);

            if (!fatura.IsValid)
                return CommandResult.Falha(fatura.Notifications.Select(n => n.Message), "Dados da fatura inválidos.");

            foreach (var input in request.Linhas ?? Enumerable.Empty<ServicoFaturaLinhaInput>())
            {
                var linha = MontarLinha(input, tenantId, usuario);
                if (!linha.IsValid)
                    return CommandResult.Falha(linha.Notifications.Select(n => n.Message), "Linha de serviço inválida.");
                fatura.AdicionarLinha(linha);
            }

            var (pct, tipo) = ResolverConfiguracaoImposto(request.EmpresaId);
            fatura.Recalcular(pct, tipo);

            _context.ServicoFaturas.Add(fatura);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fatura de serviço criada com sucesso!", new
            {
                fatura.Id,
                fatura.NumeroFatura,
                fatura.TotalGeral,
                fatura.TotalLiquido,
                fatura.SaldoDevido,
                fatura.Troco
            });
        }
    }

    public class AtualizarServicoFaturaCommandHandler : ServicoFaturaHandlerBase, ICommandHandler<AtualizarServicoFaturaCommand>
    {
        public AtualizarServicoFaturaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
            : base(context, tenantProvider, currentUser) { }

        public async Task<CommandResult> Handle(AtualizarServicoFaturaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var fatura = await _context.ServicoFaturas
                .Include(f => f.Linhas)
                .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Id == request.Id, cancellationToken);
            if (fatura == null) return CommandResult.Falha("Fatura de serviço não encontrada.");

            if (fatura.Status == EServicoFaturaStatus.Cancelado || fatura.Status == EServicoFaturaStatus.Estornado)
                return CommandResult.Falha("Fatura cancelada/estornada não pode ser alterada.");

            fatura.AlterarCabecalho(request.FuncionarioId, request.ContaPagamentoId, request.DescontoCabecalho,
                request.CustoEnvioEntrega, request.ValorPago, request.Detalhes, usuario);

            // Substitui as linhas (recálculo consistente — EF §10.13).
            var linhasAntigas = _context.ServicoFaturaLinhas.Where(l => l.TenantId == tenantId && l.FaturaId == fatura.Id);
            _context.ServicoFaturaLinhas.RemoveRange(linhasAntigas);
            fatura.LimparLinhas();

            foreach (var input in request.Linhas ?? Enumerable.Empty<ServicoFaturaLinhaInput>())
            {
                var linha = MontarLinha(input, tenantId, usuario);
                if (!linha.IsValid)
                    return CommandResult.Falha(linha.Notifications.Select(n => n.Message), "Linha de serviço inválida.");
                fatura.AdicionarLinha(linha);
            }

            var (pct, tipo) = ResolverConfiguracaoImposto(fatura.EmpresaId);
            fatura.Recalcular(pct, tipo);
            fatura.Validar();
            if (!fatura.IsValid)
                return CommandResult.Falha(fatura.Notifications.Select(n => n.Message), "Dados da fatura inválidos.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fatura de serviço atualizada com sucesso!", new { fatura.Id, fatura.TotalLiquido, fatura.SaldoDevido, fatura.Troco });
        }
    }

    public class ConfirmarServicoFaturaCommandHandler : ServicoFaturaHandlerBase, ICommandHandler<ConfirmarServicoFaturaCommand>
    {
        public ConfirmarServicoFaturaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
            : base(context, tenantProvider, currentUser) { }

        public async Task<CommandResult> Handle(ConfirmarServicoFaturaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var fatura = await _context.ServicoFaturas.Include(f => f.Linhas)
                .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Id == request.Id, cancellationToken);
            if (fatura == null) return CommandResult.Falha("Fatura de serviço não encontrada.");

            fatura.Confirmar(usuario);
            if (!fatura.IsValid) return CommandResult.Falha(fatura.Notifications.Select(n => n.Message), "Não foi possível confirmar a fatura.");

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fatura confirmada com sucesso!", new { fatura.Id, Status = fatura.Status.ToString() });
        }
    }

    public class FaturarServicoFaturaCommandHandler : ServicoFaturaHandlerBase, ICommandHandler<FaturarServicoFaturaCommand>
    {
        public FaturarServicoFaturaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
            : base(context, tenantProvider, currentUser) { }

        public async Task<CommandResult> Handle(FaturarServicoFaturaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var fatura = await _context.ServicoFaturas.Include(f => f.Linhas)
                .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Id == request.Id, cancellationToken);
            if (fatura == null) return CommandResult.Falha("Fatura de serviço não encontrada.");

            fatura.Faturar(usuario);
            if (!fatura.IsValid) return CommandResult.Falha(fatura.Notifications.Select(n => n.Message), "Não foi possível faturar.");

            // EF §12: gera as referências de lançamento financeiro (cliente débito, receita crédito e,
            // se houver valor pago, conta débito + cliente crédito). O lançamento efetivo é do Financeiro,
            // acionado por evento/Outbox — aqui só a referência de integração (status Pendente).
            var refs = _context.ServicoLancamentoFinanceiroRefs;
            refs.Add(new ServicoLancamentoFinanceiroRef(fatura.Id, fatura.NumeroFatura, ETipoLancamentoServicoFinanceiro.ClienteDebito, null, fatura.ClienteId, fatura.TotalLiquido, tenantId, usuario));
            refs.Add(new ServicoLancamentoFinanceiroRef(fatura.Id, fatura.NumeroFatura, ETipoLancamentoServicoFinanceiro.ReceitaCredito, null, fatura.ClienteId, fatura.TotalLiquido, tenantId, usuario));
            if (fatura.ValorPago > 0)
            {
                refs.Add(new ServicoLancamentoFinanceiroRef(fatura.Id, fatura.NumeroFatura, ETipoLancamentoServicoFinanceiro.ContaPagamentoDebito, fatura.ContaPagamentoId, fatura.ClienteId, fatura.ValorPago, tenantId, usuario));
                refs.Add(new ServicoLancamentoFinanceiroRef(fatura.Id, fatura.NumeroFatura, ETipoLancamentoServicoFinanceiro.ClienteCredito, null, fatura.ClienteId, fatura.ValorPago, tenantId, usuario));
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fatura faturada com sucesso!", new { fatura.Id, Status = fatura.Status.ToString(), fatura.TotalLiquido, fatura.SaldoDevido });
        }
    }

    public class CancelarServicoFaturaCommandHandler : ServicoFaturaHandlerBase, ICommandHandler<CancelarServicoFaturaCommand>
    {
        public CancelarServicoFaturaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
            : base(context, tenantProvider, currentUser) { }

        public async Task<CommandResult> Handle(CancelarServicoFaturaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var fatura = await _context.ServicoFaturas.FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Id == request.Id, cancellationToken);
            if (fatura == null) return CommandResult.Falha("Fatura de serviço não encontrada.");

            var eraFaturada = fatura.Status == EServicoFaturaStatus.Faturado;
            fatura.Cancelar(usuario);
            if (!fatura.IsValid) return CommandResult.Falha(fatura.Notifications.Select(n => n.Message), "Não foi possível cancelar a fatura.");

            // EF §13.5: cancelamento de fatura com lançamentos deve estornar os efeitos financeiros.
            if (eraFaturada)
            {
                var refs = await _context.ServicoLancamentoFinanceiroRefs
                    .Where(r => r.TenantId == tenantId && r.FaturaId == fatura.Id)
                    .ToListAsync(cancellationToken);
                foreach (var r in refs) r.MarcarEstornado(usuario);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fatura cancelada com sucesso!", new { fatura.Id, Status = fatura.Status.ToString() });
        }
    }

    public class ExcluirServicoFaturaCommandHandler : ServicoFaturaHandlerBase, ICommandHandler<ExcluirServicoFaturaCommand>
    {
        public ExcluirServicoFaturaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
            : base(context, tenantProvider, currentUser) { }

        public async Task<CommandResult> Handle(ExcluirServicoFaturaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var fatura = await _context.ServicoFaturas.FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Id == request.Id, cancellationToken);
            if (fatura == null) return CommandResult.Falha("Fatura de serviço não encontrada.");

            // EF §13.2: exclusão lógica preservando auditoria (soft delete via base).
            fatura.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Fatura excluída logicamente.", new { fatura.Id });
        }
    }
}
