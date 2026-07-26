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
    public class SincronizarCaixasCommandHandler : ICommandHandler<SincronizarCaixasCommand>
    {
        private readonly ContextVendas _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public SincronizarCaixasCommandHandler(
            ContextVendas context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SincronizarCaixasCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            if (request.Caixas == null || !request.Caixas.Any())
            {
                return CommandResult.Ok("Nenhum caixa enviado para sincronização.");
            }

            int caixasAdicionados = 0;
            int caixasFechados = 0;

            foreach (var caixaInput in request.Caixas)
            {
                // Busca se o caixa existe
                var caixaExistente = await _context.Caixas
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.TenantId == tenantId && (c.SyncId == caixaInput.SyncId || c.Id == caixaInput.Id), cancellationToken);

                Caixa caixa;

                if (caixaExistente == null)
                {
                    caixa = new Caixa(
                        caixaInput.Id == Guid.Empty ? Guid.NewGuid() : caixaInput.Id,
                        caixaInput.SyncId == Guid.Empty ? Guid.NewGuid() : caixaInput.SyncId,
                        caixaInput.OperadorId,
                        caixaInput.SaldoAbertura,
                        tenantId,
                        usuario,
                        caixaInput.CriadoEm
                    );

                    if (!caixa.IsValid)
                    {
                        return CommandResult.Falha(caixa.Notifications.Select(n => n.Message), $"Erro na validação do caixa offline {caixaInput.SyncId}");
                    }

                    _context.Caixas.Add(caixa);
                    caixasAdicionados++;
                }
                else
                {
                    caixa = caixaExistente;
                }

                // Sincronizar os movimentos desse caixa
                decimal totalMovimentos = 0;
                foreach (var movInput in caixaInput.Movimentos)
                {
                    var movExiste = await _context.CaixaMovimentos
                        .IgnoreQueryFilters()
                        .AnyAsync(m => m.TenantId == tenantId && (m.SyncId == movInput.SyncId || m.Id == movInput.Id), cancellationToken);

                    if (!movExiste)
                    {
                        var mov = new CaixaMovimento(
                            movInput.Id == Guid.Empty ? Guid.NewGuid() : movInput.Id,
                            movInput.SyncId == Guid.Empty ? Guid.NewGuid() : movInput.SyncId,
                            caixa.Id,
                            movInput.Tipo,
                            movInput.Valor,
                            movInput.Observacao,
                            tenantId,
                            usuario,
                            movInput.CriadoEm
                        );

                        if (!mov.IsValid)
                        {
                            return CommandResult.Falha(mov.Notifications.Select(n => n.Message), $"Erro na validação da movimentação de caixa {movInput.SyncId}");
                        }

                        _context.CaixaMovimentos.Add(mov);
                    }

                    if (movInput.Tipo == "Suprimento")
                    {
                        totalMovimentos += movInput.Valor;
                    }
                    else if (movInput.Tipo == "Sangria")
                    {
                        totalMovimentos -= movInput.Valor;
                    }
                }

                // Se o caixa veio fechado e no banco está aberto, fecha no banco
                if (caixaInput.Status == "Fechado" && caixa.Status == ECaixaStatus.Aberto)
                {
                    // Calcular vendas em dinheiro vinculadas a este caixa no monólito
                    // Como chave de caixa_id nas vendas pode ser texto ou Guid, comparamos com Id e SyncId
                    var totalVendas = await _context.Vendas
                        .Where(v => v.TenantId == tenantId && (v.CaixaId == caixa.Id.ToString() || v.CaixaId == caixa.SyncId.ToString()))
                        .Where(v => v.Status == EVendaStatus.Transmitido && !v.Cancelada)
                        .SumAsync(v => v.Total, cancellationToken);

                    decimal saldoCalculado = caixa.SaldoAbertura + totalMovimentos + totalVendas;

                    caixa.Fechar(caixaInput.SaldoFechamento ?? 0m, saldoCalculado, usuario);

                    if (!caixa.IsValid)
                    {
                        return CommandResult.Falha(caixa.Notifications.Select(n => n.Message), $"Erro ao fechar o caixa offline {caixa.SyncId}");
                    }

                    caixasFechados++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok($"Sincronização de caixas concluída. Adicionados: {caixasAdicionados}, Fechados: {caixasFechados}", new { Adicionados = caixasAdicionados, Fechados = caixasFechados });
        }
    }
}
