<script setup lang="ts">
/**
 * Marcações de ponto — RH / Ponto.
 * Fonte: GET/POST /rh/ponto/marcacoes. Lista + criação (marcação manual).
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Marcacao {
  id: string
  nsr?: number | null
  dataMarcacao?: string | null
  horaMarcacao?: string | null
  tipoMarcacao?: string | null
  origem?: string | null
}
interface Filtros { busca?: string }

const router = useRouter()
const lista = useApiList<Marcacao, Filtros>('/rh/ponto/marcacoes', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Marcacao>[] = [
  { key: 'nsr', label: 'NSR', sortable: false, width: '90px' },
  { key: 'dataMarcacao', label: 'Data', sortable: true, align: 'center' },
  { key: 'horaMarcacao', label: 'Hora', sortable: false, align: 'center' },
  { key: 'tipoMarcacao', label: 'Tipo', sortable: false },
  { key: 'origem', label: 'Origem', sortable: false }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'NSR ou tipo...', grow: true }
]
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}
function novo() {
  router.push('/erp/rh/ponto/marcacoes/novo')
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Marcações de ponto" subtitle="Registros de ponto dos colaboradores" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova marcação</button>
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
      row-key="id"
      empty-text="Nenhuma marcação encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataMarcacao="{ value }"><span>{{ formatarData(value as string) }}</span></template>
    </DataTable>
  </div>
</template>
