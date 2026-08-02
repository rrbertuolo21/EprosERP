<script setup lang="ts">
/**
 * PDV (frente de caixa) — emissão de NFC-e em modo caixa.
 *
 * Porta o fluxo do legado (`layouts/pos.vue` + `components/pos/*` + `vendas/emissao/nfce/[[id]].vue`)
 * para o design system novo. Estrutura em duas colunas: à esquerda as abas
 * Cliente(F1)/Produto(F2)/Pagamento(F4); à direita a comanda de itens (ou os totais no pagamento).
 *
 * Fluxo de finalização (F10): grava a venda fiscal → inclui NFC-e → transmite à SEFAZ, com
 * overlay de progresso (TransmissionOverlay) alimentado por tempo real (useRealtime). Ao concluir,
 * exibe o cupom (DANFE) e libera para nova venda. Esc cancela a venda corrente.
 *
 * IO exclusivamente via `usePdvNfce` (que usa o `useApi` compartilhado). Sem Vuetify, sem URL fixa.
 */
import { computed, onMounted, onUnmounted, reactive, ref } from 'vue'
import PdvBusca from '~/components/pdv/PdvBusca.vue'
import PdvItens from '~/components/pdv/PdvItens.vue'
import PdvCliente from '~/components/pdv/PdvCliente.vue'
import PdvPagamentos from '~/components/pdv/PdvPagamentos.vue'
import PdvCupomDialog from '~/components/pdv/PdvCupomDialog.vue'
import { usePdvNfce } from '~/components/pdv/usePdvNfce'
import {
  ModeloFiscal,
  StatusVenda,
  type BalancaPdv,
  type DestinatarioPdv,
  type DocumentoEmitido,
  type ItemPdv,
  type PagamentoPdv
} from '~/components/pdv/tipos'
import TransmissionOverlay, { type TransmissionStep } from '~/components/shared/TransmissionOverlay.vue'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import { useRealtime } from '~/composables/useRealtime'

definePageMeta({
  layout: 'pos',
  middleware: 'auth'
})

const toast = useToast()
const { formatarMoeda } = useHelper()
const {
  buscarProdutos,
  carregarBalancas,
  gravarVenda,
  incluirNfce,
  transmitir,
  baixarCupom
} = usePdvNfce()

// Tempo real: acompanha o progresso da transmissão da NFC-e emitida pela SEFAZ.
const realtime = useRealtime('/hubs/vendas')

// #region Estado da venda

type Aba = 'cliente' | 'produto' | 'pagamento'
const aba = ref<Aba>('produto')

const itens = ref<ItemPdv[]>([])
const pagamentos = ref<PagamentoPdv[]>([])
const descontoTotal = ref(0)
const acrescimoTotal = ref(0)
const informacoes = ref('')
const destinatario = reactive<DestinatarioPdv>({
  pessoaId: null,
  documentoConsumidor: '',
  descricao: '',
  enviarNaNfce: false
})

const balancas = ref<BalancaPdv[]>([])
const mensagemCaixa = ref('CAIXA LIVRE')

const buscaRef = ref<InstanceType<typeof PdvBusca> | null>(null)
const clienteRef = ref<InstanceType<typeof PdvCliente> | null>(null)

// #endregion

// #region Totais

function arredondar2(v: number): number {
  return Math.round((v + Number.EPSILON) * 100) / 100
}

const totalFinal = computed(() => {
  const totalItens = itens.value.reduce((acc, item) => {
    const base = item.quantidadeComercial * (item.valorUnitarioComercial ?? 0)
    return acc + arredondar2(base - item.valorDesconto)
  }, 0)
  return arredondar2(totalItens + acrescimoTotal.value - descontoTotal.value)
})

const totalPago = computed(() => pagamentos.value.reduce((acc, p) => acc + p.valorPagamento, 0))
const saldoRestante = computed(() => (totalPago.value >= totalFinal.value ? 0 : arredondar2(totalFinal.value - totalPago.value)))
const semItens = computed(() => itens.value.length === 0)

// #endregion

// #region Comanda

function adicionarItem(item: ItemPdv) {
  itens.value = [item, ...itens.value]
}

function removerItem(index: number) {
  itens.value = itens.value.filter((_, i) => i !== index)
}

function editarItem(index: number) {
  const item = itens.value[index]
  if (!item) return
  itens.value = itens.value.filter((_, i) => i !== index)
  aba.value = 'produto'
  requestAnimationFrame(() => buscaRef.value?.carregarParaEdicao(item))
}

