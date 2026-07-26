<script setup lang="ts">
/**
 * Listagem de Projetos — módulo PROJETOS.
 *
 * Consome GET /projetos (lista completa) e POST /projetos (via formulário).
 * Não há DELETE/PUT no contrato do agregado raiz, então a linha leva ao detalhe.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Projeto {
  id: string
  nome?: string | null
  descricao?: string | null
  clienteId?: string | null
  dataInicio?: string | null
  dataTermino?: string | null
  orcamentoTotal?: number | null
  custoAcumulado?: number | null
  percentualConclusao?: number | null
  status?: string | null
}

interface ProjetoFiltros {
  busca?: string
}

const router = useRouter()

const lista = useApiList<Projeto, ProjetoFiltros>('/projetos', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Projeto>[] = [
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' },
  { key: 'dataInicio', label: 'Início', sortable: true, width: '120px' },
  { key: 'dataTermino', label: 'Término', sortable: true, width: '120px' },
  { key: 'orcamentoTotal', label: 'Orçamento', sortable: true, align: 'right', width: '140px' },
  { key: 'percentualConclusao', label: 'Conclusão', sortable: true, align: 'right', width: '110px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Nome do projeto...', grow: true }
]

function formatarData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? '' : d.toLocaleDateString('pt-BR')
}

function formatarMoeda(v: unknown): string {
  const n = Number(v ?? 0)
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(n)
}

function novoProjeto() {
  router.push('/erp/projetos/novo')
}

function abrirProjeto(item: Projeto) {
  router.push(`/erp/projetos/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Projetos" subtitle="Cadastro e acompanhamento dos projetos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoProjeto">+ Novo projeto</button>
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
      empty-text="Nenhum projeto encontrado. Crie um novo projeto para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrirProjeto"
    >
      <template #cell-dataInicio="{ value }">{{ formatarData(value) }}</template>
      <template #cell-dataTermino="{ value }">{{ formatarData(value) }}</template>
      <template #cell-orcamentoTotal="{ value }">{{ formatarMoeda(value) }}</template>
      <template #cell-percentualConclusao="{ value }">{{ Number(value ?? 0).toFixed(0) }}%</template>
      <template #cell-status="{ value }">
        <span class="badge badge-cancelada">{{ value || '—' }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Abrir" @click.stop="abrirProjeto(row)">Abrir</button>
      </template>
    </DataTable>
  </div>
</template>
