<template>
  <div class="dashboard-layout">
    <!-- Cabeçalho Compartilhado -->
    <AppHeader />

    <!-- Conteúdo Principal -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Gerenciamento de Vendedores</h1>
        <p class="tagline">Gerencie a equipe de vendas interna e os consultores associados às revendas parceiras.</p>
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
        <!-- Tabela de Vendedores -->
        <section class="admin-section tenants-section glass-panel">
          <header class="section-header">
            <h3>Vendedores Cadastrados</h3>
            <div class="search-bar">
              <input 
                type="text" 
                v-model="searchTerm" 
                @input="loadVendedores" 
                placeholder="Buscar vendedor..." 
                class="search-input"
              />
            </div>
          </header>
          
          <div class="table-container">
            <table class="admin-table">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>Contato</th>
                  <th>Revenda Vinculada</th>
                  <th>Comissão</th>
                  <th>Status</th>
                  <th class="align-right">Ações</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="loading">
                  <td colspan="6" class="loading-cell">Carregando vendedores...</td>
                </tr>
                <tr v-else-if="vendedores.length === 0">
                  <td colspan="6" class="empty-cell">Nenhum vendedor cadastrado.</td>
                </tr>
                <tr v-else v-for="vendedor in vendedores" :key="vendedor.id">
                  <td><span class="tenant-name-txt">{{ vendedor.nome }}</span></td>
                  <td>
                    <div class="tenant-details">
                      <span class="tenant-email-txt">{{ vendedor.email }}</span>
                      <span class="tenant-email-txt">{{ vendedor.telefone || 'Sem telefone' }}</span>
                    </div>
                  </td>
                  <td>
                    <span v-if="vendedor.revendaNome" class="tenant-id-badge">{{ vendedor.revendaNome }}</span>
                    <span v-else class="text-muted">Direto (Siser)</span>
                  </td>
                  <td>{{ vendedor.percentualComissao.toFixed(2) }}%</td>
                  <td>
                    <span :class="['badge', vendedor.ativo ? 'badge-success' : 'badge-danger']">
                      {{ vendedor.ativo ? 'Ativo' : 'Inativo' }}
                    </span>
                  </td>
                  <td class="align-right">
                    <button 
                      @click="editVendedor(vendedor)" 
                      class="btn btn-secondary btn-table-action"
                    >
                      Editar
                    </button>
                    <button 
                      @click="deleteVendedor(vendedor.id)" 
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
            <h3>{{ isEditing ? 'Editar Vendedor' : 'Novo Vendedor' }}</h3>
          </header>
          <form @submit.prevent="handleSave" class="vertical-form">
            <div class="form-group">
              <label for="vend-name">Nome do Vendedor *</label>
              <input type="text" id="vend-name" v-model="form.Nome" placeholder="Ex: Roberto Silva" required />
            </div>
            <div class="form-group">
              <label for="vend-email">E-mail Comercial *</label>
              <input type="email" id="vend-email" v-model="form.Email" placeholder="roberto@epros.com" required />
            </div>
            <div class="form-group">
              <label for="vend-tel">Telefone (DDD + Número)</label>
              <input type="text" id="vend-tel" v-model="form.Telefone" placeholder="(11) 98765-4321" />
            </div>
            <div class="form-group">
              <label for="vend-revenda">Vincular à Revenda</label>
              <select id="vend-revenda" v-model="form.RevendaId">
                <option :value="null">Nenhuma (Vendedor Direto)</option>
                <option v-for="rev in revendas" :key="rev.id" :value="rev.id">
                  {{ rev.nome }}
                </option>
              </select>
            </div>
            <div class="form-group">
              <label for="vend-comissao">Percentual de Comissão (%) *</label>
              <input type="number" id="vend-comissao" v-model.number="form.PercentualComissao" step="0.01" min="0" max="100" required />
            </div>
            <div class="form-group toggle-row">
              <label for="vend-ativo">Vendedor Ativo</label>
              <input type="checkbox" id="vend-ativo" v-model="form.Ativo" />
            </div>
            <div class="form-actions">
              <button type="submit" class="btn btn-primary btn-block">
                {{ isEditing ? 'Salvar Alterações' : 'Cadastrar Vendedor' }}
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
const vendedores = ref([])
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
  Email: '',
  Telefone: '',
  PercentualComissao: 5.0,
  RevendaId: null,
  Ativo: true
})

