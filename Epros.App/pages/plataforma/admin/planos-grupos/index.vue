<template>
  <div class="dashboard-layout">
    <!-- Conteúdo Principal (cabeçalho/sidebar vêm do shell `admin`) -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Grupos de Planos</h1>
        <p class="tagline">Organize os planos comerciais em grupos (ex: Contábil, Fiscal, Transporte) usados por planos e empresas.</p>
        <div class="header-actions">
          <NuxtLink to="/plataforma/admin" class="btn btn-secondary btn-back">
            ← Voltar ao Painel
          </NuxtLink>
          <button class="btn btn-primary" @click="abrirDialogNovo">
            + Novo Grupo
          </button>
          <span class="status-pill" :class="{ 'offline': !apiOnline }">
            <span class="status-dot"></span>
            {{ apiOnline ? 'Conectado à API Gateway' : 'Modo Simulação Offline' }}
          </span>
        </div>
      </header>

      <section class="admin-section tenants-section glass-panel mt-4">
        <header class="section-header">
          <h3>Grupos Cadastrados</h3>
          <div class="search-bar">
            <input
              type="text"
              v-model="searchTerm"
              @input="onSearch"
              placeholder="Buscar grupo por descrição..."
              class="search-input"
            />
          </div>
        </header>

        <div class="table-container">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Descrição</th>
                <th>Status</th>
                <th class="align-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="3" class="loading-cell">Carregando grupos...</td>
              </tr>
              <tr v-else-if="gruposFiltrados.length === 0">
                <td colspan="3" class="empty-cell">Nenhum grupo cadastrado.</td>
              </tr>
              <tr v-else v-for="grupo in gruposFiltrados" :key="grupo.id">
                <td><span class="tenant-name-txt">{{ grupo.descricao }}</span></td>
                <td>
                  <span :class="['badge', grupo.ativo ? 'badge-success' : 'badge-danger']">
                    {{ grupo.ativo ? 'Ativo' : 'Inativo' }}
                  </span>
                </td>
                <td class="align-right">
                  <button @click="abrirDialogEditar(grupo)" class="btn btn-secondary btn-table-action">Editar</button>
                  <button @click="excluirGrupo(grupo.id)" class="btn btn-secondary btn-table-action btn-danger-action">Excluir</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </main>

    <!-- DIÁLOGO: NOVO / EDITAR GRUPO -->
    <div class="modal-backdrop" v-if="dialog.open">
      <div class="modal-card glass-panel">
        <header class="modal-header">
          <h3>{{ dialog.isEditing ? 'Editar Grupo' : 'Novo Grupo' }}</h3>
          <button type="button" @click="dialog.open = false" class="btn-close">×</button>
        </header>
        <form @submit.prevent="salvarGrupo" class="vertical-form">
          <div class="form-group">
            <label for="g-descricao">Descrição *</label>
            <input type="text" id="g-descricao" v-model="dialog.form.descricao" placeholder="Ex: Grupo Fiscal" required />
          </div>
          <div class="form-group toggle-row">
            <label for="g-ativo">Grupo Ativo</label>
            <input type="checkbox" id="g-ativo" v-model="dialog.form.ativo" />
          </div>
          <button type="submit" class="btn btn-primary btn-block mt-4" :disabled="saving">
            {{ saving ? 'Gravando...' : (dialog.isEditing ? 'Salvar Alterações' : 'Cadastrar Grupo') }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'

// Área landlord: shell administrativo (sidebar + header) via layout `admin`.
definePageMeta({ layout: 'admin' })

const apiOnline = ref(true)
const loading = ref(false)
const saving = ref(false)
const grupos = ref([])
const searchTerm = ref('')

const dialog = reactive({
  open: false,
  isEditing: false,
  editingId: null,
  form: {
    descricao: '',
    ativo: true
  }
})

const gruposFiltrados = computed(() => {
  if (!searchTerm.value) return grupos.value
  const t = searchTerm.value.toLowerCase()
  return grupos.value.filter(g => (g.descricao || '').toLowerCase().includes(t))
})

onMounted(async () => {
  await checkApiConnection()
  await carregarGrupos()
})

const checkApiConnection = async () => {
  try {
    await useApi('/plataforma/superadmin/dashboard')
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
  }
}

const extrairLista = (res) => {
  if (Array.isArray(res)) return res
  return res?.items ?? res?.dados?.items ?? res?.dados ?? res?.data ?? []
}

const onSearch = () => { /* filtro é reativo via computed */ }

const carregarGrupos = async () => {
  loading.value = true
  try {
    const res = await useApi('/plataforma/grupos-plano', { query: { tamanhoPagina: 200 } })
    grupos.value = extrairLista(res)
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
    grupos.value = []
  }
  loading.value = false
}

const abrirDialogNovo = () => {
  dialog.isEditing = false
  dialog.editingId = null
  dialog.form.descricao = ''
  dialog.form.ativo = true
  dialog.open = true
}

const abrirDialogEditar = (grupo) => {
  dialog.isEditing = true
  dialog.editingId = grupo.id
  dialog.form.descricao = grupo.descricao ?? ''
  dialog.form.ativo = grupo.ativo ?? true
  dialog.open = true
}

const salvarGrupo = async () => {
  saving.value = true
  try {
    let res
    if (dialog.isEditing) {
      res = await useApi(`/plataforma/grupos-plano/${dialog.editingId}`, {
        method: 'PUT',
        body: { Id: dialog.editingId, Descricao: dialog.form.descricao, Ativo: dialog.form.ativo }
      })
    } else {
      res = await useApi('/plataforma/grupos-plano', {
        method: 'POST',
        body: { Descricao: dialog.form.descricao, Ativo: dialog.form.ativo }
      })
    }
    if (res?.sucesso === false) {
      alert(`Falha ao salvar: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    dialog.open = false
    await carregarGrupos()
  } catch (e) {
    alert(`Erro de comunicação com a API: ${e.message}`)
  } finally {
    saving.value = false
  }
}

const excluirGrupo = async (id) => {
  if (!confirm('Deseja realmente excluir este grupo?')) return
  try {
    const res = await useApi(`/plataforma/grupos-plano/${id}`, { method: 'DELETE' })
    if (res?.sucesso === false) {
      alert(`Erro ao excluir: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    await carregarGrupos()
  } catch (e) {
    alert(`Erro de comunicação com a API: ${e.message}`)
  }
}
</script>

<style scoped>
.header-actions {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-top: 8px;
}
.btn-back {
  padding: 8px 16px;
  font-size: 13px;
  background: rgba(255,255,255,0.02);
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
}
.btn-back:hover {
  background: rgba(255,255,255,0.06);
  color: var(--text-primary);
}
.search-bar {
  margin-top: 8px;
  width: 320px;
}
.search-input {
  width: 100%;
  padding: 8px 12px;
  background: rgba(0, 0, 0, 0.2);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  color: var(--text-primary);
  font-size: 13px;
}
.search-input:focus {
  outline: none;
  border-color: var(--primary);
}
.loading-cell, .empty-cell {
  text-align: center;
  padding: 48px !important;
  color: var(--text-secondary);
}
.badge-success {
  background: rgba(16, 185, 129, 0.1);
  color: var(--success);
  border: 1px solid rgba(16, 185, 129, 0.2);
}
.badge-danger {
  background: rgba(239, 68, 68, 0.1);
  color: var(--danger);
  border: 1px solid rgba(239, 68, 68, 0.2);
}
.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.65);
  backdrop-filter: blur(8px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}
.modal-card {
  width: 480px;
  max-width: 95%;
  padding: 24px;
  border: 1px solid rgba(255,255,255,0.1);
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5);
}
.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 8px;
}
.btn-close {
  background: none;
  border: none;
  color: var(--text-secondary);
  font-size: 24px;
  cursor: pointer;
}
.btn-close:hover {
  color: var(--text-primary);
}
.mt-4 {
  margin-top: 24px;
}
</style>
