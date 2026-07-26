<template>
  <div class="dashboard-layout">
    <!-- Cabeçalho Compartilhado -->
    <AppHeader />

    <!-- Conteúdo Principal -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Gerenciamento de Revendas</h1>
        <p class="tagline">Gerencie as revendas parceiras da plataforma, comissões recorrentes e status operacionais.</p>
        <div class="header-actions">
          <NuxtLink to="/plataforma/admin" class="btn btn-secondary btn-back">
            ← Voltar ao Painel
          </NuxtLink>
          <span class="status-pill" :class="{ 'offline': !apiOnline }">
            <span class="status-dot"></span>
            {{ apiOnline ? 'Conectado à API Gateway' : 'Modo Simulação Offline' }}
          </span>
        </div>
      </header>

      <div class="admin-grid-layout">
        <!-- Tabela de Revendas -->
        <section class="admin-section tenants-section glass-panel">
          <header class="section-header">
            <h3>Revendas Cadastradas</h3>
            <div class="search-bar">
              <input 
                type="text" 
                v-model="searchTerm" 
                @input="loadRevendas" 
                placeholder="Buscar revenda por nome..." 
                class="search-input"
              />
            </div>
          </header>
          
          <div class="table-container">
            <table class="admin-table">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Comissão</th>
                  <th>Status</th>
                  <th>Criado Em</th>
                  <th class="align-right">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="loading">
                  <td colspan="5" class="loading-cell">Carregando revendas...</td>
                </tr>
                <tr v-else-if="revendas.length === 0">
                  <td colspan="5" class="empty-cell">Nenhuma revenda cadastrada.</td>
                </tr>
                <tr v-else v-for="revenda in revendas" :key="revenda.id">
                  <td><span class="tenant-name-txt">{{ revenda.nome }}</span></td>
                  <td>{{ revenda.percentualComissao.toFixed(2) }}%</td>
                  <td>
                    <span :class="['badge', revenda.ativo ? 'badge-success' : 'badge-danger']">
                      {{ revenda.ativo ? 'Ativo' : 'Inativo' }}
                    </span>
                  </td>
                  <td>{{ formatDate(revenda.criadoEm) }}</td>
                  <td class="align-right">
                    <button 
                      @click="editRevenda(revenda)" 
                      class="btn btn-secondary btn-table-action"
                    >
                      Editar
                    </button>
                    <button 
                      @click="deleteRevenda(revenda.id)" 
                      class="btn btn-secondary btn-table-action btn-danger-action"
                    >
                      Excluir
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Paginação -->
          <footer class="pagination-footer" v-if="totalPages > 1">
            <button :disabled="page === 1" @click="changePage(page - 1)" class="btn btn-page">Anterior</button>
            <span class="page-info">Página {{ page }} de {{ totalPages }}</span>
            <button :disabled="page === totalPages" @click="changePage(page + 1)" class="btn btn-page">Próximo</button>
          </footer>
        </section>

        <!-- Formulário -->
        <section class="admin-section form-card glass-panel">
          <header class="section-header">
            <h3>{{ isEditing ? 'Editar Revenda' : 'Nova Revenda' }}</h3>
          </header>
          <form @submit.prevent="handleSave" class="vertical-form">
            <div class="form-group">
              <label for="revenda-name">Nome da Revenda *</label>
              <input type="text" id="revenda-name" v-model="form.Nome" placeholder="Ex: Revenda Sul" required />
            </div>
            <div class="form-group">
              <label for="revenda-comissao">Percentual de Comissão (%) *</label>
              <input type="number" id="revenda-comissao" v-model.number="form.PercentualComissao" step="0.01" min="0" max="100" required />
            </div>
            <div class="form-group toggle-row">
              <label for="revenda-ativo">Revenda Ativa</label>
              <input type="checkbox" id="revenda-ativo" v-model="form.Ativo" />
            </div>
            <div class="form-actions">
              <button type="submit" class="btn btn-primary btn-block">
                {{ isEditing ? 'Salvar Alterações' : 'Cadastrar Revenda' }}
              </button>
              <button type="button" v-if="isEditing" @click="cancelEdit" class="btn btn-secondary btn-block">
                Cancelar
              </button>
            </div>
          </form>
        </section>
      </div>

      <!-- Console de Logs da Operação -->
      <section class="admin-section console-logs glass-panel mt-4">
        <header class="section-header">
          <h3>Terminal de Operações da Plataforma</h3>
          <button @click="clearLogs" class="btn-clear-logs">Limpar Logs</button>
        </header>
        <div class="logs-container" ref="logsBox">
          <p v-for="(log, idx) in logs" :key="idx" :class="['log-line', log.type]">
            <span class="log-time">[{{ log.time }}]</span>
            <span class="log-message">{{ log.message }}</span>
          </p>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, nextTick } from 'vue'

