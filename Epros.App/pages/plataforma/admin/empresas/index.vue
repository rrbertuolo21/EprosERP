<template>
  <div class="dashboard-content">
    <header class="page-header">
      <h1 class="glow-text">Empresas</h1>
      <p class="tagline">Cadastro de empresas (filiais/estabelecimentos) da plataforma: dados básicos, regimes e inscrições.</p>
      <div class="header-actions">
        <NuxtLink to="/plataforma/admin" class="btn btn-secondary btn-back">
          ← Voltar ao Painel
        </NuxtLink>
        <NuxtLink to="/plataforma/admin/empresas/nova" class="btn btn-primary">
          + Nova Empresa
        </NuxtLink>
        <span class="status-pill" :class="{ 'offline': !apiOnline }">
          <span class="status-dot"></span>
          {{ apiOnline ? 'Conectado à API Gateway' : 'Sem conexão com a API' }}
        </span>
      </div>
    </header>

    <section class="admin-section glass-panel mt-4">
      <header class="section-header">
        <h3>Empresas Cadastradas</h3>
        <div class="search-bar">
          <input
            type="text"
            v-model="searchTerm"
            @input="onSearch"
            placeholder="Buscar por razão social, fantasia ou CNPJ..."
            class="search-input"
          />
        </div>
      </header>

      <div class="table-container">
        <table class="admin-table">
          <thead>
            <tr>
              <th>Razão Social</th>
              <th>Nome Fantasia</th>
              <th>CNPJ</th>
              <th>Grupo Tributário</th>
              <th>Status</th>
              <th class="align-right">Ações</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="6" class="loading-cell">Carregando empresas...</td>
            </tr>
            <tr v-else-if="empresasPagina.length === 0">
              <td colspan="6" class="empty-cell">Nenhuma empresa cadastrada.</td>
            </tr>
            <tr v-else v-for="empresa in empresasPagina" :key="empresa.id">
              <td><span class="tenant-name-txt">{{ empresa.razaoSocial }}</span></td>
              <td>{{ empresa.nomeFantasia || '—' }}</td>
              <td>{{ formatCnpj(empresa.cnpj) }}</td>
              <!--
                PlanoGrupo (do Blazor legado) não existe no backend novo (entidade Empresa não tem PlanoGrupoId).
                Exibimos o Grupo Tributário, que é o agrupamento disponível no modelo atual.
              -->
              <td>
                <span class="tenant-id-badge">{{ empresa.tributarioGrupoId ? shortId(empresa.tributarioGrupoId) : '—' }}</span>
              </td>
              <td>
                <span :class="['badge', empresa.ativo ? 'badge-success' : 'badge-danger']">
                  {{ empresa.ativo ? 'Ativa' : 'Inativa' }}
                </span>
              </td>
              <td class="align-right">
                <NuxtLink
                  :to="`/plataforma/admin/empresas/${empresa.id}`"
                  class="btn btn-secondary btn-table-action"
                >
                  Editar
                </NuxtLink>
                <button
                  @click="excluirEmpresa(empresa)"
                  class="btn btn-secondary btn-table-action btn-danger-action"
                >
                  Excluir
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Paginação (client-side: o GET do backend devolve a lista completa) -->
      <footer class="pagination-footer" v-if="totalPaginas > 1">
        <button :disabled="pagina === 1" @click="mudarPagina(pagina - 1)" class="btn btn-page">Anterior</button>
        <span class="page-info">Página {{ pagina }} de {{ totalPaginas }}</span>
        <button :disabled="pagina === totalPaginas" @click="mudarPagina(pagina + 1)" class="btn btn-page">Próximo</button>
      </footer>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useApi, type CommandResult } from '~/composables/useApi'

interface Empresa {
  id: string
  razaoSocial: string
  nomeFantasia?: string | null
  cnpj: string
  tributarioGrupoId?: string | null
  ativo: boolean
}

const msgErro = (e: unknown) => (e instanceof Error ? e.message : String(e))

// Área landlord: usa o shell administrativo (sidebar + header) fornecido pelo layout `admin`.
definePageMeta({ layout: 'admin' })

const apiOnline = ref(true)
const loading = ref(false)
const empresas = ref<Empresa[]>([])
const searchTerm = ref('')
const pagina = ref(1)
const tamanhoPagina = ref(10)

const empresasFiltradas = computed(() => {
  const termo = searchTerm.value.trim().toLowerCase()
  if (!termo) return empresas.value
  return empresas.value.filter((e) =>
    (e.razaoSocial || '').toLowerCase().includes(termo) ||
    (e.nomeFantasia || '').toLowerCase().includes(termo) ||
    (e.cnpj || '').includes(termo.replace(/\D/g, ''))
  )
})

const totalPaginas = computed(() =>
  Math.max(1, Math.ceil(empresasFiltradas.value.length / tamanhoPagina.value))
)

const empresasPagina = computed(() => {
  const inicio = (pagina.value - 1) * tamanhoPagina.value
  return empresasFiltradas.value.slice(inicio, inicio + tamanhoPagina.value)
})

onMounted(async () => {
  await carregarEmpresas()
})

const carregarEmpresas = async () => {
  loading.value = true
  try {
    const res = await useApi<CommandResult<Empresa[]>>('/cadastros/empresas', {
      query: {
        pagina: pagina.value,
        tamanhoPagina: tamanhoPagina.value,
        search: searchTerm.value
      }
    })
    // O GET atual devolve CommandResult { sucesso, dados: [...] } sem paginação de servidor.
    const lista = res?.dados ?? res?.data
    empresas.value = Array.isArray(lista) ? lista : []
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
    empresas.value = []
  }
  loading.value = false
}

const onSearch = () => {
  pagina.value = 1
}

const mudarPagina = (p: number) => {
  pagina.value = p
}

const excluirEmpresa = async (empresa: Empresa) => {
  if (!confirm(`Excluir a empresa "${empresa.razaoSocial}"? Esta ação não pode ser desfeita.`)) return
  try {
    const res = await useApi<CommandResult>('/cadastros/empresas/{id}', {
      method: 'DELETE',
      params: { id: empresa.id }
    })
    if (res?.sucesso === false) {
      alert(`Não foi possível excluir: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    await carregarEmpresas()
  } catch (e) {
    alert(`Erro ao excluir empresa: ${msgErro(e)}`)
  }
}

const formatCnpj = (val?: string) => {
  if (!val) return ''
  const clean = val.replace(/\D/g, '')
  if (clean.length === 14) {
    return clean.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')
  }
  return val
}

const shortId = (id: string) => (id ? `${id.slice(0, 8)}…` : '')
</script>

<style scoped>
.header-actions {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-top: 8px;
  flex-wrap: wrap;
}
.btn-back {
  padding: 8px 16px;
  font-size: 13px;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
}
.btn-back:hover {
  background: rgba(255, 255, 255, 0.06);
  color: var(--text-primary);
}
.search-bar {
  margin-top: 8px;
  width: 340px;
  max-width: 100%;
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
.loading-cell,
.empty-cell {
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
  background: rgba(255, 255, 255, 0.02);
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
