<script setup lang="ts">
/**
 * Listagem de Devoluções (Economia Circular) — ESG / ECO / Devoluções.
 * Contrato real (EsgEcoController): `GET /esg/eco/devolucoes` + `POST /esg/eco/devolucoes`.
 * Sem GET por id / PUT / DELETE — lista + "Nova devolução".
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Devolucao {
  id: string
  numeroNf?: string | null
  chaveNfEntrada?: string | null
  tipo?: string | null
  estado?: string | null
  valorIntegral?: number | null
  valorDevolvido?: number | null
  devolucaoParcial?: boolean | null
}

interface DevolucaoFiltros {
  busca?: string
}

const router = useRouter()

const lista = useApiList<Devolucao, DevolucaoFiltros>('/esg/eco/devolucoes', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Devolucao>[] = [
  { key: 'numeroNf', label: 'Número NF', sortable: true, width: '140px' },
  { key: 'chaveNfEntrada', label: 'Chave NF entrada', sortable: false },
  { key: 'tipo', label: 'Tipo', sortable: true, width: '120px' },
  { key: 'estado', label: 'Estado', sortable: true, width: '120px' },
  { key: 'valorIntegral', label: 'Valor integral', sortable: false, align: 'right' },
  { key: 'valorDevolvido', label: 'Valor devolvido', sortable: false, align: 'right' },
  { key: 'devolucaoParcial', label: 'Parcial', sortable: false, align: 'center', width: '90px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'NF, tipo ou estado...', grow: true }
]

function moeda(v: unknown): string {
  if (v == null) return ''
  return Number(v).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

function novo() {
  router.push('/erp/esg/eco/devolucoes/novo')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Devoluções" subtitle="Devoluções de mercadorias para tratamento em economia circular" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova devolução</button>
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
      empty-text="Nenhuma devolução registrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-valorIntegral="{ value }">{{ moeda(value) }}</template>
      <template #cell-valorDevolvido="{ value }">{{ moeda(value) }}</template>
      <template #cell-devolucaoParcial="{ value }">
        <span class="badge" :class="value ? 'badge-warning' : 'badge-success'">{{ value ? 'Parcial' : 'Total' }}</span>
      </template>
    </DataTable>
  </div>
</template>
