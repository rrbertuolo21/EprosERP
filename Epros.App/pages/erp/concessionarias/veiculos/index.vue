<script setup lang="ts">
/**
 * Veículos — lista de leitura (GET /veiculos). Detalhe read-only em `[id].vue`.
 * Esta raiz não expõe POST/PUT/DELETE no digest, portanto é somente consulta.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Veiculo {
  id: string
  // Campos de exibição são um palpite (o digest não detalha o DTO de listagem).
  chassi?: string | null
  placa?: string | null
  marca?: string | null
  modelo?: string | null
  anoModelo?: number | null
}

interface FiltroBusca {
  busca?: string
}

const router = useRouter()

const lista = useApiList<Veiculo, FiltroBusca>('/veiculos', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Veiculo>[] = [
  { key: 'chassi', label: 'Chassi' },
  { key: 'placa', label: 'Placa' },
  { key: 'marca', label: 'Marca' },
  { key: 'modelo', label: 'Modelo' },
  { key: 'anoModelo', label: 'Ano', align: 'right' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Chassi, placa, modelo...', grow: true }
]

function verDetalhe(item: Veiculo) {
  router.push(`/erp/concessionarias/veiculos/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Veículos" subtitle="Consulta de veículos" :loading="lista.carregando.value" />

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
      empty-text="Nenhum veículo encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="verDetalhe"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Ver detalhe" @click.stop="verDetalhe(row)">
          Detalhe
        </button>
      </template>
    </DataTable>
  </div>
</template>
