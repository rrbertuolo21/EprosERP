<script setup lang="ts">
/**
 * Turnos — RH / Planejamento.
 * Fonte: GET/POST /rh/planejamento/turnos. Lista + criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Turno {
  id: string
  nome?: string | null
  horaInicio?: string | null
  horaFim?: string | null
  turnoNoturno?: boolean | null
}
interface Filtros { busca?: string }

const router = useRouter()
const lista = useApiList<Turno, Filtros>('/rh/planejamento/turnos', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Turno>[] = [
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'horaInicio', label: 'Início', sortable: false, align: 'center' },
  { key: 'horaFim', label: 'Fim', sortable: false, align: 'center' },
  { key: 'turnoNoturno', label: 'Noturno', sortable: false, align: 'center', width: '110px' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Nome...', grow: true }
]
function novo() {
  router.push('/erp/rh/planejamento/turnos/novo')
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Turnos" subtitle="Jornadas e turnos de trabalho" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo turno</button>
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
      empty-text="Nenhum turno encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-turnoNoturno="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-danger'">{{ value ? 'Sim' : 'Não' }}</span>
      </template>
    </DataTable>
  </div>
</template>