async function aoBuscar(termo: string) {
  const lista = await buscarProdutos(termo)
  buscaRef.value?.definirResultados(lista)
}

// #endregion

// #region Emissão (F10)

const overlayAberto = ref(false)
const overlayErro = ref<string | null>(null)
const overlayStep = ref(0)
const etapas: TransmissionStep[] = [
  { text: 'Gravando a venda' },
  { text: 'Montando a NFC-e' },
  { text: 'Transmitindo à SEFAZ' },
  { text: 'Autorização recebida' }
]

const cupomAberto = ref(false)
const pdfCupom = ref<Blob | null>(null)
const carregandoPdf = ref(false)
const documentoEmitido = reactive<DocumentoEmitido>({ vendaId: null, numero: null, chave: null, status: null })

async function finalizar() {
  if (semItens.value) {
    toast.warning('Nenhum item adicionado.')
    return
  }
  if (!pagamentos.value.length) {
    toast.warning('Nenhum pagamento adicionado.')
    aba.value = 'pagamento'
    return
  }
  if (saldoRestante.value > 0) {
    toast.warning(`Ainda faltam ${formatarMoeda(saldoRestante.value)} em pagamentos.`)
    return
  }
  destinatario.documentoConsumidor && clienteRef.value?.validarDocumento?.()

  overlayErro.value = null
  overlayStep.value = 0
  overlayAberto.value = true

  try {
    // Escuta o hub para refletir o avanço da autorização em tempo real (best-effort).
    await realtime.conectar({
      NfceTransmissionCompleted: (res: unknown) => aoConcluirTransmissao(res),
      NfTransmissionCompleted: (res: unknown) => aoConcluirTransmissao(res)
    })

    const vendaId = await gravarVenda({
      modeloFiscal: ModeloFiscal.NFCe,
      status: StatusVenda.SalvarTransmitir,
      modalidadeFrete: 9,
      destinatario: { ...destinatario },
      itens: itens.value,
      pagamentos: pagamentos.value,
      valorDesconto: descontoTotal.value,
      valorAcrescimo: acrescimoTotal.value,
      valorFrete: 0,
      informacoesComplementares: informacoes.value
    })

    if (!vendaId) {
      throw new Error('Não foi possível obter o identificador da venda gravada.')
    }
    documentoEmitido.vendaId = vendaId
    overlayStep.value = 1

    await incluirNfce(vendaId)
    overlayStep.value = 2

    const resultado = await transmitir(vendaId)
    overlayStep.value = 3
    documentoEmitido.numero = resultado.numero
    documentoEmitido.chave = resultado.chave
    documentoEmitido.status = StatusVenda.Transmitida

    await concluirEmissao()
  } catch (e) {
    overlayErro.value = e instanceof Error ? e.message : 'Falha ao emitir a NFC-e.'
  }
}

function aoConcluirTransmissao(res: unknown) {
  if (res && typeof res === 'object') {
    const r = res as Record<string, unknown>
    if (r.chave) documentoEmitido.chave = String(r.chave)
    if (r.numero != null) documentoEmitido.numero = Number(r.numero)
  }
  overlayStep.value = etapas.length - 1
}

async function concluirEmissao() {
  overlayAberto.value = false
  toast.success('NFC-e transmitida com sucesso.')

  // Baixa o cupom (DANFE) para exibir no dialog, quando houver chave.
  if (documentoEmitido.chave) {
    carregandoPdf.value = true
    cupomAberto.value = true
    pdfCupom.value = await baixarCupom(documentoEmitido.chave)
    carregandoPdf.value = false
  } else {
    cupomAberto.value = true
  }
}

function imprimirCupom() {
  if (!pdfCupom.value) return
  const url = URL.createObjectURL(pdfCupom.value)
  const win = window.open(url, '_blank')
  win?.addEventListener('load', () => win.print())
  setTimeout(() => URL.revokeObjectURL(url), 60000)
}

function baixarCupomArquivo() {
  if (!pdfCupom.value) return
  const url = URL.createObjectURL(pdfCupom.value)
  const link = document.createElement('a')
  link.href = url
  link.download = `nfce-${documentoEmitido.numero ?? documentoEmitido.chave ?? 'cupom'}.pdf`
  link.click()
  URL.revokeObjectURL(url)
}

