using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>Baixa (quitação de) Fatura.</summary>
    public class BaixarFaturaCommandHandler : ICommandHandler<BaixarFaturaCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;

        public BaixarFaturaCommandHandler(
            ContextGestaoClientes context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(BaixarFaturaCommand request, CancellationToken cancellationToken)
        {
            var alteradoPor = _currentUser.GetUserId() ?? "system";

            // Busca a fatura aplicando o QueryFilter automático de Tenant e Soft-Delete
            var fatura = await _context.Faturas.FirstOrDefaultAsync(f => f.Id == request.FaturaId, cancellationToken);
            if (fatura == null)
            {
                return CommandResult.Falha("Fatura não encontrada para o inquilino atual.");
            }

            // Busca o cliente
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == fatura.ClienteId, cancellationToken);
            decimal percentualRevenda = 0;
            decimal percentualVendedor = 0;

            if (cliente != null)
            {
                if (cliente.RevendaId.HasValue)
                {
                    var revenda = await _context.Revendas.FirstOrDefaultAsync(r => r.Id == cliente.RevendaId.Value && r.Ativo, cancellationToken);
                    if (revenda != null)
                    {
                        percentualRevenda = revenda.PercentualComissao;
                    }
                }

                if (cliente.VendedorId.HasValue)
                {
                    var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id == cliente.VendedorId.Value && v.Ativo, cancellationToken);
                    if (vendedor != null)
                    {
                        percentualVendedor = vendedor.PercentualComissao;
                    }
                }
            }

            fatura.CalcularSplitComissao(percentualRevenda, percentualVendedor);

            fatura.Baixar(alteradoPor);

            if (!fatura.IsValid)
            {
                var erros = fatura.Notifications.Select(n => n.Message);
                return CommandResult.Falha(erros, "Invariantes de domínio da fatura não foram atendidas.");
            }

            // Grava o evento ComissaoApuradaEvent no Outbox do monólito
            var payloadComissao = new
            {
                FaturaId = fatura.Id,
                ClienteId = fatura.ClienteId,
                ValorTotal = fatura.Valor,
                PercentualComissaoRevenda = fatura.PercentualComissaoRevenda,
                PercentualComissaoVendedor = fatura.PercentualComissaoVendedor,
                ValorComissaoRevenda = fatura.ValorComissaoRevenda,
                ValorComissaoVendedor = fatura.ValorComissaoVendedor,
                ApuradoEm = DateTime.UtcNow
            };

            var payloadJson = JsonSerializer.Serialize(payloadComissao);
            var outboxMessage = new OutboxMessage(fatura.TenantId, "ComissaoApuradaEvent", payloadJson);
            _context.OutboxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Fatura baixada com sucesso!", new { FaturaId = fatura.Id });
        }
    }
}
