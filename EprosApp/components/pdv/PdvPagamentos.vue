<script setup lang="ts">
/**
 * PdvPagamentos — formas de pagamento + lista de pagamentos + totais do caixa.
 *
 * Une o comportamento de `components/pos/pagamento.vue` (botões de forma, desconto/acréscimo,
 * limpar, finalizar) e `components/pos/pagamentoLista.vue` (lista de pagamentos lançados,
 * valor pago, saldo restante e troco), reconstruído com o design system novo.
 *
 * Atalhos de teclado (quando visível): 1..6 selecionam a forma; '-'/'+' desconto/acréscimo;
 * F7 limpa; F10 finaliza. Os atalhos ficam desativados enquanto um modal de valor está aberto.
 */
import { computed, onMounted, onUnmounted, ref } from 'vue'
import PdvValorDialog from './PdvValorDialog.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { useHelper } from '~/composables/useHelper'
import { ROTULO_PAGAMENTO, TipoPagamento, type PagamentoPdv } from './tipos'

const props = defineProps<{
  totalFinal: number
  saldoRestante: number
  bloqueado: boolean
}>()

const pagamentos = defineModel<PagamentoPdv[]>('pagamentos', { required: true })
const descontoTotal = defineModel<number>('descontoTotal', { required: true })
const acrescimoTotal = defineModel<number>('acrescimoTotal', { required: true })
const informacoes = defineModel<string>('informacoes', { required: true })

const emit = defineEmits<{
  finalizar: []
}>()

const { formatarMoeda } = useHelper()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const dialogAberto = ref(false)
const dialogTitulo = ref('')
type ModoValor = { tipo: 'pagamento'; forma: TipoPagamento } | { tipo: 'desconto' } | { tipo: 'acrescimo' }
const modoValor = ref<ModoValor | null>(null)
const valorInicial = ref(0)

const formas: { tipo: TipoPagamento; label: string }[] = [
  { tipo: TipoPagamento.Dinheiro, label: 'Dinheiro' },
  { tipo: TipoPagamento.Pix, label: 'PIX' },
  { tipo: TipoPagamento.CartaoCredito, label: 'Crédito' },
  { tipo: TipoPagamento.CartaoDebito, label: 'Débito' },
  { tipo: TipoPagamento.Outros, label: 'Outros' },
  { tipo: TipoPagamento.Boleto, label: 'Parcelado' }
]

const totalPago = computed(() => pagamentos.value.reduce((acc, p) => acc + p.valorPagamento, 0))
const troco = computed(() => (totalPago.value > props.totalFinal ? totalPago.value - props.totalFinal : 0))

// #region Ações

function abrirPagamento(forma: TipoPagamento) {
  if (props.bloqueado) return
  modoValor.value = { tipo: 'pagamento', forma }
  dialogTitulo.value = ROTULO_PAGAMENTO[forma] ?? 'Pagamento'
  valorInicial.value = props.saldoRestante
  dialogAberto.value = true
}

function abrirDesconto() {
  if (props.bloqueado) return
  modoValor.value = { tipo: 'desconto' }
  dialogTitulo.value = 'Desconto'
  valorInicial.value = 0
  dialogAberto.value = true
}

function abrirAcrescimo() {
  if (props.bloqueado) return
  modoValor.value = { tipo: 'acrescimo' }
  dialogTitulo.value = 'Acréscimo'
  valorInicial.value = 0
  dialogAberto.value = true
}

function confirmarValor(valor: number) {
  const modo = modoValor.value
  if (!modo || valor <= 0) return
  if (modo.tipo === 'pagamento') {
    const novoTotal = totalPago.value + valor
    const trocoCalc = novoTotal > props.totalFinal ? novoTotal - props.totalFinal : 0
    pagamentos.value = [...pagamentos.value, { tipoPagamento: modo.forma, valorPagamento: valor, valorTroco: trocoCalc }]
  } else if (modo.tipo === 'desconto') {
    descontoTotal.value = valor
  } else {
    acrescimoTotal.value = valor
  }
  modoValor.value = null
}

function removerPagamento(index: number) {
  pagamentos.value = pagamentos.value.filter((_, i) => i !== index)
}

async function limpar() {
  const ok = await confirmRef.value?.open('Limpar pagamentos', 'Deseja limpar todos os pagamentos, desconto e acréscimo?', { danger: true, textoConfirmar: 'Limpar' })
  if (!ok) return
  pagamentos.value = []
  descontoTotal.value = 0
  acrescimoTotal.value = 0
}

function finalizar() {
  emit('finalizar')
}

// #endregion

// #region Atalhos de teclado

function aoTeclar(ev: KeyboardEvent) {
  if (dialogAberto.value) return
  if (ev.key === 'F7') {
    ev.preventDefault()
    limpar()
    return
  }
  if (ev.key === '-') {
    ev.preventDefault()
    abrirDesconto()
    return
  }
  if (ev.key === '+') {
    ev.preventDefault()
    abrirAcrescimo()
    return
  }
  if (/^[1-6]$/.test(ev.key)) {
    const forma = formas[Number(ev.key) - 1]
    if (forma) {
      ev.preventDefault()
      abrirPagamento(forma.tipo)
    }
  }
}

onMounted(() => window.addEventListener('keydown', aoTeclar))
onUnmounted(() => window.removeEventListener('keydown', aoTeclar))

