<script setup lang="ts">
/**
 * Mensagens & Newsletter.
 *
 * Migrado da aba "Mensagens & Newsletter" do antigo `admin.vue`.
 * Endpoints: `GET /plataforma/superadmin/newsletter`, `POST .../{id}/cancelar`,
 * `.../{id}/reativar`, `POST /plataforma/superadmin/comunicacao`.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'

definePageMeta({ layout: 'admin' })

interface Assinante {
  id: string
  email: string
  ativo: boolean
  criadoEm: string
}

const assinantes = ref<Assinante[]>([])
const carregando = ref(true)
const erro = ref<string | null>(null)
const aviso = ref<string | null>(null)

const nova = reactive<{ Titulo: string; Mensagem: string; Canais: string[] }>({
  Titulo: '',
  Mensagem: '',
  Canais: ['Email']
})

const formatarData = (d: string) => {
  if (!d) return '-'
  const dt = new Date(d)
  return dt.toLocaleDateString('pt-BR') + ' ' + dt.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}

async function carregar() {
  carregando.value = true
  erro.value = null
  try {
    const data = await useApi<Assinante[]>('/plataforma/superadmin/newsletter')
    assinantes.value = Array.isArray(data) ? data : []
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao carregar assinantes.'
  } finally {
    carregando.value = false
  }
}

async function alternar(id: string, acao: 'cancelar' | 'reativar') {
  try {
    await useApi(`/plataforma/superadmin/newsletter/${id}/${acao}`, { method: 'POST' })
    await carregar()
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao atualizar inscrição.'
  }
}

async function enviarComunicado() {
  aviso.value = null
  if (nova.Canais.length === 0) {
    aviso.value = 'Selecione pelo menos um canal de notificação.'
    return
  }
  try {
    await useApi('/plataforma/superadmin/comunicacao', { method: 'POST', body: { ...nova } })
    nova.Titulo = ''
    nova.Mensagem = ''
    nova.Canais = ['Email']
    aviso.value = 'Comunicado enfileirado para envio.'
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao enviar comunicado.'
  }
}

onMounted(carregar)
</script>

<template>
  <div class="admin-page">
    <header class="admin-page-header">
      <div>
        <h1 class="admin-page-title">Mensagens & Newsletter</h1>
        <p class="admin-page-sub">Inscrições (LGPD) e comunicador global da plataforma.</p>
      </div>
    </header>

    <p v-if="erro" class="admin-alert-error">{{ erro }}</p>
    <p v-if="aviso" class="admin-alert-ok">{{ aviso }}</p>

    <div class="grid-2">
      <section class="admin-section glass-panel">
        <header class="section-header"><h3>Inscrições da Newsletter</h3></header>
        <div class="table-container">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Data Inscrição</th>
                <th>E-mail</th>
                <th>Consentimento</th>
                <th class="align-right">Ação</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="carregando"><td colspan="4" class="td-empty">Carregando…</td></tr>
              <tr v-else-if="assinantes.length === 0"><td colspan="4" class="td-empty">Nenhum assinante.</td></tr>
              <tr v-for="s in assinantes" :key="s.id">
                <td>{{ formatarData(s.criadoEm) }}</td>
                <td>{{ s.email }}</td>
                <td><span :class="['badge', s.ativo ? 'badge-paga' : 'badge-cancelada']">{{ s.ativo ? 'Consentido' : 'Descadastrado' }}</span></td>
                <td class="align-right">
                  <button v-if="s.ativo" @click="alternar(s.id, 'cancelar')" class="btn btn-secondary btn-sm">Opt-out</button>
                  <button v-else @click="alternar(s.id, 'reativar')" class="btn btn-primary btn-sm">Opt-in</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="admin-section glass-panel">
        <header class="section-header"><h3>Comunicador Global</h3></header>
        <form @submit.prevent="enviarComunicado" class="vertical-form">
          <div class="form-group">
            <label>Assunto / Título</label>
            <input type="text" v-model="nova.Titulo" placeholder="Atualização de Termos e Políticas" required />
          </div>
          <div class="form-group">
            <label>Mensagem</label>
            <textarea v-model="nova.Mensagem" rows="5" placeholder="Comunicamos que a partir de amanhã…" required></textarea>
          </div>
          <div class="form-group">
            <label>Canais de Notificação</label>
            <div class="channels-row">
              <label><input type="checkbox" value="Email" v-model="nova.Canais" /> E-mail</label>
              <label><input type="checkbox" value="SMS" v-model="nova.Canais" /> SMS</label>
              <label><input type="checkbox" value="WhatsApp" v-model="nova.Canais" /> WhatsApp</label>
            </div>
          </div>
          <button type="submit" class="btn btn-primary btn-block">Disparar Comunicado</button>
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
.admin-alert-ok { background: rgba(16,185,129,0.08); border: 1px solid rgba(16,185,129,0.25); color: var(--success); padding: 10px 14px; border-radius: 8px; font-size: 13px; }
.grid-2 { display: grid; grid-template-columns: 2fr 1fr; gap: 20px; }
@media (max-width: 950px) { .grid-2 { grid-template-columns: 1fr; } }
.admin-section { padding: 20px; display: flex; flex-direction: column; gap: 14px; }
.section-header h3 { font-size: 15px; font-weight: 750; color: var(--text-primary); border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 10px; }
.table-container { overflow-x: auto; }
.admin-table { width: 100%; border-collapse: collapse; }
.admin-table th { padding: 10px 14px; font-size: 10.5px; font-weight: 700; text-transform: uppercase; color: var(--text-secondary); border-bottom: 1px solid var(--border-color); text-align: left; }
.admin-table td { padding: 12px 14px; font-size: 13px; border-bottom: 1px solid rgba(255,255,255,0.02); }
.td-empty { text-align: center; color: var(--text-muted); font-style: italic; }
.align-right { text-align: right; }
.btn-sm { padding: 4px 10px; font-size: 11px; }
.vertical-form { display: flex; flex-direction: column; gap: 12px; }
.form-group { display: flex; flex-direction: column; gap: 6px; }
.form-group label { font-size: 10px; font-weight: 600; text-transform: uppercase; color: var(--text-secondary); }
.form-group input, .form-group textarea { padding: 8px 12px; background: rgba(255,255,255,0.02); border: 1px solid var(--border-color); border-radius: 8px; color: var(--text-primary); font-size: 12.5px; width: 100%; }
.form-group input:focus, .form-group textarea:focus { outline: none; border-color: var(--primary); background: rgba(99,102,241,0.04); }
.channels-row { display: flex; gap: 16px; }
.channels-row label { font-size: 12px; text-transform: none; color: var(--text-primary); display: flex; align-items: center; gap: 4px; }
.channels-row input { width: auto; }
.btn-block { width: 100%; padding: 10px; }
</style>
