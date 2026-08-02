<template>
  <div class="dashboard-layout">
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Faturas</h1>
        <p class="tagline">Gerencie as faturas dos inquilinos: gere cobranças, dê baixa manual e acompanhe o status de pagamento.</p>
        <div class="header-actions">
          <NuxtLink to="/plataforma/admin" class="btn btn-secondary btn-back">
            ← Voltar ao Painel
          </NuxtLink>
          <button type="button" class="btn btn-primary" @click="openGerarModal">
            + Gerar fatura
          </button>
          <span class="status-pill" :class="{ 'offline': !apiOnline }">
            <span class="status-dot"></span>
            {{ apiOnline ? 'Conectado à API Gateway' : 'Modo Simulação Offline' }}
          </span>
        </div>
      </header>

      <!-- Filtros -->
      <section class="admin-section glass-panel mt-4">
        <header class="section-header">
          <h3>Filtros</h3>
        </header>
        <div class="form-row">
          <div class="form-group col-4">
            <label>Cliente</label>
            <select v-model="filtro.clienteId" @change="recarregar">
              <option :value="null">Todos os clientes</option>
              <option v-for="c in clientes" :key="c.id" :value="c.id">
                {{ c.razaoSocial }}
              </option>
            </select>
          </div>
          <div class="form-group col-4">
            <label>Status</label>
            <select v-model="filtro.status" @change="recarregar">
              <option :value="null">Todos</option>
              <option v-for="s in statusDisponiveis" :key="s" :value="s">{{ s }}</option>
            </select>
          </div>
          <div class="form-group col-4">
            <label>Itens por página</label>
            <select v-model.number="filtro.tamanhoPagina" @change="recarregar">
              <option :value="10">10</option>
              <option :value="25">25</option>
              <option :value="50">50</option>
            </select>
          </div>
        </div>
      </section>

      <!-- Tabela -->
      <section class="admin-section glass-panel mt-4">
        <header class="section-header">
          <h3>Faturas</h3>
        </header>

        <div class="table-container">
          <table class="admin-table">
            <thead>
              <tr>
                <th>Vencimento</th>
                <th>Cliente</th>
                <th>Valor</th>
                <th>Status</th>
                <th class="align-right">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="5" class="loading-cell">Carregando faturas...</td>
              </tr>
              <tr v-else-if="faturas.length === 0">
                <td colspan="5" class="empty-cell">Nenhuma fatura encontrada.</td>
              </tr>
              <tr v-else v-for="f in faturas" :key="f.id">
                <td>{{ formatDate(f.dataVencimento) }}</td>
                <td>
                  <div class="tenant-details">
                    <span class="tenant-name-txt">{{ f.clienteRazaoSocial }}</span>
                    <span v-if="f.dataPagamento" class="tenant-email-txt">Pago em {{ formatDate(f.dataPagamento) }}</span>
                  </div>
                </td>
                <td>{{ formatMoney(f.valor) }}</td>
                <td>
                  <span :class="['badge', getStatusBadgeClass(f.status)]">{{ f.status }}</span>
                </td>
                <td class="align-right">
                  <NuxtLink :to="`/plataforma/admin/faturas/${f.id}`" class="btn btn-secondary btn-table-action">
                    Ver
                  </NuxtLink>
                  <button
                    type="button"
                    class="btn btn-secondary btn-table-action"
                    :disabled="pixModal.gerando && pixModal.faturaId === f.id"
                    @click="gerarPix(f)"
                  >
                    {{ pixModal.gerando && pixModal.faturaId === f.id ? 'Gerando...' : 'Gerar Pix' }}
                  </button>
                  <button
                    type="button"
                    class="btn btn-primary btn-table-action btn-success-action"
                    @click="openBaixaModal(f)"
                  >
                    Baixa manual
                  </button>
                  <button
                    type="button"
                    class="btn btn-secondary btn-table-action btn-danger-action"
                    @click="excluirFatura(f)"
                  >
                    Excluir
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Paginação -->
        <footer class="table-footer">
          <span class="page-info">Página {{ filtro.pagina }} de {{ totalPaginas || 1 }}</span>
          <div class="page-actions">
            <button type="button" class="btn btn-secondary btn-sm" :disabled="filtro.pagina <= 1" @click="mudarPagina(-1)">
              ← Anterior
            </button>
            <button type="button" class="btn btn-secondary btn-sm" :disabled="filtro.pagina >= totalPaginas" @click="mudarPagina(1)">
              Próxima →
            </button>
          </div>
        </footer>
      </section>
    </main>

    <!-- MODAL: GERAR FATURA -->
    <div class="modal-backdrop" v-if="gerarModal.open">
      <div class="modal-card glass-panel">
        <header class="modal-header">
          <h3>Gerar fatura</h3>
          <button type="button" @click="gerarModal.open = false" class="btn-close">×</button>
        </header>
        <form @submit.prevent="gerarFatura" class="vertical-form">
          <div class="form-group">
            <label>Cliente *</label>
            <select v-model="gerarModal.form.clienteId" required>
              <option value="" disabled>Selecione um cliente...</option>
              <option v-for="c in clientes" :key="c.id" :value="c.id">{{ c.razaoSocial }}</option>
            </select>
          </div>
          <div class="form-row">
            <div class="form-group col-6">
              <MoneyInput v-model="gerarModal.form.valor" label="Valor" required />
            </div>
            <div class="form-group col-6">
              <DateTimeField v-model="gerarModal.form.dataVencimento" label="Vencimento" mode="date" required />
            </div>
          </div>
          <button type="submit" class="btn btn-primary btn-block mt-4" :disabled="gerarModal.saving">
            {{ gerarModal.saving ? 'Gerando...' : 'Gerar fatura' }}
          </button>
        </form>
      </div>
    </div>

    <!-- MODAL: BAIXA MANUAL -->
    <div class="modal-backdrop" v-if="baixaModal.open">
      <div class="modal-card glass-panel">
        <header class="modal-header">
          <h3>Baixa manual da fatura</h3>
          <button type="button" @click="baixaModal.open = false" class="btn-close">×</button>
        </header>
        <p class="modal-subtitle">
          {{ baixaModal.cliente }} · Vencimento {{ formatDate(baixaModal.dataVencimento) }}
        </p>
        <form @submit.prevent="baixarManual" class="vertical-form">
          <div class="form-row">
            <div class="form-group col-6">
              <MoneyInput v-model="baixaModal.form.valorPago" label="Valor pago" required />
            </div>
            <div class="form-group col-6">
              <label>Forma de pagamento *</label>
              <select v-model="baixaModal.form.formaPagamento" required>
                <option value="" disabled>Selecione...</option>
                <option v-for="fp in formasPagamento" :key="fp" :value="fp">{{ fp }}</option>
              </select>
            </div>
          </div>
          <div class="form-group">
            <DateTimeField v-model="baixaModal.form.dataPagamento" label="Data do pagamento" mode="date" />
          </div>
          <button type="submit" class="btn btn-primary btn-block mt-4" :disabled="baixaModal.saving">
            {{ baixaModal.saving ? 'Registrando...' : 'Confirmar baixa' }}
          </button>
        </form>
      </div>
    </div>

    <!-- DIÁLOGO: COBRANÇA PIX -->
    <AppDialog v-model="pixModal.open" title="Cobrança Pix" width="480px">
      <div class="pix-body">
        <div v-if="pixModal.dados.qrCodeBase64" class="pix-qr-wrap">
          <img
            class="pix-qr"
            :src="`data:image/png;base64,${pixModal.dados.qrCodeBase64}`"
            alt="QR Code Pix"
          />
        </div>

        <div class="pix-field">
          <label>Pix copia-e-cola</label>
          <div class="pix-copy-row">
            <textarea class="pix-copia-cola" readonly rows="3" :value="pixModal.dados.qrCode"></textarea>
            <button type="button" class="btn btn-secondary btn-sm" @click="copiarPix">
              {{ pixCopiado ? 'Copiado!' : 'Copiar' }}
            </button>
          </div>
        </div>

        <div class="pix-meta">
          <div v-if="pixModal.dados.paymentId" class="pix-meta-item">
            <span class="pix-meta-label">ID do pagamento</span>
            <span class="pix-meta-value">{{ pixModal.dados.paymentId }}</span>
          </div>
          <div v-if="pixModal.dados.dataExpiracao" class="pix-meta-item">
            <span class="pix-meta-label">Expira em</span>
            <span class="pix-meta-value">{{ formatDateTime(pixModal.dados.dataExpiracao) }}</span>
          </div>
        </div>

        <a
          v-if="pixModal.dados.ticketUrl"
          :href="pixModal.dados.ticketUrl"
          target="_blank"
          rel="noopener"
          class="btn btn-secondary btn-block mt-2"
        >
          Abrir link da cobrança ↗
        </a>
      </div>

      <template #footer>
        <button type="button" class="btn btn-primary" @click="pixModal.open = false">Fechar</button>
      </template>
    </AppDialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import AppDialog from '~/components/shared/AppDialog.vue'

