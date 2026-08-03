<template>
  <div class="dashboard-layout">
    <!-- Cabeçalho Compartilhado -->
    <AppHeader />

    <!-- Conteúdo Principal -->
    <main class="dashboard-content">
      <!-- Banner de Alerta Inadimplência se estiver Atrasado -->
      <div v-if="user && user.status === 'Atrasado'" class="alert-banner glass-panel">
        <div class="alert-icon">⚠️</div>
        <div class="alert-text">
          <h2>Acesso Suspenso</h2>
          <p>Seu inquilino <strong>{{ user.tenantId }}</strong> possui uma fatura vencida há mais de 15 dias. O acesso aos demais módulos do sistema (Vendas, Estoque, Financeiro, Fiscal) foi suspenso temporariamente. Realize o pagamento da fatura atrasada abaixo para reestabelecer o acesso imediatamente.</p>
        </div>
      </div>

      <!-- Card Central das Faturas -->
      <section class="faturas-section glass-panel">
        <header class="section-header">
          <div class="header-title-wrapper">
            <h2>Histórico de Cobrança</h2>
            <p>Gerencie as faturas mensais da assinatura do seu ERP</p>
          </div>
          <button @click="refreshInvoices" class="btn btn-secondary btn-refresh" :disabled="loading">
            <span>🔄</span> Atualizar
          </button>
        </header>

        <!-- Loading State -->
        <div v-if="loading" class="loading-state">
          <span class="spinner"></span>
          <p>Carregando faturas...</p>
        </div>

        <!-- Empty State -->
        <div v-else-if="invoices.length === 0" class="empty-state">
          <p>Nenhuma fatura encontrada.</p>
        </div>

        <!-- Tabela de Invoices -->
        <div v-else class="table-container">
          <table class="invoices-table">
            <thead>
              <tr>
                <th>Fatura ID / Período</th>
                <th>Vencimento</th>
                <th>Valor</th>
                <th>Status</th>
                <th class="align-right">Ação</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="invoice in invoices" :key="invoice.id" :class="{ 'row-highlight': invoice.status === 'Atrasada' }">
                <td>
                  <div class="id-wrapper">
                    <span class="invoice-period">{{ invoice.period }}</span>
                    <span class="invoice-id">{{ invoice.id }}</span>
                  </div>
                </td>
                <td>{{ formatDate(invoice.dueDate) }}</td>
                <td class="invoice-amount">{{ formatAmount(invoice.amount) }}</td>
                <td>
                  <span :class="['badge', 'badge-' + invoice.status.toLowerCase()]">
                    {{ invoice.status }}
                  </span>
                </td>
                <td class="align-right">
                  <button 
                    v-if="invoice.status === 'Pendente' || invoice.status === 'Atrasada'" 
                    @click="openPaymentModal(invoice)" 
                    class="btn btn-primary btn-pay"
                  >
                    Pagar com PIX
                  </button>
                  <span v-else class="text-completed">
                    ✓ Baixada
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </main>

    <!-- Modal de Pagamento PIX -->
    <div v-if="showPaymentModal" class="modal-overlay">
      <div class="modal-card glass-panel animate-scale">
        <header class="modal-header">
          <h3>Pagamento da Assinatura</h3>
          <button @click="closePaymentModal" class="btn-close-modal">✕</button>
        </header>

        <div class="modal-body" v-if="selectedInvoice">
          <div class="payment-details">
            <div class="detail-row">
              <span class="label">Período:</span>
              <span class="val">{{ selectedInvoice.period }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Valor:</span>
              <span class="val highlight">{{ formatAmount(selectedInvoice.amount) }}</span>
            </div>
          </div>

          <div class="pix-instructions">
            <p class="inst-title">Escaneie o QR Code abaixo para pagar:</p>
            
            <!-- Simulador de QR Code com SVG de Design Premium -->
            <div class="qr-code-box">
              <svg width="180" height="180" viewBox="0 0 180 180" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect width="180" height="180" rx="12" fill="#fafafa" />
                <!-- Bordas de Alinhamento Pix -->
                <rect x="15" y="15" width="40" height="40" rx="4" fill="none" stroke="#09090b" stroke-width="4" />
                <rect x="25" y="25" width="20" height="20" rx="2" fill="#09090b" />
                
                <rect x="125" y="15" width="40" height="40" rx="4" fill="none" stroke="#09090b" stroke-width="4" />
                <rect x="135" y="25" width="20" height="20" rx="2" fill="#09090b" />
                
                <rect x="15" y="125" width="40" height="40" rx="4" fill="none" stroke="#09090b" stroke-width="4" />
                <rect x="25" y="135" width="20" height="20" rx="2" fill="#09090b" />
                
                <!-- Blocos Similares a QR Code Fictício -->
                <rect x="70" y="20" width="15" height="15" rx="2" fill="#09090b" />
                <rect x="95" y="25" width="15" height="25" rx="2" fill="#09090b" />
                <rect x="70" y="50" width="40" height="10" rx="2" fill="#09090b" />
                <rect x="20" y="70" width="25" height="15" rx="2" fill="#09090b" />
                <rect x="135" y="70" width="25" height="15" rx="2" fill="#09090b" />
                <rect x="55" y="90" width="70" height="15" rx="3" fill="#6366f1" /> <!-- Marca d'água de destaque -->
                <rect x="20" y="95" width="25" height="15" rx="2" fill="#09090b" />
                <rect x="135" y="95" width="25" height="15" rx="2" fill="#09090b" />
                <rect x="70" y="125" width="20" height="20" rx="2" fill="#09090b" />
                <rect x="100" y="125" width="15" height="35" rx="2" fill="#09090b" />
                <rect x="130" y="145" width="25" height="15" rx="2" fill="#09090b" />
              </svg>
              <div class="qr-glow-effect"></div>
            </div>

            <!-- Copia e Cola -->
            <div class="pix-copia-cola">
              <label for="copiaCola">Código PIX Copia e Cola</label>
              <div class="copia-cola-wrapper">
                <input 
                  type="text" 
                  id="copiaCola" 
                  readonly 
                  :value="pixString" 
                  ref="copiaColaInput"
                />
                <button @click="copyPixString" class="btn btn-secondary btn-copy">
                  {{ copied ? 'Copiado!' : 'Copiar' }}
                </button>
              </div>
            </div>

            <!-- Simulação de Confirmação -->
            <div class="simulador-webhook-box">
              <p class="sim-help">Para testes locais, você pode disparar a confirmação manual imediata que simula a notificação do webhook do Mercado Pago:</p>
              <button @click="triggerSimulatedPayment" class="btn btn-success btn-sim-pay" :disabled="paying">
                <span v-if="!paying">Confirmar Pagamento Simulado</span>
                <span v-else class="spinner"></span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from '#app'
import AppHeader from '~/components/AppHeader.vue'
import { useApi } from '~/composables/useApi'

// Página autocontida (já renderiza seu próprio AppHeader) — sem layout do shell ERP.
definePageMeta({ layout: false })

const router = useRouter()
const loading = ref(false)
const paying = ref(false)
const showPaymentModal = ref(false)
const selectedInvoice = ref(null)
const copied = ref(false)
const apiOnline = ref(false)
const pixText = ref('')

const user = ref(null)
const invoices = ref([])

const pixString = computed(() => {
  return pixText.value
})

// Inicialização e Carga das Faturas
onMounted(async () => {
  const storedUser = localStorage.getItem('epros_user')
  if (!storedUser) {
    router.push('/')
    return
  }
  
  user.value = JSON.parse(storedUser)

  // Detecta conectividade com o Backend
  try {
    await useApi('/plataforma/clientes')
    apiOnline.value = true
  } catch (e) {
    apiOnline.value = false
  }

  loadInvoices()
})

const loadInvoices = async () => {
  loading.value = true
  
  if (apiOnline.value) {
    try {
      const data = await useApi('/aplicativo/assinaturas/faturas')
      if (data && Array.isArray(data)) {
        invoices.value = data.map(f => ({
          id: f.id,
          period: `Assinatura ERP - Período ${new Date(f.dataVencimento).getMonth() + 1}/${new Date(f.dataVencimento).getFullYear()}`,
          dueDate: f.dataVencimento,
          amount: f.valor,
          status: f.dataPagamento || f.status === 'Pago' ? 'Paga' : (new Date(f.dataVencimento) < new Date() ? 'Atrasada' : 'Pendente')
        }))
        loading.value = false
        return
      }
    } catch (e) {
      console.warn('Erro ao carregar faturas da API real. Falling back para simulado.', e)
    }
  }

  // Fallback para dados simulados
  setTimeout(() => {
    loading.value = false
    
    // Lista básica simulada baseada no status do inquilino
    const baseInvoices = [
      {
        id: '9d2c20a4-3759-4d8e-9cb9-6d55d7f1e582',
        period: 'Assinatura ERP - Maio/2026',
        dueDate: new Date(2026, 4, 10),
        amount: 299.90,
        status: 'Paga'
      },
      {
        id: 'f87a32c2-8419-482d-bf41-79b8cfa4e7a8',
        period: 'Assinatura ERP - Junho/2026',
        dueDate: new Date(2026, 5, 10),
        amount: 299.90,
        status: 'Pendente'
      }
    ]

    // Se o inquilino for bloqueado por inadimplência, adicionamos uma fatura atrasada com mais de 15 dias
    if (user.value && user.value.status === 'Atrasado') {
      baseInvoices.unshift({
        id: '2a1b9c34-df52-4a0b-93e1-884c6e9a721c',
        period: 'Assinatura ERP - Abril/2026',
        dueDate: new Date(2026, 3, 10), // Vencida em 10/Abril (mais de 15 dias de atraso)
        amount: 299.90,
        status: 'Atrasada'
      })
    }

    invoices.value = baseInvoices
  }, 600)
}

const refreshInvoices = () => {
  loadInvoices()
}

// Formatação
const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleDateString('pt-BR')
}

