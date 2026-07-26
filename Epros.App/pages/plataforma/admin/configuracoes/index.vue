<script setup lang="ts">
/**
 * Configurações Globais (System Settings).
 *
 * Migrado da aba "Configurações Globais" do antigo `admin.vue`.
 * Endpoints: `GET/POST /plataforma/superadmin/configuracoes`.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'

definePageMeta({ layout: 'admin' })

interface SystemSetting {
  id: string
  chave: string
  valor: string
  escopo: string
  isSecret: boolean
}

const settings = ref<SystemSetting[]>([])
const carregando = ref(true)
const erro = ref<string | null>(null)

const nova = reactive({ Chave: '', Valor: '', Escopo: 'global', EhSegredo: false })

async function carregar() {
  carregando.value = true
  erro.value = null
  try {
    const data = await useApi<SystemSetting[]>('/plataforma/superadmin/configuracoes')
    settings.value = Array.isArray(data) ? data : []
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao carregar configurações.'
  } finally {
    carregando.value = false
  }
}

async function definir() {
  try {
    await useApi('/plataforma/superadmin/configuracoes', { method: 'POST', body: { ...nova } })
    nova.Chave = ''
    nova.Valor = ''
    nova.Escopo = 'global'
    nova.EhSegredo = false
    await carregar()
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao salvar parâmetro.'
  }
}

onMounted(carregar)
</script>

<template>
  <div class="admin-page">
    <header class="admin-page-header">
      <div>
        <h1 class="admin-page-title">Configurações Globais</h1>
        <p class="admin-page-sub">Parâmetros de sistema e segredos do ambiente.</p>
      </div>
    </header>

    <p v-if="erro" class="admin-alert-error">{{ erro }}</p>

    <div class="grid-2">
      <section class="admin-section glass-panel">
        <header class="section-header"><h3>Parâmetros Globais</h3></header>
        <div class="table-container">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Chave</th>
                <th>Valor</th>
                <th>Escopo</th>
                <th>Tipo</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="carregando"><td colspan="4" class="td-empty">Carregando…</td></tr>
              <tr v-else-if="settings.length === 0"><td colspan="4" class="td-empty">Nenhum parâmetro definido.</td></tr>
              <tr v-for="s in settings" :key="s.id">
                <td><span class="mono-badge">{{ s.chave }}</span></td>
                <td>
                  <span v-if="s.isSecret" class="secret-value">•••••••• (segredo no cofre)</span>
                  <span v-else>{{ s.valor }}</span>
                </td>
                <td><span class="mono-badge">{{ s.escopo }}</span></td>
                <td><span :class="['badge', s.isSecret ? 'badge-cancelada' : 'badge-paga']">{{ s.isSecret ? 'Segredo' : 'Livre' }}</span></td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="admin-section glass-panel">
        <header class="section-header"><h3>Definir Parâmetro</h3></header>
        <form @submit.prevent="definir" class="vertical-form">
          <div class="form-group">
            <label>Chave de Configuração</label>
            <input type="text" v-model="nova.Chave" placeholder="smtp_host" required />
          </div>
          <div class="form-group">
            <label>Valor do Parâmetro</label>
            <input type="text" v-model="nova.Valor" placeholder="smtp.gmail.com" required />
          </div>
          <div class="form-group">
            <label>Escopo</label>
            <select v-model="nova.Escopo">
              <option value="global">Global (Siser)</option>
              <option value="tenant">Por Inquilino</option>
            </select>
          </div>
          <div class="form-group toggle-row">
            <label>Criptografar valor no cofre (Vault)</label>
            <input type="checkbox" v-model="nova.EhSegredo" />
          </div>
          <button type="submit" class="btn btn-primary btn-block">Salvar Parâmetro</button>
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
.mono-badge { font-family: monospace; font-size: 11px; background: rgba(255,255,255,0.03); padding: 2px 6px; border-radius: 4px; border: 1px solid var(--border-color); }
.secret-value { color: var(--text-muted); font-family: monospace; font-size: 11px; }
.vertical-form { display: flex; flex-direction: column; gap: 12px; }
.form-group { display: flex; flex-direction: column; gap: 6px; }
.form-group label { font-size: 10px; font-weight: 600; text-transform: uppercase; color: var(--text-secondary); }
.form-group input, .form-group select { padding: 8px 12px; background: rgba(255,255,255,0.02); border: 1px solid var(--border-color); border-radius: 8px; color: var(--text-primary); font-size: 12.5px; width: 100%; }
.form-group input:focus, .form-group select:focus { outline: none; border-color: var(--primary); background: rgba(99,102,241,0.04); }
.toggle-row { flex-direction: row; justify-content: space-between; align-items: center; }
.toggle-row input[type="checkbox"] { width: auto; }
.btn-block { width: 100%; padding: 10px; }
</style>