definePageMeta({ layout: 'admin' })

const apiOnline = ref(true)
const loading = ref(false)
const faturas = ref([])
const clientes = ref([])
const totalPaginas = ref(1)
const totalRegistros = ref(0)

const statusDisponiveis = ['Pendente', 'Pago', 'Vencida', 'Cancelada']
const formasPagamento = ['Pix', 'Boleto', 'Cartão', 'Transferência', 'Dinheiro']

const filtro = reactive({
  pagina: 1,
  tamanhoPagina: 25,
  clienteId: null,
  status: null
})

const gerarModal = reactive({
  open: false,
  saving: false,
  form: { clienteId: '', valor: 0, dataVencimento: new Date().toISOString().split('T')[0] }
})

const baixaModal = reactive({
  open: false,
  saving: false,
  faturaId: '',
  cliente: '',
  dataVencimento: '',
  form: { valorPago: 0, formaPagamento: '', dataPagamento: new Date().toISOString().split('T')[0] }
})

const pixModal = reactive({
  open: false,
  gerando: false,
  faturaId: '',
  dados: { paymentId: '', qrCode: '', qrCodeBase64: '', ticketUrl: '', dataExpiracao: '' }
})
const pixCopiado = ref(false)

onMounted(async () => {
  await checkApiConnection()
  await Promise.all([loadClientes(), loadFaturas()])
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
  try {
    const res = await useApi('/plataforma/superadmin/clientes')
    clientes.value = (Array.isArray(res) ? res : res?.items ?? []).map(c => ({
      id: c.id,
      razaoSocial: c.razaoSocial
    }))
  } catch (e) {
    clientes.value = []
  }
}

const loadFaturas = async () => {
  loading.value = true
  try {
    const query = {
      pagina: filtro.pagina,
      tamanhoPagina: filtro.tamanhoPagina
    }
    if (filtro.clienteId) query.clienteId = filtro.clienteId
    if (filtro.status) query.status = filtro.status

    const res = await useApi('/plataforma/faturas', { query })
    faturas.value = res?.items ?? []
    totalPaginas.value = res?.totalPaginas ?? 1
    totalRegistros.value = res?.totalRegistros ?? faturas.value.length
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
    faturas.value = []
    totalPaginas.value = 1
  }
  loading.value = false
}

const recarregar = () => {
  filtro.pagina = 1
  loadFaturas()
}

const mudarPagina = (delta) => {
  const nova = filtro.pagina + delta
  if (nova < 1 || nova > totalPaginas.value) return
  filtro.pagina = nova
  loadFaturas()
}

const openGerarModal = () => {
  gerarModal.form.clienteId = filtro.clienteId || ''
  gerarModal.form.valor = 0
  gerarModal.form.dataVencimento = new Date().toISOString().split('T')[0]
  gerarModal.open = true
}

const gerarFatura = async () => {
  if (!gerarModal.form.clienteId) {
    alert('Selecione um cliente.')
    return
  }
  gerarModal.saving = true
  try {
    const res = await useApi('/plataforma/clientes/faturas', {
      method: 'POST',
      body: {
        ClienteId: gerarModal.form.clienteId,
        Valor: gerarModal.form.valor,
        DataVencimento: gerarModal.form.dataVencimento
      }
    })
    if (res?.sucesso === false) {
      alert(`Falha ao gerar fatura: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    gerarModal.open = false
    await loadFaturas()
  } catch (e) {
    alert(`Erro ao gerar fatura: ${e.message}`)
  } finally {
    gerarModal.saving = false
  }
}

const openBaixaModal = (fatura) => {
  baixaModal.faturaId = fatura.id
  baixaModal.cliente = fatura.clienteRazaoSocial
  baixaModal.dataVencimento = fatura.dataVencimento
  baixaModal.form.valorPago = fatura.valor
  baixaModal.form.formaPagamento = ''
  baixaModal.form.dataPagamento = new Date().toISOString().split('T')[0]
  baixaModal.open = true
}

const baixarManual = async () => {
  baixaModal.saving = true
  try {
    const res = await useApi(`/plataforma/faturas/${baixaModal.faturaId}/baixar-manual`, {
      method: 'POST',
      body: {
        FaturaId: baixaModal.faturaId,
        ValorPago: baixaModal.form.valorPago,
        FormaPagamento: baixaModal.form.formaPagamento,
        DataPagamento: baixaModal.form.dataPagamento || null
      }
    })
    if (res?.sucesso === false) {
      alert(`Falha na baixa manual: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    baixaModal.open = false
    await loadFaturas()
  } catch (e) {
    alert(`Erro na baixa manual: ${e.message}`)
  } finally {
    baixaModal.saving = false
  }
}

const excluirFatura = async (fatura) => {
  if (!confirm(`Excluir a fatura de ${fatura.clienteRazaoSocial} (${formatMoney(fatura.valor)})?`)) return
  try {
    const res = await useApi(`/plataforma/faturas/${fatura.id}`, { method: 'DELETE' })
    if (res?.sucesso === false) {
      alert(`Falha ao excluir: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    await loadFaturas()
  } catch (e) {
    alert(`Erro ao excluir fatura: ${e.message}`)
  }
}

const gerarPix = async (fatura) => {
  pixModal.gerando = true
  pixModal.faturaId = fatura.id
  try {
    const res = await useApi(`/plataforma/faturas/${fatura.id}/gerar-cobranca-pix`, { method: 'POST' })
    if (res?.sucesso === false) {
      const msg = (res?.mensagem ?? '').toLowerCase()
      if (msg.includes('gateway') || msg.includes('configurad') || msg.includes('não configurad')) {
        alert('Nenhum gateway de pagamento configurado. Configure um provedor em Operação → Integrações / Gateways antes de gerar a cobrança Pix.')
      } else {
        alert(`Falha ao gerar Pix: ${res.mensagem ?? 'erro desconhecido'}`)
      }
      return
    }
    const dados = res?.dados ?? res
    pixModal.dados = {
      paymentId: dados?.paymentId ?? '',
      qrCode: dados?.qrCode ?? '',
      qrCodeBase64: dados?.qrCodeBase64 ?? '',
      ticketUrl: dados?.ticketUrl ?? '',
      dataExpiracao: dados?.dataExpiracao ?? ''
    }
    pixCopiado.value = false
    pixModal.open = true
  } catch (e) {
    // 404/400 do backend quando não há gateway ativo para o escopo.
    const msg = (e?.data?.mensagem ?? e?.message ?? '').toLowerCase()
    if (msg.includes('gateway') || msg.includes('configurad')) {
      alert('Nenhum gateway de pagamento configurado. Configure um provedor em Operação → Integrações / Gateways antes de gerar a cobrança Pix.')
    } else {
      alert(`Erro ao gerar cobrança Pix: ${e.message}`)
    }
  } finally {
    pixModal.gerando = false
  }
}

const copiarPix = async () => {
  const texto = pixModal.dados.qrCode
  if (!texto) return
  try {
    if (import.meta.client && navigator.clipboard) {
      await navigator.clipboard.writeText(texto)
    }
    pixCopiado.value = true
    setTimeout(() => { pixCopiado.value = false }, 2000)
  } catch (e) {
    alert('Não foi possível copiar automaticamente. Selecione o texto e copie manualmente.')
  }
}

const formatDateTime = (dateStr) => {
  if (!dateStr) return '—'
  return new Date(dateStr).toLocaleString('pt-BR')
}

const getStatusBadgeClass = (status) => {
  const s = (status || '').toLowerCase()
  if (s.includes('pago') || s.includes('quitad')) return 'badge-success'
  if (s.includes('venc') || s.includes('atras') || s.includes('inadimp')) return 'badge-danger'
  if (s.includes('cancel')) return 'badge-danger'
  return 'badge-warning'
}

const formatDate = (dateStr) => {
  if (!dateStr) return '—'
  return new Date(dateStr).toLocaleDateString('pt-BR')
}

const formatMoney = (v) => {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v ?? 0)
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
.form-row {
  display: flex;
  gap: 16px;
  margin-bottom: 12px;
}
.col-4 { flex: 0 0 calc(33.33% - 10.6px); }
.col-6 { flex: 0 0 calc(50% - 8px); }
@media (max-width: 600px) {
  .form-row { flex-direction: column; gap: 12px; }
  .col-4, .col-6 { flex: 0 0 100%; }
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
.badge-warning {
  background: rgba(245, 158, 11, 0.1);
  color: #fbbf24;
  border: 1px solid rgba(245, 158, 11, 0.2);
}
.table-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 16px;
  padding-top: 12px;
  border-top: 1px solid var(--border-color);
}
.page-info {
  font-size: 13px;
  color: var(--text-secondary);
}
.page-actions {
  display: flex;
  gap: 8px;
}
.btn-sm {
  padding: 6px 12px;
  font-size: 12px;
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
.pix-body {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.pix-qr-wrap {
  display: flex;
  justify-content: center;
}
.pix-qr {
  width: 220px;
  height: 220px;
  background: #fff;
  padding: 8px;
  border-radius: 8px;
  image-rendering: pixelated;
}
.pix-field label {
  display: block;
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-secondary);
  margin-bottom: 6px;
}
.pix-copy-row {
  display: flex;
  gap: 8px;
  align-items: flex-start;
}
.pix-copia-cola {
  flex: 1;
  padding: 8px 10px;
  background: rgba(0, 0, 0, 0.2);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  color: var(--text-primary);
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  font-size: 12px;
  resize: vertical;
  word-break: break-all;
}
.pix-meta {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.pix-meta-item {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  font-size: 13px;
}
.pix-meta-label {
  color: var(--text-secondary);
}
.pix-meta-value {
  color: var(--text-primary);
  font-weight: 600;
  word-break: break-all;
  text-align: right;
}
.mt-2 { margin-top: 12px; }
.mt-4 { margin-top: 24px; }
</style>
