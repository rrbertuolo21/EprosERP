<template>
  <div class="dashboard-layout">
    <!-- Conteúdo Principal (cabeçalho/sidebar vêm do shell `admin`) -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Módulos (Catálogo SaaS)</h1>
        <p class="tagline">Catálogo de add-ons/módulos da plataforma. Defina preços, disponibilidade e quais são usados na composição dos planos.</p>
        <div class="header-actions">
          <NuxtLink to="/plataforma/admin" class="btn btn-secondary btn-back">
            ← Voltar ao Painel
          </NuxtLink>
          <button class="btn btn-primary" @click="abrirDialogNovo">
            + Novo Módulo
          </button>
          <span class="status-pill" :class="{ 'offline': !apiOnline }">
            <span class="status-dot"></span>
            {{ apiOnline ? 'Conectado à API Gateway' : 'Modo Simulação Offline' }}
          </span>
        </div>
      </header>

      <section class="admin-section tenants-section glass-panel mt-4">
        <header class="section-header">
          <h3>Módulos do Catálogo</h3>
          <div class="search-bar">
            <input
              type="text"
              v-model="searchTerm"
              placeholder="Buscar módulo por nome..."
              class="search-input"
            />
          </div>
        </header>

        <div class="table-container">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Módulo</th>
                <th>Alias</th>
                <th>Preço Mensal</th>
                <th>Preço Anual</th>
                <th>Tipo</th>
                <th>Status</th>
                <th class="align-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="7" class="loading-cell">Carregando módulos...</td>
              </tr>
              <tr v-else-if="modulosFiltrados.length === 0">
                <td colspan="7" class="empty-cell">Nenhum módulo cadastrado.</td>
              </tr>
              <tr v-else v-for="mod in modulosFiltrados" :key="mod.id">
                <td><span class="tenant-name-txt">{{ mod.nomeModulo }}</span></td>
                <td>{{ mod.alias || '—' }}</td>
                <td>{{ formatMoney(mod.precoMensal) }}</td>
                <td>{{ formatMoney(mod.precoAnual) }}</td>
                <td>
                  <span v-if="mod.admin" class="demo-badge">Admin</span>
                  <span v-else class="tenant-id-badge">Módulo</span>
                </td>
                <td>
                  <span :class="['badge', mod.habilitado ? 'badge-success' : 'badge-danger']">
                    {{ mod.habilitado ? 'Habilitado' : 'Desabilitado' }}
                  </span>
                </td>
                <td class="align-right">
                  <button @click="abrirDialogEditar(mod)" class="btn btn-secondary btn-table-action">Editar</button>
                  <button
                    v-if="mod.habilitado"
                    @click="alternarHabilitado(mod, false)"
                    class="btn btn-secondary btn-table-action btn-danger-action"
                  >
                    Desabilitar
                  </button>
                  <button
                    v-else
                    @click="alternarHabilitado(mod, true)"
                    class="btn btn-primary btn-table-action btn-success-action"
                  >
                    Habilitar
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <p class="nota-rodape">
          Este catálogo é gerido pelo <strong>Catálogo SaaS</strong> (add-ons). Os módulos habilitados ficam disponíveis
          para composição dos <NuxtLink to="/plataforma/admin/planos" class="link-inline">Planos Comerciais</NuxtLink>.
        </p>
      </section>
    </main>

    <!-- DIÁLOGO: NOVO / EDITAR MÓDULO -->
    <div class="modal-backdrop" v-if="dialog.open">
      <div class="modal-card glass-panel">
        <header class="modal-header">
          <h3>{{ dialog.isEditing ? 'Editar Módulo' : 'Novo Módulo' }}</h3>
          <button type="button" @click="dialog.open = false" class="btn-close">×</button>
        </header>
        <form @submit.prevent="salvarModulo" class="vertical-form">
          <div class="form-group">
            <label for="m-nome">Nome do Módulo *</label>
            <input
              type="text"
              id="m-nome"
              v-model="dialog.form.nomeModulo"
              placeholder="Ex: Nota Fiscal Eletrônica"
              :disabled="dialog.isEditing"
              required
            />
            <small v-if="dialog.isEditing" class="hint">O nome do módulo não pode ser alterado após a criação.</small>
          </div>
          <div class="form-group">
            <label for="m-alias">Alias / Descrição curta</label>
            <input type="text" id="m-alias" v-model="dialog.form.alias" placeholder="Ex: nfe" />
          </div>
          <div class="form-row">
            <div class="form-group col-6">
              <label for="m-mensal">Preço Mensal (R$) *</label>
              <input type="number" id="m-mensal" v-model.number="dialog.form.precoMensal" step="0.01" min="0" required />
            </div>
            <div class="form-group col-6">
              <label for="m-anual">Preço Anual (R$) *</label>
              <input type="number" id="m-anual" v-model.number="dialog.form.precoAnual" step="0.01" min="0" required />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group toggle-row col-6" v-if="!dialog.isEditing">
              <label for="m-habilitado">Já Habilitado</label>
              <input type="checkbox" id="m-habilitado" v-model="dialog.form.habilitado" />
            </div>
            <div class="form-group toggle-row col-6" v-if="!dialog.isEditing">
              <label for="m-admin">Módulo Administrativo</label>
              <input type="checkbox" id="m-admin" v-model="dialog.form.admin" />
            </div>
          </div>
          <button type="submit" class="btn btn-primary btn-block mt-4" :disabled="saving">
            {{ saving ? 'Gravando...' : (dialog.isEditing ? 'Salvar Alterações' : 'Cadastrar Módulo') }}
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
const modulos = ref([])
const searchTerm = ref('')

