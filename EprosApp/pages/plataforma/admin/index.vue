<script setup lang="ts">
/**
 * Dashboard global da Administração SaaS (Landlord).
 *
 * KPIs vindos de `GET /plataforma/superadmin/dashboard` (ObterDashboardGlobalQuery) +
 * tabela resumida de clientes de `GET /plataforma/superadmin/clientes`.
 */
import { ref, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'

definePageMeta({ layout: 'admin' })

interface DashboardGlobal {
  totalTenants: number
  totalAssinaturasAtivas: number
  receitaEstimadaMRR: number
  novasAssinaturasMes: number
  churnRate: number
  receitaTotal: number
}

interface ClienteResumo {
  id: string
  tenantId: string
  razaoSocial: string
  cnpj: string
  email: string
  planoNome: string
  statusSaaS: string
  ativo: boolean
}

const carregando = ref(true)
const erro = ref<string | null>(null)
const kpis = ref<DashboardGlobal | null>(null)
const clientes = ref<ClienteResumo[]>([])

const formatarMoeda = (v: number | undefined) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v ?? 0)

const badgeStatus = (c: ClienteResumo) =>
  c.statusSaaS === 'Active' || c.ativo ? 'badge-paga' : 'badge-cancelada'

async function carregar() {
  carregando.value = true
  erro.value = null
  try {
    const [dash, lista] = await Promise.all([
      useApi<DashboardGlobal>('/plataforma/superadmin/dashboard'),
      useApi<ClienteResumo[]>('/plataforma/superadmin/clientes')
    ])
    kpis.value = dash
    clientes.value = Array.isArray(lista) ? lista : []
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao carregar o dashboard.'
  } finally {
    carregando.value = false
  }
}

onMounted(carregar)
</script>

