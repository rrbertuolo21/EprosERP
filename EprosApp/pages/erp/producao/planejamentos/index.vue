<script setup lang="ts">
/**
 * Listagem de Planejamentos de Produção — Produção / Planejamentos.
 * GET lista (+ filtro status) + GET/{id} + POST criar + snapshots + workflow. Sem PUT/DELETE.
 * Fonte: ProducaoPlanejamentoController (api/v1/producao/planejamentos) + PlanejamentoProducaoQueries.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import { filtroStatusWorkflow, rotuloStatusWorkflow, classeBadgeStatus, formatarData } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface Planejamento {
  id: string
  codigo?: string | null
  status?: number | string | null
  criadoEm?: string | null
}
interface PlanejamentoFiltros { status?: string | null }

const router = useRouter()
const lista = useApiList<Planejamento, PlanejamentoFiltros>('/producao/planejamentos', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Planejamento>[] = [
  { key: 'codigo', label: 'Código' },
  { key: 'status', label: 'Status', align: 'center', width: '140px' },
  { key: 'criadoEm', label: 'Criado em', width: '150px' }
]
const camposFiltro: FilterField[] = [{ key: 'status', label: 'Status', type: 'select', options: filtroStatusWorkflow }]

function novo() { router.push('/erp/producao/planejamentos/novo') }
function abrir(item: Planejamento) { router.push(`/erp/producao/planejamentos/${item.id}`) }

onMounted(() => { void lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar title="Planejamentos de Produção" subtitle="Planos de produção e snapshots de operações" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo planejamento</button>
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
      empty-text="Nenhum planejamento encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @row-click="abrir"
    >
      <template #cell-status="{ value }">
        <span class="badge" :class="classeBadgeStatus(rotuloStatusWorkflow(value as number | string))">{{ rotuloStatusWorkflow(value as number | string) }}</span>
      </template>
      <template #cell-criadoEm="{ value }">{{ formatarData(value as string) }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Ver</button>
      </template>
    </DataTable>
  </div>
</template>
