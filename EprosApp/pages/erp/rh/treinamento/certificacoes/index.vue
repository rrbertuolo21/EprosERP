<script setup lang="ts">
/**
 * Certificações — RH / Treinamento (somente leitura).
 * Fonte: GET /rh/treinamento/certificacoes. A API não expõe POST/PUT/DELETE — lista apenas.
 */
import { onMounted } from 'vue'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Certificacao {
  id: string
  nome?: string | null
  colaboradorId?: string | null
  dataEmissao?: string | null
  dataValidade?: string | null
}
interface Filtros { busca?: string }

const lista = useApiList<Certificacao, Filtros>('/rh/treinamento/certificacoes', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Certificacao>[] = [
  { key: 'nome', label: 'Certificação', sortable: true },
  { key: 'dataEmissao', label: 'Emissão', sortable: false, align: 'center' },
  { key: 'dataValidade', label: 'Validade', sortable: false, align: 'center' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Certificação...', grow: true }
]
function formatarData(v?: string | null) {
  return v ? new Date(v).toLocaleDateString('pt-BR') : ''
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Certificações" subtitle="Certificações dos colaboradores (consulta)" :loading="lista.carregando.value" />

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
      empty-text="Nenhuma certificação encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataEmissao="{ value }"><span>{{ formatarData(value as string) }}</span></template>
      <template #cell-dataValidade="{ value }"><span>{{ formatarData(value as string) }}</span></template>
    </DataTable>
  </div>
</template>
