using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Interfaces;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>
    /// 1.08A (fecha a lacuna 1.07 §3.c) — Ao FIM DO TRIAL, gera a 1ª <see cref="Fatura"/> do plano e
    /// dispara a cobrança de forma MÉTODO-AGNÓSTICA:
    ///   • se houver cartão-on-file → gancho <see cref="ICobrancaRecorrenteGateway.CobrarCartaoRecorrenteAsync"/>
    ///     (no-op/log na passada A; cartão entra na passada B);
    ///   • senão → gera cobrança PIX real (via <c>GerarCobrancaPixCommand</c>), que agora concilia pelo
    ///     motor de webhook unificado.
    /// Move o <see cref="StatusSaaS"/> do tenant de <c>TrialGratuito</c> → <c>AguardandoPagamento</c> (§12).
    /// A transição para bloqueio (Falha / somente-leitura, REG-021) ao não pagar dentro da tolerância do
    /// plano é feita pelo <c>VerificarFaturasVencidasJob</c> (fatura vencida → suspensão).
    ///
    /// Idempotente: só age em assinaturas com <c>TrialAte &lt; referência</c> e <c>TrialConvertidoEm == null</c>;
    /// ao processar, marca <see cref="AssinaturaCliente.MarcarTrialConvertido"/> — não gera fatura dupla no ciclo.
    ///
    /// ⛔ DIFERIDO: NÃO aplica reconhecimento de receita/diferimento de plano anual (REG-025), NÃO afirma
    /// que a tolerância de inadimplência está juridicamente validada. Apenas registra fatos.
    /// </summary>
    public class EncerrarTrialsExpiradosCommandHandler : ICommandHandler<EncerrarTrialsExpiradosCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        private readonly ICobrancaRecorrenteGateway _cobrancaRecorrente;
        private readonly IMediator _mediator;
        private readonly ILogger<EncerrarTrialsExpiradosCommandHandler>? _logger;

        public EncerrarTrialsExpiradosCommandHandler(
            ContextGestaoClientes context,
            ICurrentUser currentUser,
            ICobrancaRecorrenteGateway cobrancaRecorrente,
            IMediator mediator,
            ILogger<EncerrarTrialsExpiradosCommandHandler>? logger = null)
        {
            _context = context;
            _currentUser = currentUser;
            _cobrancaRecorrente = cobrancaRecorrente;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<CommandResult> Handle(EncerrarTrialsExpiradosCommand request, CancellationToken cancellationToken)
        {
            var referencia = request.Referencia ?? DateTime.UtcNow;
            var criadoPor = _currentUser.GetUserId() ?? "system";

            // Trials expirados ainda não convertidos (query filters ON → apenas o tenant corrente).
            var assinaturas = await _context.AssinaturasClientes
                .Where(a => a.Status == AssinaturaStatus.Ativa
                            && a.TrialAte != null
                            && a.TrialAte < referencia
                            && a.TrialConvertidoEm == null
                            && !a.Arquivada)
                .ToListAsync(cancellationToken);

            var faturasParaCobrar = new List<(Guid faturaId, Guid clienteId)>();
            var trialsEncerrados = 0;

            foreach (var assinatura in assinaturas)
            {
                var plano = await _context.Planos.FirstOrDefaultAsync(p => p.Id == assinatura.PlanoId, cancellationToken);

                // Gera a 1ª fatura apenas quando há valor a cobrar (plano pago). Plano free → apenas transita
                // o StatusSaaS (o tenant precisa contratar um plano pago para seguir).
                if (plano != null && plano.Preco > 0)
                {
                    var fatura = new Fatura(
                        clienteId: assinatura.ClienteId,
                        valor: plano.Preco,
                        dataVencimento: referencia.AddDays(5),
                        tenantId: assinatura.TenantId,
                        criadoPor: criadoPor);

                    if (fatura.IsValid)
                    {
                        _context.Faturas.Add(fatura);
                        faturasParaCobrar.Add((fatura.Id, assinatura.ClienteId));
                    }
                }

                // §12: fim do trial → aguardando pagamento.
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == assinatura.ClienteId, cancellationToken);
                cliente?.AtualizarStatusSaaS(StatusSaaS.AguardandoPagamento, criadoPor);

                assinatura.MarcarTrialConvertido(criadoPor);
                trialsEncerrados++;
            }

            if (trialsEncerrados > 0)
                await _context.SaveChangesAsync(cancellationToken);

            // Dispara a cobrança método-agnóstica (best-effort) após persistir as faturas.
            foreach (var (faturaId, clienteId) in faturasParaCobrar)
            {
                try
                {
                    var fatura = await _context.Faturas.FirstOrDefaultAsync(f => f.Id == faturaId, cancellationToken);
                    if (fatura == null) continue;

                    if (await _cobrancaRecorrente.PossuiCartaoOnFileAsync(clienteId, cancellationToken))
                    {
                        // Passada A: no-op/log (cartão-on-file deferido para a passada B).
                        await _cobrancaRecorrente.CobrarCartaoRecorrenteAsync(fatura, clienteId, cancellationToken);
                    }
                    else
                    {
                        // Caminho PIX real (que agora concilia pelo webhook unificado). Best-effort:
                        // sem gateway configurado o comando falha graciosamente e o cliente ainda pode
                        // gerar o PIX pela área do cliente.
                        var pix = await _mediator.Send(new GerarCobrancaPixCommand(faturaId), cancellationToken);
                        if (!pix.Sucesso)
                            _logger?.LogWarning("[Trial] Cobrança PIX de fim-de-trial não gerada agora para fatura {FaturaId}: {Msg}", faturaId, pix.Mensagem);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[Trial] Falha ao disparar cobrança de fim-de-trial para fatura {FaturaId}.", faturaId);
                }
            }

            return CommandResult.Ok("Encerramento de trials concluído.",
                new Dictionary<string, int> { { "TrialsEncerrados", trialsEncerrados }, { "FaturasGeradas", faturasParaCobrar.Count } });
        }
    }
}
