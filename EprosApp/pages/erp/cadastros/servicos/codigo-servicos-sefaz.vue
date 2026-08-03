<script setup lang="ts">
/**
 * Listagem de Códigos de Serviços SEFAZ (cadastros/servicos/codigo-servicos-sefaz).
 * Porta o comportamento de `cadastros/servicos/codigo-servicos-sefaz.vue` do legado:
 * tabela de consulta paginada com busca textual, somente leitura (sem CRUD no legado).
 */
import { onMounted, ref } from 'vue'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({
  middleware: 'auth',
  layout: 'default'
})

interface CodigoServicoSefaz {
  id: number
  codigo: string
  descricao: string
}

interface CodigoSefazFiltros extends Record<string, unknown> {
  localizar: string
}

const lista = useApiList<CodigoServicoSefaz, CodigoSefazFiltros>('/codigos-servicos-sefaz', {
  filtrosIniciais: { localizar: '' },
  tamanhoPaginaInicial: 25
})

const filtrosForm = ref<Record<string, unknown>>({ localizar: '' })

const camposFiltro: FilterField[] = [
  { key: 'localizar', label: 'Buscar', type: 'text', placeholder: 'Buscar códigos SEFAZ...', grow: true }
]

const colunas: DataTableColumn<CodigoServicoSefaz>[] = [
  { key: 'id', label: '#', sortable: true, width: '70px' },
  { key: 'codigo', label: 'Código', sortable: true },
  { key: 'descricao', label: 'Descrição', sortable: true }
]

let debounceTimer: ReturnType<typeof setTimeout> | undefined

function aoMudarFiltros(valores: Record<string, unknown>) {
  filtrosForm.value = valores
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    void lista.aplicarFiltros({ localizar: (valores.localizar as string) || '' } as Partial<CodigoSefazFiltros>)
  }, 500)
}

function aoBuscar(valores: Record<string, unknown>) {
  if (debounceTimer) clearTimeout(debounceTimer)
  void lista.aplicarFiltros({ localizar: (valores.localizar as string) || '' } as Partial<CodigoSefazFiltros>)
}

function aoLimpar() {
  filtrosForm.value = { localizar: '' }
  void lista.limpar()
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Códigos de Serviços SEFAZ" subtitle="Consulta de códigos NBS/SEFAZ" :loading="lista.carregando.value" />

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
      empty-text="Nenhum código SEFAZ encontrado"
      @update:page="(p) => lista.irParaPagina(p)"
      @update:page-size="(ps) => lista.buscar({ tamanhoPagina: ps, pagina: 1 })"
      @update:sort="(s) => lista.buscar({ ordenacao: s })"
    />
  </div>
</template>
