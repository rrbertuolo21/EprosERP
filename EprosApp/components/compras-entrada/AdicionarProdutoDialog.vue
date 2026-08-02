<script setup lang="ts">
/**
 * AdicionarProdutoDialog — diálogo para incluir/editar um item da nota de entrada.
 *
 * Porta o comportamento essencial de
 * `components/compras/entrada-mercadorias/AdicionarProdutoEntradaDialog.vue` do legado,
 * reconstruído no design system do novo app (sem Vuetify):
 *   - busca de produto por código/descrição em `estoque-produtos` (debounce);
 *   - ao selecionar, carrega dados fiscais básicos (NCM, unidade, preço de compra sugerido);
 *   - campos de quantidade, valor unitário, desconto e valores de ICMS/IPI do item;
 *   - total do item calculado (quantidade × unitário − desconto);
 *   - valida e emite o item pronto para o payload de `LancarCompraCommand.Itens`.
 *
 * Endpoint consumido: estoque-produtos (busca de produto).
 */
import { computed, ref, watch } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import type { ItemEntrada } from './tipos'

const props = defineProps<{
  /** Controla a visibilidade (v-model). */
  modelValue: boolean
  /** Item em edição; quando null, o diálogo abre em modo de inclusão. */
  itemEmEdicao?: ItemEntrada | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  /** Item confirmado (inclusão). */
  adicionar: [item: ItemEntrada]
  /** Item confirmado (edição) com o índice original. */
  editar: [payload: { index: number; item: ItemEntrada }]
}>()

const toast = useToast()
const { formatarMoeda } = useHelper()

/** Produto retornado pela busca em estoque-produtos. */
interface ProdutoBusca {
  id: number
  codigo: string | null
  descricao: string
  ean: string | null
  codigoNcm: string | null
  unidadeMedida: string | null
  valorCompra: number | null
}

// --- Formulário do item ---
const produtoId = ref<number | null>(null)
const sku = ref('')
const nomeProduto = ref('')
const ncm = ref('')
const unidade = ref('')
const codigoEan = ref('')
const cfop = ref('')
const quantidade = ref<number | null>(1)
const valorUnitario = ref<number | null>(0)
const valorDesconto = ref<number | null>(0)
const valorIcms = ref<number | null>(0)
const valorIpi = ref<number | null>(0)

// --- Busca de produto ---
const termoBusca = ref('')
const resultados = ref<ProdutoBusca[]>([])
const buscando = ref(false)
const salvando = ref(false)

const emEdicao = computed(() => props.itemEmEdicao != null)
const indiceEdicao = computed(() => props.itemEmEdicao?.index ?? -1)

const totalItem = computed(() => {
  const base = (quantidade.value ?? 0) * (valorUnitario.value ?? 0)
  const desconto = valorDesconto.value ?? 0
  return arredondar(base - desconto)
})

function arredondar(v: number): number {
  return Math.round((v + Number.EPSILON) * 100) / 100
}

function limparFormulario() {
  produtoId.value = null
  sku.value = ''
  nomeProduto.value = ''
  ncm.value = ''
  unidade.value = ''
  codigoEan.value = ''
  cfop.value = ''
  quantidade.value = 1
  valorUnitario.value = 0
  valorDesconto.value = 0
  valorIcms.value = 0
  valorIpi.value = 0
  termoBusca.value = ''
  resultados.value = []
}

function hidratarDeItem(item: ItemEntrada) {
  produtoId.value = item.produtoId ?? null
  sku.value = item.sku
  nomeProduto.value = item.nomeProduto
  ncm.value = item.ncm ?? ''
  unidade.value = item.unidade ?? ''
  codigoEan.value = item.codigoEan ?? ''
  cfop.value = item.cfop ?? ''
  quantidade.value = item.quantidade
  valorUnitario.value = item.precoUnitario
  valorDesconto.value = item.valorDesconto ?? 0
  valorIcms.value = item.valorIcms ?? 0
  valorIpi.value = item.valorIpi ?? 0
  termoBusca.value = `${item.sku} - ${item.nomeProduto}`
}

