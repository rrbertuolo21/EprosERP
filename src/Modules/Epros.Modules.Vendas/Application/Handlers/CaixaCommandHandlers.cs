using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Handlers
{
    public class AbrirCaixaCommandHandler : ICommandHandler<AbrirCaixaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AbrirCaixaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AbrirCaixaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // Verifica se o operador já tem um caixa aberto
            var caixaAberto = await _context.Caixas
                .AnyAsync(c => c.TenantId == tenantId && c.OperadorId == request.OperadorId && c.Status == ECaixaStatus.Aberto, cancellationToken);

            if (caixaAberto)
            {
                return CommandResult.Falha("Este operador já possui uma sessão de caixa aberta.");
            }

            var caixa = new Caixa(
                Guid.NewGuid(),
                Guid.NewGuid(),
                request.OperadorId,
                request.SaldoAbertura,
                tenantId,
                usuario,
                DateTime.UtcNow
            );

            if (!caixa.IsValid)
            {
                return CommandResult.Falha(caixa.Notifications.Select(n => n.Message), "Dados de abertura de caixa inválidos.");
            }

            _context.Caixas.Add(caixa);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Caixa aberto com sucesso!", new { CaixaId = caixa.Id });
        }
    }

    public class FecharCaixaCommandHandler : ICommandHandler<FecharCaixaCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public FecharCaixaCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(FecharCaixaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var caixa = await _context.Caixas
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.CaixaId, cancellationToken);

            if (caixa == null)
            {
                return CommandResult.Falha("Sessão de caixa não encontrada.");
            }

            if (caixa.Status == ECaixaStatus.Fechado)
            {
                return CommandResult.Falha("Este caixa já está fechado.");
            }

            // Calcular saldo esperado
            var movimentos = await _context.CaixaMovimentos
                .Where(m => m.TenantId == tenantId && m.CaixaId == caixa.Id)
                .ToListAsync(cancellationToken);

            decimal totalMovimentos = 0;
            foreach (var mov in movimentos)
            {
                if (mov.Tipo == "Suprimento") totalMovimentos += mov.Valor;
                else if (mov.Tipo == "Sangria") totalMovimentos -= mov.Valor;
            }

            var totalVendas = await _context.Vendas
                .Where(v => v.TenantId == tenantId && v.CaixaId == caixa.Id.ToString())
                .Where(v => v.Status == EVendaStatus.Transmitido && !v.Cancelada)
                .SumAsync(v => v.Total, cancellationToken);

            decimal saldoCalculado = caixa.SaldoAbertura + totalMovimentos + totalVendas;

            caixa.Fechar(request.SaldoFechamento, saldoCalculado, usuario);

            if (!caixa.IsValid)
            {
                return CommandResult.Falha(caixa.Notifications.Select(n => n.Message), "Erro ao validar o fechamento do caixa.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Caixa fechado com sucesso!", new
            {
                CaixaId = caixa.Id,
                SaldoCalculado = saldoCalculado,
                Diferenca = caixa.DiferencaFechamento
            });
        }
    }

    public class RegistrarCaixaMovimentoCommandHandler : ICommandHandler<RegistrarCaixaMovimentoCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarCaixaMovimentoCommandHandler(ContextVendas context, ITenantProvider tenantProvider, ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarCaixaMovimentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var caixa = await _context.Caixas
                .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == request.CaixaId, cancellationToken);

            if (caixa == null)
            {
                return CommandResult.Falha("Sessão de caixa não encontrada.");
            }

            if (caixa.Status == ECaixaStatus.Fechado)
            {
                return CommandResult.Falha("Não é possível realizar movimentações em um caixa fechado.");
            }

            var movimento = new CaixaMovimento(
                Guid.NewGuid(),
                Guid.NewGuid(),
                caixa.Id,
                request.Tipo,
                request.Valor,
                request.Observacao,
                tenantId,
                usuario,
                DateTime.UtcNow
            );

            if (!movimento.IsValid)
            {
                return CommandResult.Falha(movimento.Notifications.Select(n => n.Message), "Movimentação de caixa inválida.");
            }

            _context.CaixaMovimentos.Add(movimento);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Movimentação registrada com sucesso!", new { MovimentoId = movimento.Id });
        }
    }
}
