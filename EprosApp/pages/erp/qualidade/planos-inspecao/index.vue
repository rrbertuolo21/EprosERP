<script setup lang="ts">
/**
 * Listagem de Planos de Inspeção (QLD-INS) — Qualidade / Planos de Inspeção.
 *
 * Fonte: GET /qualidade/planos-inspecao (paginado, filtro por status).
 * Backend expõe apenas leitura + criação (sem detalhe/edição/exclusão).
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
  CONTEXTO_PLANO,
  rotuloEnum
} from '~/components/qualidade-shared/enums'

definePageMeta({ layout: 'default' })

interface PlanoInspecao {
  id: string
  codigo?: string | null
  descricao?: string | null
  contexto: number
  status: number
  dataInicioVigencia?: string | null
  criadoEm?: string | null
}

interface PlanoFiltros {
  status?: string | null
}

const router = useRouter()

const lista = useApiList<PlanoInspecao, PlanoFiltros>('/qualidade/planos-inspecao', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<PlanoInspecao>[] = [
  { key: 'codigo', label: 'Código', sortable: false, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: false },
  { key: 'contexto', label: 'Contexto', sortable: false, width: '150px' },
  { key: 'status', label: 'Status', sortable: false, align: 'center', width: '120px' }
]

const camposFiltro: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: STATUS_REGISTRO_OPCOES_FILTRO }
]

function novoPlano() {
  router.push('/erp/qualidade/planos-inspecao/novo')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Planos de Inspeção" subtitle="Planos de inspeção e amostragem por contexto" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoPlano">+ Novo plano</button>
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
      empty-text="Nenhum plano de inspeção encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
    >
      <template #cell-contexto="{ value }">{{ rotuloEnum(CONTEXTO_PLANO, value) }}</template>
      <template #cell-status="{ value }">
        <span class="badge badge-neutral">{{ rotuloEnum(STATUS_REGISTRO, value) }}</span>
      </template>
    </DataTable>
  </div>
</template>