const dialog = reactive({
  open: false,
  isEditing: false,
  editingId: null,
  form: {
    nomeModulo: '',
    alias: '',
    precoMensal: 0,
    precoAnual: 0,
    habilitado: true,
    admin: false,
    parentAddOnId: null
  }
})

const modulosFiltrados = computed(() => {
  if (!searchTerm.value) return modulos.value
  const t = searchTerm.value.toLowerCase()
  return modulos.value.filter(m => (m.nomeModulo || '').toLowerCase().includes(t))
})

onMounted(async () => {
  await checkApiConnection()
  await carregarModulos()
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
  return res?.dados ?? res?.items ?? res?.data ?? []
}

const carregarModulos = async () => {
  loading.value = true
  try {
    const res = await useApi('/plataforma/add-ons')
    modulos.value = extrairLista(res)
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
    modulos.value = []
  }
  loading.value = false
}

const abrirDialogNovo = () => {
  dialog.isEditing = false
  dialog.editingId = null
  Object.assign(dialog.form, {
    nomeModulo: '',
    alias: '',
    precoMensal: 0,
    precoAnual: 0,
    habilitado: true,
    admin: false,
    parentAddOnId: null
  })
  dialog.open = true
}

const abrirDialogEditar = (mod) => {
  dialog.isEditing = true
  dialog.editingId = mod.id
  Object.assign(dialog.form, {
    nomeModulo: mod.nomeModulo ?? '',
    alias: mod.alias ?? '',
    precoMensal: Number(mod.precoMensal ?? 0),
    precoAnual: Number(mod.precoAnual ?? 0),
    habilitado: mod.habilitado ?? true,
    admin: mod.admin ?? false,
    parentAddOnId: mod.parentAddOnId ?? null
  })
  dialog.open = true
}

const salvarModulo = async () => {
  saving.value = true
  try {
    let res
    if (dialog.isEditing) {
      // AtualizarAddOnCommand não altera Nome/Habilitado/Admin (usar toggle p/ habilitar).
      res = await useApi(`/plataforma/add-ons/${dialog.editingId}`, {
        method: 'PUT',
        body: {
          Id: dialog.editingId,
          Alias: dialog.form.alias || null,
          PrecoMensal: dialog.form.precoMensal,
          PrecoAnual: dialog.form.precoAnual,
          Midia: null,
          ParentAddOnId: dialog.form.parentAddOnId
        }
      })
    } else {
      res = await useApi('/plataforma/add-ons', {
        method: 'POST',
        body: {
          NomeModulo: dialog.form.nomeModulo,
          Alias: dialog.form.alias || null,
          PrecoMensal: dialog.form.precoMensal,
          PrecoAnual: dialog.form.precoAnual,
          Midia: null,
          Habilitado: dialog.form.habilitado,
          Admin: dialog.form.admin,
          ParentAddOnId: dialog.form.parentAddOnId
        }
      })
    }
    if (res?.sucesso === false) {
      alert(`Falha ao salvar: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    dialog.open = false
    await carregarModulos()
  } catch (e) {
    alert(`Erro de comunicação com a API: ${e.message}`)
  } finally {
    saving.value = false
  }
}

const alternarHabilitado = async (mod, habilitar) => {
  const acao = habilitar ? 'habilitar' : 'desabilitar'
  try {
    const res = await useApi(`/plataforma/add-ons/${mod.id}/${acao}`, { method: 'POST' })
    if (res?.sucesso === false) {
      alert(`Falha ao ${acao}: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    await carregarModulos()
  } catch (e) {
    alert(`Erro de comunicação com a API: ${e.message}`)
  }
}

const formatMoney = (v) => {
  const n = Number(v ?? 0)
  return n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
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
.demo-badge {
  font-size: 10px;
  padding: 2px 6px;
  border-radius: 4px;
  background: rgba(168, 85, 247, 0.15);
  color: #c084fc;
  border: 1px solid rgba(168, 85, 247, 0.3);
}
.nota-rodape {
  margin-top: 16px;
  padding-top: 12px;
  border-top: 1px solid var(--border-color);
  font-size: 12.5px;
  color: var(--text-secondary);
}
.link-inline {
  color: var(--primary);
}
.hint {
  display: block;
  margin-top: 4px;
  font-size: 11.5px;
  color: var(--text-secondary);
}
.form-row {
  display: flex;
  gap: 16px;
  margin-bottom: 12px;
}
.col-6 { flex: 0 0 calc(50% - 8px); }
@media (max-width: 600px) {
  .form-row { flex-direction: column; gap: 12px; }
  .col-6 { flex: 0 0 100%; }
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
  width: 520px;
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
