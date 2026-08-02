<script setup lang="ts">
/**
 * Saldo de Estoque por produto/local (erp/estoque/saldo).
 * Consulta o saldo consolidado do módulo Estoque — `GET /estoque-produtos`
 * (EstoqueProdutosController → ListarEstoqueProdutosQuery). Mostra saldo, reservado,
 * disponível (saldo − reservado), valor de saldo, custo médio e tipo de custeio.
 *
 * Endpoint: estoque-produtos (consulta de saldo; parametrização vive em Análise/Planejamento).
 */
import { computed, onMounted } from 'vue'
import { useApiList } from '~/composables/useApiList'
import { useHelper } from '~/composables/useHelper'
import { useEstoqueEnums, classeBadge } from '~/composables/useEstoqueEnums'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

interface SaldoEstoque {
  id: string
  empresaId: string
  produtoId: string
  quantidadeSaldoEstoque: number
  quantidadeEstoqueMinimo: number
  quantidadeEstoqueMaximo: number
  quantidadeEstoqueReservado: number
  valorSaldo: number
  valorCustoMedio: number
  tipoCusteioEstoque: number
}

interface SaldoFiltros extends Record<string, unknown> {
  empresaId?: string
  produtoId?: string
}

const { formatarMoeda, formatarNumero } = useHelper()
const { tipoCusteio } = useEstoqueEnums()

const lista = useApiList<SaldoEstoque, SaldoFiltros>('/estoque-produtos', {
  filtrosIniciais: {},
  tamanhoPaginaInicial: 25
})

const camposFiltro: FilterField[] = [
  { key: 'produtoId', label: 'Produto (ID)', type: 'text', placeholder: 'GUID do produto', grow: true },
  { key: 'empresaId', label: 'Empresa (ID)', type: 'text', placeholder: 'GUID da empresa' }
]

function disponivel(row: SaldoEstoque): number {
  return (row.quantidadeSaldoEstoque ?? 0) - (row.quantidadeEstoqueReservado ?? 0)
}

const colunas: DataTableColumn<SaldoEstoque>[] = [
  { key: 'produtoId', label: 'Produto', sortable: false },
  { key: 'quantidadeSaldoEstoque', label: 'Saldo', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'quantidadeEstoqueReservado', label: 'Reservado', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'disponivel', label: 'Disponível', align: 'right' },
  { key: 'quantidadeEstoqueMinimo', label: 'Mínimo', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'quantidadeEstoqueMaximo', label: 'Máximo', align: 'right', formatter: (v) => formatarNumero(v as number, 0, 4) },
  { key: 'valorCustoMedio', label: 'Custo médio', align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'valorSaldo', label: 'Valor do saldo', align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'tipoCusteioEstoque', label: 'Custeio', align: 'center', width: '120px' }
]

const totalValor = computed(() => lista.itens.value.reduce((acc, i) => acc + (i.valorSaldo ?? 0), 0))

function normalizar(v: Record<string, unknown>): Partial<SaldoFiltros> {
  return {
    produtoId: (v.produtoId as string) || undefined,
    empresaId: (v.empresaId as string) || undefined
  }
}

function abrir(item: SaldoEstoque) {
  navigateTo(`/erp/estoque/saldo/${item.id}`)
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Saldo de Estoque" subtitle="Posição por produto: saldo, reservado, disponível e custo" :loading="lista.carregando.value" />

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="(v) => lista.aplicarFiltros(normalizar(v as Record<string, unknown>))"
      @clear="lista.limpar()"
    />

    <div class="resumo-linha">
      <span>Valor total em estoque (página): <strong>{{ formatarMoeda(totalValor) }}</strong></span>
    </div>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      row-key="id"
      empty-text="Nenhum saldo encontrado"
      @update:page="(p) => lista.irParaPagina(p)"
      @update:page-size="(ps) => lista.buscar({ tamanhoPagina: ps, pagina: 1 })"
      @row-click="abrir"
    >
      <template #cell-disponivel="{ row }">
        {{ formatarNumero(disponivel(row), 0, 4) }}
      </template>
      <template #cell-tipoCusteioEstoque="{ value }">
        <span class="badge" :class="classeBadge(tipoCusteio.cor(value as number))">{{ tipoCusteio.label(value as number) }}</span>
      </template>
    </DataTable>
  </div>
</template>

<style scoped>
.resumo-linha {
  margin: 12px 4px;
  font-size: 13px;
  color: var(--text-secondary);
}
</style>
