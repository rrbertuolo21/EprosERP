<script setup lang="ts">
/**
 * Listagem de Serviços (cadastros/servicos).
 * Porta o comportamento de `cadastros/servicos/index.vue` do legado:
 * busca textual (debounce), tabela paginada server-side, editar/excluir por linha.
 */
import { computed, onMounted, ref } from 'vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

interface Servico {
  id: number
  codigo: string
  descricao: string
  cnae: number | null
  servicoAtivo: boolean
}

interface ServicoFiltros extends Record<string, unknown> {
  localizar: string
}

const toast = useToast()

const lista = useApiList<Servico, ServicoFiltros>('/servicos', {
  filtrosIniciais: { localizar: '' },
  tamanhoPaginaInicial: 25
})

const filtrosForm = ref<Record<string, unknown>>({ localizar: '' })

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Buscar serviços...', grow: true }
]

const colunas: DataTableColumn<Servico>[] = [
  { key: 'id', label: '#', sortable: true, width: '70px' },
  { key: 'codigo', label: 'Código', sortable: true },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'cnae', label: 'CNAE', sortable: true },
  { key: 'servicoAtivo', label: 'Ativo', sortable: true, align: 'center' }
]

let debounceTimer: ReturnType<typeof setTimeout> | undefined

function aoMudarFiltros(valores: Record<string, unknown>) {
  filtrosForm.value = valores
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    void lista.aplicarFiltros({ localizar: (valores.localizar as string) || '' } as Partial<ServicoFiltros>)
  }, 500)
}

function aoBuscar(valores: Record<string, unknown>) {
  if (debounceTimer) clearTimeout(debounceTimer)
  void lista.aplicarFiltros({ localizar: (valores.localizar as string) || '' } as Partial<ServicoFiltros>)
}

function aoLimpar() {
  filtrosForm.value = { localizar: '' }
  void lista.limpar()
}

function criarNovo() {
  navigateTo('/erp/cadastros/servicos/novo')
}

function editar(item: Servico) {
  navigateTo(`/erp/cadastros/servicos/${item.id}`)
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<Servico | null>(null)

function pedirExclusao(item: Servico) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/servicos/{id}`, { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Serviço excluído com sucesso')
    excluirVisivel.value = false
    itemParaExcluir.value = null
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    excluindo.value = false
  }
}

const tituloExclusao = computed(() => itemParaExcluir.value?.descricao ?? '')

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Serviços" subtitle="Cadastro de serviços prestados" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="criarNovo">+ Novo Serviço</button>
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
      empty-text="Nenhum serviço encontrado"
      @update:page="(p) => lista.irParaPagina(p)"
      @update:page-size="(ps) => lista.buscar({ tamanhoPagina: ps, pagina: 1 })"
      @update:sort="(s) => lista.buscar({ ordenacao: s })"
      @row-click="editar"
    >
      <template #cell-servicoAtivo="{ row }">
        <span class="badge" :class="row.servicoAtivo ? 'badge-success' : 'badge-danger'">
          {{ row.servicoAtivo ? 'Ativo' : 'Inativo' }}
        </span>
      </template>

      <template #actions="{ row }">
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
  </div>
</template>
