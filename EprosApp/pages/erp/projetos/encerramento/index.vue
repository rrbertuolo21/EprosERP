<script setup lang="ts">
/**
 * Listagem de Encerramentos de projeto — PROJETOS / Encerramento.
 * GET /projetos/encerramento (paginado por status) · POST via formulário.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import { STATUS_WORKFLOW_OPCOES, rotuloStatusWorkflow } from '~/components/projetos-shared/statusWorkflow'

definePageMeta({ layout: 'default' })

interface Encerramento {
  id: string; codigo?: string | null; descricao?: string | null; status?: number | null
  projetoId?: string | null; responsavelId?: string | null; versao?: number | null
}
interface Filtros { status?: string }

const router = useRouter()
const lista = useApiList<Encerramento, Filtros>('/projetos/encerramento', {
  filtrosIniciais: { status: '' }, tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Encerramento>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '150px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'projetoId', label: 'Projeto (ID)', sortable: false },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]
const camposFiltro: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: STATUS_WORKFLOW_OPCOES }
]

function novo() { router.push('/erp/projetos/encerramento/novo') }
function abrir(item: Encerramento) { router.push(`/erp/projetos/encerramento/${item.id}`) }
onMounted(() => { void lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar title="Encerramentos" subtitle="Encerramento formal de projetos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo encerramento</button>
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
      empty-text="Nenhum encerramento encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrir"
    >
      <template #cell-status="{ value }"><span class="badge badge-cancelada">{{ rotuloStatusWorkflow(value) }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Abrir</button>
      </template>
    </DataTable>
  </div>
</template>
