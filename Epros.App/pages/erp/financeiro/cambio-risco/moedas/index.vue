<script setup lang="ts">
/**
 * Listagem de Moedas — Câmbio/Risco.
 * CRUD completo: GET /cambio-risco/moedas, POST, PUT/{id}, DELETE/{id}.
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

interface Moeda {
  id: string
  codigoIso?: string | null
  simbolo?: string | null
  nome?: string | null
}
interface MoedaFiltros {
  busca?: string
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Moeda, MoedaFiltros>('/cambio-risco/moedas', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Moeda>[] = [
  { key: 'codigoIso', label: 'Código ISO', sortable: true, width: '140px' },
  { key: 'simbolo', label: 'Símbolo', sortable: true, width: '120px' },
  { key: 'nome', label: 'Nome', sortable: true }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Código, símbolo ou nome...', grow: true }
]

const excluirVisivel = ref(false)
const excluindo = ref(false)
const itemParaExcluir = ref<Moeda | null>(null)

function novo() {
  router.push('/erp/financeiro/cambio-risco/moedas/novo')
}
function editar(item: Moeda) {
  router.push(`/erp/financeiro/cambio-risco/moedas/${item.id}`)
}
function pedirExclusao(item: Moeda) {
  itemParaExcluir.value = item
  excluirVisivel.value = true
}
async function confirmarExclusao() {
  if (!itemParaExcluir.value) return
  excluindo.value = true
  try {
    await useApi('/cambio-risco/moedas/{id}', { method: 'DELETE', params: { id: itemParaExcluir.value.id } })
    toast.success('Moeda excluída com sucesso.')
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
    <PageToolbar title="Moedas" subtitle="Moedas para operações de câmbio e risco" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova moeda</button>
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
      empty-text="Nenhuma moeda cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editar"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Editar" @click.stop="editar(row)">Editar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Excluir" @click.stop="pedirExclusao(row)">Excluir</button>
      </template>
    </DataTable>

    <DeleteAlert
      v-model="excluirVisivel"
      :item-label="itemParaExcluir?.nome || itemParaExcluir?.codigoIso || undefined"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>
