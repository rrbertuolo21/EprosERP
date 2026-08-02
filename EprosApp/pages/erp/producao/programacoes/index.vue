<script setup lang="ts">
/**
 * Listagem de Programações (Sequenciamento / ESC) — Produção / Programações.
 * GET lista (+ filtro status) + GET/{id} + POST criar + workflow. Sem PUT/DELETE.
 * Fonte: ProducaoEscController (api/v1/producao/esc/programacoes) + EscProgramacaoQueries.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import { filtroStatusWorkflow, rotuloStatusWorkflow, classeBadgeStatus, formatarData } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface Programacao {
  id: string
  codigo?: string | null
  status?: number | string | null
  prioridade?: number | null
  criadoEm?: string | null
}
interface ProgramacaoFiltros { status?: string | null }

const router = useRouter()
const lista = useApiList<Programacao, ProgramacaoFiltros>('/producao/esc/programacoes', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Programacao>[] = [
  { key: 'codigo', label: 'Código' },
  { key: 'status', label: 'Status', align: 'center', width: '140px' },
  { key: 'prioridade', label: 'Prioridade', align: 'center', width: '120px' },
  { key: 'criadoEm', label: 'Criado em', width: '150px' }
]
const camposFiltro: FilterField[] = [{ key: 'status', label: 'Status', type: 'select', options: filtroStatusWorkflow }]

function novo() { router.push('/erp/producao/programacoes/novo') }
function abrir(item: Programacao) { router.push(`/erp/producao/programacoes/${item.id}`) }

onMounted(() => { void lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar title="Programações de Produção" subtitle="Sequenciamento de operações (ESC)" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova programação</button>
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
      empty-text="Nenhuma programação encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @row-click="abrir"
    >
      <template #cell-status="{ value }">
        <span class="badge" :class="classeBadgeStatus(rotuloStatusWorkflow(value as number | string))">{{ rotuloStatusWorkflow(value as number | string) }}</span>
      </template>
      <template #cell-prioridade="{ value }">{{ value ?? '—' }}</template>
      <template #cell-criadoEm="{ value }">{{ formatarData(value as string) }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Ver</button>
      </template>
    </DataTable>
  </div>
</template>
