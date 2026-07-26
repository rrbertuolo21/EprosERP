<script setup lang="ts">
/**
 * Listagem de Licenças Ambientais — ESG / EHS / Licenças.
 * Contrato real (EsgEhsController): `GET /esg/ehs/licencas` + `POST /esg/ehs/licencas`.
 * Sem GET por id / PUT / DELETE — lista + "Nova licença".
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import { statusEsgOptions, formatarStatusEsg } from '~/components/esg-comum/statusEsg'

definePageMeta({ layout: 'default' })

interface Licenca {
  id: string
  tipo?: string | null
  numero?: string | null
  autoridade?: string | null
  dataEmissao?: string | null
  dataValidade?: string | null
  status?: number | string | null
}

interface LicencaFiltros {
  busca?: string
  status?: number | null
}

const router = useRouter()

const lista = useApiList<Licenca, LicencaFiltros>('/esg/ehs/licencas', {
  filtrosIniciais: { busca: '', status: null },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Licenca>[] = [
  { key: 'tipo', label: 'Tipo', sortable: true, width: '150px' },
  { key: 'numero', label: 'Número', sortable: true, width: '150px' },
  { key: 'autoridade', label: 'Autoridade', sortable: true },
  { key: 'dataEmissao', label: 'Emissão', sortable: true, width: '120px' },
  { key: 'dataValidade', label: 'Validade', sortable: true, width: '120px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '120px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Tipo, número ou autoridade...', grow: true },
  { key: 'status', label: 'Status', type: 'select', options: statusEsgOptions }
]

function formatarData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleDateString('pt-BR')
}

function novo() {
  router.push('/erp/esg/ehs/licencas/novo')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Licenças Ambientais" subtitle="Licenças e condicionantes ambientais com prazos de validade" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova licença</button>
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
      empty-text="Nenhuma licença cadastrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataEmissao="{ value }">{{ formatarData(value) }}</template>
      <template #cell-dataValidade="{ value }">{{ formatarData(value) }}</template>
      <template #cell-status="{ value }">
        <span class="badge">{{ formatarStatusEsg(value) }}</span>
      </template>
    </DataTable>
  </div>
</template>