// Página autocontida (já renderiza seu próprio AppHeader) — sem layout do shell ERP.
definePageMeta({ layout: false })

const apiOnline = ref(true)
const loading = ref(false)
const revendas = ref([])
const totalPages = ref(1)
const page = ref(1)
const searchTerm = ref('')
const isEditing = ref(false)
const editingId = ref(null)

const logs = ref([])
const logsBox = ref(null)

const form = reactive({
  Nome: '',
  PercentualComissao: 10.0,
  Ativo: true
})

onMounted(async () => {
  await checkApiConnection()
  await loadRevendas()
  addLog('Gerenciamento de Revendas inicializado.', 'info')
})

const checkApiConnection = async () => {
  try {
    await useApi('/plataforma/superadmin/dashboard')
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
    addLog('API Offline. Rodando em modo de simulação local.', 'warn')
  }
}

const addLog = (message, type = 'info') => {
  const now = new Date()
  const timeStr = now.toTimeString().split(' ')[0]
  logs.value.push({ time: timeStr, message, type })
  nextTick(() => {
    if (logsBox.value) {
      logsBox.value.scrollTop = logsBox.value.scrollHeight
    }
  })
}

const clearLogs = () => {
  logs.value = []
}

const loadRevendas = async () => {
  loading.value = true
  if (apiOnline.value) {
    try {
      const res = await useApi('/plataforma/revendas', {
        query: {
          pagina: page.value,
          tamanhoPagina: 8,
          search: searchTerm.value
        }
      })
      revendas.value = res.items
      totalPages.value = res.totalPaginas
    } catch (e) {
      addLog(`Erro ao carregar revendas do servidor: ${e.message}`, 'error')
      loadSimulado()
    }
  } else {
    loadSimulado()
  }
  loading.value = false
}

const loadSimulado = () => {
  let stored = localStorage.getItem('epros_revendas')
  if (!stored) {
    const list = [
      { id: '1b10b651-789a-41f2-95b7-a3a8080c0001', nome: 'Revenda São Paulo TI', percentualComissao: 15.00, ativo: true, criadoEm: new Date().toISOString() },
      { id: '2b10b651-789a-41f2-95b7-a3a8080c0002', nome: 'Revenda Nordeste Sistemas', percentualComissao: 12.50, ativo: true, criadoEm: new Date().toISOString() },
      { id: '3b10b651-789a-41f2-95b7-a3a8080c0003', nome: 'Sul Tecnologia & ERP', percentualComissao: 10.00, ativo: false, criadoEm: new Date().toISOString() }
    ]
    localStorage.setItem('epros_revendas', JSON.stringify(list))
    stored = JSON.stringify(list)
  }
  let list = JSON.parse(stored)
  if (searchTerm.value) {
    list = list.filter(r => r.nome.toLowerCase().includes(searchTerm.value.toLowerCase()))
  }
  revendas.value = list
  totalPages.value = 1
}

const changePage = async (p) => {
  page.value = p
  await loadRevendas()
}

const editRevenda = (revenda) => {
  isEditing.value = true
  editingId.value = revenda.id
  form.Nome = revenda.nome
  form.PercentualComissao = revenda.percentualComissao
  form.Ativo = revenda.ativo
  addLog(`Editando revenda: ${revenda.nome}`, 'info')
}

