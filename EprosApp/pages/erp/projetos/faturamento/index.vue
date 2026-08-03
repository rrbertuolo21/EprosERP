<script setup lang="ts">
/**
 * Listagem de Faturamentos de projeto — PROJETOS / Faturamento.
 * GET /projetos/faturamento (paginado por status) · POST via formulário.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import {
  STATUS_WORKFLOW_OPCOES, rotuloStatusWorkflow, fmtMoeda,
  MODALIDADE_FATURAMENTO_OPCOES
} from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface Faturamento {
  id: string; codigo?: string | null; descricao?: string | null; status?: number | null
  projetoId?: string | null; modalidadeFaturamento?: number | null; valorTotal?: number | null
  moeda?: string | null
}
interface Filtros { status?: string }

const router = useRouter()
const lista = useApiList<Faturamento, Filtros>('/projetos/faturamento', {
  filtrosIniciais: { status: '' }, tamanhoPaginaInicial: 20
})

function rotuloModalidade(v: unknown): string {
  const f = MODALIDADE_FATURAMENTO_OPCOES.find((o) => String(o.value) === String(v))
  return f ? f.label : '—'
}

const colunas: DataTableColumn<Faturamento>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '150px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'modalidadeFaturamento', label: 'Modalidade', sortable: true, width: '150px' },
  { key: 'valorTotal', label: 'Valor total', sortable: true, align: 'right', width: '140px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]
const camposFiltro: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: STATUS_WORKFLOW_OPCOES }
]

function novo() { router.push('/erp/projetos/faturamento/novo') }
function abrir(item: Faturamento) { router.push(`/erp/projetos/faturamento/${item.id}`) }
onMounted(() => { void lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar title="Faturamentos" subtitle="Faturamento de projetos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo faturamento</button>
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
      empty-text="Nenhum faturamento encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrir"
    >
      <template #cell-modalidadeFaturamento="{ value }">{{ rotuloModalidade(value) }}</template>
      <template #cell-valorTotal="{ value }">{{ fmtMoeda(value) }}</template>
      <template #cell-status="{ value }"><span class="badge badge-cancelada">{{ rotuloStatusWorkflow(value) }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Abrir</button>
      </template>
    </DataTable>
  </div>
</template>
