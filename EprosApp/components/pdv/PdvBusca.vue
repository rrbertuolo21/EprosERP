<script setup lang="ts">
/**
 * PdvBusca — busca de produto + entrada de quantidade/preço/desconto do item corrente.
 *
 * Porta o comportamento de `components/pos/produto.vue` do legado:
 *   - autocomplete de produto por nome/código/EAN (debounce);
 *   - leitura de código de barras de balança (13 dígitos) → extrai peso/valor;
 *   - sintaxe "N*termo" para quantidade;
 *   - cálculo de desconto por percentual e por valor (sincronizados);
 *   - fluxo de foco por Enter (quantidade → preço → desconto% → descontoR$ → incluir).
 *
 * Reconstruído com os campos compartilhados (QuantityInput/MoneyInput/PercentInput) e o
 * design system novo — sem Vuetify.
 */
import { computed, nextTick, onMounted, reactive, ref, watch } from 'vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import type { BalancaPdv, ItemPdv, ProdutoPdv } from './tipos'

const props = defineProps<{
  balancas: BalancaPdv[]
}>()

const emit = defineEmits<{
  'add-item': [item: ItemPdv]
  mensagem: [texto: string]
  buscar: [termo: string]
}>()

const toast = useToast()
const { formatarMoeda } = useHelper()

/** Lista de resultados da busca (preenchida externamente via `definirResultados`). */
const resultados = ref<ProdutoPdv[]>([])
const termo = ref('')
const carregando = ref(false)
const mostrarLista = ref(false)
let debounce: ReturnType<typeof setTimeout> | null = null

const buscaRef = ref<HTMLInputElement | null>(null)
const qtdRef = ref<InstanceType<typeof QuantityInput> | null>(null)
const precoRef = ref<InstanceType<typeof MoneyInput> | null>(null)
const descPercRef = ref<InstanceType<typeof PercentInput> | null>(null)
const descValorRef = ref<InstanceType<typeof MoneyInput> | null>(null)

const descontoPercentual = ref(0)
const descontoValor = ref(0)

const item = reactive<ItemPdv>({
  produtoId: '',
  produto: null,
  quantidadeComercial: 1,
  valorUnitarioComercial: 0,
  valorDesconto: 0
})

const temProduto = computed(() => !!item.produtoId)

const totalItem = computed(() => {
  const base = item.quantidadeComercial * item.valorUnitarioComercial
  return arredondar2(base - item.valorDesconto)
})

const padraoQuantidade = /\d+\*.+/

function arredondar2(v: number): number {
  return Math.round((v + Number.EPSILON) * 100) / 100
}
function arredondar3(v: number): number {
  return Math.round((v + Number.EPSILON) * 1000) / 1000
}

// #region Busca

function aoDigitarBusca() {
  mostrarLista.value = true
  if (debounce) clearTimeout(debounce)
  debounce = setTimeout(() => processarBusca(termo.value), 450)
}

async function processarBusca(texto: string) {
  if (!texto?.trim()) {
    resultados.value = []
    return
  }

  let termoBusca = texto.trim()

  // Sintaxe "N*produto" → define a quantidade e mantém apenas o termo.
  if (padraoQuantidade.test(termoBusca)) {
    const partes = termoBusca.split('*')
    const qtd = parseFloat(partes[0]?.trim() ?? '0')
    if (qtd > 1) item.quantidadeComercial = qtd
    termoBusca = partes[1]?.trim() ?? ''
  }

  // Código de barras de balança (13 dígitos) → decodifica peso/valor.
  if (/^\d{13}$/.test(termoBusca)) {
    const balanca = encontrarBalanca(termoBusca)
    if (balanca) {
      const dados = extrairDadosBalanca(termoBusca, balanca)
      if (dados) {
        emit('buscar', dados.codigoProduto)
        // A página busca o produto por código; guardamos peso/valor para aplicar no select.
        pendenteBalanca.value = dados
        return
      }
    }
  }

  carregando.value = true
  emit('buscar', termoBusca)
}

/** Chamado pela página com os resultados da API. */
function definirResultados(lista: ProdutoPdv[]) {
  carregando.value = false
  resultados.value = lista

  // Se havia leitura de balança pendente, aplica o produto e o peso/valor.
  if (pendenteBalanca.value && lista.length > 0) {
    const bal = pendenteBalanca.value
    const produto = lista.find((p) => p.balancaId === bal.balancaId) ?? lista[0]
    aplicarBalanca(produto, bal)
    pendenteBalanca.value = null
    return
  }

  // EAN exato único → seleciona direto.
  if (/^\d{13}$/.test(termo.value.trim()) && lista.length === 1) {
    selecionarProduto(lista[0])
  }
}

