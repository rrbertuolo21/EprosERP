using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Handlers
{
    /// <summary>
    /// Baixa o estoque de uma venda faturada pelo MOTOR ÚNICO (kardex, D1).
    ///
    /// CORREÇÃO T3/D8: o handler RESPEITA o motor. Antes gravava o <see cref="MovimentoEstoque"/> físico
    /// mesmo quando o motor bloqueava a saída (saldo insuficiente e produto sem permissão de estoque
    /// negativo), divergindo o registro físico do saldo real, e não emitia evento. Agora:
    ///   - se o motor BLOQUEIA, NÃO grava o movimento nem o fato gerador (sem órfão/kardex divergente);
    ///   - se o motor APLICA, grava o movimento físico e EMITE o evento de saída no Outbox.
    /// Idempotente por (tenant + produto + "Venda ID").
    /// </summary>
    public class VendaFaturadaEstoqueHandler : INotificationHandler<VendaFaturadaEventNotification>
    {
        /// <summary>Evento de saída confirmada de estoque por venda faturada (Outbox).</summary>
        public const string EventoSaidaConfirmada = "est.vnd.saida_confirmada";

        private readonly ContextEstoque _context;

        public VendaFaturadaEstoqueHandler(ContextEstoque context)
        {
            _context = context;
        }

        public async Task Handle(VendaFaturadaEventNotification notification, CancellationToken cancellationToken)
        {
            // Baixa de estoque pelo MOTOR ÚNICO (kardex, D1).
            var motor = new MotorMovimentacaoEstoque(_context, notification.TenantId, notification.UserId);

            foreach (var item in notification.Itens)
            {
                // 1. Validar idempotência
                var historicoIdentificador = $"Venda ID: {notification.VendaId}";
                var jaProcessado = await _context.MovimentosEstoque
                    .IgnoreQueryFilters()
                    .AnyAsync(m => m.TenantId == notification.TenantId &&
                                   m.ProdutoId == item.ProdutoId &&
                                   m.Tipo == "Saida" &&
                                   m.Historico.Contains(historicoIdentificador), cancellationToken);

                if (jaProcessado)
                {
                    continue; // Pula item já processado
                }

                // 2. Carregar o produto para atualizar o saldo físico
                var produto = await _context.Produtos
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.TenantId == notification.TenantId && p.Id == item.ProdutoId, cancellationToken);

                if (produto == null)
                    continue;

                // Fato gerador criado, mas só PERSISTIDO se o motor aplicar a saída (evita fato órfão).
                var fato = new FatoGeradorEstoque(notification.VendaId, null, null, EOrigemFatoGeradorEstoque.SaidaFiscal, notification.TenantId, notification.UserId, referenciaExterna: historicoIdentificador);

                // Efetua a saída pelo kardex. O motor é a AUTORIDADE: se bloquear (D8: saldo insuficiente
                // sem permissão de estoque negativo), a saída NÃO acontece e nada físico é gravado.
                var resultado = await motor.AplicarSaidaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, item.ProdutoId, item.Quantidade, fato.Id, null, cancellationToken);
                if (!resultado.Sucesso)
                {
                    // Motor bloqueou: não grava MovimentoEstoque nem fato; não emite evento de saída.
                    // (Kardex e registro físico permanecem coerentes; a pendência é observável pelo saldo.)
                    continue;
                }

                _context.FatosGeradoresEstoque.Add(fato);

                // Cria o registro físico de movimentação de saída
                var movimento = new MovimentoEstoque(
                    produtoId: item.ProdutoId,
                    quantidade: item.Quantidade,
                    tipo: "Saida",
                    historico: $"Saída de Venda Faturada ({historicoIdentificador})",
                    tenantId: notification.TenantId,
                    criadoPor: notification.UserId
                );

                _context.MovimentosEstoque.Add(movimento);

                // EMITE o evento de saída no Outbox (efeito confirmado pelo motor).
                var payload = new
                {
                    VendaId = notification.VendaId,
                    ProdutoId = item.ProdutoId,
                    Quantidade = item.Quantidade,
                    FatoId = fato.Id,
                    TenantId = notification.TenantId
                };
                _context.OutboxMessages.Add(new OutboxMessage(
                    notification.TenantId, EventoSaidaConfirmada, JsonSerializer.Serialize(payload)));
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
