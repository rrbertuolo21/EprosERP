<script setup lang="ts">
/**
 * Listagem de Portfólios — PROJETOS / Portfólio e Priorização.
 * GET /projetos/portfolio (paginado por status) · POST via formulário.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import { STATUS_WORKFLOW_OPCOES, rotuloStatusWorkflow } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface Portfolio {
  id: string
  codigo?: string | null
  descricao?: string | null
  status?: number | null
  responsavelId?: string | null
  tipoPortfolio?: string | null
  scoreTotal?: number | null
  versao?: number | null
}
interface Filtros { status?: string }

const router = useRouter()
const lista = useApiList<Portfolio, Filtros>('/projetos/portfolio', {
  filtrosIniciais: { status: '' },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Portfolio>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'tipoPortfolio', label: 'Tipo', sortable: true, width: '140px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' },
  { key: 'scoreTotal', label: 'Score', sortable: true, align: 'right', width: '100px' }
]
const camposFiltro: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: STATUS_WORKFLOW_OPCOES }
]

function novo() { router.push('/erp/projetos/portfolio/novo') }
function abrir(item: Portfolio) { router.push(`/erp/projetos/portfolio/${item.id}`) }

onMounted(() => { void lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar title="Portfólios" subtitle="Portfólio e priorização de iniciativas" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo portfólio</button>
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
      empty-text="Nenhum portfólio encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrir"
    >
      <template #cell-status="{ value }"><span class="badge badge-cancelada">{{ rotuloStatusWorkflow(value) }}</span></template>
      <template #cell-scoreTotal="{ value }">{{ value == null ? '—' : Number(value).toLocaleString('pt-BR') }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Abrir</button>
      </template>
    </DataTable>
  </div>
</template>
