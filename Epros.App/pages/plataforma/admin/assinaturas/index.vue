<template>
  <div class="dashboard-layout">
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Assinaturas</h1>
        <p class="tagline">Acompanhe a assinatura vigente e as faturas por inquilino. Aprove assinaturas manualmente quando necessário.</p>
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

      <div class="assinatura-layout mt-4">
        <!-- Lista de clientes -->
        <section class="admin-section glass-panel clientes-panel">
          <header class="section-header">
            <h3>Clientes</h3>
            <div class="search-bar">
              <input
                type="text"
                v-model="searchTerm"
                placeholder="Buscar cliente..."
                class="search-input"
              />
            </div>
          </header>
          <div class="clientes-list">
            <p v-if="loadingClientes" class="list-empty">Carregando clientes...</p>
            <p v-else-if="clientesFiltrados.length === 0" class="list-empty">Nenhum cliente encontrado.</p>
            <button
              v-for="c in clientesFiltrados"
              :key="c.id"
              type="button"
              :class="['cliente-item', { 'active': selecionado && selecionado.id === c.id }]"
              @click="selecionarCliente(c)"
            >
              <span class="cliente-nome">{{ c.razaoSocial }}</span>
              <span :class="['badge', getStatusBadgeClass(c.statusSaaS, c.ativo)]">
                {{ getStatusLabel(c.statusSaaS, c.ativo) }}
              </span>
            </button>
          </div>
        </section>

        <!-- Detalhe da assinatura -->
        <section class="admin-section glass-panel detalhe-panel">
          <div v-if="!selecionado" class="detalhe-vazio">
            <p>Selecione um cliente para ver a assinatura e as faturas.</p>
          </div>

          <template v-else>
            <header class="section-header">
              <div>
                <h3>{{ selecionado.razaoSocial }}</h3>
                <span class="cliente-cnpj">{{ formatCnpj(selecionado.cnpj) }}</span>
              </div>
              <button type="button" class="btn btn-primary" @click="openAprovarModal">
                Aprovar assinatura manual
              </button>
            </header>

            <!-- Assinatura vigente -->
            <div class="subsection">
              <h4 class="subsection-title">Assinatura vigente</h4>
              <p class="context-note">Reflete o contexto de assinatura atual (self-service do tenant).</p>
              <div v-if="loadingDetalhe" class="list-empty">Carregando...</div>
              <div v-else-if="assinatura" class="info-grid">
                <div class="info-item">
                  <span class="info-label">Status</span>
                  <span class="info-value">
                    <span :class="['badge', getStatusBadgeClass(assinatura.status, true)]">{{ assinatura.status }}</span>
                  </span>
                </div>
                <div class="info-item">
                  <span class="info-label">Método</span>
                  <span class="info-value">{{ assinatura.metodoPagamento || '—' }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Início</span>
                  <span class="info-value">{{ formatDate(assinatura.dataInicio) }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Fim</span>
                  <span class="info-value">{{ formatDate(assinatura.dataFim) }}</span>
                </div>
                <div class="info-item">
                  <span class="info-label">Trial até</span>
                  <span class="info-value">{{ formatDate(assinatura.trialAte) }}</span>
                </div>
              </div>
              <p v-else class="list-empty">Sem assinatura vigente disponível.</p>
            </div>

            <!-- Faturas -->
            <div class="subsection">
              <h4 class="subsection-title">Faturas</h4>
              <div class="table-container">
                <table class="admin-table">
                  <thead>
                    <tr>
                      <th>Vencimento</th>
                      <th>Valor</th>
                      <th>Pagamento</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-if="loadingDetalhe">
                      <td colspan="4" class="empty-cell">Carregando faturas...</td>
                    </tr>
                    <tr v-else-if="faturas.length === 0">
                      <td colspan="4" class="empty-cell">Nenhuma fatura disponível.</td>
                    </tr>
                    <tr v-else v-for="f in faturas" :key="f.id">
                      <td>{{ formatDate(f.dataVencimento) }}</td>
                      <td>{{ formatMoney(f.valor) }}</td>
                      <td>{{ f.dataPagamento ? formatDate(f.dataPagamento) : '—' }}</td>
                      <td><span :class="['badge', getStatusBadgeClass(f.status, false)]">{{ f.status }}</span></td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </template>
        </section>
      </div>
    </main>

    <!-- MODAL: APROVAR ASSINATURA MANUAL -->
    <div class="modal-backdrop" v-if="aprovarModal.open">
      <div class="modal-card glass-panel">
        <header class="modal-header">
          <h3>Aprovar assinatura manual</h3>
          <button type="button" @click="aprovarModal.open = false" class="btn-close">×</button>
        </header>
        <p class="modal-subtitle">{{ selecionado?.razaoSocial }}</p>
        <form @submit.prevent="aprovarAssinaturaManual" class="vertical-form">
          <div class="form-row">
            <div class="form-group col-6">
              <DateTimeField v-model="aprovarModal.form.dataInicio" label="Data de início" mode="date" required />
            </div>
            <div class="form-group col-6">
              <DateTimeField v-model="aprovarModal.form.dataFim" label="Data de fim (opcional)" mode="date" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group col-6">
              <label>Dia de vencimento (1 a 28) *</label>
              <input type="number" v-model.number="aprovarModal.form.diaVencimento" min="1" max="28" required />
            </div>
            <div class="form-group col-6">
              <MoneyInput v-model="aprovarModal.form.valorRecorrente" label="Valor recorrente" :min="0.01" required />
            </div>
          </div>
          <div class="form-group">
            <label>Operador responsável *</label>
            <input type="text" v-model="aprovarModal.form.operador" placeholder="Nome do operador" required />
          </div>
          <div class="form-group">
            <label>Justificativa (mín. 10 caracteres) *</label>
            <textarea v-model="aprovarModal.form.justificativa" rows="3" minlength="10" placeholder="Motivo da aprovação manual da assinatura" required></textarea>
          </div>
          <button type="submit" class="btn btn-primary btn-block mt-4" :disabled="aprovarModal.saving">
            {{ aprovarModal.saving ? 'Enviando...' : 'Confirmar aprovação' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'

definePageMeta({ layout: 'admin' })

const apiOnline = ref(true)
const loadingClientes = ref(false)
const loadingDetalhe = ref(false)
const clientes = ref([])
const searchTerm = ref('')
const selecionado = ref(null)
const assinatura = ref(null)
const faturas = ref([])

const aprovarModal = reactive({
  open: false,
  saving: false,
  form: {
    dataInicio: new Date().toISOString().split('T')[0],
    dataFim: '',
    diaVencimento: 10,
    valorRecorrente: 0,
    operador: '',
    justificativa: ''
  }
})

const clientesFiltrados = computed(() => {
  const termo = searchTerm.value.trim().toLowerCase()
  if (!termo) return clientes.value
  return clientes.value.filter(c =>
    (c.razaoSocial || '').toLowerCase().includes(termo) ||
    (c.cnpj || '').includes(termo)
  )
})

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
  loadingClientes.value = true
  try {
    const res = await useApi('/plataforma/superadmin/clientes')
    const lista = Array.isArray(res) ? res : res?.items ?? []
    clientes.value = lista.map(c => ({
      id: c.id,
      razaoSocial: c.razaoSocial,
      cnpj: c.cnpj,
      statusSaaS: c.statusSaaS,
      ativo: c.ativo
    }))
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
    clientes.value = []
  }
  loadingClientes.value = false
}

const selecionarCliente = async (cliente) => {
  selecionado.value = cliente
  assinatura.value = null
  faturas.value = []
  loadingDetalhe.value = true
  try {
    const [vig, fats] = await Promise.allSettled([
      useApi('/aplicativo/assinaturas/vigente'),
      useApi('/aplicativo/assinaturas/faturas')
    ])
    if (vig.status === 'fulfilled') assinatura.value = vig.value
    if (fats.status === 'fulfilled') {
      const lista = Array.isArray(fats.value) ? fats.value : fats.value?.items ?? []
      faturas.value = lista
    }
  } catch (e) {
    // painel informativo — mantém vazio em caso de indisponibilidade
  }
  loadingDetalhe.value = false
}

const openAprovarModal = () => {
  aprovarModal.form.dataInicio = new Date().toISOString().split('T')[0]
  aprovarModal.form.dataFim = ''
  aprovarModal.form.diaVencimento = 10
  aprovarModal.form.valorRecorrente = 0
  aprovarModal.form.operador = ''
  aprovarModal.form.justificativa = ''
  aprovarModal.open = true
}

const aprovarAssinaturaManual = async () => {
  if (!selecionado.value) return
  if (aprovarModal.form.justificativa.trim().length < 10) {
    alert('A justificativa deve ter no mínimo 10 caracteres.')
    return
  }
  aprovarModal.saving = true
  try {
    const res = await useApi(`/plataforma/superadmin/clientes/${selecionado.value.id}/aprovar-assinatura-manual`, {
      method: 'POST',
      body: {
        ClienteId: selecionado.value.id,
        DataInicio: aprovarModal.form.dataInicio,
        DataFim: aprovarModal.form.dataFim || null,
        DiaVencimento: aprovarModal.form.diaVencimento,
        ValorRecorrente: aprovarModal.form.valorRecorrente,
        FaturaPendenteIdParaBaixar: null,
        Operador: aprovarModal.form.operador,
        Justificativa: aprovarModal.form.justificativa
      }
    })
    if (res?.sucesso === false) {
      alert(`Falha ao aprovar assinatura: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    alert('Assinatura manual aprovada com sucesso!')
    aprovarModal.open = false
    await selecionarCliente(selecionado.value)
  } catch (e) {
    alert(`Erro ao aprovar assinatura: ${e.message}`)
  } finally {
    aprovarModal.saving = false
  }
}

const getStatusBadgeClass = (status, ativoFallback) => {
  const s = (status || '').toLowerCase()
  if (s.includes('active') || s.includes('ativ') || s.includes('pago') || s.includes('vigent')) return 'badge-success'
  if (s.includes('suspend') || s.includes('cancel') || s.includes('venc') || s.includes('inadimp')) return 'badge-danger'
  if (s.includes('trial') || s.includes('pendente') || s.includes('aguard')) return 'badge-warning'
  return ativoFallback ? 'badge-success' : 'badge-danger'
}

const getStatusLabel = (status, ativo) => {
  if (status) return status
  return ativo ? 'Ativo' : 'Suspenso'
}

const formatDate = (dateStr) => {
  if (!dateStr) return '—'
  return new Date(dateStr).toLocaleDateString('pt-BR')
}

const formatMoney = (v) => {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v ?? 0)
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
.assinatura-layout {
  display: grid;
  grid-template-columns: 340px 1fr;
  gap: 24px;
  align-items: start;
}
@media (max-width: 900px) {
  .assinatura-layout { grid-template-columns: 1fr; }
}
.search-bar {
  width: 180px;
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
.clientes-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-top: 12px;
  max-height: 560px;
  overflow-y: auto;
}
.cliente-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  background: rgba(255,255,255,0.02);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  color: var(--text-primary);
  cursor: pointer;
  text-align: left;
  transition: all 0.15s ease;
}
.cliente-item:hover {
  background: rgba(255,255,255,0.05);
}
.cliente-item.active {
  border-color: var(--primary);
  box-shadow: 0 0 10px rgba(99, 102, 241, 0.15);
}
.cliente-nome {
  font-size: 13.5px;
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.cliente-cnpj {
  font-size: 12px;
  color: var(--text-secondary);
}
.list-empty {
  color: var(--text-secondary);
  font-size: 13px;
  padding: 16px 4px;
}
.detalhe-vazio {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 200px;
  color: var(--text-secondary);
}
.subsection {
  margin-top: 24px;
}
.subsection-title {
  font-size: 14px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 6px;
}
.context-note {
  font-size: 12px;
  color: var(--text-secondary);
  margin-bottom: 12px;
}
.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 16px;
}
.info-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.info-label {
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-secondary);
}
.info-value {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary);
}
.empty-cell {
  text-align: center;
  padding: 32px !important;
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
.badge-warning {
  background: rgba(245, 158, 11, 0.1);
  color: #fbbf24;
  border: 1px solid rgba(245, 158, 11, 0.2);
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
.modal-subtitle {
  color: var(--text-secondary);
  font-size: 13px;
  margin-bottom: 16px;
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
.mt-4 { margin-top: 24px; }
</style>
