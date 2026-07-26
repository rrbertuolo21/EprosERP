<script setup lang="ts">
/**
 * Rubricas de folha — RH / Folha.
 * Fonte: GET/POST /rh/folha/rubricas. Lista + criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Rubrica {
  id: string
  codigo?: string | null
  nome?: string | null
  tipo?: string | null
  unidade?: string | null
  baseCalculo?: string | null
  taxa?: number | null
  ativo?: boolean
}
interface Filtros { busca?: string }

const router = useRouter()
const lista = useApiList<Rubrica, Filtros>('/rh/folha/rubricas', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Rubrica>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '110px' },
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'tipo', label: 'Tipo', sortable: false },
  { key: 'unidade', label: 'Unidade', sortable: false },
  { key: 'baseCalculo', label: 'Base de cálculo', sortable: false },
  { key: 'ativo', label: 'Status', sortable: false, align: 'center', width: '110px' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Código ou nome...', grow: true }
]
function novo() {
  router.push('/erp/rh/folha/rubricas/novo')
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Rubricas de folha" subtitle="Verbas e rubricas da folha de pagamento" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova rubrica</button>
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
      empty-text="Nenhuma rubrica encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-ativo="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-danger'">{{ value ? 'Ativo' : 'Inativo' }}</span>
      </template>
    </DataTable>
  </div>
</template>
