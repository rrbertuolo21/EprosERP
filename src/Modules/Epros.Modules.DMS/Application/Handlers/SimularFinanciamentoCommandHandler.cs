using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Domain.Entities;
using Epros.Modules.DMS.Domain.Financiamento;
using Epros.Modules.DMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.DMS.Application.Handlers
{
    /// <summary>
    /// NF-01 — Motor de cálculo F&amp;I. Aterra a skill AGNÓSTICA
    /// <c>Negocio-acumulado/financeiro/credito</c>: dado principal + taxa + prazo + sistema, roda o
    /// <see cref="MotorFinanciamento"/> (Price/SAC + IOF por vigência + CET por TIR) e persiste o
    /// resultado na <see cref="SimulacaoFin"/>. A TAXA praticada é parâmetro do cliente; as alíquotas
    /// de IOF são // valida-contador (<see cref="TabelaIof"/>). Idempotente por
    /// (TenantId, ChaveIdempotencia). Emite <c>con.fin.simulacao_calculada</c> pós-commit (Outbox).
    /// </summary>
    public class SimularFinanciamentoCommandHandler : ICommandHandler<SimularFinanciamentoCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public SimularFinanciamentoCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(SimularFinanciamentoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // Idempotência (skill T-06 / RN-CFIN): mesma chave → devolve a simulação já calculada.
            var existente = await _context.SimulacoesFin
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.ChaveIdempotencia == request.ChaveIdempotencia, cancellationToken);
            if (existente != null)
                return CommandResult.Ok("Simulação já processada (idempotência).", ProjetarResumo(existente));

            var saldo = request.PrecoVeiculo - request.Entrada;
            if (saldo <= 0m)
                return CommandResult.Falha("Não há saldo a financiar (entrada >= preço).");

            var sistema = ParsearSistema(request.Sistema);
            if (sistema == null)
                return CommandResult.Falha("Sistema de amortização inválido. Use 'Price' ou 'Sac'.");

            // Taxa periódica que casa com a parcela (skill Princípio 1): se veio a.a., converte por
            // equivalência COMPOSTA (nunca /12).
            var taxaMensal = request.TaxaAnual
                ? MotorFinanciamento.TaxaAnualParaMensal(request.TaxaJurosMensal)
                : request.TaxaJurosMensal;

            var custos = new CustosOperacao(
                TarifasFinanciadas: request.TarifasFinanciadas,
                TarifasDescontadas: request.TarifasDescontadas,
                SeguroPorParcela: request.SeguroPorParcela,
                IofFinanciado: request.IofFinanciado);

            var mutuario = request.PessoaJuridica ? TipoMutuario.PessoaJuridica : TipoMutuario.PessoaFisica;

            ResultadoSimulacao resultado;
            try
            {
                resultado = MotorFinanciamento.Simular(
                    principal: saldo,
                    taxaPeriodica: taxaMensal,
                    prazo: request.PrazoQuantidade,
                    sistema: sistema.Value,
                    custos: custos,
                    mutuario: mutuario,
                    dataOperacao: DateTime.UtcNow);
            }
            catch (ArgumentException ex)
            {
                return CommandResult.Falha(ex.Message);
            }

            var entidade = new SimulacaoFin(
                request.JornadaId,
                request.ChaveIdempotencia,
                request.PrecoVeiculo,
                request.Entrada,
                saldo,
                request.PrazoQuantidade,
                "Meses",
                request.OrigemVersao,
                memoriaJson: null,
                tenantId,
                usuario);

            if (!entidade.IsValid)
                return CommandResult.Falha(entidade.Notifications.Select(n => n.Message));

            var memoria = SerializarMemoria(resultado, taxaMensal, custos, mutuario);
            entidade.RegistrarCalculo(
                sistema: sistema.Value.ToString(),
                taxaJurosMensal: Math.Round(taxaMensal, 8, MidpointRounding.AwayFromZero),
                valorParcela: resultado.PrimeiraPrestacao,
                totalPago: resultado.TotalPago,
                totalJuros: resultado.TotalJuros,
                iof: resultado.Iof,
                cetAnual: resultado.CetAnual,
                memoriaJson: memoria,
                usuario: usuario);

            _context.SimulacoesFin.Add(entidade);

            // Evento pós-commit (Outbox) — CON-EVT F&I.
            _context.OutboxMessages.Add(new OutboxMessage(
                tenantId,
                CatalogoEventosIntegracao.Concessionarias.FinSimulacaoCalculada,
                JsonSerializer.Serialize(new
                {
                    simulacaoId = entidade.Id,
                    jornadaId = entidade.JornadaId,
                    sistema = entidade.Sistema,
                    valorParcela = entidade.ValorParcela,
                    cetAnual = entidade.CetAnual,
                    iof = entidade.Iof,
                    tenantId
                })));

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Simulação calculada com sucesso!", ProjetarResumo(entidade));
        }

        private static SistemaAmortizacao? ParsearSistema(string sistema)
        {
            if (string.IsNullOrWhiteSpace(sistema)) return null;
            return sistema.Trim().ToUpperInvariant() switch
            {
                "PRICE" => SistemaAmortizacao.Price,
                "SAC" => SistemaAmortizacao.Sac,
                _ => null
            };
        }

        private static object ProjetarResumo(SimulacaoFin s) => new
        {
            s.Id,
            s.JornadaId,
            s.PrecoVeiculo,
            s.Entrada,
            s.Saldo,
            s.PrazoQuantidade,
            s.Sistema,
            s.TaxaJurosMensal,
            s.ValorParcela,
            s.TotalPago,
            s.TotalJuros,
            s.Iof,
            s.CetAnual,
            s.Calculada
        };

        private static string SerializarMemoria(
            ResultadoSimulacao r, decimal taxaMensal, CustosOperacao custos, TipoMutuario mutuario) =>
            JsonSerializer.Serialize(new
            {
                fonte = "Negocio-acumulado/financeiro/credito (Price/SAC + IOF Dec.6.306/2007 + CET/TIR)",
                sistema = r.Sistema.ToString(),
                principalFinanciado = r.Principal,
                taxaJurosMensal = taxaMensal,
                taxaJurosAnual = r.TaxaJurosAnual,
                prazo = r.Prazo,
                primeiraPrestacao = r.PrimeiraPrestacao,
                totalPago = r.TotalPago,
                totalJuros = r.TotalJuros,
                iof = r.Iof,
                cetAnual = r.CetAnual,
                mutuario = mutuario.ToString(),
                iofFinanciado = custos.IofFinanciado,
                parcelas = r.Parcelas.Select(p => new
                {
                    p.Numero,
                    p.Prestacao,
                    p.Juros,
                    p.Amortizacao,
                    p.SaldoDevedor
                })
            });
    }
}