onMounted(async () => {
  await checkApiConnection()
  await loadRevendasSelect()
  await loadVendedores()
  addLog('Gerenciamento de Vendedores inicializado.', 'info')
})

const checkApiConnection = async () => {
  try {
    await $fetch('http://localhost:5000/api/v1/plataforma/superadmin/dashboard')
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

const loadRevendasSelect = async () => {
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/plataforma/revendas', {
        params: { pagina: 1, tamanhoPagina: 100 }
      })
      revendas.value = res.items
    } catch (e) {
      loadRevendasSimulado()
    }
  } else {
    loadRevendasSimulado()
  }
}

const loadRevendasSimulado = () => {
  const stored = localStorage.getItem('epros_revendas')
  if (stored) {
    revendas.value = JSON.parse(stored)
  }
}

const loadVendedores = async () => {
  loading.value = true
  if (apiOnline.value) {
    try {
      const res = await $fetch('http://localhost:5000/api/v1/plataforma/vendedores', {
        params: {
          pagina: page.value,
          tamanhoPagina: 8,
          search: searchTerm.value
        }
      })
      vendedores.value = res.items
      totalPages.value = res.totalPaginas
    } catch (e) {
      addLog(`Erro ao carregar vendedores do servidor: ${e.message}`, 'error')
      loadSimulado()
    }
  } else {
    loadSimulado()
  }
  loading.value = false
}

const loadSimulado = () => {
  let stored = localStorage.getItem('epros_vendedores')
  if (!stored) {
    const list = [
      { id: '1a10b651-789a-41f2-95b7-a3a8080c0001', nome: 'Roberto Silva', email: 'roberto@epros.com.br', telefone: '(11) 98888-7777', percentualComissao: 5.0, revendaId: '1b10b651-789a-41f2-95b7-a3a8080c0001', revendaNome: 'Revenda São Paulo TI', ativo: true, criadoEm: new Date().toISOString() },
      { id: '2a10b651-789a-41f2-95b7-a3a8080c0002', nome: 'Ana Souza', email: 'ana@epros.com.br', telefone: '(81) 97777-6666', percentualComissao: 7.5, revendaId: '2b10b651-789a-41f2-95b7-a3a8080c0002', revendaNome: 'Revenda Nordeste Sistemas', ativo: true, criadoEm: new Date().toISOString() },
      { id: '3a10b651-789a-41f2-95b7-a3a8080c0003', nome: 'Carlos Oliveira', email: 'carlos@epros.com.br', telefone: '(51) 96666-5555', percentualComissao: 6.0, revendaId: null, revendaNome: null, ativo: false, criadoEm: new Date().toISOString() }
    ]
    localStorage.setItem('epros_vendedores', JSON.stringify(list))
    stored = JSON.stringify(list)
  }
  let list = JSON.parse(stored)
  if (searchTerm.value) {
    list = list.filter(v => v.nome.toLowerCase().includes(searchTerm.value.toLowerCase()) || v.email.toLowerCase().includes(searchTerm.value.toLowerCase()))
  }
  vendedores.value = list
  totalPages.value = 1
}

const changePage = async (p) => {
  page.value = p
  await loadVendedores()
}

const editVendedor = (vendedor) => {
  isEditing.value = true
  editingId.value = vendedor.id
  form.Nome = vendedor.nome
  form.Email = vendedor.email
  form.Telefone = vendedor.telefone
  form.PercentualComissao = vendedor.percentualComissao
  form.RevendaId = vendedor.revendaId
  form.Ativo = vendedor.ativo
  addLog(`Editando vendedor: ${vendedor.nome}`, 'info')
}

const cancelEdit = () => {
  isEditing.value = false
  editingId.value = null
  resetForm()
}

const resetForm = () => {
  form.Nome = ''
  form.Email = ''
  form.Telefone = ''
  form.PercentualComissao = 5.0
  form.RevendaId = null
  form.Ativo = true
}

