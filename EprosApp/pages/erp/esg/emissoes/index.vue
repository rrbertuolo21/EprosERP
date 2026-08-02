<script setup lang="ts">
/**
 * Listagem de Emissões de Carbono — ESG / Emissões.
 *
 * Contrato real (ESGController): `GET /esg/emissoes` (lista) + `POST /esg/emissoes` (criar).
 * Não há GET por id, PUT ou DELETE — a tela é lista + botão "Nova emissão" (create-only).
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Emissao {
  id: string
  fonteEmissao?: string | null
  escopo?: number | null
  categoriaGhg?: string | null
  quantidadeConsumo?: number | null
  unidadeMedida?: string | null
  fatorEmissao?: number | null
  totalCo2e?: number | null
  dataTransacao?: string | null
}

interface EmissaoFiltros {
  busca?: string
}

const router = useRouter()

const lista = useApiList<Emissao, EmissaoFiltros>('/esg/emissoes', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Emissao>[] = [
  { key: 'fonteEmissao', label: 'Fonte de emissão', sortable: true },
  { key: 'escopo', label: 'Escopo', sortable: true, align: 'center', width: '90px' },
  { key: 'categoriaGhg', label: 'Categoria GHG', sortable: true },
  { key: 'quantidadeConsumo', label: 'Consumo', sortable: false, align: 'right' },
  { key: 'unidadeMedida', label: 'Unidade', sortable: false, width: '100px' },
  { key: 'totalCo2e', label: 'Total CO₂e', sortable: false, align: 'right' },
  { key: 'dataTransacao', label: 'Data', sortable: true, width: '120px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Fonte ou categoria...', grow: true }
]

function novo() {
  router.push('/erp/esg/emissoes/novo')
}

function formatarData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleDateString('pt-BR')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Emissões de Carbono" subtitle="Lançamentos de emissões (Protocolo GHG) por fonte e escopo" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova emissão</button>
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
      empty-text="Nenhuma emissão lançada. Adicione uma nova emissão para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-escopo="{ value }">
        <span v-if="value != null">{{ value }}</span>
      </template>
      <template #cell-quantidadeConsumo="{ value }">
        <span v-if="value != null">{{ Number(value).toLocaleString('pt-BR') }}</span>
      </template>
      <template #cell-totalCo2e="{ value }">
        <span v-if="value != null">{{ Number(value).toLocaleString('pt-BR') }}</span>
      </template>
      <template #cell-dataTransacao="{ value }">{{ formatarData(value) }}</template>
    </DataTable>
  </div>
</template>