interface DadosBalanca {
  codigoProduto: string
  valor: number
  tipoValor: number
  balancaId: string | null
}
const pendenteBalanca = ref<DadosBalanca | null>(null)

function encontrarBalanca(codigo: string): BalancaPdv | undefined {
  return props.balancas.find((b) => !!b.prefixo && codigo.startsWith(b.prefixo))
}

/**
 * Decodifica o código de barras de balança (padrão EAN-13 interno):
 * prefixo(2) + código do produto(5) + valor(5) + dígito(1).
 */
function extrairDadosBalanca(codigo: string, balanca: BalancaPdv): DadosBalanca | null {
  const codigoProduto = codigo.substring(2, 7).replace(/^0+/, '') || '0'
  const bruto = codigo.substring(7, 12)
  const valor = Number(bruto) / 1000
  if (!Number.isFinite(valor) || valor <= 0) return null
  return {
    codigoProduto,
    valor,
    tipoValor: balanca.tipoValor ?? 1,
    balancaId: balanca.id
  }
}

function aplicarBalanca(produto: ProdutoPdv, dados: DadosBalanca) {
  selecionarProduto(produto)
  if (dados.tipoValor === 1) {
    item.quantidadeComercial = arredondar3(dados.valor)
  } else {
    const preco = produto.valorVenda ?? 0
    if (preco > 0) item.quantidadeComercial = arredondar3(dados.valor / preco)
  }
  // Balança adiciona direto no cupom.
  adicionarItem()
}

function selecionarProduto(produto: ProdutoPdv) {
  item.produto = produto
  item.produtoId = produto.id
  item.valorUnitarioComercial = produto.valorVenda ?? 0
  descontoPercentual.value = 0
  descontoValor.value = 0
  resultados.value = []
  mostrarLista.value = false
  termo.value = `${(produto.descricao ?? '').toUpperCase()} — Cód: ${produto.codigo ?? ''}`
  emit('mensagem', produto.descricao ?? '')
  nextTick(() => qtdRef.value?.$el?.querySelector('input')?.select())
}

// #endregion

// #region Descontos

watch(descontoPercentual, () => {
  const base = item.valorUnitarioComercial * item.quantidadeComercial
  descontoValor.value = arredondar2(base * (descontoPercentual.value / 100))
  item.valorDesconto = descontoValor.value
})

function aoDigitarDescontoValor() {
  const base = item.valorUnitarioComercial * item.quantidadeComercial
  const perc = base > 0 ? (descontoValor.value / base) * 100 : 0
  descontoPercentual.value = Number.isNaN(perc) ? 0 : arredondar2(perc)
  item.valorDesconto = descontoValor.value
}

function validarDescontoPercentual() {
  if (descontoPercentual.value >= 100 && temProduto.value) {
    descontoPercentual.value = 0
    toast.warning('Desconto não pode ser maior ou igual a 100%.')
  }
}

// #endregion

// #region Fluxo de foco por Enter

function focarInput(componente: { $el?: HTMLElement } | null) {
  const input = componente?.$el?.querySelector('input') as HTMLInputElement | undefined
  input?.focus()
  input?.select()
}

function enterBusca() {
  if (resultados.value.length === 1) selecionarProduto(resultados.value[0])
  else if (temProduto.value) focarInput(qtdRef.value)
}
function enterQuantidade() {
  focarInput(precoRef.value)
}
function enterPreco() {
  if (item.valorUnitarioComercial <= 0) {
    toast.warning('Valor unitário não pode ser zero.')
    focarInput(precoRef.value)
    return
  }
  focarInput(descPercRef.value)
}
function enterDescontoPercentual() {
  validarDescontoPercentual()
  focarInput(descValorRef.value)
}

// #endregion

function adicionarItem() {
  if (!item.produtoId) {
    toast.warning('Selecione um produto.')
    return
  }
  if (item.valorUnitarioComercial <= 0) {
    toast.warning('Valor unitário não pode ser zero.')
    focarInput(precoRef.value)
    return
  }
  if (item.quantidadeComercial <= 0) {
    toast.warning('Quantidade deve ser maior que zero.')
    return
  }
  emit('add-item', { ...item, produto: item.produto ? { ...item.produto } : null })
  limpar()
  emit('mensagem', 'Selecione um produto')
}

