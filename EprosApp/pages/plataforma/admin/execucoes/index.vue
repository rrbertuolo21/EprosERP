<script setup lang="ts">
/**
 * Execuções em Massa (fluxo Maker-Checker).
 *
 * Migrado da aba "Execuções em Massa" do antigo `admin.vue`.
 * Endpoints: `GET/POST /plataforma/superadmin/execucoes-massa-global`,
 * `POST .../{id}/simular`, `.../{id}/ativar`, `.../{id}/concluir`.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'

definePageMeta({ layout: 'admin' })

interface ExecucaoMassa {
  id: string
  descricao: string
  actionPayload: string
  status: string
  criadoPor: string
  criadoEm: string
}

const execucoes = ref<ExecucaoMassa[]>([])
const carregando = ref(true)
const erro = ref<string | null>(null)

const nova = reactive({ Descricao: '', ActionPayload: '' })

const formatarData = (d: string) => {
  if (!d) return '-'
  const dt = new Date(d)
  return dt.toLocaleDateString('pt-BR') + ' ' + dt.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}

const truncar = (t: string, n: number) => (!t ? '' : t.length <= n ? t : t.slice(0, n) + '…')

const badgeStatus = (s: string) => {
  if (s === 'Concluido' || s === 'Aprovado') return 'badge-paga'
  if (s === 'Simulado' || s === 'Draft') return 'badge-pendente'
  return 'badge-cancelada'
}

async function carregar() {
  carregando.value = true
  erro.value = null
  try {
    const data = await useApi<ExecucaoMassa[]>('/plataforma/superadmin/execucoes-massa-global')
    execucoes.value = Array.isArray(data) ? data : []
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao carregar execuções.'
  } finally {
    carregando.value = false
  }
}

async function criar() {
  try {
    await useApi('/plataforma/superadmin/execucoes-massa-global', { method: 'POST', body: { ...nova } })
    nova.Descricao = ''
    nova.ActionPayload = ''
    await carregar()
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao criar solicitação.'
  }
}

async function acao(id: string, endpoint: 'simular' | 'ativar' | 'concluir') {
  try {
    await useApi(`/plataforma/superadmin/execucoes-massa-global/${id}/${endpoint}`, { method: 'POST' })
    await carregar()
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha na operação.'
  }
}

onMounted(carregar)
</script>

<template>
  <div class="admin-page">
    <header class="admin-page-header">
      <div>
        <h1 class="admin-page-title">Execuções em Massa</h1>
        <p class="admin-page-sub">Scripts e processamentos em lote com fluxo Maker-Checker.</p>
      </div>
    </header>

    <p v-if="erro" class="admin-alert-error">{{ erro }}</p>

    <div class="grid-2">
      <section class="admin-section glass-panel">
        <header class="section-header"><h3>Solicitações (Maker-Checker)</h3></header>
        <div class="table-container">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Data/Hora</th>
                <th>Descrição</th>
                <th>Criador</th>
                <th>Status</th>
                <th class="align-right">Fluxo</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="carregando"><td colspan="5" class="td-empty">Carregando…</td></tr>
              <tr v-else-if="execucoes.length === 0"><td colspan="5" class="td-empty">Nenhuma execução registrada.</td></tr>
              <tr v-for="e in execucoes" :key="e.id">
                <td>{{ formatarData(e.criadoEm) }}</td>
                <td>
                  <div class="cell-stack">
                    <span class="cell-strong">{{ e.descricao }}</span>
                    <span class="cell-muted">Payload: {{ truncar(e.actionPayload, 50) }}</span>
                  </div>
                </td>
                <td>{{ e.criadoPor }}</td>
                <td><span :class="['badge', badgeStatus(e.status)]">{{ e.status }}</span></td>
                <td class="align-right actions-cell">
                  <button v-if="e.status === 'Draft'" @click="acao(e.id, 'simular')" class="btn btn-secondary btn-sm">⚡ Simular</button>
                  <button v-if="e.status === 'Simulado'" @click="acao(e.id, 'ativar')" class="btn btn-primary btn-sm">✓ Aprovar</button>
                  <button v-if="e.status === 'Aprovado'" @click="acao(e.id, 'concluir')" class="btn btn-primary btn-sm">🚀 Executar</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="admin-section glass-panel">
        <header class="section-header"><h3>Criar Solicitação</h3></header>
        <form @submit.prevent="criar" class="vertical-form">
          <div class="form-group">
            <label>Descrição do Script</label>
            <input type="text" v-model="nova.Descricao" placeholder="Reajustar preços IGP-M do Plano Gold" required />
          </div>
          <div class="form-group">
            <label>Action / Payload (JSON)</label>
            <textarea v-model="nova.ActionPayload" rows="5" placeholder='{ "planoId": "gold", "indice": 1.05 }' required></textarea>
          </div>
          <button type="submit" class="btn btn-primary btn-block">Cadastrar Solicitação</button>
        </form>
      </section>
    </div>
  </div>
</template>

<style scoped>
.admin-page { display: flex; flex-direction: column; gap: 20px; }
.admin-page-header { display: flex; justify-content: space-between; align-items: flex-end; gap: 12px; flex-wrap: wrap; }
.admin-page-title { font-size: 24px; font-weight: 800; letter-spacing: -0.5px; color: var(--text-primary); }
.admin-page-sub { font-size: 13px; color: var(--text-secondary); margin-top: 2px; }
.admin-alert-error { background: rgba(239,68,68,0.08); border: 1px solid rgba(239,68,68,0.25); color: var(--danger); padding: 10px 14px; border-radius: 8px; font-size: 13px; }
.grid-2 { display: grid; grid-template-columns: 2fr 1fr; gap: 20px; }
@media (max-width: 950px) { .grid-2 { grid-template-columns: 1fr; } }
.admin-section { padding: 20px; display: flex; flex-direction: column; gap: 14px; }
.section-header h3 { font-size: 15px; font-weight: 750; color: var(--text-primary); border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 10px; }
.table-container { overflow-x: auto; }
.admin-table { width: 100%; border-collapse: collapse; }
.admin-table th { padding: 10px 14px; font-size: 10.5px; font-weight: 700; text-transform: uppercase; color: var(--text-secondary); border-bottom: 1px solid var(--border-color); text-align: left; }
.admin-table td { padding: 12px 14px; font-size: 13px; border-bottom: 1px solid rgba(255,255,255,0.02); }
.td-empty { text-align: center; color: var(--text-muted); font-style: italic; }
.cell-stack { display: flex; flex-direction: column; }
.cell-strong { font-weight: 600; color: var(--text-primary); }
.cell-muted { font-size: 11px; color: var(--text-muted); }
.align-right { text-align: right; }
.actions-cell { display: flex; gap: 6px; justify-content: flex-end; }
.btn-sm { padding: 4px 10px; font-size: 11px; }
.vertical-form { display: flex; flex-direction: column; gap: 12px; }
.form-group { display: flex; flex-direction: column; gap: 6px; }
.form-group label { font-size: 10px; font-weight: 600; text-transform: uppercase; color: var(--text-secondary); }
.form-group input, .form-group textarea { padding: 8px 12px; background: rgba(255,255,255,0.02); border: 1px solid var(--border-color); border-radius: 8px; color: var(--text-primary); font-size: 12.5px; width: 100%; }
.form-group input:focus, .form-group textarea:focus { outline: none; border-color: var(--primary); background: rgba(99,102,241,0.04); }
.btn-block { width: 100%; padding: 10px; }
</style>
