<script setup lang="ts">
/**
 * Listagem de Produtos (cadastros/produtos).
 * Porta o comportamento de `cadastros/produto/item/index.vue` do legado, consumindo o
 * catálogo do módulo Estoque — `GET /estoque/produtos` (EstoqueController,
 * `ListarProdutosQuery`): Guid, campos Sku/Nome/PrecoVenda/SaldoEstoque/CustoMedio.
 * Filtro único (`localizar`) casado com o backend (busca por Sku ou Nome).
 *
 * Endpoint principal: estoque/produtos (catálogo).
 */
import { computed, onMounted, ref } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'

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
  ativo: boolean
}

interface ProdutoFiltros extends Record<string, unknown> {
  localizar: string
  ativo: boolean
}

const toast = useToast()
const { formatarMoeda } = useHelper()

const lista = useApiList<ProdutoListagem, ProdutoFiltros>('/estoque/produtos', {
  filtrosIniciais: {
    localizar: '',
    ativo: true
  },
  tamanhoPaginaInicial: 20
})

const filtrosForm = ref<Record<string, unknown>>({
  localizar: '',
  ativo: true
})

const camposFiltro = computed<FilterField[]>(() => [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'SKU ou nome do produto', grow: true },
  { key: 'ativo', label: 'Ativo', type: 'boolean', placeholder: 'Somente ativos' }
])

const colunas: DataTableColumn<ProdutoListagem>[] = [
  { key: 'sku', label: 'SKU', sortable: true, width: '140px' },
  { key: 'nome', label: 'Nome', sortable: true },
  {
    key: 'precoVenda',
    label: 'Valor Venda',
    sortable: true,
    align: 'right',
    formatter: (v) => formatarMoeda(v as number | null)
  },
  {
    key: 'saldoEstoque',
    label: 'Estoque',
    sortable: false,
    align: 'right',
    formatter: (v) => (v == null ? '-' : String(v))
  },
  { key: 'ativo', label: 'Situação', sortable: true, align: 'center' }
]

let debounceTimer: ReturnType<typeof setTimeout> | undefined

function aoMudarFiltros(valores: Record<string, unknown>) {
  filtrosForm.value = valores
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    void lista.aplicarFiltros(normalizarFiltros(valores))
  }, 500)
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
  filtrosForm.value = {
    localizar: '',
    ativo: true
  }
  void lista.limpar()
}

function criarNovo() {
  navigateTo('/erp/cadastros/produtos/novo')
}

function editar(item: ProdutoListagem) {
  navigateTo(`/erp/cadastros/produtos/${item.id}`)
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
    await useApi(`/estoque/produtos/{id}`, { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
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

const tituloExclusao = computed(() => itemParaExcluir.value?.nome ?? '')

// --- Duplicar produto ---
// Sem endpoint de duplicação no backend novo (`estoque/produtos` não expõe `/duplicar`).
// Stub honesto: dialogo mantém a UX mas a ação fica desabilitada até o endpoint existir.
const duplicarVisivel = ref(false)
const produtoParaDuplicar = ref<ProdutoListagem | null>(null)
const descricaoDuplicada = ref('')

function abrirDuplicar(item: ProdutoListagem) {
  produtoParaDuplicar.value = item
  descricaoDuplicada.value = `${item.nome} - Cópia`
  duplicarVisivel.value = true
}

function fecharDuplicar() {
  duplicarVisivel.value = false
  produtoParaDuplicar.value = null
  descricaoDuplicada.value = ''
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Produtos" subtitle="Cadastro de produtos e itens de estoque" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="criarNovo">+ Novo Produto</button>
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
      empty-text="Nenhum produto encontrado"
      @update:page="(p) => lista.irParaPagina(p)"
      @update:page-size="(ps) => lista.buscar({ tamanhoPagina: ps, pagina: 1 })"
      @update:sort="(s) => lista.buscar({ ordenacao: s })"
      @row-click="editar"
    >
      <template #cell-ativo="{ row }">
        <span class="badge" :class="row.ativo ? 'badge-success' : 'badge-danger'">
          {{ row.ativo ? 'Ativo' : 'Inativo' }}
        </span>
      </template>

      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Duplicar" @click.stop="abrirDuplicar(row)">⧉</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">✎</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Excluir" @click.stop="pedirExclusao(row)">🗑</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="tituloExclusao"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />

    <AppDialog v-model="duplicarVisivel" title="Duplicar Produto" width="480px">
      <div class="form-grid">
        <div class="col-12">
          <TextField v-model="descricaoDuplicada" label="Nome do novo produto" required placeholder="Descrição do produto duplicado" />
        </div>
        <div class="col-12 aviso-em-implementacao">
          Duplicação de produto em implementação — o backend ainda não expõe este endpoint.
        </div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="fecharDuplicar">Cancelar</button>
        <button type="button" class="btn btn-primary" disabled title="Em implementação">
          Duplicar
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.aviso-em-implementacao {
  font-size: 12px;
  color: var(--text-secondary);
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid var(--border-color);
  border-radius: 6px;
  padding: 8px 10px;
}
</style>