function limpar() {
  item.produtoId = ''
  item.produto = null
  item.quantidadeComercial = 1
  item.valorUnitarioComercial = 0
  item.valorDesconto = 0
  descontoPercentual.value = 0
  descontoValor.value = 0
  termo.value = ''
  resultados.value = []
  mostrarLista.value = false
  focar()
}

/** Carrega um item existente para edição (reabrindo os campos). */
function carregarParaEdicao(existente: ItemPdv) {
  limpar()
  item.produtoId = existente.produtoId
  item.produto = existente.produto
  item.quantidadeComercial = existente.quantidadeComercial
  item.valorUnitarioComercial = existente.valorUnitarioComercial
  item.valorDesconto = existente.valorDesconto
  descontoValor.value = existente.valorDesconto
  aoDigitarDescontoValor()
  if (existente.produto) {
    termo.value = `${(existente.produto.descricao ?? '').toUpperCase()} — Cód: ${existente.produto.codigo ?? ''}`
  }
  emit('mensagem', `Editando: ${existente.produto?.descricao ?? ''}`)
  nextTick(() => focarInput(qtdRef.value))
}

function focar() {
  nextTick(() => buscaRef.value?.focus())
}

onMounted(focar)

defineExpose({ definirResultados, carregarParaEdicao, limpar, focar })
</script>

<template>
  <div class="pdv-busca">
    <div class="busca-campo">
      <input
        ref="buscaRef"
        v-model="termo"
        class="input"
        type="text"
        placeholder="Pesquise por nome, código ou GTIN/EAN"
        autocomplete="off"
        @input="aoDigitarBusca"
        @keydown.enter.prevent="enterBusca"
      />
      <span v-if="carregando" class="spinner busca-spinner"></span>

      <ul v-if="mostrarLista && resultados.length" class="busca-lista glass-panel">
        <li
          v-for="p in resultados"
          :key="p.id"
          class="busca-item"
          @click="selecionarProduto(p)"
        >
          <strong>{{ (p.descricao ?? '').toUpperCase() }}</strong>
          <span class="busca-meta">Cód: {{ p.codigo }} · {{ formatarMoeda(p.valorVenda) }}</span>
        </li>
      </ul>
    </div>

    <div class="pdv-grid">
      <QuantityInput
        ref="qtdRef"
        v-model="item.quantidadeComercial"
        label="Quantidade"
        :decimais="3"
        :disabled="!temProduto"
        @keydown.enter.prevent="enterQuantidade"
      />
      <MoneyInput
        ref="precoRef"
        v-model="item.valorUnitarioComercial"
        label="Preço Unitário"
        :disabled="!temProduto"
        @keydown.enter.prevent="enterPreco"
      />
      <PercentInput
        ref="descPercRef"
        v-model="descontoPercentual"
        label="Desconto %"
        :disabled="!temProduto"
        @keydown.enter.prevent="enterDescontoPercentual"
      />
      <MoneyInput
        ref="descValorRef"
        v-model="descontoValor"
        label="Desconto R$"
        :disabled="!temProduto"
        @update:model-value="aoDigitarDescontoValor"
        @keydown.enter.prevent="adicionarItem"
      />
      <MoneyInput
        :model-value="totalItem"
        label="Preço Total"
        readonly
      />
      <div class="pdv-incluir">
        <button type="button" class="btn btn-primary btn-incluir" :disabled="!temProduto" @click="adicionarItem">
          Incluir Item
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.pdv-busca { display: flex; flex-direction: column; gap: 12px; }
.busca-campo { position: relative; }
.busca-spinner { position: absolute; right: 12px; top: 50%; transform: translateY(-50%); }
.busca-lista {
  position: absolute;
  z-index: 20;
  top: calc(100% + 4px);
  left: 0;
  right: 0;
  max-height: 320px;
  overflow-y: auto;
  padding: 6px;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.busca-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: 8px 10px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 13px;
}
.busca-item:hover { background: var(--primary-glow); }
.busca-meta { font-size: 11px; color: var(--text-secondary); }
.pdv-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}
.pdv-incluir { display: flex; align-items: flex-end; }
.btn-incluir { width: 100%; height: 42px; }
@media (max-width: 720px) {
  .pdv-grid { grid-template-columns: repeat(2, 1fr); }
}
</style>
