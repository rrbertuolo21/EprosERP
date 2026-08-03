<script setup lang="ts">
/**
 * Listagem de Análises de Aceitação/Rejeição (QLD-ACR) — Qualidade / Aceitação e Rejeição.
 *
 * Fonte: GET /qualidade/aceitacao-rejeicao (paginado, filtro por status).
 * Backend expõe apenas leitura + criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import {
  STATUS_REGISTRO,
  STATUS_REGISTRO_OPCOES_FILTRO,
  TIPO_ANALISE_ACR,
  rotuloEnum
} from '~/components/qualidade-shared/enums'

definePageMeta({ layout: 'default' })

interface AnaliseAcr {
  id: string
  codigo?: string | null
  descricao?: string | null
  tipoAnalise: number
  status: number
  criadoEm?: string | null
}

interface AcrFiltros {
  status?: string | null
}

const router = useRouter()

const lista = useApiList<AnaliseAcr, AcrFiltros>('/qualidade/aceitacao-rejeicao', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<AnaliseAcr>[] = [
  { key: 'codigo', label: 'Código', sortable: false, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: false },
  { key: 'tipoAnalise', label: 'Tipo de análise', sortable: false, width: '160px' },
  { key: 'status', label: 'Status', sortable: false, align: 'center', width: '120px' }
]

const camposFiltro: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: STATUS_REGISTRO_OPCOES_FILTRO }
]

function novaAnalise() {
  router.push('/erp/qualidade/aceitacao-rejeicao/novo')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Aceitação e Rejeição" subtitle="Análises de aceite/rejeição de materiais e lotes" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novaAnalise">+ Nova análise</button>
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
      empty-text="Nenhuma análise encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
    >
      <template #cell-tipoAnalise="{ value }">{{ rotuloEnum(TIPO_ANALISE_ACR, value) }}</template>
      <template #cell-status="{ value }">
        <span class="badge badge-neutral">{{ rotuloEnum(STATUS_REGISTRO, value) }}</span>
      </template>
    </DataTable>
  </div>
</template>