const handleSave = async () => {
  if (apiOnline.value) {
    try {
      if (isEditing.value) {
        const res = await $fetch(`http://localhost:5000/api/v1/plataforma/vendedores/${editingId.value}`, {
          method: 'PUT',
          body: {
            Id: editingId.value,
            Nome: form.Nome,
            Email: form.Email,
            Telefone: form.Telefone,
            PercentualComissao: form.PercentualComissao,
            RevendaId: form.RevendaId,
            Ativo: form.Ativo
          }
        })
        if (res.sucesso) {
          addLog(`Vendedor '${form.Nome}' atualizado com sucesso!`, 'success')
          cancelEdit()
          await loadVendedores()
        } else {
          addLog(`Erro ao atualizar: ${res.mensagem}`, 'error')
        }
      } else {
        const res = await $fetch('http://localhost:5000/api/v1/plataforma/vendedores', {
          method: 'POST',
          body: {
            Nome: form.Nome,
            Email: form.Email,
            Telefone: form.Telefone,
            PercentualComissao: form.PercentualComissao,
            RevendaId: form.RevendaId,
            Ativo: form.Ativo
          }
        })
        if (res.sucesso) {
          addLog(`Vendedor '${form.Nome}' cadastrado com sucesso!`, 'success')
          resetForm()
          await loadVendedores()
        } else {
          addLog(`Erro ao cadastrar: ${res.mensagem}`, 'error')
        }
      }
    } catch (e) {
      addLog(`Erro de comunicação com a API: ${e.message}`, 'error')
    }
  } else {
    // Modo Simulado
    let stored = JSON.parse(localStorage.getItem('epros_vendedores') || '[]')
    const revName = form.RevendaId ? (revendas.value.find(r => r.id === form.RevendaId)?.nome || null) : null

    if (isEditing.value) {
      const idx = stored.findIndex(v => v.id === editingId.value)
      if (idx !== -1) {
        stored[idx].nome = form.Nome
        stored[idx].email = form.Email
        stored[idx].telefone = form.Telefone
        stored[idx].percentualComissao = form.PercentualComissao
        stored[idx].revendaId = form.RevendaId
        stored[idx].revendaNome = revName
        stored[idx].ativo = form.Ativo
        localStorage.setItem('epros_vendedores', JSON.stringify(stored))
        addLog(`[Simulado] Vendedor '${form.Nome}' atualizado.`, 'success')
        cancelEdit()
        loadSimulado()
      }
    } else {
      const newVend = {
        id: crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(36).substring(2),
        nome: form.Nome,
        email: form.Email,
        telefone: form.Telefone,
        percentualComissao: form.PercentualComissao,
        revendaId: form.RevendaId,
        revendaNome: revName,
        ativo: form.Ativo,
        criadoEm: new Date().toISOString()
      }
      stored.push(newVend)
      localStorage.setItem('epros_vendedores', JSON.stringify(stored))
      addLog(`[Simulado] Vendedor '${form.Nome}' cadastrado.`, 'success')
      resetForm()
      loadSimulado()
    }
  }
}

const deleteVendedor = async (id) => {
  if (confirm('Deseja realmente excluir este vendedor?')) {
    if (apiOnline.value) {
      try {
        const res = await $fetch(`http://localhost:5000/api/v1/plataforma/vendedores/${id}`, {
          method: 'DELETE'
        })
        if (res.sucesso) {
          addLog('Vendedor excluído com sucesso!', 'success')
          await loadVendedores()
        } else {
          addLog(`Erro ao excluir: ${res.mensagem}`, 'error')
        }
      } catch (e) {
        addLog(`Erro de comunicação com a API: ${e.message}`, 'error')
      }
    } else {
      let stored = JSON.parse(localStorage.getItem('epros_vendedores') || '[]')
      stored = stored.filter(v => v.id !== id)
      localStorage.setItem('epros_vendedores', JSON.stringify(stored))
      addLog('[Simulado] Vendedor excluído.', 'success')
      loadSimulado()
    }
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
