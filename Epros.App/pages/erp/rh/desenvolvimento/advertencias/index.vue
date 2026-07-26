<script setup lang="ts">
/**
 * Advertências — RH / Desenvolvimento.
 * Fonte: GET/POST /rh/desenvolvimento/advertencias. Lista + criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Advertencia {
  id: string
  assunto?: string | null
  severidade?: string | null
  dataAdvertencia?: string | null
  descricao?: string | null
}
interface Filtros { busca?: string }

const router = useRouter()
const lista = useApiList<Advertencia, Filtros>('/rh/desenvolvimento/advertencias', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Advertencia>[] = [
  { key: 'assunto', label: 'Assunto', sortable: true },
  { key: 'severidade', label: 'Severidade', sortable: false, align: 'center' },
  { key: 'dataAdvertencia', label: 'Data', sortable: true, align: 'center' },
  { key: 'descricao', label: 'Descrição', sortable: false }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Assunto...', grow: true }
]
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}
function novo() {
  router.push('/erp/rh/desenvolvimento/advertencias/novo')
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Advertências" subtitle="Advertências disciplinares" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova advertência</button>
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
      empty-text="Nenhuma advertência encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataAdvertencia="{ value }">
        <span>{{ formatarData(value as string) }}</span>
      </template>
    </DataTable>
  </div>
</template>
