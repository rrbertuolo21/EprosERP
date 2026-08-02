<script setup lang="ts">
/**
 * Listagem de Estimativas de Produção — Produção / Estimativas.
 *
 * Agregado com GET lista + GET/{id} + POST criar + workflow (submeter/aprovar/rejeitar/
 * converter/inativar/reativar/encerrar). Sem PUT/DELETE: a lista é de leitura, o form é
 * só-criação e as transições vivem na tela de detalhe. Filtro server-side por status.
 * Fonte: ProducaoEstimativasController + EstimativaQueries.
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import { filtroStatusWorkflow, rotuloStatusWorkflow, classeBadgeStatus, formatarData, formatarMoeda } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface Estimativa {
  id: string
  codigo?: string | null
  status?: number | string | null
  custoPrevistoTotal?: number | null
  responsavelId?: string | null
  criadoEm?: string | null
}

interface EstimativaFiltros {
  status?: string | null
}

const router = useRouter()

const lista = useApiList<Estimativa, EstimativaFiltros>('/producao/estimativas', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Estimativa>[] = [
  { key: 'codigo', label: 'Código', sortable: false },
  { key: 'status', label: 'Status', sortable: false, align: 'center', width: '140px' },
  { key: 'custoPrevistoTotal', label: 'Custo Previsto', sortable: false, align: 'right', width: '160px' },
  { key: 'criadoEm', label: 'Criado em', sortable: false, width: '150px' }
]

const camposFiltro: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: filtroStatusWorkflow }
]

function novo() {
  router.push('/erp/producao/estimativas/novo')
}
function abrir(item: Estimativa) {
  router.push(`/erp/producao/estimativas/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Estimativas de Produção" subtitle="Estimativas industriais e conversão em planejamento" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova estimativa</button>
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
      empty-text="Nenhuma estimativa encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @row-click="abrir"
    >
      <template #cell-status="{ value }">
        <span class="badge" :class="classeBadgeStatus(rotuloStatusWorkflow(value as number | string))">
          {{ rotuloStatusWorkflow(value as number | string) }}
        </span>
      </template>
      <template #cell-custoPrevistoTotal="{ value }">{{ formatarMoeda(value as number) }}</template>
      <template #cell-criadoEm="{ value }">{{ formatarData(value as string) }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Ver</button>
      </template>
    </DataTable>
  </div>
</template>
