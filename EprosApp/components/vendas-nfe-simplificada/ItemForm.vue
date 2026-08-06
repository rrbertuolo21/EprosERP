<script setup lang="ts">
/**
 * ItemForm — adição de itens à NF-e simplificada.
 *
 * Porta o comportamento do `components/pos/produto.vue` do legado, sem Vuetify:
 * busca o produto em `estoque-produtos`, preenche preço/unidade automaticamente e
 * permite ajustar quantidade, valor unitário e desconto antes de adicionar à lista.
 *
 * Contrato:
 *   emits:
 *     'adicionar-item': [item: ItemVenda]
 * Método exposto:
 *   carregarParaEdicao(item)  — recarrega o formulário com um item removido da lista.
 */
import { ref, computed } from 'vue'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { useHelper } from '~/composables/useHelper'
import { useToast } from '~/composables/useToast'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'
import type { ItemVenda, ProdutoResumo } from './tipos'

const emit = defineEmits<{
  'adicionar-item': [item: ItemVenda]
}>()

const { formatarMoeda } = useHelper()
const toast = useToast()

interface ProdutoBusca {
  id: number
  codigo?: string
  descricao?: string
  unidadeComercial?: string
  valorUnitario?: number
  valorVenda?: number
}

const termo = ref('')
const resultados = ref<ProdutoBusca[]>([])
const buscando = ref(false)

const produtoSelecionado = ref<ProdutoResumo | null>(null)
const quantidade = ref<number>(1)
const valorUnitario = ref<number>(0)
const valorDesconto = ref<number>(0)

const totalItem = computed(() => {
  const base = quantidade.value * valorUnitario.value - valorDesconto.value
  return base > 0 ? Math.round((base + Number.EPSILON) * 100) / 100 : 0
})

const podeAdicionar = computed(
  () => !!produtoSelecionado.value && quantidade.value > 0 && valorUnitario.value > 0
)

async function buscarProduto() {
  const q = termo.value.trim()
  if (q.length < 2) {
    resultados.value = []
    return
  }
  buscando.value = true
  try {
    const resposta = await useApi('/estoque-produtos', {
      query: { termo: q, pagina: 1, tamanhoPagina: 20 }
    })
    resultados.value = extrairLista<ProdutoBusca>(resposta) ?? []
  } catch (e) {
    toast.error('Erro ao buscar produtos.')
    console.error('[ItemForm.buscarProduto]', e)
  } finally {
    buscando.value = false
  }
}

function selecionar(produto: ProdutoBusca) {
  produtoSelecionado.value = {
    id: produto.id,
    codigo: produto.codigo,
    descricao: produto.descricao,
    unidadeComercial: produto.unidadeComercial,
    valorUnitario: produto.valorUnitario ?? produto.valorVenda ?? 0
  }
  valorUnitario.value = produtoSelecionado.value.valorUnitario ?? 0
  quantidade.value = 1
  valorDesconto.value = 0
  resultados.value = []
  termo.value = ''
}

function adicionar() {
  if (!produtoSelecionado.value) return
  const p = produtoSelecionado.value
  emit('adicionar-item', {
    produtoId: p.id,
    produto: p,
    descricao: p.descricao || `Produto #${p.id}`,
    quantidadeComercial: quantidade.value,
    valorUnitarioComercial: valorUnitario.value,
    valorDesconto: valorDesconto.value
  })
  limpar()
}

function limpar() {
  produtoSelecionado.value = null
  quantidade.value = 1
  valorUnitario.value = 0
  valorDesconto.value = 0
}

/** Recarrega o formulário com um item retirado da lista para reedição. */
function carregarParaEdicao(item: ItemVenda) {
  produtoSelecionado.value = item.produto ?? {
    id: item.produtoId,
    descricao: item.descricao
  }
  quantidade.value = item.quantidadeComercial
  valorUnitario.value = item.valorUnitarioComercial
  valorDesconto.value = item.valorDesconto
}

defineExpose({ carregarParaEdicao, limpar })
</script>

<template>
  <div class="item-form glass-panel">
    <div class="if-header"><span class="if-title">Adicionar produto</span></div>

    <template v-if="!produtoSelecionado">
      <div class="if-busca">
        <TextField
          v-model="termo"
          label="Buscar produto"
          placeholder="Código ou descrição"
          @blur="buscarProduto"
        />
        <button type="button" class="btn btn-secondary btn-sm" :disabled="buscando" @click="buscarProduto">
          {{ buscando ? 'Buscando...' : 'Buscar' }}
        </button>
      </div>

      <ul v-if="resultados.length" class="if-resultados">
        <li v-for="p in resultados" :key="p.id" class="if-resultado" @click="selecionar(p)">
          <span class="if-resultado-desc">
            <strong>{{ p.codigo }}</strong> · {{ p.descricao }}
          </span>
          <span class="if-resultado-valor">
            {{ formatarMoeda(p.valorUnitario ?? p.valorVenda ?? 0) }}
          </span>
        </li>
      </ul>
    </template>

    <template v-else>
      <div class="if-selecionado">
        <div class="if-prod-nome">
          <strong>{{ produtoSelecionado.codigo }}</strong> · {{ produtoSelecionado.descricao }}
        </div>
        <button type="button" class="btn btn-ghost btn-sm" @click="limpar">Trocar</button>
      </div>

      <div class="if-grid">
        <QuantityInput
          v-model="quantidade"
          label="Quantidade"
          :min="0"
          :suffix="produtoSelecionado.unidadeComercial || 'UN'"
        />
        <MoneyInput v-model="valorUnitario" label="Valor unitário" />
        <MoneyInput v-model="valorDesconto" label="Desconto" />
      </div>

      <div class="if-total">
        <span>Total do item</span>
        <strong>{{ formatarMoeda(totalItem) }}</strong>
      </div>

      <button type="button" class="btn btn-primary" :disabled="!podeAdicionar" @click="adicionar">
        Adicionar item
      </button>
    </template>
  </div>
</template>

<style scoped>
.item-form { padding: 14px; display: flex; flex-direction: column; gap: 12px; }
.if-header { display: flex; }
.if-title { font-weight: 600; font-size: 14px; }
.if-busca { display: flex; align-items: flex-end; gap: 8px; }
.if-busca .field { flex: 1; }
.if-resultados {
  list-style: none;
  max-height: 220px;
  overflow-y: auto;
  border: 1px solid var(--border-color);
  border-radius: 8px;
}
.if-resultado {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  padding: 8px 12px;
  cursor: pointer;
  font-size: 13px;
  border-bottom: 1px solid var(--border-color);
}
.if-resultado:last-child { border-bottom: none; }
.if-resultado:hover { background: rgba(255, 255, 255, 0.05); }
.if-resultado-valor { color: var(--text-muted); white-space: nowrap; }
.if-selecionado {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding: 8px 12px;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.03);
  font-size: 13px;
}
.if-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
.if-total {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 14px;
  padding: 6px 2px;
}
.if-total strong { font-size: 16px; }
@media (max-width: 640px) {
  .if-grid { grid-template-columns: 1fr; }
}
</style>
