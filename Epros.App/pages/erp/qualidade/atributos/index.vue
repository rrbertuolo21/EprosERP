<script setup lang="ts">
/**
 * Listagem de Atributos (QLD-ATR) — Qualidade / Atributos.
 *
 * Fonte: GET /qualidade/atributos (paginado, filtro por status).
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
  TIPO_ATRIBUTO,
  TIPO_DADO_ATRIBUTO,
  ESCOPO_ATRIBUTO,
  rotuloEnum
} from '~/components/qualidade-shared/enums'

definePageMeta({ layout: 'default' })

interface Atributo {
  id: string
  codigo?: string | null
  nomeInterno?: string | null
  rotulo?: string | null
  tipoAtributo: number
  tipoDado: number
  escopo: number
  obrigatorio: boolean
  sensivelLgpd: boolean
  status: number
  criadoEm?: string | null
}

interface AtributoFiltros {
  status?: string | null
}

const router = useRouter()

const lista = useApiList<Atributo, AtributoFiltros>('/qualidade/atributos', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Atributo>[] = [
  { key: 'codigo', label: 'Código', sortable: false, width: '130px' },
  { key: 'rotulo', label: 'Rótulo', sortable: false },
  { key: 'tipoAtributo', label: 'Tipo', sortable: false, width: '130px' },
  { key: 'tipoDado', label: 'Tipo de dado', sortable: false, width: '130px' },
  { key: 'escopo', label: 'Escopo', sortable: false, width: '120px' },
  { key: 'obrigatorio', label: 'Obrigatório', sortable: false, align: 'center', width: '110px' },
  { key: 'status', label: 'Status', sortable: false, align: 'center', width: '120px' }
]

const camposFiltro: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: STATUS_REGISTRO_OPCOES_FILTRO }
]

function novoAtributo() {
  router.push('/erp/qualidade/atributos/novo')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Atributos" subtitle="Definição de atributos de qualidade e características" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoAtributo">+ Novo atributo</button>
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
      empty-text="Nenhum atributo encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
    >
      <template #cell-tipoAtributo="{ value }">{{ rotuloEnum(TIPO_ATRIBUTO, value) }}</template>
      <template #cell-tipoDado="{ value }">{{ rotuloEnum(TIPO_DADO_ATRIBUTO, value) }}</template>
      <template #cell-escopo="{ value }">{{ rotuloEnum(ESCOPO_ATRIBUTO, value) }}</template>
      <template #cell-obrigatorio="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-neutral'">{{ value ? 'Sim' : 'Não' }}</span>
      </template>
      <template #cell-status="{ value }">
        <span class="badge badge-neutral">{{ rotuloEnum(STATUS_REGISTRO, value) }}</span>
      </template>
    </DataTable>
  </div>
</template>
