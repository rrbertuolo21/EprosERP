<script setup lang="ts">
/**
 * Inventário Físico e Contagem Cíclica (erp/estoque/inventarios).
 * Listagem paginada — `GET /estoque-inventarios?situacao` (ListarInventariosQuery).
 * O fluxo (criar → contagem → conferência → aprovar → ajuste) vive no detalhe [id].vue.
 */
import { onMounted } from 'vue'
import { useApiList } from '~/composables/useApiList'
import { useHelper } from '~/composables/useHelper'
import { useEstoqueEnums, classeBadge } from '~/composables/useEstoqueEnums'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface InventarioListagem {
  id: string
  empresaId: string
  dataContagem: string | null
  tipoInventario: number
  situacao: number
  acuracidade: number | null
  estoqueAtualizado: boolean
  criadoEm: string
}

interface InventarioFiltros extends Record<string, unknown> {
  situacao?: number | null
}

const { formatarData, formatarDataHora, formatarPorcentagem } = useHelper()
const { tipoInventario, situacaoInventario } = useEstoqueEnums()

const lista = useApiList<InventarioListagem, InventarioFiltros>('/estoque-inventarios', {
  filtrosIniciais: {},
  tamanhoPaginaInicial: 25
})

const camposFiltro: FilterField[] = [
  { key: 'situacao', label: 'Situação', type: 'select', options: situacaoInventario.opcoes, grow: true }
]

const colunas: DataTableColumn<InventarioListagem>[] = [
  { key: 'criadoEm', label: 'Criado em', width: '150px', formatter: (v) => formatarDataHora(v as string) },
  { key: 'dataContagem', label: 'Data contagem', width: '130px', formatter: (v) => formatarData(v as string) },
  { key: 'tipoInventario', label: 'Tipo', align: 'center', formatter: (v) => tipoInventario.label(v as number) },
  { key: 'situacao', label: 'Situação', align: 'center', width: '140px' },
  { key: 'acuracidade', label: 'Acurácia', align: 'right', width: '110px' },
  { key: 'estoqueAtualizado', label: 'Ajustado', align: 'center', width: '100px' }
]

function abrir(item: InventarioListagem) {
  navigateTo(`/erp/estoque/inventarios/${item.id}`)
}

function novo() {
  navigateTo('/erp/estoque/inventarios/novo')
}

function normalizar(v: Record<string, unknown>): Partial<InventarioFiltros> {
  const s = v.situacao
  return { situacao: s === '' || s == null ? undefined : Number(s) }
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Inventário Físico" subtitle="Contagem cíclica e geral: contagem → conferência → aprovação → ajuste" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo inventário</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="(v) => lista.aplicarFiltros(normalizar(v as Record<string, unknown>))"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      row-key="id"
      empty-text="Nenhum inventário encontrado"
      @update:page="(p) => lista.irParaPagina(p)"
      @update:page-size="(ps) => lista.buscar({ tamanhoPagina: ps, pagina: 1 })"
      @row-click="abrir"
    >
      <template #cell-situacao="{ value }">
        <span class="badge" :class="classeBadge(situacaoInventario.cor(value as number))">{{ situacaoInventario.label(value as number) }}</span>
      </template>
      <template #cell-acuracidade="{ value }">
        {{ value == null ? '-' : formatarPorcentagem(value as number, 2) }}
      </template>
      <template #cell-estoqueAtualizado="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-muted'">{{ value ? 'Sim' : 'Não' }}</span>
      </template>
    </DataTable>
  </div>
</template>