function fecharCupomEIniciarNova() {
  cupomAberto.value = false
  pdfCupom.value = null
  novaVenda()
}

// #endregion

// #region Cancelar / limpar

function cancelarVenda() {
  if (semItens.value && !pagamentos.value.length) return
  novaVenda()
  toast.info('Venda cancelada. Caixa livre.')
}

function novaVenda() {
  itens.value = []
  pagamentos.value = []
  descontoTotal.value = 0
  acrescimoTotal.value = 0
  informacoes.value = ''
  destinatario.pessoaId = null
  destinatario.documentoConsumidor = ''
  destinatario.descricao = ''
  destinatario.enviarNaNfce = false
  documentoEmitido.vendaId = null
  documentoEmitido.numero = null
  documentoEmitido.chave = null
  documentoEmitido.status = null
  aba.value = 'produto'
  mensagemCaixa.value = 'CAIXA LIVRE'
  buscaRef.value?.limpar()
}

// #endregion

// #region Atalhos globais

function aoTeclar(ev: KeyboardEvent) {
  if (overlayAberto.value || cupomAberto.value) return
  switch (ev.key) {
    case 'F1':
      ev.preventDefault()
      aba.value = 'cliente'
      break
    case 'F2':
      ev.preventDefault()
      aba.value = 'produto'
      break
    case 'F4':
      ev.preventDefault()
      aba.value = 'pagamento'
      break
    case 'F10':
      ev.preventDefault()
      finalizar()
      break
    case 'Escape':
      ev.preventDefault()
      cancelarVenda()
      break
    default:
      break
  }
}

// #endregion

onMounted(async () => {
  window.addEventListener('keydown', aoTeclar)
  balancas.value = await carregarBalancas()
})

onUnmounted(() => {
  window.removeEventListener('keydown', aoTeclar)
  realtime.desconectar()
})
</script>

<template>
  <div class="pdv">
    <div class="pdv-status">
      <span class="pdv-status-msg">{{ mensagemCaixa }}</span>
      <span class="pdv-status-total">Total: <strong>{{ formatarMoeda(totalFinal) }}</strong></span>
    </div>

    <div class="pdv-colunas">
      <section class="pdv-esquerda glass-panel">
        <nav class="pdv-tabs">
          <button type="button" class="pdv-tab" :class="{ ativa: aba === 'cliente' }" @click="aba = 'cliente'">Cliente <small>(F1)</small></button>
          <button type="button" class="pdv-tab" :class="{ ativa: aba === 'produto' }" @click="aba = 'produto'">Produto <small>(F2)</small></button>
          <button type="button" class="pdv-tab" :class="{ ativa: aba === 'pagamento' }" :disabled="semItens" @click="!semItens && (aba = 'pagamento')">Pagamento <small>(F4)</small></button>
        </nav>

        <div class="pdv-tab-conteudo">
          <PdvCliente
            v-show="aba === 'cliente'"
            ref="clienteRef"
            v-model:destinatario="destinatario"
          />
          <PdvBusca
            v-show="aba === 'produto'"
            ref="buscaRef"
            :balancas="balancas"
            @add-item="adicionarItem"
            @buscar="aoBuscar"
            @mensagem="mensagemCaixa = $event"
          />
          <PdvPagamentos
            v-if="aba === 'pagamento'"
            v-model:pagamentos="pagamentos"
            v-model:desconto-total="descontoTotal"
            v-model:acrescimo-total="acrescimoTotal"
            v-model:informacoes="informacoes"
            :total-final="totalFinal"
            :saldo-restante="saldoRestante"
            :bloqueado="semItens"
            @finalizar="finalizar"
          />
        </div>
      </section>

      <section class="pdv-direita">
        <PdvItens
          :itens="itens"
          @editar-item="editarItem"
          @remover-item="removerItem"
        />
      </section>
    </div>

    <footer class="pdv-rodape glass-panel">
      <button type="button" class="btn btn-secondary" @click="cancelarVenda">Cancelar (ESC)</button>
      <div class="pdv-rodape-total">
        <span>Total</span>
        <strong>{{ formatarMoeda(totalFinal) }}</strong>
      </div>
      <button type="button" class="btn btn-success" :disabled="semItens" @click="finalizar">Finalizar (F10)</button>
    </footer>

    <!-- Barra de teclas de função — atalhos de teclado do PDV (paridade com o legado). -->
    <nav class="pdv-fkeys glass-panel" aria-label="Atalhos de teclado do PDV">
      <button type="button" class="pdv-fkey" :class="{ ativa: aba === 'cliente' }" @click="aba = 'cliente'">
        <kbd>F1</kbd> Cliente
      </button>
      <button type="button" class="pdv-fkey" :class="{ ativa: aba === 'produto' }" @click="aba = 'produto'">
        <kbd>F2</kbd> Produto
      </button>
      <button
        type="button"
        class="pdv-fkey"
        :class="{ ativa: aba === 'pagamento' }"
        :disabled="semItens"
        @click="!semItens && (aba = 'pagamento')"
      >
        <kbd>F4</kbd> Pagamento
      </button>
      <button type="button" class="pdv-fkey" :disabled="semItens" @click="finalizar">
        <kbd>F10</kbd> Finalizar
      </button>
      <button type="button" class="pdv-fkey pdv-fkey-danger" @click="cancelarVenda">
        <kbd>ESC</kbd> Cancelar
      </button>
    </nav>

    <TransmissionOverlay
      v-model="overlayAberto"
      title="Transmitindo NFC-e"
      message="Aguarde a autorização da SEFAZ."
      :steps="etapas"
      :current-step="overlayStep"
      :error="overlayErro"
    />

    <PdvCupomDialog
      v-model="cupomAberto"
      :documento="documentoEmitido"
      :pdf="pdfCupom"
      :carregando-pdf="carregandoPdf"
      @imprimir="imprimirCupom"
      @baixar="baixarCupomArquivo"
      @cancelar="() => {}"
      @fechar="fecharCupomEIniciarNova"
    />
  </div>