<template>
  <div class="admin-page">
    <header class="admin-page-header">
      <div>
        <h1 class="admin-page-title">Dashboard da Plataforma</h1>
        <p class="admin-page-sub">Visão geral das assinaturas, receita e inquilinos ativos.</p>
      </div>
      <button type="button" class="btn btn-secondary btn-sm" @click="carregar" :disabled="carregando">
        {{ carregando ? 'Atualizando…' : 'Atualizar' }}
      </button>
    </header>

    <p v-if="erro" class="admin-alert-error">{{ erro }}</p>

    <!-- KPIs -->
    <section class="kpis-grid">
      <div class="kpi-card glass-panel">
        <span class="kpi-icon">💰</span>
        <div class="kpi-info">
          <span class="kpi-label">MRR (Recorrência Mensal)</span>
          <span class="kpi-value">{{ formatarMoeda(kpis?.receitaEstimadaMRR) }}</span>
          <span class="kpi-sub">{{ kpis?.novasAssinaturasMes ?? 0 }} novas assinaturas no mês</span>
        </div>
      </div>
      <div class="kpi-card glass-panel">
        <span class="kpi-icon">🏢</span>
        <div class="kpi-info">
          <span class="kpi-label">Assinaturas Ativas</span>
          <span class="kpi-value">{{ kpis?.totalAssinaturasAtivas ?? 0 }}</span>
          <span class="kpi-sub">{{ kpis?.totalTenants ?? 0 }} inquilinos cadastrados</span>
        </div>
      </div>
      <div class="kpi-card glass-panel">
        <span class="kpi-icon">⚠️</span>
        <div class="kpi-info">
          <span class="kpi-label">Churn / Inadimplência</span>
          <span class="kpi-value">{{ ((kpis?.churnRate ?? 0)).toFixed(1) }}%</span>
          <span class="kpi-sub">Taxa de cancelamento estimada</span>
        </div>
      </div>
      <div class="kpi-card glass-panel">
        <span class="kpi-icon">📈</span>
        <div class="kpi-info">
          <span class="kpi-label">Receita Total (Pago)</span>
          <span class="kpi-value">{{ formatarMoeda(kpis?.receitaTotal) }}</span>
          <span class="kpi-sub">Consolidado liquidado</span>
        </div>
      </div>
    </section>

    <!-- Tabela resumida de clientes -->
    <section class="admin-section glass-panel">
      <header class="section-header section-header-row">
        <h3>Inquilinos (Tenants)</h3>
        <NuxtLink to="/plataforma/admin/clientes" class="btn btn-secondary btn-sm">Ver todos</NuxtLink>
      </header>
      <div class="table-container">
        <table class="admin-table">
          <thead>
            <tr>
              <th>Inquilino</th>
              <th>Razão Social / CNPJ</th>
              <th>Plano</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="carregando">
              <td colspan="4" class="td-empty">Carregando…</td>
            </tr>
            <tr v-else-if="clientes.length === 0">
              <td colspan="4" class="td-empty">Nenhum inquilino encontrado.</td>
            </tr>
            <tr v-for="c in clientes" :key="c.id">
              <td><span class="mono-badge">{{ c.tenantId }}</span></td>
              <td>
                <div class="cell-stack">
                  <span class="cell-strong">{{ c.razaoSocial }}</span>
                  <span class="cell-muted">{{ c.cnpj }} · {{ c.email }}</span>
                </div>
              </td>
              <td>{{ c.planoNome }}</td>
              <td><span :class="['badge', badgeStatus(c)]">{{ c.statusSaaS || (c.ativo ? 'Ativo' : 'Suspenso') }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>

<style scoped>
.admin-page { display: flex; flex-direction: column; gap: 20px; }
.admin-page-header { display: flex; justify-content: space-between; align-items: flex-end; gap: 12px; flex-wrap: wrap; }
.admin-page-title { font-size: 24px; font-weight: 800; letter-spacing: -0.5px; color: var(--text-primary); }
.admin-page-sub { font-size: 13px; color: var(--text-secondary); margin-top: 2px; }
.admin-alert-error {
  background: rgba(239, 68, 68, 0.08);
  border: 1px solid rgba(239, 68, 68, 0.25);
  color: var(--danger);
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 13px;
}
.kpis-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 16px; }
.kpi-card { display: flex; align-items: center; gap: 16px; padding: 20px; }
.kpi-icon { font-size: 26px; padding: 10px; background: rgba(255, 255, 255, 0.02); border-radius: 10px; border: 1px solid var(--border-color); }
.kpi-info { display: flex; flex-direction: column; gap: 2px; }
.kpi-label { font-size: 10px; font-weight: 700; text-transform: uppercase; color: var(--text-secondary); }
.kpi-value { font-size: 22px; font-weight: 700; color: var(--text-primary); }
.kpi-sub { font-size: 11px; color: var(--text-muted); }
.admin-section { padding: 20px; display: flex; flex-direction: column; gap: 14px; }
.section-header h3 { font-size: 15px; font-weight: 750; color: var(--text-primary); }
.section-header-row { display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 10px; }
.table-container { overflow-x: auto; }
.admin-table { width: 100%; border-collapse: collapse; }
.admin-table th { padding: 10px 14px; font-size: 10.5px; font-weight: 700; text-transform: uppercase; color: var(--text-secondary); border-bottom: 1px solid var(--border-color); text-align: left; }
.admin-table td { padding: 12px 14px; font-size: 13px; border-bottom: 1px solid rgba(255,255,255,0.02); }
.td-empty { text-align: center; color: var(--text-muted); font-style: italic; }
.mono-badge { font-family: monospace; font-size: 11px; background: rgba(255,255,255,0.03); padding: 2px 6px; border-radius: 4px; border: 1px solid var(--border-color); }
.cell-stack { display: flex; flex-direction: column; }
.cell-strong { font-weight: 600; color: var(--text-primary); }
.cell-muted { font-size: 11px; color: var(--text-muted); }
</style>
