using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Domain.Services;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Services
{
    /// <summary>
    /// Wiring evento→ledger (FIN-CGL / TEC-8). Dado um fato de integração (VendaFaturada, CompraLancada,
    /// FolhaProcessada, ProjetoFaturado…), gera o <see cref="LancamentoContabil"/> por partida dobrada
    /// (cita Negocio-acumulado/contabil) usando o <see cref="MotorContabilizacao"/> e o DE-PARA
    /// evento→conta (<see cref="RegraContabilizacao"/>).
    ///
    /// ⛔ Regra #0 / valida-contador: a ESCOLHA das contas do plano do cliente é parâmetro do contador.
    /// Enquanto o de-para não estiver configurado (ou estiver incompleto), o motor cai no DEFAULT SEGURO:
    /// lança contra uma CONTA TRANSITÓRIA de contabilização, em estado RASCUNHO — que NÃO impacta saldos
    /// até um humano confirmar. Nunca se inventa número de conta contábil. O lançamento é idempotente por
    /// (tipo de evento + id de origem), gravado no NumeroLancamento.
    /// </summary>
    public static class ContabilizacaoEventoService
    {
        /// <summary>
        /// Adiciona (sem SaveChanges — o chamador salva) o lançamento contábil automático do evento, se
        /// ainda não existir. Retorna o lançamento gerado, ou null se já era idempotente/valor inválido.
        /// </summary>
        public static async Task<LancamentoContabil?> GerarLancamentoAsync(
            ContextFinanceiro context, string tenantId, string usuario,
            string tipoEvento, Guid origemId, decimal valor, string historicoBase, CancellationToken ct = default)
        {
            if (valor <= 0m) return null;

            var numeroLancamento = $"AUTO-{tipoEvento}-{origemId:N}";

            var jaExiste = await context.LancamentosContabeis
                .IgnoreQueryFilters()
                .AnyAsync(l => l.TenantId == tenantId && l.NumeroLancamento == numeroLancamento, ct);
            if (jaExiste) return null;

            var regra = await context.RegrasContabilizacao
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.TipoEvento == tipoEvento && r.Ativo, ct);

            var transitoriaId = await GarantirContaTransitoriaAsync(context, tenantId, usuario, ct);

            var contaDebito = regra?.ContaDebitoId ?? transitoriaId;
            var contaCredito = regra?.ContaCreditoId ?? transitoriaId;

            var completa = regra != null && regra.Completa;
            var historico = completa
                ? (regra!.Historico ?? historicoBase)
                : $"{historicoBase} — // valida-contador: configurar RegraContabilizacao para '{tipoEvento}' (conta transitória)";

            // Nasce em Rascunho (MotorContabilizacao) — não impacta saldos até confirmação humana.
            var lancamento = MotorContabilizacao.GerarPartidaSimples(
                periodoContabilId: null,
                numeroLancamento: numeroLancamento,
                data: DateTime.UtcNow,
                contaDebitoId: contaDebito,
                contaCreditoId: contaCredito,
                valor: valor,
                historico: historico,
                tenantId: tenantId,
                usuario: usuario);

            if (!lancamento.IsValid) return null;

            context.LancamentosContabeis.Add(lancamento);
            return lancamento;
        }

        /// <summary>
        /// Garante a existência de uma CONTA TRANSITÓRIA de contabilização automática (determinística por
        /// tenant) usada como default seguro quando o de-para não está configurado. Não é conta do plano
        /// do cliente — é uma conta técnica de backlog contábil (// valida-contador reclassifica depois).
        /// </summary>
        private static async Task<Guid> GarantirContaTransitoriaAsync(
            ContextFinanceiro context, string tenantId, string usuario, CancellationToken ct)
        {
            var id = FinanceiroFielFactory.GetDeterministicGuid($"ContaTransitoriaContabil_{tenantId}");

            var existe = await context.ContasContabeis.IgnoreQueryFilters().AnyAsync(c => c.Id == id, ct);
            if (existe) return id;

            var conta = new ContaContabil(
                codigoConta: "9.9.9.99",
                nomeConta: "Conta Transitoria de Contabilizacao Automatica",
                contaPaiId: null,
                nivel: 1,
                tipoConta: ETipoContaContabil.Passivo,
                aceitaLancamento: true,
                participaContabilidadeGeral: true,
                participaOrcamento: false,
                participaDepreciacao: false,
                tenantId: tenantId,
                criadoPor: usuario);
            ForcarId(conta, id);
            context.ContasContabeis.Add(conta);
            return id;
        }

        private static void ForcarId(object entidade, Guid id)
        {
            var field = typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(entidade, id);
        }
    }
}
