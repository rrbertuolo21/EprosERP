<script setup lang="ts">
/**
 * Listagem de Equipamentos — Manutenção / Equipamentos.
 * Fonte: GET /manutencao/equipamentos. Sem endpoint de exclusão (não há ação de excluir).
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Equipamento {
  id: string
  nome?: string | null
  codigo?: string | null
  setor?: string | null
  status?: string | null
  criticidade?: string | null
  dataAquisicao?: string | null
}

interface EquipamentoFiltros {
  busca?: string
}

const router = useRouter()

const lista = useApiList<Equipamento, EquipamentoFiltros>('/manutencao/equipamentos', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Equipamento>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '120px' },
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'setor', label: 'Setor', sortable: true },
  { key: 'criticidade', label: 'Criticidade', sortable: true, align: 'center', width: '120px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' },
  { key: 'dataAquisicao', label: 'Aquisição', sortable: true, width: '140px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Nome, código ou setor...', grow: true }
]

function formatarData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleDateString('pt-BR')
}

function novo() {
  router.push('/erp/manutencao/equipamentos/novo')
}

function editar(item: Equipamento) {
  router.push(`/erp/manutencao/equipamentos/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Equipamentos" subtitle="Cadastro de equipamentos e ativos de manutenção" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo equipamento</button>
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
      empty-text="Nenhum equipamento encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="editar"
    >
      <template #cell-dataAquisicao="{ value }">
        <span>{{ formatarData(value) }}</span>
      </template>
      <template #cell-status="{ value }">
        <span class="badge" :class="value === 'Inativo' ? 'badge-danger' : 'badge-success'">{{ value }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Configurar" @click.stop="editar(row)">Configurar</button>
      </template>
    </DataTable>
  </div>
</template>
