<script setup lang="ts">
/**
 * Listagem de NCM Tributação — Fiscal / NCM Tributação.
 *
 * Porta o comportamento de `fiscal/ncm-tributacao/index.vue` do legado: busca por código de
 * regra/descrição, tabela paginada server-side, editar/excluir.
 * Endpoint: `fiscal/ncm-tributacoes`.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import DeleteAlert from '~/components/shared/DeleteAlert.vue'

definePageMeta({ layout: 'default' })

interface NcmTributacao {
  id: string
  codRegra: number
  descricao: string
  cfopNotaFiscal: number
  origem: number
}

interface NcmTributacaoFiltros {
  localizar?: string
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<NcmTributacao, NcmTributacaoFiltros>('/fiscal/ncm-tributacoes', {
  filtrosIniciais: { localizar: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<NcmTributacao>[] = [
  { key: 'codRegra', label: 'Código Regra', sortable: true, width: '130px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'cfopNotaFiscal', label: 'CFOP NF-e Saída', sortable: false, width: '150px' }
]

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Código ou descrição...', grow: true }
]

async function buscarPagina() {
  await lista.buscar()
}

function novaTributacao() {
  router.push('/erp/fiscal/ncm-tributacao/novo')
}

function editarTributacao(item: NcmTributacao) {
  router.push(`/erp/fiscal/ncm-tributacao/${item.id}`)
}

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<NcmTributacao | null>(null)

function pedirExclusao(item: NcmTributacao) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}

async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi(`/fiscal/ncm-tributacoes/${itemParaExcluir.value.id}`, { method: 'DELETE' })
    toast.success('Regra de tributação excluída com sucesso.')
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
  void buscarPagina()
})
</script>

<template>
  <div>
    <PageToolbar title="NCM Tributação" subtitle="Regras de tributação vinculadas a NCM e grupo tributário" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novaTributacao">+ Nova regra</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event as Partial<typeof lista.filtros.value>)"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma regra de tributação cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editarTributacao"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="editarTributacao(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.descricao"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
