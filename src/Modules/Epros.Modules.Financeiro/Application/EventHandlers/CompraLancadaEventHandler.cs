using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.EventHandlers
{
    /// <summary>
    /// Consome o evento CompraLancada publicado pelo módulo de Estoque no Outbox.
    /// Cria automaticamente um título a pagar (modelo FIEL: ContasAPagar + FatoGeradorFinanceiro)
    /// para o fornecedor da nota de entrada.
    /// </summary>
    public class CompraLancadaEventHandler : INotificationHandler<CompraLancadaEventNotification>
    {
        private readonly ContextFinanceiro _context;

        public CompraLancadaEventHandler(ContextFinanceiro context)
        {
            _context = context;
        }

        public async Task Handle(CompraLancadaEventNotification notification, CancellationToken cancellationToken)
        {
            // Idempotência: não gera duas contas a pagar para a mesma compra.
            var jaProcessado = await _context.ContasAPagarAgregado
                .IgnoreQueryFilters()
                .AnyAsync(cp => cp.TenantId == notification.TenantId
                             && cp.FatoGeradorFinanceiro.CompraId == notification.CompraId, cancellationToken);

            if (jaProcessado)
                return;

            var planoItemId = await FinanceiroFielFactory.GarantirPlanoItemPadraoAsync(
                _context, notification.TenantId, "system", cancellationToken);

            var fato = FinanceiroFielFactory.CriarFatoGerador(_context, EOrigem.EmissaoEntradaNFe,
                null, notification.CompraId,
                $"Compra NF {notification.NumeroNota}",
                notification.TenantId, notification.UserId);

            var vencimento = notification.DataVencimento != default
                ? notification.DataVencimento
                : DateTime.UtcNow.AddDays(30).Date;

            var conta = new ContasAPagar(
                pessoaId: notification.FornecedorId,
                planoDeContasFinanceiroItemId: planoItemId,
                fatoGeradorFinanceiroId: fato.Id,
                nomePessoa: null,
                situacao: ESituacao.Aberto,
                dataVencimento: vencimento,
                dataEmissao: DateTime.UtcNow.Date,
                dataBaixa: null,
                documento: (notification.NumeroNota ?? string.Empty).Length > 30
                    ? notification.NumeroNota!.Substring(0, 30)
                    : notification.NumeroNota ?? string.Empty,
                valorTitulo: notification.ValorTotal,
                valorInicialDesconto: 0m,
                valorInicialMulta: 0m,
                valorInicialJuros: 0m,
                valorInicialAcrescimo: 0m,
                numeroParcela: 1,
                detalhamento: $"Gerado automaticamente a partir da Compra ID {notification.CompraId}",
                justificativaCancelamento: null,
                tenantId: notification.TenantId,
                criadoPor: "system");

            if (!conta.IsValid)
                return;

            _context.ContasAPagarAgregado.Add(conta);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
