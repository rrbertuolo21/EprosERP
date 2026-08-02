<script setup lang="ts">
/**
 * Equipe Siser — usuários internos da plataforma (operadores).
 *
 * Migrado da aba "Equipe Siser" do antigo `admin.vue` monolítico.
 * Endpoints: `GET/POST /plataforma/superadmin/usuarios-internos`,
 * `PUT .../{id}/alterar-senha`, `POST .../{id}/tornar-admin`.
 */
import { ref, reactive, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'

definePageMeta({ layout: 'admin' })

interface UsuarioInterno {
  id: string
  nome: string
  email: string
  timezone: string
  primaryAdmin: boolean
}

const usuarios = ref<UsuarioInterno[]>([])
const carregando = ref(true)
const erro = ref<string | null>(null)

const novo = reactive({ Nome: '', Email: '', Senha: '', Timezone: 'America/Sao_Paulo', PrimaryAdmin: false })

const modalSenha = reactive<{ aberto: boolean; usuario: UsuarioInterno | null; novaSenha: string }>({
  aberto: false,
  usuario: null,
  novaSenha: ''
})

async function carregar() {
  carregando.value = true
  erro.value = null
  try {
    const data = await useApi<UsuarioInterno[]>('/plataforma/superadmin/usuarios-internos')
    usuarios.value = Array.isArray(data) ? data : []
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao carregar usuários internos.'
  } finally {
    carregando.value = false
  }
}

async function criarUsuario() {
  try {
    await useApi('/plataforma/superadmin/usuarios-internos', { method: 'POST', body: { ...novo } })
    novo.Nome = ''
    novo.Email = ''
    novo.Senha = ''
    novo.Timezone = 'America/Sao_Paulo'
    novo.PrimaryAdmin = false
    await carregar()
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao criar usuário.'
  }
}

async function promoverAdmin(id: string) {
  try {
    await useApi(`/plataforma/superadmin/usuarios-internos/${id}/tornar-admin`, { method: 'POST' })
    await carregar()
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao promover usuário.'
  }
}

function abrirModalSenha(u: UsuarioInterno) {
  modalSenha.usuario = u
  modalSenha.novaSenha = ''
  modalSenha.aberto = true
}

function fecharModalSenha() {
  modalSenha.aberto = false
  modalSenha.usuario = null
  modalSenha.novaSenha = ''
}

async function alterarSenha() {
  if (!modalSenha.usuario) return
  const id = modalSenha.usuario.id
  try {
    await useApi(`/plataforma/superadmin/usuarios-internos/${id}/alterar-senha`, {
      method: 'PUT',
      body: { UsuarioInternoId: id, NovaSenha: modalSenha.novaSenha }
    })
    fecharModalSenha()
  } catch (e) {
    erro.value = e instanceof Error ? e.message : 'Falha ao alterar senha.'
  }
}

onMounted(carregar)
</script>

<template>
  <div class="admin-page">
    <header class="admin-page-header">
      <div>
        <h1 class="admin-page-title">Equipe Siser</h1>
        <p class="admin-page-sub">Usuários internos (operadores) da plataforma.</p>
      </div>
    </header>

    <p v-if="erro" class="admin-alert-error">{{ erro }}</p>

    <div class="grid-2">
      <section class="admin-section glass-panel">
        <header class="section-header"><h3>Membros da Equipe</h3></header>
        <div class="table-container">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>E-mail</th>
                <th>Fuso Horário</th>
                <th>Permissão</th>
                <th class="align-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="carregando"><td colspan="5" class="td-empty">Carregando…</td></tr>
              <tr v-else-if="usuarios.length === 0"><td colspan="5" class="td-empty">Nenhum usuário interno.</td></tr>
              <tr v-for="u in usuarios" :key="u.id">
                <td class="cell-strong">{{ u.nome }}</td>
                <td>{{ u.email }}</td>
                <td><span class="mono-badge">{{ u.timezone }}</span></td>
                <td>
                  <span :class="['badge', u.primaryAdmin ? 'badge-paga' : 'badge-cancelada']">
                    {{ u.primaryAdmin ? 'Administrador Principal' : 'Operador' }}
                  </span>
                </td>
                <td class="align-right actions-cell">
                  <button v-if="!u.primaryAdmin" @click="promoverAdmin(u.id)" class="btn btn-primary btn-sm">Promover a Admin</button>
                  <button @click="abrirModalSenha(u)" class="btn btn-secondary btn-sm">Nova Senha</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="admin-section glass-panel">
        <header class="section-header"><h3>Adicionar à Equipe</h3></header>
        <form @submit.prevent="criarUsuario" class="vertical-form">
          <div class="form-group">
            <label>Nome Completo</label>
            <input type="text" v-model="novo.Nome" placeholder="Diretor de Operações" required />
          </div>
          <div class="form-group">
            <label>E-mail</label>
            <input type="email" v-model="novo.Email" placeholder="admin2@epros.com" required />
          </div>
          <div class="form-group">
            <label>Senha de Acesso</label>
            <input type="password" v-model="novo.Senha" placeholder="••••••••" required />
          </div>
          <div class="form-group">
            <label>Fuso Horário</label>
            <select v-model="novo.Timezone">
              <option value="America/Sao_Paulo">Brasília (America/Sao_Paulo)</option>
              <option value="America/Manaus">Manaus (America/Manaus)</option>
              <option value="UTC">UTC / GMT</option>
            </select>
          </div>
          <div class="form-group toggle-row">
            <label>Administrador Principal</label>
            <input type="checkbox" v-model="novo.PrimaryAdmin" />
          </div>
          <button type="submit" class="btn btn-primary btn-block">Adicionar Usuário</button>
        </form>
      </section>
    </div>

    <!-- Modal alterar senha -->
    <div v-if="modalSenha.aberto" class="modal-backdrop">
      <div class="modal-card glass-panel">
        <header class="modal-header">
          <h3>Alterar Senha</h3>
          <button class="btn-close-modal" @click="fecharModalSenha">✕</button>
        </header>
        <p class="modal-desc">Defina a nova senha para <strong>{{ modalSenha.usuario?.nome }}</strong> ({{ modalSenha.usuario?.email }}).</p>
        <form @submit.prevent="alterarSenha" class="vertical-form">
          <div class="form-group">
            <label>Nova Senha</label>
            <input type="password" v-model="modalSenha.novaSenha" placeholder="••••••••" required />
          </div>
          <div class="modal-actions-row">
            <button type="button" class="btn btn-secondary" @click="fecharModalSenha">Cancelar</button>
            <button type="submit" class="btn btn-primary">Salvar Nova Senha</button>
          </div>
        </form>
      </div>
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
.cell-strong { font-weight: 600; color: var(--text-primary); }
.mono-badge { font-family: monospace; font-size: 11px; background: rgba(255,255,255,0.03); padding: 2px 6px; border-radius: 4px; border: 1px solid var(--border-color); }
.align-right { text-align: right; }
.actions-cell { display: flex; gap: 6px; justify-content: flex-end; }
.btn-sm { padding: 4px 10px; font-size: 11px; }
.vertical-form { display: flex; flex-direction: column; gap: 12px; }
.form-group { display: flex; flex-direction: column; gap: 6px; }
.form-group label { font-size: 10px; font-weight: 600; text-transform: uppercase; color: var(--text-secondary); }
.form-group input, .form-group select { padding: 8px 12px; background: rgba(255,255,255,0.02); border: 1px solid var(--border-color); border-radius: 8px; color: var(--text-primary); font-size: 12.5px; width: 100%; }
.form-group input:focus, .form-group select:focus { outline: none; border-color: var(--primary); background: rgba(99,102,241,0.04); }
.toggle-row { flex-direction: row; justify-content: space-between; align-items: center; }
.toggle-row input[type="checkbox"] { width: auto; }
.btn-block { width: 100%; padding: 10px; }
.modal-backdrop { position: fixed; inset: 0; background: rgba(0,0,0,0.65); display: flex; justify-content: center; align-items: center; z-index: 200; backdrop-filter: blur(4px); }
.modal-card { width: 440px; padding: 24px; }
.modal-header { display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 12px; margin-bottom: 12px; }
.modal-header h3 { font-size: 16px; font-weight: 700; }
.btn-close-modal { background: transparent; border: none; color: var(--text-muted); font-size: 16px; cursor: pointer; }
.modal-desc { font-size: 12.5px; color: var(--text-secondary); line-height: 1.5; margin-bottom: 16px; }
.modal-actions-row { display: flex; justify-content: flex-end; gap: 10px; margin-top: 16px; }
</style>
