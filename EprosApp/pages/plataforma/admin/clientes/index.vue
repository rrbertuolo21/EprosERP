<template>
  <div class="dashboard-layout">
    <!-- Conteúdo Principal (cabeçalho vem do shell `admin`) -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Gerenciamento de Clientes (Tenants)</h1>
        <p class="tagline">Gerencie a carteira de inquilinos SaaS da plataforma, status de faturamento e limites operacionais.</p>
        <div class="header-actions">
          <NuxtLink to="/plataforma/admin" class="btn btn-secondary btn-back">
            ← Voltar ao Painel
          </NuxtLink>
          <NuxtLink to="/plataforma/admin/clientes/novo" class="btn btn-primary">
            + Novo Inquilino
          </NuxtLink>
          <span class="status-pill" :class="{ 'offline': !apiOnline }">
            <span class="status-dot"></span>
            {{ apiOnline ? 'Conectado à API Gateway' : 'Modo Simulação Offline' }}
          </span>
        </div>
      </header>

      <!-- Grid Principal de Clientes -->
      <section class="admin-section tenants-section glass-panel mt-4">
        <header class="section-header">
          <h3>Clientes Ativos</h3>
          <div class="search-bar">
            <input 
              type="text" 
              v-model="searchTerm" 
              @input="loadClientes" 
              placeholder="Buscar cliente por nome ou CNPJ..." 
              class="search-input"
            />
          </div>
        </header>

        <div class="table-container">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Razão Social / CNPJ</th>
                <th>Contato</th>
                <th>Plano Contratado</th>
                <th>Vencimento</th>
                <th>Status</th>
                <th class="align-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="6" class="loading-cell">Carregando carteira de clientes...</td>
              </tr>
              <tr v-else-if="clientes.length === 0">
                <td colspan="6" class="empty-cell">Nenhum inquilino cadastrado.</td>
              </tr>
              <tr v-else v-for="cliente in clientes" :key="cliente.id">
                <td>
                  <div class="tenant-details">
                    <span class="tenant-name-txt">{{ cliente.razaoSocial }}</span>
                    <span class="tenant-email-txt">{{ formatCnpj(cliente.cnpj) }}</span>
                  </div>
                </td>
                <td>
                  <div class="tenant-details">
                    <span class="tenant-email-txt">{{ cliente.email }}</span>
                    <span class="tenant-email-txt">{{ cliente.telefone || 'Sem telefone' }}</span>
                  </div>
                </td>
                <td>
                  <span class="tenant-id-badge">{{ cliente.planoNome || cliente.planoId }}</span>
                  <span v-if="cliente.isDemo" class="demo-badge">Demo</span>
                </td>
                <td>Dia {{ cliente.diaVencimento }}</td>
                <td>
                  <span :class="['badge', getStatusBadgeClass(cliente.statusSaaS, cliente.ativo)]">
                    {{ getStatusLabel(cliente.statusSaaS, cliente.ativo) }}
                  </span>
                </td>
                <td class="align-right">
                  <NuxtLink 
                    :to="`/plataforma/admin/clientes/${cliente.id}`" 
                    class="btn btn-secondary btn-table-action"
                  >
                    Editar
                  </NuxtLink>
                  <button 
                    v-if="cliente.statusSaaS === 'Active' || cliente.ativo" 
                    @click="updateStatus(cliente.id, 'Suspenso')" 
                    class="btn btn-secondary btn-table-action btn-danger-action"
                  >
                    Suspender
                  </button>
                  <button 
                    v-else 
                    @click="updateStatus(cliente.id, 'Ativo')" 
                    class="btn btn-primary btn-table-action btn-success-action"
                  >
                    Ativar
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

definePageMeta({ layout: 'admin' })

const apiOnline = ref(true)
const loading = ref(false)
const clientes = ref([])
const searchTerm = ref('')

onMounted(async () => {
  await checkApiConnection()
  await loadClientes()
})

const checkApiConnection = async () => {
  try {
    await useApi('/plataforma/superadmin/dashboard')
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
  }
}

