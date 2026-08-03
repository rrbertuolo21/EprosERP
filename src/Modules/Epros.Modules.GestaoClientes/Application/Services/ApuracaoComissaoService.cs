using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.GestaoClientes.Application.Services
{
    /// <summary>
    /// 1.08I — MECANISMO de APURAÇÃO de comissão de parceiro, parametrizável. Lê a BASE (bruto|líquido) e o
    /// MOMENTO (competência|caixa) de <see cref="ConfiguracaoGlobal"/> (chaves globais do landlord) e calcula
    /// o valor da comissão por fatura, registrando uma <see cref="ApuracaoComissao"/>. Complementa o
    /// <see cref="ComissaoApuradaEvent"/> factual e os campos <c>ValorComissaoRevenda/Vendedor</c> da Fatura.
    /// NÃO chama SaveChanges (o orquestrador decide o commit).
    ///
    /// Base normativa (skill Negocio-acumulado/financeiro):
    ///   • Base bruto × líquido = PARÂMETRO comercial (RN07 — decisão de contrato, sem norma única).
    ///   • Momento competência × caixa = PARÂMETRO (RN08 — competência casa despesa×receita, CPC 47 91–94).
    ///   • Clawback (estorno em cancelamento/chargeback) = PARÂMETRO (RN09) — ver <see cref="AcionarClawbackAsync"/>.
    ///
    /// ⚠️ O % de comissão vem de <c>Revenda/Vendedor.PercentualComissao</c> (não hardcoded). Base/momento vêm
    /// da config; na ausência usa DEFAULT SEGURO (<see cref="BaseDefault"/>/<see cref="MomentoDefault"/>),
    /// marcado VALIDA CONTADOR. NENHUM número fiscal é inventado aqui.
    /// </summary>
    public class ApuracaoComissaoService
    {
        /// <summary>Chave de configuração global da base de comissão (valor: "Bruto"|"Liquido").</summary>
        public const string ChaveBaseComissao = "comissao.base";

        /// <summary>Chave de configuração global do momento da comissão (valor: "Competencia"|"Caixa").</summary>
        public const string ChaveMomentoComissao = "comissao.momento";

        /// <summary>Default seguro da base (RN07) — ⚠️ VALIDA CONTADOR. Bruto = valor cheio cobrado.</summary>
        public const BaseComissao BaseDefault = BaseComissao.Bruto;

        /// <summary>Default seguro do momento (RN08-b) — ⚠️ VALIDA CONTADOR. Caixa = no recebimento.</summary>
        public const MomentoComissao MomentoDefault = MomentoComissao.Caixa;

        private readonly ContextGestaoClientes _context;

        public ApuracaoComissaoService(ContextGestaoClientes context)
        {
            _context = context;
        }

        /// <summary>
        /// Lê os PARÂMETROS de comissão da <see cref="ConfiguracaoGlobal"/>. Na ausência de linha (ou valor
        /// não-parseável) usa o default seguro documentado. Troca o parâmetro → muda o cálculo.
        /// </summary>
        public async Task<(BaseComissao Base, MomentoComissao Momento)> LerParametrosAsync(CancellationToken cancellationToken)
        {
            var baseConfig = await LerValorAsync(ChaveBaseComissao, cancellationToken);
            var momentoConfig = await LerValorAsync(ChaveMomentoComissao, cancellationToken);

            var baseComissao = Enum.TryParse<BaseComissao>(baseConfig, ignoreCase: true, out var b) ? b : BaseDefault;
            var momento = Enum.TryParse<MomentoComissao>(momentoConfig, ignoreCase: true, out var m) ? m : MomentoDefault;
            return (baseComissao, momento);
        }

        private async Task<string?> LerValorAsync(string chave, CancellationToken cancellationToken)
        {
            var cfg = await _context.ConfiguracoesGlobais
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Chave == chave && c.DeletadoEm == null, cancellationToken);
            return cfg?.Valor;
        }

        /// <summary>
        /// Apura (SEM persistir SaveChanges) a comissão da fatura conforme os PARÂMETROS lidos da config.
        /// A base bruto usa <c>fatura.Valor</c>; a base líquido usa <paramref name="valorLiquido"/> (retorno
        /// real do gateway — RN02) quando informado, senão cai para o bruto. Idempotente por fatura.
        /// </summary>
        public async Task<ApuracaoComissao?> ApurarAsync(
            Fatura fatura,
            decimal? valorLiquido,
            string criadoPor,
            CancellationToken cancellationToken,
            DateTime? competencia = null)
        {
            var jaApurada = await _context.ApuracoesComissao
                .IgnoreQueryFilters()
                .AnyAsync(a => a.FaturaId == fatura.Id && a.DeletadoEm == null, cancellationToken);
            if (jaApurada)
                return null;

            var (baseComissao, momento) = await LerParametrosAsync(cancellationToken);

            var cliente = await _context.Clientes.IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == fatura.ClienteId, cancellationToken);

            decimal percentualRevenda = 0m, percentualVendedor = 0m;
            Guid? revendaId = null, vendedorId = null;
            if (cliente != null)
            {
                if (cliente.RevendaId.HasValue)
                {
                    var revenda = await _context.Revendas.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(r => r.Id == cliente.RevendaId.Value && r.Ativo, cancellationToken);
                    if (revenda != null)
                    {
                        percentualRevenda = revenda.PercentualComissao; // % = PARÂMETRO (não hardcoded)
                        revendaId = revenda.Id;
                    }
                }
                if (cliente.VendedorId.HasValue)
                {
                    var vendedor = await _context.Vendedores.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(v => v.Id == cliente.VendedorId.Value && v.Ativo, cancellationToken);
                    if (vendedor != null)
                    {
                        percentualVendedor = vendedor.PercentualComissao; // % = PARÂMETRO
                        vendedorId = vendedor.Id;
                    }
                }
            }

            // MECANISMO: escolhe a base conforme o PARÂMETRO. Líquido só quando há retorno real do gateway.
            var valorBase = baseComissao == BaseComissao.Liquido
                ? (valorLiquido ?? fatura.Valor)
                : fatura.Valor;

            var apuracao = new ApuracaoComissao(
                fatura.Id, fatura.ClienteId, revendaId, vendedorId,
                baseComissao, momento, valorBase,
                percentualRevenda, percentualVendedor,
                fatura.TenantId, criadoPor,
                competencia: competencia ?? DateTime.UtcNow);

            await _context.ApuracoesComissao.AddAsync(apuracao, cancellationToken);
            return apuracao;
        }

        /// <summary>
        /// 1.08I — GANCHO DE CLAWBACK (RN09): ao cancelar/estornar a fatura, estorna as apurações de
        /// comissão dela. ⚠️ A JANELA e a política (estorna sempre? só dentro de N dias?) são PARÂMETRO —
        /// VALIDA CONTADOR. O mecanismo só executa o estorno registrado. Retorna quantas apurações estornou.
        /// NÃO persiste (o orquestrador commita).
        /// </summary>
        public async Task<int> AcionarClawbackAsync(
            Guid faturaId,
            string motivo,
            string alteradoPor,
            CancellationToken cancellationToken)
        {
            var apuracoes = await _context.ApuracoesComissao
                .IgnoreQueryFilters()
                .Where(a => a.FaturaId == faturaId && !a.Estornada && a.DeletadoEm == null)
                .ToListAsync(cancellationToken);

            var estornadas = 0;
            foreach (var apuracao in apuracoes)
            {
                if (apuracao.Estornar(motivo, alteradoPor))
                    estornadas++;
            }
            return estornadas;
        }
    }
}