// Ao abrir, prepara o estado conforme inclusão/edição.
watch(
  () => props.modelValue,
  (aberto) => {
    if (!aberto) return
    if (props.itemEmEdicao) hidratarDeItem(props.itemEmEdicao)
    else limparFormulario()
  }
)

let debounceBusca: ReturnType<typeof setTimeout> | undefined

function aoDigitarBusca(valor: string) {
  termoBusca.value = valor
  if (debounceBusca) clearTimeout(debounceBusca)
  if (!valor || valor.trim().length < 2) {
    resultados.value = []
    return
  }
  debounceBusca = setTimeout(() => void buscarProdutos(valor.trim()), 400)
}

async function buscarProdutos(termo: string) {
  buscando.value = true
  try {
    const resposta = await useApi('/estoque-produtos', {
      query: { descricao: termo, ativo: true, pagina: 1, tamanhoPagina: 20 }
    })
    resultados.value = extrairDados<ProdutoBusca[]>(resposta) ?? []
  } catch (e) {
    resultados.value = []
    console.error('[AdicionarProdutoDialog] busca de produtos', e)
  } finally {
    buscando.value = false
  }
}

function selecionarProduto(p: ProdutoBusca) {
  produtoId.value = p.id
  sku.value = p.codigo ?? String(p.id)
  nomeProduto.value = p.descricao
  ncm.value = p.codigoNcm ?? ''
  unidade.value = p.unidadeMedida ?? ''
  codigoEan.value = p.ean ?? ''
  if (!valorUnitario.value || valorUnitario.value === 0) {
    valorUnitario.value = p.valorCompra ?? 0
  }
  termoBusca.value = `${sku.value} - ${p.descricao}`
  resultados.value = []
}

function validar(): boolean {
  if (!sku.value.trim() || !nomeProduto.value.trim()) {
    toast.error('Selecione um produto válido')
    return false
  }
  if (!quantidade.value || quantidade.value <= 0) {
    toast.error('Quantidade deve ser maior que zero')
    return false
  }
  if (!valorUnitario.value || valorUnitario.value <= 0) {
    toast.error('Valor unitário deve ser maior que zero')
    return false
  }
  const base = (quantidade.value ?? 0) * (valorUnitario.value ?? 0)
  if ((valorDesconto.value ?? 0) >= base && base > 0) {
    toast.error('O desconto não pode ser maior ou igual ao valor do produto')
    return false
  }
  return true
}

function montarItem(): ItemEntrada {
  return {
    index: indiceEdicao.value,
    produtoId: produtoId.value ?? undefined,
    sku: sku.value.trim(),
    nomeProduto: nomeProduto.value.trim(),
    ncm: ncm.value.trim() || undefined,
    unidade: unidade.value.trim() || undefined,
    codigoEan: codigoEan.value.trim() || undefined,
    cfop: cfop.value.trim() || undefined,
    quantidade: Number(quantidade.value ?? 0),
    precoUnitario: Number(valorUnitario.value ?? 0),
    valorDesconto: Number(valorDesconto.value ?? 0),
    valorIcms: Number(valorIcms.value ?? 0),
    valorIpi: Number(valorIpi.value ?? 0),
    totalItem: totalItem.value
  }
}

function confirmar() {
  if (salvando.value) return
  if (!validar()) return
  salvando.value = true
  try {
    const item = montarItem()
    if (emEdicao.value) emit('editar', { index: indiceEdicao.value, item })
    else emit('adicionar', item)
    emit('update:modelValue', false)
  } finally {
    salvando.value = false
  }
}

