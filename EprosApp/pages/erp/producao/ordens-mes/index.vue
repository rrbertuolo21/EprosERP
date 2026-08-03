<script setup lang="ts">
/**
 * Listagem de Ordens de Manufatura (MES) — Produção / Ordens MES.
 * GET lista (+ filtro status) + GET/{id} + POST criar + workflow + finalizar. Sem PUT/DELETE.
 * Fonte: ProducaoMesController (api/v1/producao/mes/ordens) + MesOrdemQueries.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import { filtroStatusMes, rotuloStatusMes, classeBadgeStatus, formatarData, formatarMoeda } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface MesOrdem {
  id: string
  referencia?: string | null
  status?: number | string | null
  custoTotalPrevisto?: number | null
  previsaoEntrega?: string | null
  criadoEm?: string | null
}
interface MesFiltros { status?: string | null }

const router = useRouter()
const lista = useApiList<MesOrdem, MesFiltros>('/producao/mes/ordens', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<MesOrdem>[] = [
  { key: 'referencia', label: 'Referência' },
  { key: 'status', label: 'Status', align: 'center', width: '140px' },
  { key: 'custoTotalPrevisto', label: 'Custo Previsto', align: 'right', width: '160px' },
  { key: 'previsaoEntrega', label: 'Previsão Entrega', width: '150px' },
  { key: 'criadoEm', label: 'Criado em', width: '150px' }
]
const camposFiltro: FilterField[] = [{ key: 'status', label: 'Status', type: 'select', options: filtroStatusMes }]

function novo() { router.push('/erp/producao/ordens-mes/novo') }
function abrir(item: MesOrdem) { router.push(`/erp/producao/ordens-mes/${item.id}`) }

onMounted(() => { void lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar title="Ordens de Manufatura (MES)" subtitle="Execução de ordens de manufatura e apontamentos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova ordem</button>
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
      empty-text="Nenhuma ordem de manufatura encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @row-click="abrir"
    >
      <template #cell-status="{ value }">
        <span class="badge" :class="classeBadgeStatus(rotuloStatusMes(value as number | string))">{{ rotuloStatusMes(value as number | string) }}</span>
      </template>
      <template #cell-custoTotalPrevisto="{ value }">{{ formatarMoeda(value as number) }}</template>
      <template #cell-previsaoEntrega="{ value }">{{ formatarData(value as string) }}</template>
      <template #cell-criadoEm="{ value }">{{ formatarData(value as string) }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Ver</button>
      </template>
    </DataTable>
  </div>
</template>
