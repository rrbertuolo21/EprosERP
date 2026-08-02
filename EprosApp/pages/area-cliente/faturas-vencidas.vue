<script setup lang="ts">
/**
 * Faturas Vencidas — Área do Cliente (portal SaaS).
 *
 * Porta o comportamento de `area-cliente/faturas-vencidas.vue` do legado: lista as faturas
 * em atraso do tenant logado e permite gerar cobrança PIX para regularização, restabelecendo
 * o acesso ao ERP. Tela de bloqueio: usada quando `useAuth().getUser().status === 'Atrasado'`.
 *
 * Endpoints: GET /aplicativo/assinaturas/faturas (filtradas por vencidas),
 *            POST /aplicativo/assinaturas/faturas/{id}/pix
 *            POST /aplicativo/assinaturas/faturas/{id}/boleto  (1.08B)
 */
import { ref, onMounted, computed } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useAuth } from '~/composables/useAuth'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import AppDialog from '~/components/shared/AppDialog.vue'

definePageMeta({ layout: 'guest' })

interface Fatura {
  id: string
  dataVencimento: string
  dataPagamento?: string | null
  valorTotal: number
  valorPago?: number | null
  statusFatura: string
}

interface PixResponse {
  qrCode: string
  qrCodeBase64: string
  dataExpiracao: string
  paymentId?: string
  ticketUrl?: string
}

const toast = useToast()
const { getUser } = useAuth()
const { formatarData, formatarDataHora, formatarMoeda } = useHelper()

const faturas = ref<Fatura[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)

const colunas: DataTableColumn<Fatura>[] = [
  { key: 'dataVencimento', label: 'Vencimento', align: 'center', formatter: (v) => formatarData(v as string) },
  { key: 'valorTotal', label: 'Valor total', align: 'right', formatter: (v) => formatarMoeda(v as number) },
  {
    key: 'dataPagamento',
    label: 'Pagamento',
    align: 'center',
    formatter: (v) => (v ? formatarDataHora(v as string) : 'Não pago')
  },
  { key: 'statusFatura', label: 'Status', align: 'center' }
]

const usuario = computed(() => getUser())

async function buscarFaturasVencidas() {
  carregando.value = true
  try {
    const resposta = await useApi<{ dados?: Fatura[]; data?: Fatura[]; totalRegistros?: number }>(
      '/aplicativo/assinaturas/faturas',
      {
        query: {
          apenasVencidas: true,
          pagina: pagina.value,
          tamanhoPagina: tamanhoPagina.value
        }
      }
    )
    faturas.value = extrairDados<Fatura[]>(resposta) ?? []
    total.value = resposta?.totalRegistros ?? faturas.value.length
  } catch (e) {
    toast.error(obterMensagemErro(e))
    faturas.value = []
    total.value = 0
  } finally {
    carregando.value = false
  }
}

const pixDialogVisivel = ref(false)
const gerandoPix = ref(false)
const pixData = ref<PixResponse | null>(null)
const copiado = ref(false)