function fechar() {
  emit('update:modelValue', false)
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    :title="emEdicao ? 'Editar Item' : 'Adicionar Produto'"
    width="720px"
    persistent
    @update:model-value="emit('update:modelValue', $event)"
  >
    <div class="form-grid">
      <!-- Busca de produto (oculta em edição para preservar o item selecionado) -->
      <div v-if="!emEdicao" class="col-12 busca-wrap">
        <label class="field-label">Produto</label>
        <input
          class="input"
          type="text"
          placeholder="Busque por código ou descrição (mín. 2 caracteres)"
          :value="termoBusca"
          @input="aoDigitarBusca(($event.target as HTMLInputElement).value)"
        />
        <div v-if="buscando" class="busca-status">Buscando...</div>
        <ul v-else-if="resultados.length" class="busca-resultados glass-panel">
          <li
            v-for="p in resultados"
            :key="p.id"
            class="busca-item"
            @click="selecionarProduto(p)"
          >
            <span class="busca-cod">{{ p.codigo ?? p.id }}</span>
            <span class="busca-desc">{{ p.descricao }}</span>
            <span v-if="p.unidadeMedida" class="busca-un">{{ p.unidadeMedida }}</span>
          </li>
        </ul>
      </div>

      <div class="col-8">
        <TextField v-model="nomeProduto" label="Descrição do produto" :readonly="!emEdicao" required />
      </div>
      <div class="col-4">
        <TextField v-model="sku" label="Código (SKU)" :readonly="!emEdicao" required />
      </div>

      <div class="col-4">
        <TextField v-model="ncm" label="NCM" placeholder="NCM" />
      </div>
      <div class="col-4">
        <TextField v-model="cfop" label="CFOP" placeholder="CFOP" />
      </div>
      <div class="col-4">
        <TextField v-model="unidade" label="Unidade" placeholder="UN" />
      </div>

      <div class="col-3">
        <QuantityInput v-model="quantidade" label="Quantidade" :suffix="unidade || undefined" required />
      </div>
      <div class="col-3">
        <MoneyInput v-model="valorUnitario" label="Valor Unitário" required />
      </div>
      <div class="col-3">
        <MoneyInput v-model="valorDesconto" label="Desconto" />
      </div>
      <div class="col-3">
        <div class="field">
          <label class="field-label">Total do Item</label>
          <div class="total-item">{{ formatarMoeda(totalItem) }}</div>
        </div>
      </div>

      <div class="col-6">
        <MoneyInput v-model="valorIcms" label="Valor ICMS" />
      </div>
      <div class="col-6">
        <MoneyInput v-model="valorIpi" label="Valor IPI" />
      </div>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" :disabled="salvando" @click="fechar">Cancelar</button>
      <button type="button" class="btn btn-primary" :disabled="salvando" @click="confirmar">
        <span v-if="salvando" class="spinner"></span>
        <span v-else>{{ emEdicao ? 'Salvar Item' : 'Adicionar' }}</span>
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.busca-wrap {
  position: relative;
}
.busca-status {
  margin-top: 6px;
  font-size: 12px;
  color: var(--text-secondary);
}
.busca-resultados {
  list-style: none;
  margin: 6px 0 0;
  padding: 4px;
  max-height: 240px;
  overflow-y: auto;
  position: absolute;
  z-index: 20;
  left: 0;
  right: 0;
}
.busca-item {
  display: grid;
  grid-template-columns: 90px 1fr auto;
  gap: 10px;
  align-items: center;
  padding: 8px 10px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 13px;
}
.busca-item:hover {
  background: rgba(255, 255, 255, 0.06);
}
.busca-cod {
  color: var(--text-secondary);
  font-variant-numeric: tabular-nums;
}
.busca-desc {
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.busca-un {
  color: var(--text-secondary);
  font-size: 11px;
  text-transform: uppercase;
}
.total-item {
  padding: 9px 12px;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.05);
  font-weight: 700;
  text-align: right;
  font-variant-numeric: tabular-nums;
}
</style>