const loadClientes = async () => {
  loading.value = true
  if (apiOnline.value) {
    try {
      // 1. Busca os clientes da API superadmin
      const res = await useApi('/plataforma/superadmin/clientes')
      
      // Mapeia para corresponder ao DTO detalhado
      clientes.value = res.map(c => ({
        id: c.id,
        razaoSocial: c.razaoSocial,
        cnpj: c.cnpj,
        email: c.email,
        planoId: c.planoNome || 'Prata',
        planoNome: c.planoNome || 'Prata',
        diaVencimento: c.diaVencimento || 10,
        statusSaaS: c.statusSaaS || (c.ativo ? 'Active' : 'Suspended'),
        ativo: c.ativo,
        isDemo: c.isDemo || false,
        telefone: c.telefone
      }))

      if (searchTerm.value) {
        clientes.value = clientes.value.filter(c => 
          c.razaoSocial.toLowerCase().includes(searchTerm.value.toLowerCase()) || 
          c.cnpj.includes(searchTerm.value)
        )
      }
    } catch (e) {
      loadSimulado()
    }
  } else {
    loadSimulado()
  }
  loading.value = false
}

const loadSimulado = () => {
  let stored = localStorage.getItem('epros_tenants')
  if (stored) {
    const list = JSON.parse(stored)
    clientes.value = list.map(c => ({
      id: c.id,
      razaoSocial: c.name,
      cnpj: c.cnpj.replace(/\D/g, ''),
      email: c.email,
      planoNome: c.plan,
      planoId: c.plan,
      diaVencimento: c.diaVencimento || 10,
      statusSaaS: c.status === 'Ativo' ? 'Active' : 'Suspended',
      ativo: c.status === 'Ativo',
      isDemo: c.isDemo || false,
      telefone: c.telefone || ''
    }))
  } else {
    clientes.value = [
      { id: '1c10b651-789a-41f2-95b7-a3a8080c0001', razaoSocial: 'Alfa Transportes Ltda', cnpj: '11111111000111', email: 'contato@alfa.com.br', planoNome: 'Plano Silver', diaVencimento: 10, statusSaaS: 'Active', ativo: true, isDemo: false, telefone: '(11) 97777-6666' },
      { id: '2c10b651-789a-41f2-95b7-a3a8080c0002', razaoSocial: 'Beta Distribuidora de Alimentos', cnpj: '22222222000122', email: 'financeiro@beta.com.br', planoNome: 'Plano Gold', diaVencimento: 15, statusSaaS: 'Active', ativo: true, isDemo: false, telefone: '(11) 98888-5555' },
      { id: '3c10b651-789a-41f2-95b7-a3a8080c0003', razaoSocial: 'Gama Metalurgica S.A.', cnpj: '33333333000133', email: 'ti@gama.com.br', planoNome: 'Plano Platinum', diaVencimento: 20, statusSaaS: 'Suspended', ativo: false, isDemo: true, telefone: '(11) 96666-4444' }
    ]
  }

  if (searchTerm.value) {
    clientes.value = clientes.value.filter(c => 
      c.razaoSocial.toLowerCase().includes(searchTerm.value.toLowerCase()) || 
      c.cnpj.includes(searchTerm.value)
    )
  }
}

const updateStatus = async (id, newStatus) => {
  if (apiOnline.value) {
    try {
      const endpoint = newStatus === 'Suspenso' ? 'suspender' : 'ativar'
      await useApi(`/plataforma/clientes/${id}/${endpoint}`, {
        method: 'POST'
      })
      await loadClientes()
    } catch (e) {
      alert(`Erro de comunicação com a API: ${e.message}`)
    }
  } else {
    // Simulado
    const stored = JSON.parse(localStorage.getItem('epros_tenants') || '[]')
    const idx = stored.findIndex(t => t.id === id)
    if (idx !== -1) {
      stored[idx].status = newStatus === 'Suspenso' ? 'Suspenso' : 'Ativo'
      localStorage.setItem('epros_tenants', JSON.stringify(stored))
      loadSimulado()
    }
  }
}

const getStatusBadgeClass = (status, ativo) => {
  if (status === 'Active' || ativo) return 'badge-success'
  return 'badge-danger'
}

const getStatusLabel = (status, ativo) => {
  if (status === 'Active' || ativo) return 'Ativo'
  return 'Suspenso'
}

const formatCnpj = (val) => {
  if (!val) return ''
  const clean = val.replace(/\D/g, '')
  if (clean.length === 14) {
    return clean.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')
  }
  return val
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
  margin-left: 8px;
  font-size: 10px;
  padding: 2px 6px;
  border-radius: 4px;
  background: rgba(168, 85, 247, 0.15);
  color: #c084fc;
  border: 1px solid rgba(168, 85, 247, 0.3);
}
.mt-4 {
  margin-top: 24px;
}
</style>