</template>

<style scoped>
.pdv { display: flex; flex-direction: column; gap: 12px; height: calc(100vh - 130px); }
.pdv-status {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 16px;
  border-radius: 10px;
  background: var(--primary-glow);
  border: 1px solid rgba(99, 102, 241, 0.3);
  font-size: 13px;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}
.pdv-status-total strong { font-size: 15px; }
.pdv-colunas { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; flex: 1; min-height: 0; }
.pdv-esquerda { padding: 12px; display: flex; flex-direction: column; gap: 12px; min-height: 0; }
.pdv-tabs { display: grid; grid-template-columns: repeat(3, 1fr); gap: 6px; }
.pdv-tab {
  padding: 10px;
  border-radius: 8px;
  border: 1px solid var(--border-color);
  background: transparent;
  color: var(--text-secondary);
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
}
.pdv-tab small { font-weight: 400; opacity: 0.7; }
.pdv-tab:hover:not(:disabled) { color: var(--text-primary); }
.pdv-tab.ativa { background: var(--primary); color: #fff; border-color: var(--primary); }
.pdv-tab:disabled { opacity: 0.4; cursor: not-allowed; }
.pdv-tab-conteudo { flex: 1; overflow-y: auto; min-height: 0; }
.pdv-direita { min-height: 0; }
.pdv-rodape {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 20px;
  border-radius: 10px;
}
.pdv-rodape-total { display: flex; align-items: baseline; gap: 10px; }
.pdv-rodape-total span { font-size: 14px; color: var(--text-secondary); text-transform: uppercase; }
.pdv-rodape-total strong { font-size: 28px; font-weight: 800; }
@media (max-width: 900px) {
  .pdv-colunas { grid-template-columns: 1fr; }
  .pdv { height: auto; }
}

.pdv-fkeys {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px;
  border-radius: 10px;
  overflow-x: auto;
}
.pdv-fkey {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  flex: 1 1 0;
  justify-content: center;
  padding: 8px 12px;
  border-radius: 8px;
  border: 1px solid var(--border-color);
  background: var(--surface-raised);
  color: var(--text-secondary);
  font-size: 12.5px;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.15s ease, border-color 0.15s ease, color 0.15s ease;
}
.pdv-fkey:hover:not(:disabled) { background: var(--surface-raised-hover); color: var(--text-primary); }
.pdv-fkey.ativa { background: var(--primary-glow); border-color: var(--primary); color: var(--primary); }
.pdv-fkey:disabled { opacity: 0.45; cursor: not-allowed; }
.pdv-fkey-danger:hover:not(:disabled) { border-color: var(--danger); color: var(--danger); }
.pdv-fkey kbd {
  font-family: inherit;
  font-size: 10.5px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  color: var(--text-muted);
}
@media (max-width: 700px) {
  .pdv-fkey span { display: none; }
}
</style>
