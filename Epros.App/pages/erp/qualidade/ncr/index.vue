<script setup lang="ts">
/**
 * Listagem de Não Conformidades (QLD-NCR) — Qualidade / NCR.
 *
 * Fonte: GET /qualidade/ncr (paginado, filtro por status). O backend não expõe
 * detalhe/edição/exclusão para esta raiz — a tela é de leitura + criação (＋ Nova).
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
  NCR_ORIGEM,
  NCR_PRIORIDADE,
  NCR_ETAPA,
  rotuloEnum
} from '~/components/qualidade-shared/enums'

definePageMeta({ layout: 'default' })

interface Ncr {
  id: string
  codigo?: string | null
  titulo?: string | null
  origemPrincipal: number
  prioridade: number
  statusRegistro: number
  etapaNcr: number
  dataOcorrencia?: string | null
  criadoEm?: string | null
}

interface NcrFiltros {
  status?: string | null
}

const router = useRouter()

const lista = useApiList<Ncr, NcrFiltros>('/qualidade/ncr', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Ncr>[] = [
  { key: 'codigo', label: 'Código', sortable: false, width: '130px' },
  { key: 'titulo', label: 'Título', sortable: false },
  { key: 'origemPrincipal', label: 'Origem', sortable: false, width: '150px' },
  { key: 'prioridade', label: 'Prioridade', sortable: false, width: '110px' },
  { key: 'etapaNcr', label: 'Etapa', sortable: false, width: '130px' },
  { key: 'statusRegistro', label: 'Status', sortable: false, align: 'center', width: '120px' }
]

const camposFiltro: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: STATUS_REGISTRO_OPCOES_FILTRO }
]

function novaNcr() {
  router.push('/erp/qualidade/ncr/novo')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Não Conformidades" subtitle="Gestão de NCRs — origem, prioridade e ciclo de vida" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novaNcr">+ Nova NCR</button>
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
      empty-text="Nenhuma não conformidade encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
    >
      <template #cell-origemPrincipal="{ value }">{{ rotuloEnum(NCR_ORIGEM, value) }}</template>
      <template #cell-prioridade="{ value }">{{ rotuloEnum(NCR_PRIORIDADE, value) }}</template>
      <template #cell-etapaNcr="{ value }">{{ rotuloEnum(NCR_ETAPA, value) }}</template>
      <template #cell-statusRegistro="{ value }">
        <span class="badge badge-neutral">{{ rotuloEnum(STATUS_REGISTRO, value) }}</span>
      </template>
    </DataTable>
  </div>
</template>
