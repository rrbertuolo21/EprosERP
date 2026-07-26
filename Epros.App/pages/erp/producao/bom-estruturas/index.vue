<script setup lang="ts">
/**
 * Listagem de Estruturas de Produto (BOM) — Produção / Estruturas.
 * GET lista (+ filtro status) + GET/{id} + POST criar + workflow. Sem PUT/DELETE.
 * Fonte: ProducaoBomController (api/v1/producao/bom-estruturas) + BomEstruturaQueries.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import { filtroStatusWorkflow, rotuloStatusWorkflow, classeBadgeStatus, formatarData, formatarMoeda, formatarQuantidade } from '~/components/producao-shared/producao'

definePageMeta({ layout: 'default' })

interface BomEstrutura {
  id: string
  codigo?: string | null
  status?: number | string | null
  quantidadeTotal?: number | null
  precoFinal?: number | null
  versao?: string | null
  criadoEm?: string | null
}
interface BomFiltros { status?: string | null }

const router = useRouter()
const lista = useApiList<BomEstrutura, BomFiltros>('/producao/bom-estruturas', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<BomEstrutura>[] = [
  { key: 'codigo', label: 'Código' },
  { key: 'status', label: 'Status', align: 'center', width: '140px' },
  { key: 'versao', label: 'Versão', width: '100px' },
  { key: 'quantidadeTotal', label: 'Qtd. Total', align: 'right', width: '130px' },
  { key: 'precoFinal', label: 'Preço Final', align: 'right', width: '150px' },
  { key: 'criadoEm', label: 'Criado em', width: '150px' }
]
const camposFiltro: FilterField[] = [{ key: 'status', label: 'Status', type: 'select', options: filtroStatusWorkflow }]

function novo() { router.push('/erp/producao/bom-estruturas/novo') }
function abrir(item: BomEstrutura) { router.push(`/erp/producao/bom-estruturas/${item.id}`) }

onMounted(() => { void lista.buscar() })
</script>

<template>
  <div>
    <PageToolbar title="Estruturas de Produto (BOM)" subtitle="Estruturas, componentes e vigência de custo" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova estrutura</button>
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
      empty-text="Nenhuma estrutura encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @row-click="abrir"
    >
      <template #cell-status="{ value }">
        <span class="badge" :class="classeBadgeStatus(rotuloStatusWorkflow(value as number | string))">{{ rotuloStatusWorkflow(value as number | string) }}</span>
      </template>
      <template #cell-quantidadeTotal="{ value }">{{ formatarQuantidade(value as number) }}</template>
      <template #cell-precoFinal="{ value }">{{ formatarMoeda(value as number) }}</template>
      <template #cell-criadoEm="{ value }">{{ formatarData(value as string) }}</template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" @click.stop="abrir(row)">Ver</button>
      </template>
    </DataTable>
  </div>
</template>
