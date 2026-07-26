<script setup lang="ts">
/**
 * Metas — RH / Talentos.
 * Fonte: GET/POST /rh/talentos/metas. Lista + criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Meta {
  id: string
  titulo?: string | null
  dataInicio?: string | null
  dataFim?: string | null
  alvo?: number | null
  progresso?: number | null
}
interface Filtros { busca?: string }

const router = useRouter()
const lista = useApiList<Meta, Filtros>('/rh/talentos/metas', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Meta>[] = [
  { key: 'titulo', label: 'Título', sortable: true },
  { key: 'dataInicio', label: 'Início', sortable: false, align: 'center' },
  { key: 'dataFim', label: 'Fim', sortable: false, align: 'center' },
  { key: 'alvo', label: 'Alvo', sortable: false, align: 'right' },
  { key: 'progresso', label: 'Progresso', sortable: false, align: 'right' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Título...', grow: true }
]
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}
function novo() {
  router.push('/erp/rh/talentos/metas/novo')
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Metas" subtitle="Metas e objetivos dos colaboradores" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova meta</button>
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
      empty-text="Nenhuma meta encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataInicio="{ value }"><span>{{ formatarData(value as string) }}</span></template>
      <template #cell-dataFim="{ value }"><span>{{ formatarData(value as string) }}</span></template>
    </DataTable>
  </div>
</template>
