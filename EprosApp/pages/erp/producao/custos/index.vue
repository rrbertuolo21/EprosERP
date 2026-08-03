<script setup lang="ts">
/**
 * Listagem de Custos de Produção — Produção / Custos.
 * GET lista (+ filtro status) + GET/{id} + POST criar + workflow. Sem PUT/DELETE.
 * Fonte: ProducaoCustosController + CustoProducaoQueries.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import { filtroStatusWorkflow, rotuloStatusWorkflow, classeBadgeStatus, formatarData, formatarMoeda } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface Custo {
  id: string
  codigo?: string | null
  status?: number | string | null
  custoTotalPrevisto?: number | null
  custoTotalRealizado?: number | null
  criadoEm?: string | null
}
interface CustoFiltros { status?: string | null }

const router = useRouter()
const lista = useApiList<Custo, CustoFiltros>('/producao/custos', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Custo>[] = [
  { key: 'codigo', label: 'Código' },
  { key: 'status', label: 'Status', align: 'center', width: '140px' },
  { key: 'custoTotalPrevisto', label: 'Previsto', align: 'right', width: '150px' },
  { key: 'custoTotalRealizado', label: 'Realizado', align: 'right', width: '150px' },
  { key: 'criadoEm', label: 'Criado em', width: '150px' }
]
const camposFiltro: FilterField[] = [{ key: 'status', label: 'Status', type: 'select', options: filtroStatusWorkflow }]

function novo() { router.push('/erp/producao/custos/novo') }
function abrir(item: Custo) { router.push(`/erp/producao/custos/${item.id}`) }

onMounted(() => { void lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar title="Custos de Produção" subtitle="Apuração de custos previstos e realizados" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo custo</button>
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
      empty-text="Nenhum registro de custo encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @row-click="abrir"
    >
      <template #cell-status="{ value }">
        <span class="badge" :class="classeBadgeStatus(rotuloStatusWorkflow(value as number | string))">{{ rotuloStatusWorkflow(value as number | string) }}</span>
      </template>
      <template #cell-custoTotalPrevisto="{ value }">{{ formatarMoeda(value as number) }}</template>
      <template #cell-custoTotalRealizado="{ value }">{{ formatarMoeda(value as number) }}</template>
      <template #cell-criadoEm="{ value }">{{ formatarData(value as string) }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Ver</button>
      </template>
    </DataTable>
  </div>
</template>
