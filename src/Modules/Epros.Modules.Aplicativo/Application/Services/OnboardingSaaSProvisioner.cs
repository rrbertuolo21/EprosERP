using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Services
{
    /// <summary>
    /// 1.07 (APP-TEN-002) — Lógica de provisionamento SaaS do onboarding, extraída para um ponto único
    /// reutilizado pelo self-register (RegistrarNovoTenantCommandHandler) e pelo handler avulso de
    /// Cliente SaaS (CriarClienteSaaSOnboardingCommandHandler) — sem duplicar regra.
    ///
    /// Fecha a lacuna §3.2 da verificação: o tenant nascia bloqueado porque ObterContextoSessao faz
    /// <c>if (cliente == null) block = true</c>. Aqui o self-register passa a criar, na MESMA transação
    /// atômica, um Cliente SaaS em <see cref="StatusSaaS.TrialGratuito"/> + assinatura com data-fim de
    /// trial, vinculados a um plano trial (global/sistema se existir, senão criado por tenant), de modo
    /// que o tenant opera na hora, sem pagar (fim do trial recai no gating de StatusSaaS — 1.01/REG-021).
    /// </summary>
    public static class OnboardingSaaSProvisioner
    {
        /// <summary>Chave da ConfiguracaoGlobal (1.01) que define a duração do trial em dias.</summary>
        public const string ChaveTrialDays = "trial_days";

        /// <summary>Duração padrão do trial quando a ConfiguracaoGlobal <c>trial_days</c> está ausente.</summary>
        public const int TrialDaysPadrao = 30;

        /// <summary>Nome canônico do plano trial (marca de trial usada para resolvê-lo idempotentemente).</summary>
        public const string NomePlanoTrial = "Trial Gratuito";

        /// <summary>
        /// Lê a duração do trial (em dias) de <c>ConfiguracaoGlobal.trial_days</c>; retorna
        /// <see cref="TrialDaysPadrao"/> (30) quando ausente ou não numérica. Nunca inventa a regra:
        /// o default é o valor de negócio já acordado na 1.01.
        /// </summary>
        public static async Task<int> ObterTrialDaysAsync(ContextGestaoClientes ctx, CancellationToken ct)
        {
            var cfg = await ctx.Set<ConfiguracaoGlobal>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Chave == ChaveTrialDays && c.DeletadoEm == null, ct);

            if (cfg != null && int.TryParse(cfg.Valor, out var dias) && dias > 0)
            {
                return dias;
            }
            return TrialDaysPadrao;
        }

        /// <summary>
        /// Resolve um plano trial GLOBAL/sistema (marca de trial + módulos core ligados). Se não existir,
        /// cria idempotentemente um plano trial para o próprio tenant (o interceptor SaaS carimba o tenant
        /// corrente em entidades não-globais; por isso o plano de fallback é por-tenant, não "system").
        /// Persiste dentro da transação corrente (SaveChanges no contexto compartilhado).
        /// </summary>
        public static async Task<Plano> ObterOuCriarPlanoTrialAsync(
            ContextGestaoClientes ctx, string tenantId, string criadoPor, CancellationToken ct)
        {
            // 1) Plano trial global/sistema do catálogo (1.01) — reaproveitado por todos os tenants.
            var planoGlobal = await ctx.Planos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.TenantId == "system" && p.Nome == NomePlanoTrial
                                          && p.Ativo && p.DeletadoEm == null, ct);
            if (planoGlobal != null)
            {
                return planoGlobal;
            }

            // 2) Idempotência por tenant: se já existe um plano trial deste tenant, reusa.
            var planoTenant = await ctx.Planos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Nome == NomePlanoTrial
                                          && p.DeletadoEm == null, ct);
            if (planoTenant != null)
            {
                return planoTenant;
            }

            // 3) Cria o plano trial (módulos core ligados; 0 = ilimitado durante o trial).
            var plano = new Plano(
                nome: NomePlanoTrial,
                preco: 0m,
                grupoPlanoId: null,
                limiteUsuarios: 0,
                limiteEmpresas: 0,
                recursosInclusos: "Trial gratuito — módulos core liberados durante o período de avaliação.",
                tenantId: tenantId,
                criadoPor: criadoPor,
                descricaoCurta: "Plano de avaliação gratuita",
                descricaoCompleta: null,
                dataInicio: DateTime.UtcNow,
                dataFim: null,
                duration: PlanoDuration.Mensal,
                moduloCrm: true,
                moduloProjetos: true,
                moduloRh: true,
                moduloFinanceiro: true,
                moduloPdv: true,
                limiteClientes: 0,
                diasToleranciaInadimplencia: 15);

            ctx.Planos.Add(plano);
            await ctx.SaveChangesAsync(ct);
            return plano;
        }

        /// <summary>
        /// Constrói um Cliente SaaS (assinante — dono GestaoClientes). Ponto único de criação reutilizado
        /// pelo self-register (trial) e pelo handler avulso de Cliente SaaS (ativo). O documento é o CNPJ
        /// (PJ) ou o CPF (PF): a entidade Cliente exige um documento não-vazio para faturamento.
        /// </summary>
        public static Cliente CriarCliente(
            string tenantId,
            string razaoSocial,
            string documento,
            string email,
            Guid planoId,
            StatusSaaS statusSaaS,
            int diaVencimento,
            string criadoPor,
            string? telefone,
            string? nomeContato,
            bool isDemo)
            => new Cliente(
                razaoSocial: razaoSocial,
                cnpj: documento,
                email: email,
                planoId: planoId,
                revendaId: null,
                vendedorId: null,
                diaVencimento: diaVencimento,
                statusSaaS: statusSaaS,
                tenantId: tenantId,
                criadoPor: criadoPor,
                telefone: telefone,
                nomeContato: nomeContato,
                isDemo: isDemo,
                tokenAcesso: null);

        /// <summary>
        /// Constrói a assinatura do trial (AssinaturaCliente) com data-fim = agora + trialDays e o mesmo
        /// instante em <see cref="AssinaturaCliente.TrialAte"/>. Status Ativa: o trial vale desde já.
        /// </summary>
        public static AssinaturaCliente CriarAssinaturaTrial(
            string tenantId, Guid clienteId, Guid planoId, DateTime trialAte, string criadoPor)
            => new AssinaturaCliente(
                clienteId: clienteId,
                planoId: planoId,
                status: AssinaturaStatus.Ativa,
                dataInicio: DateTime.UtcNow,
                dataFim: trialAte,
                trialAte: trialAte,
                metodoPagamento: "Trial",
                transacaoId: null,
                detalhesPacoteJson: null,
                tenantId: tenantId,
                criadoPor: criadoPor);
    }
}
