using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>
    /// 1.08I — APURAÇÃO de comissão de parceiro (revenda/vendedor) por fatura. Registra o RESULTADO do
    /// mecanismo parametrizável: qual <see cref="Base"/> (bruto|líquido) e qual <see cref="Momento"/>
    /// (competência|caixa) foram usados, o valor-base aplicado e os valores apurados.
    ///
    /// MECANISMO universal (skill Negocio-acumulado/financeiro):
    ///   • Base bruto × líquido = PARÂMETRO comercial (RN07 — não há norma única; decisão de contrato).
    ///   • Momento competência × caixa = PARÂMETRO (RN08 — CPC 47 itens 91–94 para o caminho competência).
    ///
    /// ⚠️ O % de comissão vem de <c>Revenda.PercentualComissao</c> / <c>Vendedor.PercentualComissao</c>
    /// (já existentes) e a base/momento vêm de configuração — NADA é hardcoded aqui. Valores default do
    /// mecanismo (Bruto/Caixa) são apenas fallback seguro e estão marcados VALIDA CONTADOR.
    /// </summary>
    public class ApuracaoComissao : EntidadeSaaSBase
    {
        public Guid FaturaId { get; private set; }
        public Guid ClienteId { get; private set; }
        public Guid? RevendaId { get; private set; }
        public Guid? VendedorId { get; private set; }

        /// <summary>Base usada (PARÂMETRO lido da configuração).</summary>
        public BaseComissao Base { get; private set; }

        /// <summary>Momento usado (PARÂMETRO lido da configuração).</summary>
        public MomentoComissao Momento { get; private set; }

        /// <summary>Valor sobre o qual o % incidiu (bruto ou líquido, conforme <see cref="Base"/>).</summary>
        public decimal ValorBase { get; private set; }

        public decimal PercentualRevenda { get; private set; }
        public decimal PercentualVendedor { get; private set; }
        public decimal ValorComissaoRevenda { get; private set; }
        public decimal ValorComissaoVendedor { get; private set; }

        /// <summary>Competência de referência quando <see cref="Momento"/> = Competencia (1º dia do mês); null no caixa.</summary>
        public DateTime? Competencia { get; private set; }

        // --- Gancho de clawback (RN09 — estorno em cancelamento/chargeback) ---
        public bool Estornada { get; private set; }
        public DateTime? EstornadaEm { get; private set; }
        public string? MotivoEstorno { get; private set; }

        protected ApuracaoComissao() { } // EF Core

        public ApuracaoComissao(
            Guid faturaId,
            Guid clienteId,
            Guid? revendaId,
            Guid? vendedorId,
            BaseComissao baseComissao,
            MomentoComissao momento,
            decimal valorBase,
            decimal percentualRevenda,
            decimal percentualVendedor,
            string tenantId,
            string criadoPor,
            DateTime? competencia = null)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ApuracaoComissao>()
                .Requires()
                .AreNotEquals(faturaId, Guid.Empty, nameof(FaturaId), "FaturaId é obrigatório")
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "ClienteId é obrigatório")
                .IsGreaterThan(valorBase, -0.01m, nameof(ValorBase), "Valor-base deve ser maior ou igual a zero")
                .IsGreaterThan(percentualRevenda, -0.01m, nameof(PercentualRevenda), "Percentual de revenda inválido")
                .IsGreaterThan(percentualVendedor, -0.01m, nameof(PercentualVendedor), "Percentual de vendedor inválido")
            );

            FaturaId = faturaId;
            ClienteId = clienteId;
            RevendaId = revendaId;
            VendedorId = vendedorId;
            Base = baseComissao;
            Momento = momento;
            ValorBase = valorBase;
            PercentualRevenda = percentualRevenda;
            PercentualVendedor = percentualVendedor;
            // MECANISMO: valor = base × % (o % é PARÂMETRO vindo da Revenda/Vendedor).
            ValorComissaoRevenda = Math.Round(valorBase * (percentualRevenda / 100m), 2, MidpointRounding.AwayFromZero);
            ValorComissaoVendedor = Math.Round(valorBase * (percentualVendedor / 100m), 2, MidpointRounding.AwayFromZero);
            Competencia = momento == MomentoComissao.Competencia && competencia.HasValue
                ? ReconhecimentoReceita.NormalizarCompetencia(competencia.Value)
                : (DateTime?)null;
        }

        /// <summary>
        /// 1.08I — GANCHO DE CLAWBACK: estorna a comissão apurada em caso de cancelamento/chargeback
        /// dentro da janela. [Negocio-acumulado/financeiro RN09] · ⚠️ política de clawback = PARÂMETRO,
        /// VALIDA CONTADOR. Idempotente. O mecanismo apenas registra o estorno (não decide a janela).
        /// </summary>
        public bool Estornar(string motivo, string alteradoPor)
        {
            if (Estornada)
                return false;

            Estornada = true;
            EstornadaEm = DateTime.UtcNow;
            MotivoEstorno = motivo;
            MarcarAlterado(alteradoPor);
            return true;
        }
    }
}
