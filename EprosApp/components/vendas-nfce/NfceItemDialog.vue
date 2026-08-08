<script setup lang="ts">
/**
 * NfceItemDialog — adicionar/editar um item da NFC-e.
 *
 * Porta o comportamento de `components/pos/produto.vue` do legado (busca de produto,
 * quantidade, valor unitário, desconto em valor/percentual e total do item), sem Vuetify.
 *
 * Busca produtos em `/estoque-produtos` (via useApi) e emite o item pronto para a lista.
 */
import { computed, ref, watch } from 'vue'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import AppDialog from '~/components/shared/AppDialog.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import PercentInput from '~/components/shared/fields/PercentInput.vue'
import { useHelper } from '~/composables/useHelper'
import type { NfceItem, ProdutoResumo } from './types'

const props = withDefaults(
  defineProps<{
    modelValue: boolean
    /** Item em edição (quando reabrindo para alterar); null = novo item. */
    item?: NfceItem | null
  }>(),
  { item: null }
)

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  /** Item confirmado (adicionar ou salvar edição). */
  confirmar: [item: NfceItem]
}>()

const { formatarMoeda } = useHelper()

const round2 = (v: number) => Math.round((v + Number.EPSILON) * 100) / 100
const round3 = (v: number) => Math.round((v + Number.EPSILON) * 1000) / 1000

// --- Estado de busca de produto
const termoBusca = ref('')
const produtos = ref<ProdutoResumo[]>([])
const buscando = ref(false)
const erroBusca = ref<string | null>(null)

// --- Estado do item corrente
const produtoSelecionado = ref<ProdutoResumo | null>(null)
const quantidade = ref(1)
const valorUnitario = ref(0)
const descontoValor = ref(0)
const descontoPercentual = ref(0)

const editando = computed(() => !!props.item)

const totalItem = computed(() => {
  const base = quantidade.value * valorUnitario.value
  return round2(base - descontoValor.value)
})

const podeConfirmar = computed(
  () => !!produtoSelecionado.value && quantidade.value > 0 && valorUnitario.value >= 0
)

async function buscarProdutos() {
  const termo = termoBusca.value.trim()
  if (termo.length < 2) {
    produtos.value = []
    return
  }
  buscando.value = true
  erroBusca.value = null
  try {
    const resposta = await useApi('/estoque-produtos', {
      query: { termo, pagina: 1, tamanhoPagina: 20 }
    })
    produtos.value = extrairLista<ProdutoResumo>(resposta) ?? []
  } catch (e) {
    erroBusca.value = 'Não foi possível buscar produtos.'
    produtos.value = []
    console.error('[NfceItemDialog.buscarProdutos]', e)
  } finally {
    buscando.value = false
  }
}

function selecionarProduto(p: ProdutoResumo) {
  produtoSelecionado.value = p
  valorUnitario.value = Number(p.valorVenda ?? 0)
  quantidade.value = 1
  descontoValor.value = 0
  descontoPercentual.value = 0
  produtos.value = []
  termoBusca.value = ''
}

/** Sincroniza desconto % -> valor com base no subtotal. */
function aoMudarPercentual(v: number | null) {
  descontoPercentual.value = v ?? 0
  const base = quantidade.value * valorUnitario.value
  descontoValor.value = round2(base * (descontoPercentual.value / 100))
}

/** Sincroniza desconto valor -> % com base no subtotal. */
function aoMudarDescontoValor(v: number | null) {
  descontoValor.value = v ?? 0
  const base = quantidade.value * valorUnitario.value
  descontoPercentual.value = base > 0 ? round2((descontoValor.value / base) * 100) : 0
}

function limpar() {
  produtoSelecionado.value = null
  quantidade.value = 1
  valorUnitario.value = 0
  descontoValor.value = 0
  descontoPercentual.value = 0
  termoBusca.value = ''
  produtos.value = []
  erroBusca.value = null
}

// Ao abrir: se houver item para editar, carrega; senão limpa.
watch(
  () => props.modelValue,
  (aberto) => {
    if (!aberto) return
    if (props.item) {
      produtoSelecionado.value = {
        id: props.item.produtoId,
        descricao: props.item.descricao,
        unidadeComercial: props.item.unidade
      }
      quantidade.value = props.item.quantidadeComercial
      valorUnitario.value = props.item.valorUnitarioComercial
      descontoValor.value = props.item.valorDesconto
      const base = props.item.quantidadeComercial * props.item.valorUnitarioComercial
      descontoPercentual.value = base > 0 ? round2((props.item.valorDesconto / base) * 100) : 0
    } else {
      limpar()
    }
  }
)