async function gerarPix(fatura: Fatura) {
  gerandoPix.value = true
  try {
    const resposta = await useApi<{ dados?: PixResponse; data?: PixResponse }>(
      '/aplicativo/assinaturas/faturas/{id}/pix',
      { method: 'POST', params: { id: fatura.id } }
    )
    const dados = extrairDados<PixResponse>(resposta)
    if (dados) {
      pixData.value = dados
      pixDialogVisivel.value = true
      copiado.value = false
    } else {
      toast.error('Não foi possível gerar a cobrança PIX.')
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    gerandoPix.value = false
  }
}

function copiarCodigoPix() {
  if (!pixData.value) return
  navigator.clipboard.writeText(pixData.value.qrCode)
  copiado.value = true
  setTimeout(() => (copiado.value = false), 2000)
}

// 1.08B — Boleto: emite via gateway (concilia pelo mesmo webhook do PIX) e mostra linha digitável/PDF.
interface BoletoResponse {
  linhaDigitavel?: string
  codigoBarras?: string
  urlBoleto?: string
  dataVencimento?: string
}
const boletoDialogVisivel = ref(false)
const gerandoBoleto = ref(false)
const boletoData = ref<BoletoResponse | null>(null)
const boletoCopiado = ref(false)

async function gerarBoleto(fatura: Fatura) {
  gerandoBoleto.value = true
  try {
    const resposta = await useApi<{ dados?: BoletoResponse; data?: BoletoResponse }>(
      '/aplicativo/assinaturas/faturas/{id}/boleto',
      { method: 'POST', params: { id: fatura.id } }
    )
    const dados = extrairDados<BoletoResponse>(resposta)
    if (dados) {
      boletoData.value = dados
      boletoDialogVisivel.value = true
      boletoCopiado.value = false
    } else {
      toast.error('Não foi possível gerar o boleto.')
    }
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    gerandoBoleto.value = false
  }
}

function copiarLinhaDigitavel() {
  if (!boletoData.value?.linhaDigitavel) return
  navigator.clipboard.writeText(boletoData.value.linhaDigitavel)
  boletoCopiado.value = true
  setTimeout(() => (boletoCopiado.value = false), 2000)
}

function irParaPagina(p: number) {
  pagina.value = p
  void buscarFaturasVencidas()
}

onMounted(() => {
  void buscarFaturasVencidas()
})
</script>

<template>
  <div class="faturas-vencidas-wrapper">
    <div class="faturas-vencidas-content">
      <PageToolbar title="Faturas vencidas" subtitle="Regularize sua assinatura para reestabelecer o acesso ao ERP" :loading="carregando" />

      <div class="alert-banner glass-panel">
        <div class="alert-icon">⚠️</div>
        <div class="alert-text">
          <h2>Acesso suspenso</h2>
          <p>
            Seu inquilino <strong>{{ usuario?.tenantId }}</strong> possui fatura(s) vencida(s).
            O acesso aos demais módulos do sistema foi suspenso temporariamente. Realize o
            pagamento abaixo para reestabelecer o acesso imediatamente.
          </p>
        </div>
      </div>

      <DataTable
        :items="faturas"
        :columns="colunas"
        :total="total"
        :page="pagina"
        :page-size="tamanhoPagina"
        :loading="carregando"
        empty-text="Nenhuma fatura vencida encontrada."
        @update:page="irParaPagina"
      >
        <template #actions="{ row }">
          <button type="button" class="btn btn-primary btn-sm" :disabled="gerandoPix" @click.stop="gerarPix(row)">
            Pagar com PIX
          </button>
          <button type="button" class="btn btn-secondary btn-sm" :disabled="gerandoBoleto" @click.stop="gerarBoleto(row)">
            Boleto
          </button>
        </template>
      </DataTable>
    </div>

    <AppDialog v-model="pixDialogVisivel" title="Pagamento da assinatura" width="480px">
      <div v-if="pixData" class="pix-body">
        <div class="qr-code-box">
          <img :src="`data:image/png;base64,${pixData.qrCodeBase64}`" alt="QR Code PIX" class="qr-code-image" />
        </div>

        <div class="field">
          <label class="field-label">Código copia e cola</label>
          <div class="copia-cola-wrapper">
            <input type="text" class="input" readonly :value="pixData.qrCode" />
            <button type="button" class="btn btn-secondary btn-sm" @click="copiarCodigoPix">
              {{ copiado ? 'Copiado!' : 'Copiar' }}
            </button>
          </div>
        </div>

        <p v-if="pixData.dataExpiracao" class="pix-expira">
          Expira em: {{ formatarDataHora(pixData.dataExpiracao) }}
        </p>

        <div class="pix-instrucoes">
          <strong>Como pagar:</strong>
          <ol>
            <li>Abra o app do seu banco</li>
            <li>Escaneie o QR Code acima</li>
            <li>Confirme o pagamento</li>
          </ol>
        </div>
      </div>

      <template #footer>
        <button type="button" class="btn btn-secondary" @click="pixDialogVisivel = false">Fechar</button>
      </template>
    </AppDialog>

    <AppDialog v-model="boletoDialogVisivel" title="Boleto da assinatura" width="520px">
      <div v-if="boletoData" class="pix-body">
        <div class="field">
          <label class="field-label">Linha digitável</label>
          <div class="copia-cola-wrapper">
            <input type="text" class="input" readonly :value="boletoData.linhaDigitavel" />
            <button type="button" class="btn btn-secondary btn-sm" @click="copiarLinhaDigitavel">
              {{ boletoCopiado ? 'Copiado!' : 'Copiar' }}
            </button>
          </div>
        </div>

        <p v-if="boletoData.dataVencimento" class="pix-expira">
          Vencimento: {{ formatarData(boletoData.dataVencimento) }}
        </p>

        <a v-if="boletoData.urlBoleto" :href="boletoData.urlBoleto" target="_blank" rel="noopener" class="btn btn-primary btn-sm">
          Abrir boleto (PDF)
        </a>
      </div>

      <template #footer>
        <button type="button" class="btn btn-secondary" @click="boletoDialogVisivel = false">Fechar</button>
      </template>
    </AppDialog>
  </div>
</template>


<style scoped>
.faturas-vencidas-wrapper {
  min-height: 100vh;
  display: flex;
  justify-content: center;
  padding: 40px 20px;
}
.faturas-vencidas-content {
  width: 100%;
  max-width: 960px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}
.alert-banner {
  display: flex;
  align-items: flex-start;
  gap: 20px;
  padding: 24px;
  border-left: 4px solid var(--danger);
  background: rgba(239, 68, 68, 0.04);
}
.alert-icon { font-size: 28px; line-height: 1; }
.alert-text h2 { font-size: 18px; font-weight: 700; color: var(--text-primary); margin-bottom: 6px; }
.alert-text p { font-size: 13px; line-height: 1.6; color: var(--text-secondary); }
.alert-text strong { color: #fff; }

.pix-body { display: flex; flex-direction: column; align-items: center; gap: 20px; }
.qr-code-box { padding: 16px; background: #fafafa; border-radius: 16px; }
.qr-code-image { width: 200px; height: 200px; }
.copia-cola-wrapper { display: flex; gap: 8px; }
.copia-cola-wrapper .input { flex: 1; }
.pix-expira { font-size: 12px; color: var(--text-secondary); }
.pix-instrucoes { width: 100%; font-size: 13px; color: var(--text-secondary); }
.pix-instrucoes ol { margin: 8px 0 0 18px; }
</style>
