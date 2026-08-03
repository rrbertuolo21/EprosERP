<template>
  <div class="dashboard-layout">
    <!-- Conteúdo Principal (cabeçalho/sidebar vêm do shell `admin`) -->
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Planos Comerciais</h1>
        <p class="tagline">Gerencie os planos de assinatura da plataforma, valores, limites operacionais e módulos inclusos.</p>
        <div class="header-actions">
          <NuxtLink to="/plataforma/admin" class="btn btn-secondary btn-back">
            ← Voltar ao Painel
          </NuxtLink>
          <NuxtLink to="/plataforma/admin/planos/novo" class="btn btn-primary">
            + Novo Plano
          </NuxtLink>
          <span class="status-pill" :class="{ 'offline': !apiOnline }">
            <span class="status-dot"></span>
            {{ apiOnline ? 'Conectado à API Gateway' : 'Modo Simulação Offline' }}
          </span>
        </div>
      </header>

      <section class="admin-section tenants-section glass-panel mt-4">
        <header class="section-header">
          <h3>Planos Cadastrados</h3>
          <div class="search-bar">
            <input
              type="text"
              v-model="searchTerm"
              @input="onSearch"
              placeholder="Buscar plano por nome..."
              class="search-input"
            />
          </div>
        </header>

        <div class="table-container">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>Grupo</th>
                <th>Valor</th>
                <th>Limite Usuários</th>
                <th>Limite Empresas</th>
                <th>Status</th>
                <th class="align-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="7" class="loading-cell">Carregando planos...</td>
              </tr>
              <tr v-else-if="planos.length === 0">
                <td colspan="7" class="empty-cell">Nenhum plano cadastrado.</td>
              </tr>
              <tr v-else v-for="plano in planos" :key="plano.id">
                <td><span class="tenant-name-txt">{{ plano.nome }}</span></td>
                <td>
                  <span class="tenant-id-badge">{{ nomeGrupo(plano) }}</span>
                </td>
                <td>{{ formatMoney(plano.valor) }}</td>
                <td>{{ plano.limiteUsuarios ?? '—' }}</td>
                <td>{{ plano.limiteEmpresas ?? '—' }}</td>
                <td>
                  <span :class="['badge', plano.ativo ? 'badge-success' : 'badge-danger']">
                    {{ plano.ativo ? 'Ativo' : 'Inativo' }}
                  </span>
                </td>
                <td class="align-right">
                  <NuxtLink
                    :to="`/plataforma/admin/planos/${plano.id}`"
                    class="btn btn-secondary btn-table-action"
                  >
                    Editar
                  </NuxtLink>
                  <button
                    @click="excluirPlano(plano.id)"
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
        <footer class="pagination-footer" v-if="totalPaginas > 1">
          <button :disabled="pagina === 1" @click="mudarPagina(pagina - 1)" class="btn btn-page">Anterior</button>
          <span class="page-info">Página {{ pagina }} de {{ totalPaginas }}</span>
          <button :disabled="pagina === totalPaginas" @click="mudarPagina(pagina + 1)" class="btn btn-page">Próximo</button>
        </footer>
      </section>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

// Área landlord: usa o shell administrativo (sidebar + header) do layout `admin`.
definePageMeta({ layout: 'admin' })

const apiOnline = ref(true)
const loading = ref(false)
const planos = ref([])
const grupos = ref([])
const searchTerm = ref('')
const pagina = ref(1)
const totalPaginas = ref(1)
let buscaTimer = null

onMounted(async () => {
  await checkApiConnection()
  await Promise.all([carregarGrupos(), carregarPlanos()])
})

const checkApiConnection = async () => {
  try {
    await useApi('/plataforma/superadmin/dashboard')
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
  }
}

// Normaliza respostas paginadas ou envelopadas em CommandResult.
const extrairLista = (res) => {
  if (Array.isArray(res)) return res
  return res?.items ?? res?.dados?.items ?? res?.dados ?? res?.data ?? []
}

const carregarGrupos = async () => {
  try {
    const res = await useApi('/plataforma/grupos-plano', { query: { tamanhoPagina: 200 } })
    grupos.value = extrairLista(res)
  } catch (e) {
    grupos.value = []
  }
}

const carregarPlanos = async () => {
  loading.value = true
  try {
    const res = await useApi('/plataforma/planos', {
      query: {
        pagina: pagina.value,
        tamanhoPagina: 10,
        search: searchTerm.value || undefined
      }
    })
    planos.value = extrairLista(res)
    totalPaginas.value = res?.totalPaginas ?? res?.dados?.totalPaginas ?? 1
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
    planos.value = []
    totalPaginas.value = 1
  }
  loading.value = false
}

const onSearch = () => {
  clearTimeout(buscaTimer)
  buscaTimer = setTimeout(() => {
    pagina.value = 1
    carregarPlanos()
  }, 350)
}

const mudarPagina = async (p) => {
  pagina.value = p
  await carregarPlanos()
}

const excluirPlano = async (id) => {
  if (!confirm('Deseja realmente excluir este plano?')) return
  try {
    const res = await useApi(`/plataforma/planos/${id}`, { method: 'DELETE' })
    if (res?.sucesso === false) {
      alert(`Erro ao excluir: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    await carregarPlanos()
  } catch (e) {
    alert(`Erro de comunicação com a API: ${e.message}`)
  }
}

const nomeGrupo = (plano) => {
  if (plano.grupoPlanoNome) return plano.grupoPlanoNome
  if (plano.grupoPlanoDescricao) return plano.grupoPlanoDescricao
  const g = grupos.value.find(x => x.id === plano.grupoPlanoId)
  return g?.descricao ?? g?.nome ?? '—'
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
.mt-4 {
  margin-top: 24px;
}
</style>
