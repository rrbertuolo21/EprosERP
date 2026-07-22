using System;
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
    // ----- PLANO DE CONTAS FINANCEIRO HANDLERS -----
    public class CriarPlanoDeContasFinanceiroCommandHandler : IRequestHandler<CriarPlanoDeContasFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPlanoDeContasFinanceiroCommandHandler(ContextFinanceiro context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPlanoDeContasFinanceiroCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var plano = new PlanoDeContasFinanceiro(
                request.ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId,
                request.ConfiguracaoCodigoNaturezaFinanceiraPagamentoId,
                request.Descricao,
                request.Mascara,
                tenantId,
                userId);

            if (!plano.IsValid)
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            if (request.EmpresaIds != null)
                plano.SubstituirEmpresas(request.EmpresaIds, userId);

            _context.PlanosDeContas.Add(plano);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Plano de contas financeiro cadastrado com sucesso.", new { plano.Id });
        }
    }

    public class AtualizarPlanoDeContasFinanceiroCommandHandler : IRequestHandler<AtualizarPlanoDeContasFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarPlanoDeContasFinanceiroCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarPlanoDeContasFinanceiroCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var plano = await _context.PlanosDeContas
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plano == null)
                return CommandResult.Falha("Plano de contas financeiro não encontrado.");

            plano.Alterar(
                request.ConfiguracaoCodigoNaturezaFinanceiraRecebimentoId,
                request.ConfiguracaoCodigoNaturezaFinanceiraPagamentoId,
                request.Descricao,
                request.Mascara,
                userId);

            if (!plano.IsValid)
                return CommandResult.Falha(plano.Notifications.Select(n => n.Message));

            if (request.EmpresaIds != null)
            {
                await _context.Entry(plano).Collection(p => p.Empresas).LoadAsync(cancellationToken);
                plano.SubstituirEmpresas(request.EmpresaIds, userId);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Plano de contas financeiro atualizado com sucesso.");
        }
    }

    public class DeletarPlanoDeContasFinanceiroCommandHandler : IRequestHandler<DeletarPlanoDeContasFinanceiroCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarPlanoDeContasFinanceiroCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarPlanoDeContasFinanceiroCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var plano = await _context.PlanosDeContas.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (plano == null)
                return CommandResult.Falha("Plano de contas financeiro não encontrado.");

            plano.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Plano de contas financeiro excluído logicamente.");
        }
    }


    // ----- PLANO DE CONTAS FINANCEIRO ITEM HANDLERS -----
    public class CriarPlanoDeContasFinanceiroItemCommandHandler : IRequestHandler<CriarPlanoDeContasFinanceiroItemCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPlanoDeContasFinanceiroItemCommandHandler(ContextFinanceiro context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPlanoDeContasFinanceiroItemCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var planoExiste = await _context.PlanosDeContas.AnyAsync(p => p.Id == request.PlanoDeContasFinanceiroId, cancellationToken);
            if (!planoExiste)
                return CommandResult.Falha("Plano de contas financeiro não encontrado.");

            var item = new PlanoDeContasFinanceiroItem(
                request.PlanoDeContasFinanceiroId,
                request.Codigo,
                request.Descricao,
                request.TipoDetalhamento,
                request.MovimentaCaixa,
                tenantId,
                userId);

            if (!item.IsValid)
                return CommandResult.Falha(item.Notifications.Select(n => n.Message));

            _context.PlanoDeContasItens.Add(item);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Item do plano de contas cadastrado com sucesso.", new { item.Id });
        }
    }

    public class AtualizarPlanoDeContasFinanceiroItemCommandHandler : IRequestHandler<AtualizarPlanoDeContasFinanceiroItemCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarPlanoDeContasFinanceiroItemCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarPlanoDeContasFinanceiroItemCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var item = await _context.PlanoDeContasItens.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (item == null)
                return CommandResult.Falha("Item do plano de contas não encontrado.");

            item.Alterar(request.Codigo, request.Descricao, request.TipoDetalhamento, request.MovimentaCaixa, userId);

            if (!item.IsValid)
                return CommandResult.Falha(item.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Item do plano de contas atualizado com sucesso.");
        }
    }

    public class DeletarPlanoDeContasFinanceiroItemCommandHandler : IRequestHandler<DeletarPlanoDeContasFinanceiroItemCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarPlanoDeContasFinanceiroItemCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarPlanoDeContasFinanceiroItemCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var item = await _context.PlanoDeContasItens.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (item == null)
                return CommandResult.Falha("Item do plano de contas não encontrado.");

            item.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Item do plano de contas excluído logicamente.");
        }
    }


    // ----- CONFIGURACAO CODIGO NATUREZA FINANCEIRA HANDLERS -----
    public class CriarConfiguracaoCodigoNaturezaFinanceiraCommandHandler : IRequestHandler<CriarConfiguracaoCodigoNaturezaFinanceiraCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarConfiguracaoCodigoNaturezaFinanceiraCommandHandler(ContextFinanceiro context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarConfiguracaoCodigoNaturezaFinanceiraCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var userId = _currentUser.GetUserId() ?? "system";

            var natureza = new ConfiguracaoCodigoNaturezaFinanceira(
                request.EmpresaId,
                request.Descricao,
                request.TipoConfiguracaoNatureza,
                request.ItemPlanoDeContasFinanceiroDinheiroId,
                request.ItemPlanoDeContasFinanceiroCartaoChequeId,
                request.ItemPlanoDeContasFinanceiroCartaoCreditoId,
                request.ItemPlanoDeContasFinanceiroCartaoDebitoId,
                request.ItemPlanoDeContasFinanceiroCartaoDaLojaId,
                request.ItemPlanoDeContasFinanceiroValeAlimentacaoId,
                request.ItemPlanoDeContasFinanceiroValeRefeicaoId,
                request.ItemPlanoDeContasFinanceiroValePresenteId,
                request.ItemPlanoDeContasFinanceiroValeCombustivelId,
                request.ItemPlanoDeContasFinanceiroDuplicataMercantilId,
                request.ItemPlanoDeContasFinanceiroBoletoBancarioId,
                request.ItemPlanoDeContasFinanceiroDepositoBancarioId,
                request.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId,
                request.ItemPlanoDeContasFinanceiroTransferenciaBancariaId,
                request.ItemPlanoDeContasFinanceiroProgramaDeFidelidadeId,
                request.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId,
                request.ItemPlanoDeContasFinanceiroCreditoEmLojaId,
                request.ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId,
                request.ItemPlanoDeContasFinanceiroOutrosId,
                request.ItemPlanoDeContasFinanceiroDescontoId,
                request.ItemPlanoDeContasFinanceiroAcrescimoId,
                request.ItemPlanoDeContasFinanceiroJurosId,
                request.ItemPlanoDeContasFinanceiroMultaId,
                request.ItemPlanoDeContasFinanceiroTrocoId,
                tenantId,
                userId);

            if (!natureza.IsValid)
                return CommandResult.Falha(natureza.Notifications.Select(n => n.Message));

            _context.NaturezasFinanceiras.Add(natureza);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Configuração de natureza financeira cadastrada com sucesso.", new { natureza.Id });
        }
    }

    public class AtualizarConfiguracaoCodigoNaturezaFinanceiraCommandHandler : IRequestHandler<AtualizarConfiguracaoCodigoNaturezaFinanceiraCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public AtualizarConfiguracaoCodigoNaturezaFinanceiraCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AtualizarConfiguracaoCodigoNaturezaFinanceiraCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var natureza = await _context.NaturezasFinanceiras.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (natureza == null)
                return CommandResult.Falha("Configuração de natureza financeira não encontrada.");

            natureza.Alterar(
                request.EmpresaId,
                request.Descricao,
                request.TipoConfiguracaoNatureza,
                request.ItemPlanoDeContasFinanceiroDinheiroId,
                request.ItemPlanoDeContasFinanceiroCartaoChequeId,
                request.ItemPlanoDeContasFinanceiroCartaoCreditoId,
                request.ItemPlanoDeContasFinanceiroCartaoDebitoId,
                request.ItemPlanoDeContasFinanceiroCartaoDaLojaId,
                request.ItemPlanoDeContasFinanceiroValeAlimentacaoId,
                request.ItemPlanoDeContasFinanceiroValeRefeicaoId,
                request.ItemPlanoDeContasFinanceiroValePresenteId,
                request.ItemPlanoDeContasFinanceiroValeCombustivelId,
                request.ItemPlanoDeContasFinanceiroDuplicataMercantilId,
                request.ItemPlanoDeContasFinanceiroBoletoBancarioId,
                request.ItemPlanoDeContasFinanceiroDepositoBancarioId,
                request.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId,
                request.ItemPlanoDeContasFinanceiroTransferenciaBancariaId,
                request.ItemPlanoDeContasFinanceiroProgramaDeFidelidadeId,
                request.ItemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId,
                request.ItemPlanoDeContasFinanceiroCreditoEmLojaId,
                request.ItemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId,
                request.ItemPlanoDeContasFinanceiroOutrosId,
                request.ItemPlanoDeContasFinanceiroDescontoId,
                request.ItemPlanoDeContasFinanceiroAcrescimoId,
                request.ItemPlanoDeContasFinanceiroJurosId,
                request.ItemPlanoDeContasFinanceiroMultaId,
                request.ItemPlanoDeContasFinanceiroTrocoId,
                userId);

            if (!natureza.IsValid)
                return CommandResult.Falha(natureza.Notifications.Select(n => n.Message));

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResult.Ok("Configuração de natureza financeira atualizada com sucesso.");
        }
    }

    public class DeletarConfiguracaoCodigoNaturezaFinanceiraCommandHandler : IRequestHandler<DeletarConfiguracaoCodigoNaturezaFinanceiraCommand, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        private readonly ICurrentUser _currentUser;

        public DeletarConfiguracaoCodigoNaturezaFinanceiraCommandHandler(ContextFinanceiro context, ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarConfiguracaoCodigoNaturezaFinanceiraCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.GetUserId() ?? "system";
            var natureza = await _context.NaturezasFinanceiras.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (natureza == null)
                return CommandResult.Falha("Configuração de natureza financeira não encontrada.");

            natureza.Deletar(userId);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Configuração de natureza financeira excluída logicamente.");
        }
    }
}