const formatAmount = (val) => {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val)
}

// Modal de Pagamento
const openPaymentModal = async (invoice) => {
  selectedInvoice.value = invoice
  showPaymentModal.value = true
  copied.value = false
  
  // Define o Pix copia-e-cola padrão (simulado)
  pixText.value = `00020101021226830014br.gov.bcb.pix2561api.mercadopago.com/v2/cobrancas/${invoice.id}`
  
  if (apiOnline.value) {
    try {
      const res = await useApi(`/aplicativo/assinaturas/faturas/${invoice.id}/pix`, {
        method: 'POST'
      })
      if (res && res.sucesso && res.dados) {
        if (res.dados.checkoutUrl) {
          pixText.value = res.dados.checkoutUrl
        }
      }
    } catch (e) {
      console.warn('Erro ao obter PIX real do backend, mantendo simulado.', e)
    }
  }
}

const closePaymentModal = () => {
  showPaymentModal.value = false
  selectedInvoice.value = null
}

const copyPixString = () => {
  navigator.clipboard.writeText(pixString.value)
  copied.value = true
  setTimeout(() => {
    copied.value = false
  }, 2000)
}

// Fluxo de Confirmação Simulado (MercadoPago Webhook ou Atualização Local)
const triggerSimulatedPayment = async () => {
  if (!selectedInvoice.value) return
  
  paying.value = true
  
  try {
    // Tenta enviar o payload real para o backend monolítico local
    const webhookPayload = {
      Action: 'payment.created',
      Data: {
        Id: selectedInvoice.value.id
      }
    }

    // Tenta chamada fetch para o monólito local (porta do backend REST)
    await useApi('/plataforma/clientes/webhooks/mercadopago', {
      method: 'POST',
      body: webhookPayload
    }).catch(err => {
      console.warn('Backend monolítico indisponível ou erro de CORS. Atualizando estado simulado localmente.', err)
    })
    
    // Atualiza localmente a lista de faturas para fins de demonstração visual
    const targetInvoice = invoices.value.find(i => i.id === selectedInvoice.value.id)
    if (targetInvoice) {
      targetInvoice.status = 'Paga'
    }

    // Se a fatura paga for a atrasada que estava bloqueando o inquilino, atualizamos o status do tenant!
    if (selectedInvoice.value.status === 'Atrasada') {
      user.value.status = 'Ativo'
      localStorage.setItem('epros_user', JSON.stringify(user.value))
    }

    // Fecha o modal e alerta
    paying.value = false
    closePaymentModal()
    alert('Pagamento recebido e processado reativamente com sucesso!')
  } catch (error) {
    paying.value = false
    console.error('Erro durante simulação de pagamento:', error)
  }
}
</script>

