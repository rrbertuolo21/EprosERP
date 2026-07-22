<script setup lang="ts">
/**
 * Listagem de Produtos (erp/estoque/produtos).
 * Consome o catálogo de produtos do módulo Estoque — `GET /estoque/produtos`
 * (EstoqueController, `ListarProdutosQuery`): Guid, Sku/Nome/PrecoVenda/SaldoEstoque/CustoMedio.
 *
 * Endpoint: estoque/produtos (catálogo de produto — CRUD completo).
 */
import { onMounted, ref } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

interface ProdutoListagem {
  id: string
  sku: string | null
  nome: string
  precoVenda: number | null
  saldoEstoque: number | null
  custoMedio: number | null
  ativo: boolean
}

interface ProdutoFiltros extends Record<string, unknown> {
  localizar: string
  ativo: boolean
}

const toast = useToast()
const { formatarMoeda, formatarNumero } = useHelper()

const lista = useApiList<ProdutoListagem, ProdutoFiltros>('/estoque/produtos', {
  filtrosIniciais: { localizar: '', ativo: true },
  tamanhoPaginaInicial: 25
})

const filtrosForm = ref<Record<string, unknown>>({ localizar: '', ativo: true })

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'SKU ou nome do produto', grow: true },
  { key: 'ativo', label: 'Ativo', type: 'boolean', placeholder: 'Somente ativos' }
]

const colunas: DataTableColumn<ProdutoListagem>[] = [
  { key: 'sku', label: 'SKU', sortable: true, width: '140px' },
  { key: 'nome', label: 'Nome', sortable: true },
  {
    key: 'precoVenda',
    label: 'Preço de Venda',
    sortable: true,
    align: 'right',
    formatter: (v) => formatarMoeda(v as number | null)
  },
  {
    key: 'saldoEstoque',
    label: 'Saldo',
    align: 'right',
    formatter: (v) => formatarNumero(v as number | null, 0, 4)
  },
  {
    key: 'custoMedio',
    label: 'Custo Médio',
    align: 'right',
    formatter: (v) => formatarMoeda(v as number | null)
  }
]

let debounceTimer: ReturnType<typeof setTimeout> | undefined

function aoMudarFiltros(valores: Record<string, unknown>) {
  filtrosForm.value = valores
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => void lista.aplicarFiltros(normalizarFiltros(valores)), 500)
}

function aoBuscar(valores: Record<string, unknown>) {
  if (debounceTimer) clearTimeout(debounceTimer)
  void lista.aplicarFiltros(normalizarFiltros(valores))
}

function normalizarFiltros(valores: Record<string, unknown>): Partial<ProdutoFiltros> {
  return {
    localizar: (valores.localizar as string) || '',
    ativo: valores.ativo !== undefined ? Boolean(valores.ativo) : true
  }
}

function aoLimpar() {
  filtrosForm.value = { localizar: '', ativo: true }
  void lista.limpar()
}

function criarNovo() {
  navigateTo('/erp/estoque/produtos/novo')
}

function editar(item: ProdutoListagem) {
  navigateTo(`/erp/estoque/produtos/${item.id}`)
}

// --- Exclusão ---
const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<ProdutoListagem | null>(null)

function pedirExclusao(item: ProdutoListagem) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi('/estoque/produtos/{id}', { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Produto excluído com sucesso')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Produtos" subtitle="Catálogo de produtos e saldo de estoque" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="criarNovo">+ Novo</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="filtrosForm"
      :loading="lista.carregando.value"
      @update:model-value="aoMudarFiltros"
      @search="aoBuscar"
      @clear="aoLimpar"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      row-key="id"
      empty-text="Nenhum produto encontrado"
      @update:page="(p) => lista.irParaPagina(p)"
      @update:page-size="(ps) => lista.buscar({ tamanhoPagina: ps, pagina: 1 })"
      @update:sort="(s) => lista.buscar({ ordenacao: s })"
      @row-click="editar"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">✎</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Excluir" @click.stop="pedirExclusao(row)">🗑</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.nome ?? ''"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