const cancelEdit = () => {
  isEditing.value = false
  editingId.value = null
  resetForm()
}

const resetForm = () => {
  form.Nome = ''
  form.PercentualComissao = 10.0
  form.Ativo = true
}

const handleSave = async () => {
  if (apiOnline.value) {
    try {
      if (isEditing.value) {
        const res = await useApi(`/plataforma/revendas/${editingId.value}`, {
          method: 'PUT',
          body: {
            Id: editingId.value,
            Nome: form.Nome,
            PercentualComissao: form.PercentualComissao,
            Ativo: form.Ativo
          }
        })
        if (res.sucesso) {
          addLog(`Revenda '${form.Nome}' atualizada com sucesso!`, 'success')
          cancelEdit()
          await loadRevendas()
        } else {
          addLog(`Erro ao atualizar: ${res.mensagem}`, 'error')
        }
      } else {
        const res = await useApi('/plataforma/revendas', {
          method: 'POST',
          body: {
            Nome: form.Nome,
            PercentualComissao: form.PercentualComissao,
            Ativo: form.Ativo
          }
        })
        if (res.sucesso) {
          addLog(`Revenda '${form.Nome}' cadastrada com sucesso!`, 'success')
          resetForm()
          await loadRevendas()
        } else {
          addLog(`Erro ao cadastrar: ${res.mensagem}`, 'error')
        }
      }
    } catch (e) {
      addLog(`Erro de comunicação com a API: ${e.message}`, 'error')
    }
  } else {
    // Modo Simulado
    let stored = JSON.parse(localStorage.getItem('epros_revendas') || '[]')
    if (isEditing.value) {
      const idx = stored.findIndex(r => r.id === editingId.value)
      if (idx !== -1) {
        stored[idx].nome = form.Nome
        stored[idx].percentualComissao = form.PercentualComissao
        stored[idx].ativo = form.Ativo
        localStorage.setItem('epros_revendas', JSON.stringify(stored))
        addLog(`[Simulado] Revenda '${form.Nome}' atualizada.`, 'success')
        cancelEdit()
        loadSimulado()
      }
    } else {
      const newRev = {
        id: crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(36).substring(2),
        nome: form.Nome,
        percentualComissao: form.PercentualComissao,
        ativo: form.Ativo,
        criadoEm: new Date().toISOString()
      }
      stored.push(newRev)
      localStorage.setItem('epros_revendas', JSON.stringify(stored))
      addLog(`[Simulado] Revenda '${form.Nome}' cadastrada.`, 'success')
      resetForm()
      loadSimulado()
    }
  }
}

const deleteRevenda = async (id) => {
  if (confirm('Deseja realmente excluir esta revenda?')) {
    if (apiOnline.value) {
      try {
        const res = await useApi(`/plataforma/revendas/${id}`, {
          method: 'DELETE'
        })
        if (res.sucesso) {
          addLog('Revenda excluída com sucesso!', 'success')
          await loadRevendas()
        } else {
          addLog(`Erro ao excluir: ${res.mensagem}`, 'error')
        }
      } catch (e) {
        addLog(`Erro de comunicação com a API: ${e.message}`, 'error')
      }
    } else {
      let stored = JSON.parse(localStorage.getItem('epros_revendas') || '[]')
      stored = stored.filter(r => r.id !== id)
      localStorage.setItem('epros_revendas', JSON.stringify(stored))
      addLog('[Simulado] Revenda excluída.', 'success')
      loadSimulado()
    }
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  return d.toLocaleDateString('pt-BR')
}
</script>

<style scoped>
/* Os estilos reutilizam as classes gerais da plataforma ou as redefinem de forma idêntica */
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
  width: 280px;
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
  padding: 32px !important;
  color: var(--text-secondary);
}
.pagination-footer {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 16px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid var(--border-color);
}
.btn-page {
  padding: 6px 12px;
  font-size: 12px;
  background: rgba(255,255,255,0.02);
  border: 1px solid var(--border-color);
}
.page-info {
  font-size: 12.5px;
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
.form-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 16px;
}
.mt-4 {
  margin-top: 24px;
}
</style>