<style scoped>
.dashboard-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  z-index: 10;
}

.dashboard-content {
  flex: 1;
  padding: 0 20px 40px;
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* Banner Inadimplência */
.alert-banner {
  display: flex;
  align-items: flex-start;
  gap: 20px;
  padding: 24px;
  border-left: 4px solid var(--danger);
  background: rgba(239, 68, 68, 0.04);
}

.alert-icon {
  font-size: 28px;
  line-height: 1;
}

.alert-text h2 {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 6px;
}

.alert-text p {
  font-size: 13px;
  line-height: 1.6;
  color: var(--text-secondary);
}

.alert-text strong {
  color: #fff;
}

/* Tabela de Faturas */
.faturas-section {
  padding: 32px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  padding-bottom: 20px;
}

.header-title-wrapper h2 {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-primary);
  margin-bottom: 4px;
}

.header-title-wrapper p {
  font-size: 13px;
  color: var(--text-secondary);
}

.btn-refresh {
  padding: 8px 16px;
  font-size: 13px;
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 0;
  gap: 16px;
  color: var(--text-secondary);
  font-size: 14px;
}

.empty-state {
  text-align: center;
  padding: 60px 0;
  color: var(--text-muted);
}

.table-container {
  overflow-x: auto;
}

.invoices-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.invoices-table th {
  padding: 14px 20px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.8px;
  color: var(--text-secondary);
  border-bottom: 1px solid var(--border-color);
}

