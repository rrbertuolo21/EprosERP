<template>
  <div class="dashboard-layout">
    <main class="dashboard-content">
      <header class="page-header">
        <h1 class="glow-text">Detalhe da Fatura</h1>
        <p class="tagline">Consulte os dados da fatura, os pagamentos registrados e o split de comissão. Altere o vencimento/valor ou registre a baixa manual.</p>
        <div class="header-actions">
          <NuxtLink to="/plataforma/admin/faturas" class="btn btn-secondary btn-back">
            ← Voltar para Faturas
          </NuxtLink>
          <button v-if="fatura" type="button" class="btn btn-secondary" @click="openAlterarModal">
            Alterar
          </button>
          <button
            v-if="fatura"
            type="button"
            class="btn btn-secondary"
            :disabled="pixModal.gerando"
            @click="gerarPix"
          >
            {{ pixModal.gerando ? 'Gerando...' : 'Gerar Pix' }}
          </button>
          <button v-if="fatura" type="button" class="btn btn-primary btn-success-action" @click="openBaixaModal">
            Baixa manual
          </button>
          <span class="status-pill" :class="{ 'offline': !apiOnline }">
            <span class="status-dot"></span>
            {{ apiOnline ? 'Conectado à API Gateway' : 'Modo Simulação Offline' }}
          </span>
        </div>
      </header>

      <div v-if="loading" class="admin-section glass-panel mt-4">
        <p class="empty-cell">Carregando fatura...</p>
      </div>

      <div v-else-if="!fatura" class="admin-section glass-panel mt-4">
        <p class="empty-cell">Fatura não encontrada.</p>
      </div>

      <template v-else>
        <!-- Dados da fatura -->
        <section class="admin-section glass-panel mt-4">
          <header class="section-header">
            <h3>Dados da Fatura</h3>
            <span :class="['badge', getStatusBadgeClass(fatura.status)]">{{ fatura.status }}</span>
          </header>
          <div class="info-grid">
            <div class="info-item">
              <span class="info-label">Cliente</span>
              <span class="info-value">{{ fatura.clienteRazaoSocial }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Valor</span>
              <span class="info-value">{{ formatMoney(fatura.valor) }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Vencimento</span>
              <span class="info-value">{{ formatDate(fatura.dataVencimento) }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Pagamento</span>
              <span class="info-value">{{ fatura.dataPagamento ? formatDate(fatura.dataPagamento) : '—' }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Número</span>
              <span class="info-value">{{ fatura.numero || '—' }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Quitada</span>
              <span class="info-value">
                <span :class="['badge', fatura.quitada ? 'badge-ok' : 'badge-pend']">{{ fatura.quitada ? 'Sim' : 'Não' }}</span>
              </span>
            </div>
            <div class="info-item">
              <span class="info-label">Valor pago</span>
              <span class="info-value">{{ formatMoney(fatura.valorPago || 0) }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Criada em</span>
              <span class="info-value">{{ formatDate(fatura.criadoEm) }}</span>
            </div>
          </div>
          <div v-if="fatura.observacoes" class="fatura-obs">
            <span class="info-label">Observações</span>
            <p>{{ fatura.observacoes }}</p>
          </div>
        </section>

        <!-- Itens da fatura -->
        <section v-if="fatura.itens && fatura.itens.length" class="admin-section glass-panel mt-4">
          <header class="section-header"><h3>Itens da Fatura</h3></header>
          <table class="data-table">
            <thead><tr><th>Descrição</th><th class="text-right">Valor</th></tr></thead>
            <tbody>
              <tr v-for="it in fatura.itens" :key="it.id">
                <td>{{ it.descricao }}</td>
                <td class="text-right">{{ formatMoney(it.valor) }}</td>
              </tr>
            </tbody>
            <tfoot>
              <tr><td class="text-right"><strong>Total</strong></td>
                  <td class="text-right"><strong>{{ formatMoney(fatura.itens.reduce((a, i) => a + Number(i.valor || 0), 0)) }}</strong></td></tr>
            </tfoot>
          </table>
        </section>

        <!-- Split de comissão -->
        <section class="admin-section glass-panel mt-4">
          <header class="section-header">
            <h3>Split de Comissão</h3>
          </header>
          <div class="info-grid">
            <div class="info-item">
              <span class="info-label">% Revenda</span>
              <span class="info-value">{{ formatPercent(fatura.percentualComissaoRevenda) }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Comissão Revenda</span>
              <span class="info-value">{{ formatMoney(fatura.valorComissaoRevenda) }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">% Vendedor</span>
              <span class="info-value">{{ formatPercent(fatura.percentualComissaoVendedor) }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Comissão Vendedor</span>
              <span class="info-value">{{ formatMoney(fatura.valorComissaoVendedor) }}</span>
            </div>
          </div>
        </section>

        <!-- Pagamentos -->
        <section class="admin-section glass-panel mt-4">
          <header class="section-header">
            <h3>Pagamentos</h3>
          </header>
          <div class="table-container">
            <table class="admin-table">
              <thead>
                <tr>
                  <th>Tipo</th>
                  <th>Status</th>
                  <th>Valor Pago</th>
                  <th>Manual?</th>
                  <th>Data</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="!fatura.pagamentos || fatura.pagamentos.length === 0">
                  <td colspan="5" class="empty-cell">Nenhum pagamento registrado.</td>
                </tr>
                <tr v-else v-for="p in fatura.pagamentos" :key="p.id">
                  <td>{{ p.tipoPagamento }}</td>
                  <td><span :class="['badge', getStatusBadgeClass(p.status)]">{{ p.status }}</span></td>
                  <td>{{ formatMoney(p.valorPago) }}</td>
                  <td>
                    <span :class="['badge', p.pagoManualmente ? 'badge-warning' : 'badge-success']">
                      {{ p.pagoManualmente ? 'Manual' : 'Automático' }}
                    </span>
                  </td>
                  <td>{{ p.dataPagamento ? formatDate(p.dataPagamento) : '—' }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </template>
    </main>

    <!-- MODAL: ALTERAR -->
    <div class="modal-backdrop" v-if="alterarModal.open">
      <div class="modal-card glass-panel">
        <header class="modal-header">
          <h3>Alterar fatura</h3>
          <button type="button" @click="alterarModal.open = false" class="btn-close">×</button>
        </header>
        <form @submit.prevent="alterarFatura" class="vertical-form">
          <div class="form-row">
            <div class="form-group col-6">
              <MoneyInput v-model="alterarModal.form.valor" label="Valor" required />
            </div>
            <div class="form-group col-6">
              <DateTimeField v-model="alterarModal.form.dataVencimento" label="Vencimento" mode="date" required />
            </div>
          </div>
          <button type="submit" class="btn btn-primary btn-block mt-4" :disabled="alterarModal.saving">
            {{ alterarModal.saving ? 'Salvando...' : 'Salvar alterações' }}
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
import { useRoute } from 'vue-router'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import AppDialog from '~/components/shared/AppDialog.vue'

definePageMeta({ layout: 'admin' })

const route = useRoute()
const apiOnline = ref(true)
const loading = ref(false)
const fatura = ref(null)

const formasPagamento = ['Pix', 'Boleto', 'Cartão', 'Transferência', 'Dinheiro']

const alterarModal = reactive({
  open: false,
  saving: false,
  form: { valor: 0, dataVencimento: '' }
})

const baixaModal = reactive({
  open: false,
  saving: false,
  form: { valorPago: 0, formaPagamento: '', dataPagamento: new Date().toISOString().split('T')[0] }
})

const pixModal = reactive({
  open: false,
  gerando: false,
  dados: { paymentId: '', qrCode: '', qrCodeBase64: '', ticketUrl: '', dataExpiracao: '' }
})
const pixCopiado = ref(false)

onMounted(async () => {
  await loadFatura()
})

const loadFatura = async () => {
  loading.value = true
  try {
    const res = await useApi(`/plataforma/faturas/${route.params.id}`)
    fatura.value = res
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
    fatura.value = null
  }
  loading.value = false
}

const openAlterarModal = () => {
  alterarModal.form.valor = fatura.value.valor
  alterarModal.form.dataVencimento = (fatura.value.dataVencimento || '').split('T')[0]
  alterarModal.open = true
}

const alterarFatura = async () => {
  alterarModal.saving = true
  try {
    const res = await useApi(`/plataforma/faturas/${route.params.id}`, {
      method: 'PUT',
      body: {
        Id: route.params.id,
        Valor: alterarModal.form.valor,
        DataVencimento: alterarModal.form.dataVencimento
      }
    })
    if (res?.sucesso === false) {
      alert(`Falha ao alterar: ${res.mensagem ?? 'erro desconhecido'}`)
      return
    }
    alterarModal.open = false
    await loadFatura()
  } catch (e) {
    alert(`Erro ao alterar fatura: ${e.message}`)
  } finally {
    alterarModal.saving = false
  }
}

const openBaixaModal = () => {
  baixaModal.form.valorPago = fatura.value.valor
  baixaModal.form.formaPagamento = ''
  baixaModal.form.dataPagamento = new Date().toISOString().split('T')[0]
  baixaModal.open = true
}

const baixarManual = async () => {
  baixaModal.saving = true
  try {
    const res = await useApi(`/plataforma/faturas/${route.params.id}/baixar-manual`, {
      method: 'POST',
      body: {
        FaturaId: route.params.id,
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
    await loadFatura()
  } catch (e) {
    alert(`Erro na baixa manual: ${e.message}`)
  } finally {
    baixaModal.saving = false
  }
}

const gerarPix = async () => {
  pixModal.gerando = true
  try {
    const res = await useApi(`/plataforma/faturas/${route.params.id}/gerar-cobranca-pix`, { method: 'POST' })
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
  if (s.includes('pago') || s.includes('quitad') || s.includes('aprovad')) return 'badge-success'
  if (s.includes('venc') || s.includes('atras') || s.includes('inadimp') || s.includes('cancel') || s.includes('recusad')) return 'badge-danger'
  return 'badge-warning'
}

const formatDate = (dateStr) => {
  if (!dateStr) return '—'
  return new Date(dateStr).toLocaleDateString('pt-BR')
}

const formatMoney = (v) => {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v ?? 0)
}

const formatPercent = (v) => {
  return `${new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(v ?? 0)}%`
}
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
  background: rgba(255,255,255,0.02);
  border: 1px solid var(--border-color);
  color: var(--text-secondary);
}
.btn-back:hover {
  background: rgba(255,255,255,0.06);
  color: var(--text-primary);
}
.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 16px;
  margin-top: 8px;
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
  font-size: 15px;
  font-weight: 600;
  color: var(--text-primary);
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
.empty-cell {
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
.btn-sm {
  padding: 6px 12px;
  font-size: 12px;
}
.mt-2 { margin-top: 12px; }
.mt-4 { margin-top: 24px; }
/* 1.01 front — fatura: quitada, valor pago, itens */
.text-right { text-align: right; }
.badge-ok { background: #dcfce7; color: #15803d; padding: 2px 8px; border-radius: 10px; font-size: 12px; font-weight: 600; }
.badge-pend { background: #fef3c7; color: #b45309; padding: 2px 8px; border-radius: 10px; font-size: 12px; font-weight: 600; }
.fatura-obs { margin-top: 12px; padding-top: 12px; border-top: 1px solid rgba(148,163,184,.25); }
.fatura-obs p { margin: 4px 0 0; }
</style>
