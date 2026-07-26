<script setup lang="ts">
/**
 * Listagem de Fatores de Emissão GEE — ESG / GHG / Fatores.
 *
 * Contrato real (EsgGhgController): `GET /esg/ghg/fatores` + `POST /esg/ghg/fatores`.
 * Sem GET por id / PUT / DELETE — lista + "Novo fator".
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Fator {
  id: string
  codigo?: string | null
  versao?: string | null
  fonteReferencia?: string | null
  valor?: number | null
  unidadeEntrada?: string | null
  unidadeSaida?: string | null
  inicioVigencia?: string | null
  fimVigencia?: string | null
}

interface FatorFiltros {
  busca?: string
}

const router = useRouter()

const lista = useApiList<Fator, FatorFiltros>('/esg/ghg/fatores', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Fator>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '140px' },
  { key: 'versao', label: 'Versão', sortable: true, width: '90px' },
  { key: 'fonteReferencia', label: 'Fonte de referência', sortable: true },
  { key: 'valor', label: 'Valor', sortable: false, align: 'right' },
  { key: 'unidadeEntrada', label: 'Un. entrada', sortable: false, width: '110px' },
  { key: 'unidadeSaida', label: 'Un. saída', sortable: false, width: '110px' },
  { key: 'inicioVigencia', label: 'Início vigência', sortable: true, width: '130px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Código ou fonte...', grow: true }
]

function novo() {
  router.push('/erp/esg/ghg/fatores/novo')
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
    <PageToolbar title="Fatores de Emissão GEE" subtitle="Fatores de conversão por versão e vigência (Protocolo GHG)" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo fator</button>
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
      empty-text="Nenhum fator cadastrado. Adicione um novo fator para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-valor="{ value }">
        <span v-if="value != null">{{ Number(value).toLocaleString('pt-BR', { maximumFractionDigits: 6 }) }}</span>
      </template>
      <template #cell-inicioVigencia="{ value }">{{ formatarData(value) }}</template>
    </DataTable>
  </div>
</template>