function confirmar() {
  if (!podeConfirmar.value || !produtoSelecionado.value) return
  const item: NfceItem = {
    produtoId: produtoSelecionado.value.id,
    descricao: produtoSelecionado.value.descricao ?? '',
    unidade: produtoSelecionado.value.unidadeComercial ?? null,
    quantidadeComercial: round3(quantidade.value),
    valorUnitarioComercial: round2(valorUnitario.value),
    valorDesconto: round2(descontoValor.value)
  }
  emit('confirmar', item)
  emit('update:modelValue', false)
}

function fechar() {
  emit('update:modelValue', false)
}
</script>

<template>
  <AppDialog
    :model-value="modelValue"
    :title="editando ? 'Editar item' : 'Adicionar item'"
    width="640px"
    persistent
    @update:model-value="emit('update:modelValue', $event)"
  >
    <div class="item-dialog">
      <!-- Busca de produto (oculta ao editar item já selecionado) -->
      <div v-if="!produtoSelecionado" class="busca-produto">
        <div class="field">
          <label class="field-label">Buscar produto</label>
          <input
            v-model="termoBusca"
            class="input"
            placeholder="Código, descrição ou código de barras..."
            @keyup.enter="buscarProdutos"
          />
        </div>
        <button type="button" class="btn btn-primary" :disabled="buscando" @click="buscarProdutos">
          <span v-if="buscando" class="spinner"></span>
          <span v-else>Buscar</span>
        </button>
      </div>

      <p v-if="erroBusca" class="erro-busca">{{ erroBusca }}</p>

      <ul v-if="!produtoSelecionado && produtos.length" class="resultado-lista">
        <li v-for="p in produtos" :key="p.id" class="resultado-item" @click="selecionarProduto(p)">
          <span class="resultado-desc">{{ p.descricao }}</span>
          <span class="resultado-meta">
            Cód: {{ p.codigo ?? '—' }} · {{ formatarMoeda(Number(p.valorVenda ?? 0)) }}
          </span>
        </li>
      </ul>

      <!-- Formulário do item -->
      <div v-if="produtoSelecionado" class="item-form">
        <div class="produto-selecionado glass-panel">
          <div>
            <strong>{{ produtoSelecionado.descricao }}</strong>
            <span class="badge">{{ produtoSelecionado.unidadeComercial ?? 'UN' }}</span>
          </div>
          <button v-if="!editando" type="button" class="btn btn-ghost btn-sm" @click="limpar">Trocar</button>
        </div>

        <div class="form-grid">
          <QuantityInput
            v-model="quantidade"
            label="Quantidade"
            :decimais="3"
            :suffix="produtoSelecionado.unidadeComercial ?? 'UN'"
            class="col-4"
          />
          <MoneyInput v-model="valorUnitario" label="Valor unitário" class="col-4" />
          <MoneyInput
            :model-value="descontoValor"
            label="Desconto (R$)"
            class="col-4"
            @update:model-value="aoMudarDescontoValor"
          />
          <PercentInput
            :model-value="descontoPercentual"
            label="Desconto (%)"
            class="col-4"
            @update:model-value="aoMudarPercentual"
          />
          <div class="total-item col-8">
            <span class="total-label">Total do item</span>
            <span class="total-valor">{{ formatarMoeda(totalItem) }}</span>
          </div>
        </div>
      </div>
    </div>

    <template #footer>
      <button type="button" class="btn btn-secondary" @click="fechar">Cancelar</button>
      <button type="button" class="btn btn-primary" :disabled="!podeConfirmar" @click="confirmar">
        {{ editando ? 'Salvar item' : 'Adicionar' }}
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.item-dialog { display: flex; flex-direction: column; gap: 12px; }
.busca-produto { display: flex; gap: 8px; align-items: flex-end; }
.busca-produto .field { flex: 1; }
.erro-busca { color: var(--danger); font-size: 13px; }
.resultado-lista { list-style: none; display: flex; flex-direction: column; gap: 4px; max-height: 240px; overflow-y: auto; }
.resultado-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: 8px 10px;
  border-radius: 8px;
  cursor: pointer;
  border: 1px solid var(--border-color);
}
.resultado-item:hover { background: rgba(255, 255, 255, 0.05); }
.resultado-desc { font-weight: 600; font-size: 13px; }
.resultado-meta { font-size: 12px; color: var(--text-muted); }
.item-form { display: flex; flex-direction: column; gap: 12px; }
.produto-selecionado {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 12px;
}
.produto-selecionado .badge { margin-left: 8px; }
.form-grid { display: grid; grid-template-columns: repeat(12, 1fr); gap: 12px 16px; }
.col-4 { grid-column: span 4; }
.col-8 { grid-column: span 8; }
.total-item {
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  align-items: flex-end;
}
.total-label { font-size: 12px; color: var(--text-muted); }
.total-valor { font-size: 22px; font-weight: 700; color: var(--primary); }
@media (max-width: 720px) {
  .col-4, .col-8 { grid-column: span 12; }
}
</style>
