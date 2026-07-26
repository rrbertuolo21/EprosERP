<script setup lang="ts">
/**
 * Feriados — RH / Planejamento.
 * Fonte: GET/POST /rh/planejamento/feriados. Lista + criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Feriado {
  id: string
  nome?: string | null
  dataInicio?: string | null
  dataFim?: string | null
  remunerado?: boolean | null
  descricao?: string | null
}
interface Filtros { busca?: string }

const router = useRouter()
const lista = useApiList<Feriado, Filtros>('/rh/planejamento/feriados', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Feriado>[] = [
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'dataInicio', label: 'Início', sortable: true, align: 'center' },
  { key: 'dataFim', label: 'Fim', sortable: false, align: 'center' },
  { key: 'remunerado', label: 'Remunerado', sortable: false, align: 'center', width: '120px' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Nome...', grow: true }
]
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}
function novo() {
  router.push('/erp/rh/planejamento/feriados/novo')
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Feriados" subtitle="Calendário de feriados" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo feriado</button>
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
      empty-text="Nenhum feriado encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataInicio="{ value }"><span>{{ formatarData(value as string) }}</span></template>
      <template #cell-dataFim="{ value }"><span>{{ formatarData(value as string) }}</span></template>
      <template #cell-remunerado="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-danger'">{{ value ? 'Sim' : 'Não' }}</span>
      </template>
    </DataTable>
  </div>
</template>
