using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Interfaces;
using Epros.Modules.GestaoClientes.Application.Services;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epros.Modules.GestaoClientes.Application.Handlers
{
    /// <summary>
    /// 1.08B — RENOVAÇÃO por ciclo (fecha o desacoplamento assinatura↔recorrência e adiciona Anual/Vitalícia).
    /// Para cada <see cref="AssinaturaCliente"/> Ativa com <c>ProximaCobrancaEm &lt;= referência</c>:
    ///   • gera a próxima <see cref="Fatura"/> do plano;
    ///   • AVANÇA o vínculo do ciclo conforme <see cref="PlanoDuration"/> (Mensal → +1 mês; Anual → +1 ano);
    ///     planos Vitalícia têm <c>ProximaCobrancaEm == null</c> (cobrança única) e por isso nunca entram aqui;
    ///   • dispara a cobrança MÉTODO-AGNÓSTICA: cartão-on-file → débito automático; senão PIX real.
    /// Idempotente: o avanço de <c>ProximaCobrancaEm</c> impede gerar a fatura do mesmo ciclo duas vezes.
    ///
    /// ⛔ DIFERIDO: NÃO aplica reconhecimento de receita/diferimento de plano anual (REG-025). Só registra fatos.
    /// </summary>
    public class ProcessarRenovacaoAssinaturasCommandHandler : ICommandHandler<ProcessarRenovacaoAssinaturasCommand>
    {
        private readonly ContextGestaoClientes _context;
        private readonly ICurrentUser _currentUser;
        private readonly ICobrancaRecorrenteGateway _cobrancaRecorrente;
        private readonly IMediator _mediator;
        private readonly ILogger<ProcessarRenovacaoAssinaturasCommandHandler>? _logger;

        public ProcessarRenovacaoAssinaturasCommandHandler(
            ContextGestaoClientes context,
            ICurrentUser currentUser,
            ICobrancaRecorrenteGateway cobrancaRecorrente,
            IMediator mediator,
            ILogger<ProcessarRenovacaoAssinaturasCommandHandler>? logger = null)
        {
            _context = context;
            _currentUser = currentUser;
            _cobrancaRecorrente = cobrancaRecorrente;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<CommandResult> Handle(ProcessarRenovacaoAssinaturasCommand request, CancellationToken cancellationToken)
        {
            var referencia = request.Referencia ?? DateTime.UtcNow;
            var criadoPor = _currentUser.GetUserId() ?? "system";

            var assinaturas = await _context.AssinaturasClientes
                .Where(a => a.Status == AssinaturaStatus.Ativa
                            && a.ProximaCobrancaEm != null
                            && a.ProximaCobrancaEm <= referencia
                            && !a.Arquivada)
                .ToListAsync(cancellationToken);

            var faturasParaCobrar = new List<(Guid faturaId, Guid clienteId)>();
            var renovadas = 0;

            foreach (var assinatura in assinaturas)
            {
                var plano = await _context.Planos.FirstOrDefaultAsync(p => p.Id == assinatura.PlanoId, cancellationToken);
                if (plano == null || plano.Preco <= 0 || plano.Duration == PlanoDuration.Vitalicia)
                {
                    // Vitalícia/free não renova: desliga a âncora para não reprocessar.
                    assinatura.RegistrarRenovacao(null, criadoPor);
                    continue;
                }

                // 1.08E — CUPOM RECORRENTE: se a assinatura tem um cupom vinculado e ele ainda é válido
                // (validade + limite de uso), aplica o desconto na fatura do ciclo e registra o uso.
                // Reusa a matemática de desconto de AplicacaoCupom (mesma do fluxo de pedido — sem duplicar).
                Cupom? cupom = null;
                var valorCiclo = plano.Preco;
                if (assinatura.CupomId.HasValue)
                {
                    cupom = await _context.Cupons.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(c => c.Id == assinatura.CupomId.Value && c.DeletadoEm == null, cancellationToken);
                    var calc = AplicacaoCupom.Calcular(cupom, plano.Preco);
                    if (calc.Aplicou)
                        valorCiclo = calc.ValorFinal;
                    else
                        cupom = null; // cupom expirado/no limite/inativo → não aplica neste ciclo.
                }

                var fatura = new Fatura(
                    clienteId: assinatura.ClienteId,
                    valor: valorCiclo,
                    dataVencimento: referencia.AddDays(5),
                    tenantId: assinatura.TenantId,
                    criadoPor: criadoPor);

                if (fatura.IsValid)
                {
                    _context.Faturas.Add(fatura);
                    faturasParaCobrar.Add((fatura.Id, assinatura.ClienteId));

                    // Registra o uso do cupom recorrente nesta fatura do ciclo e incrementa o contador.
                    if (cupom != null)
                    {
                        _context.UsosCupons.Add(UsoCupom.ParaFatura(
                            assinatura.ClienteId, cupom.Id, fatura.Id, assinatura.TenantId, criadoPor));
                        cupom.IncrementarUso(criadoPor);
                    }
                }

                // Avança o ciclo a partir do vencimento agendado (mantém a cadência do plano).
                var proxima = FaturaLiquidacaoService.CalcularProximaCobranca(plano.Duration, assinatura.ProximaCobrancaEm!.Value);
                assinatura.RegistrarRenovacao(proxima, criadoPor);
                renovadas++;
            }

            if (assinaturas.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);

            // Cobrança método-agnóstica (best-effort) após persistir as faturas.
            foreach (var (faturaId, clienteId) in faturasParaCobrar)
            {
                try
                {
                    var fatura = await _context.Faturas.FirstOrDefaultAsync(f => f.Id == faturaId, cancellationToken);
                    if (fatura == null) continue;

                    if (await _cobrancaRecorrente.PossuiCartaoOnFileAsync(clienteId, cancellationToken))
                    {
                        await _cobrancaRecorrente.CobrarCartaoRecorrenteAsync(fatura, clienteId, cancellationToken);
                    }
                    else
                    {
                        var pix = await _mediator.Send(new GerarCobrancaPixCommand(faturaId), cancellationToken);
                        if (!pix.Sucesso)
                            _logger?.LogWarning("[Renovacao] Cobrança PIX de renovação não gerada agora para fatura {FaturaId}: {Msg}", faturaId, pix.Mensagem);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[Renovacao] Falha ao disparar cobrança de renovação para fatura {FaturaId}.", faturaId);
                }
            }

            return CommandResult.Ok("Renovação de assinaturas concluída.",
                new Dictionary<string, int> { { "AssinaturasRenovadas", renovadas }, { "FaturasGeradas", faturasParaCobrar.Count } });
        }
    }
}