// #endregion

function rotulo(tipo: number): string {
  return ROTULO_PAGAMENTO[tipo] ?? 'Desconhecido'
}
</script>

<template>
  <div class="pdv-pagamentos">
    <div class="formas-grid">
      <button
        v-for="(f, i) in formas"
        :key="f.tipo"
        type="button"
        class="btn btn-primary forma-btn"
        :disabled="bloqueado"
        @click="abrirPagamento(f.tipo)"
      >
        {{ f.label }} <span class="forma-num">({{ i + 1 }})</span>
      </button>
    </div>

    <div class="ajustes-grid">
      <button type="button" class="btn btn-danger" :disabled="bloqueado" @click="abrirDesconto">Desconto (−)</button>
      <button type="button" class="btn btn-success" :disabled="bloqueado" @click="abrirAcrescimo">Acréscimo (+)</button>
    </div>

    <div class="acoes-grid">
      <button type="button" class="btn btn-secondary" @click="limpar">Limpar (F7)</button>
      <button type="button" class="btn btn-success" @click="finalizar">Finalizar (F10)</button>
    </div>

    <div class="pag-lista glass-panel">
      <table class="admin-table">
        <thead>
          <tr>
            <th>Forma</th>
            <th class="td-right">Valor</th>
            <th class="td-actions"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="!pagamentos.length">
            <td colspan="3"><div class="pag-vazio">Nenhum pagamento lançado.</div></td>
          </tr>
          <tr v-for="(p, index) in pagamentos" :key="index">
            <td>{{ rotulo(p.tipoPagamento) }}</td>
            <td class="td-right">{{ formatarMoeda(p.valorPagamento, false) }}</td>
            <td class="td-actions">
              <button type="button" class="btn btn-ghost btn-sm" title="Remover" @click="removerPagamento(index)">🗑</button>
            </td>
          </tr>
          <tr v-if="descontoTotal > 0" class="linha-ajuste">
            <td>Desconto</td>
            <td class="td-right texto-danger">− {{ formatarMoeda(descontoTotal, false) }}</td>
            <td class="td-actions"><button type="button" class="btn btn-ghost btn-sm" @click="descontoTotal = 0">🗑</button></td>
          </tr>
          <tr v-if="acrescimoTotal > 0" class="linha-ajuste">
            <td>Acréscimo</td>
            <td class="td-right texto-primary">+ {{ formatarMoeda(acrescimoTotal, false) }}</td>
            <td class="td-actions"><button type="button" class="btn btn-ghost btn-sm" @click="acrescimoTotal = 0">🗑</button></td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="totais">
      <div class="total-box total-box-principal">
        <span class="total-label">Total</span>
        <span class="total-valor">{{ formatarMoeda(totalFinal) }}</span>
      </div>
      <div class="total-linha">
        <div class="total-box" :class="saldoRestante > 0 ? 'box-restante' : 'box-ok'">
          <span class="total-label">Restante</span>
          <span class="total-valor-sm">{{ formatarMoeda(saldoRestante) }}</span>
        </div>
        <div class="total-box" :class="troco > 0 ? 'box-troco' : ''">
          <span class="total-label">Troco</span>
          <span class="total-valor-sm">{{ formatarMoeda(troco) }}</span>
        </div>
      </div>
    </div>

    <div class="field">
      <label class="field-label">Observações da venda</label>
      <textarea v-model="informacoes" class="input pdv-obs" rows="2" placeholder="Digite aqui as observações para a venda"></textarea>
    </div>

    <PdvValorDialog
      v-model="dialogAberto"
      :titulo="dialogTitulo"
      :valor-inicial="valorInicial"
      @confirmar="confirmarValor"
    />
    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.pdv-pagamentos { display: flex; flex-direction: column; gap: 10px; }
.formas-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px; }
.ajustes-grid,
.acoes-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
.forma-num { opacity: 0.75; font-size: 11px; }
.pag-lista { padding: 6px 10px; max-height: 220px; overflow: auto; }
.pag-vazio { text-align: center; padding: 16px 0; color: var(--text-muted); font-size: 12px; }
.linha-ajuste td { font-weight: 600; }
.texto-danger { color: var(--danger); }
.texto-primary { color: var(--primary-hover); }
.totais { display: flex; flex-direction: column; gap: 8px; }
.total-linha { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
.total-box {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: 10px 14px;
  border-radius: 10px;
  background: var(--bg-card);
  border: 1px solid var(--border-color);
}
.total-box-principal { background: var(--primary-glow); border-color: rgba(99, 102, 241, 0.35); }
.total-label { font-size: 11px; text-transform: uppercase; color: var(--text-secondary); letter-spacing: 0.04em; }
.total-valor { font-size: 26px; font-weight: 800; text-align: right; }
.total-valor-sm { font-size: 18px; font-weight: 700; text-align: right; }
.box-restante { border-color: rgba(239, 68, 68, 0.4); }
.box-restante .total-valor-sm { color: var(--danger); }
.box-troco { border-color: rgba(245, 158, 11, 0.4); }
.box-troco .total-valor-sm { color: var(--warning); }
.box-ok { border-color: rgba(16, 185, 129, 0.35); }
.pdv-obs { resize: none; min-height: 52px; font-family: inherit; }
@media (max-width: 720px) {
  .formas-grid { grid-template-columns: repeat(2, 1fr); }
}
</style>