.invoices-table td {
  padding: 18px 20px;
  font-size: 14px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.03);
}

.row-highlight {
  background: rgba(239, 68, 68, 0.01);
}

.id-wrapper {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.invoice-period {
  font-weight: 600;
  color: var(--text-primary);
}

.invoice-id {
  font-size: 11px;
  font-family: monospace;
  color: var(--text-muted);
}

.invoice-amount {
  font-weight: 600;
  color: var(--text-primary);
}

.align-right {
  text-align: right;
}

.btn-pay {
  padding: 8px 16px;
  font-size: 12px;
}

.text-completed {
  font-size: 12px;
  font-weight: 600;
  color: var(--success);
}

/* Modal Pagamento */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(4px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 200;
}

.modal-card {
  width: 460px;
  padding: 32px;
}

.animate-scale {
  animation: scale-up 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}

@keyframes scale-up {
  from { transform: scale(0.9); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.modal-header h3 {
  font-size: 18px;
  font-weight: 700;
}

.btn-close-modal {
  background: none;
  border: none;
  color: var(--text-secondary);
  font-size: 16px;
  cursor: pointer;
  padding: 4px;
  transition: color 0.2s;
}

.btn-close-modal:hover {
  color: var(--text-primary);
}

.payment-details {
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 24px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
}

.detail-row .label {
  color: var(--text-secondary);
}

.detail-row .val {
  font-weight: 600;
}

.detail-row .val.highlight {
  color: var(--primary-hover);
  font-size: 16px;
}

.pix-instructions {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
}

.inst-title {
  font-size: 13px;
  color: var(--text-secondary);
  text-align: center;
}

.qr-code-box {
  position: relative;
  padding: 16px;
  background: #fafafa;
  border-radius: 16px;
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.3);
}

.qr-glow-effect {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  border-radius: 16px;
  box-shadow: 0 0 30px rgba(99, 102, 241, 0.2);
  pointer-events: none;
  animation: glow-pulse 2s infinite alternate;
}

@keyframes glow-pulse {
  0% { transform: scale(1); opacity: 0.3; }
  100% { transform: scale(1.05); opacity: 0.8; }
}

.pix-copia-cola {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.pix-copia-cola label {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.copia-cola-wrapper {
  display: flex;
  gap: 8px;
  width: 100%;
}

.copia-cola-wrapper input {
  flex: 1;
  padding: 10px 14px;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--border-color);
  border-radius: 8px;
  color: var(--text-muted);
  font-size: 12px;
}

.btn-copy {
  padding: 0 16px;
  font-size: 12px;
  white-space: nowrap;
}

.simulador-webhook-box {
  width: 100%;
  margin-top: 16px;
  padding: 16px;
  background: rgba(16, 185, 129, 0.04);
  border: 1px dashed rgba(16, 185, 129, 0.3);
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.sim-help {
  font-size: 11px;
  color: var(--text-secondary);
  line-height: 1.5;
  text-align: left;
}

.btn-sim-pay {
  width: 100%;
  padding: 12px;
  font-size: 13px;
}
</style>
