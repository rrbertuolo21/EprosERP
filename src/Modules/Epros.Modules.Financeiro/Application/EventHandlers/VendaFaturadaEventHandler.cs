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
    /// Consome o evento VendaFaturada publicado pelo módulo Vendas no Outbox.
    /// Cria automaticamente um título a receber (modelo FIEL: ContasAReceber + FatoGeradorFinanceiro).
    /// </summary>
    public class VendaFaturadaEventHandler : INotificationHandler<VendaFaturadaEventNotification>
    {
        private readonly ContextFinanceiro _context;

        public VendaFaturadaEventHandler(ContextFinanceiro context)
        {
            _context = context;
        }

        public async Task Handle(VendaFaturadaEventNotification notification, CancellationToken cancellationToken)
        {
            // Idempotência: verifica se já existe um título a receber gerado por esta venda (via FatoGerador).
            var jaProcessado = await _context.ContasAReceberAgregado
                .IgnoreQueryFilters()
                .AnyAsync(cr => cr.TenantId == notification.TenantId
                             && cr.FatoGeradorFinanceiro.VendaId == notification.VendaId, cancellationToken);

            if (jaProcessado)
                return;

            // Resolve o cliente: usa um cliente existente do tenant ou cria o "Cliente Balcão" determinístico.
            var cliente = await _context.PessoasLookup
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.TenantId == notification.TenantId && p.EhCliente, cancellationToken);

            Guid clienteId;

            if (cliente != null)
            {
                clienteId = cliente.Id;
            }
            else
            {
                clienteId = FinanceiroFielFactory.GetDeterministicGuid($"DefaultCliente_{notification.TenantId}");

                var clienteExiste = await _context.PessoasLookup
                    .IgnoreQueryFilters()
                    .AnyAsync(p => p.Id == clienteId, cancellationToken);

                if (!clienteExiste)
                {
                    _context.PessoasLookup.Add(new PessoaLookup
                    {
                        Id = clienteId,
                        EhCliente = true,
                        Status = 3,
                        TipoPessoa = 1,
                        TenantId = notification.TenantId,
                        CriadoPor = "system_outbox",
                        CriadoEm = DateTime.UtcNow
                    });

                    _context.PessoasFisicasLookup.Add(new PessoaFisicaLookup
                    {
                        PessoaId = clienteId,
                        Cpf = "00000000000",
                        Nome = "Cliente Balcão",
                        TenantId = notification.TenantId,
                        CriadoPor = "system_outbox",
                        CriadoEm = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            var planoItemId = await FinanceiroFielFactory.GarantirPlanoItemPadraoAsync(
                _context, notification.TenantId, "system_outbox", cancellationToken);

            var fato = FinanceiroFielFactory.CriarFatoGerador(_context, EOrigem.EmissaoSaidaNFce,
                notification.VendaId, null,
                $"Venda PDV {notification.VendaId.ToString().Substring(0, 8)}",
                notification.TenantId, notification.UserId);

            var vencimento = notification.CriadoEm.AddDays(30).Date;
            var documento = notification.VendaId.ToString().Substring(0, 8);

            var conta = new ContasAReceber(
                pessoaId: clienteId,
                planoDeContasFinanceiroItemId: planoItemId,
                fatoGeradorFinanceiroId: fato.Id,
                nomePessoa: cliente == null ? "Cliente Balcão" : null,
                situacao: ESituacao.Aberto,
                dataVencimento: vencimento,
                dataEmissao: notification.CriadoEm.Date,
                dataBaixa: null,
                documento: documento,
                valorTitulo: notification.Total,
                valorInicialDesconto: 0m,
                valorInicialMulta: 0m,
                valorInicialJuros: 0m,
                valorInicialAcrescimo: 0m,
                numeroParcela: 1,
                detalhamento: $"Gerada automaticamente a partir da Venda ID {notification.VendaId}",
                justificativaCancelamento: null,
                tenantId: notification.TenantId,
                criadoPor: notification.UserId);

            if (!conta.IsValid)
                return;

            _context.ContasAReceberAgregado.Add(conta);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
