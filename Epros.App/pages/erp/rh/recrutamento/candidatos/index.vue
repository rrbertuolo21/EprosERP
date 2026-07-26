<script setup lang="ts">
/**
 * Candidatos — RH / Recrutamento.
 * Fonte: GET/POST /rh/recrutamento/candidatos. Lista + criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'

definePageMeta({ layout: 'default' })

interface Candidato {
  id: string
  primeiroNome?: string | null
  sobrenome?: string | null
  email?: string | null
  anosExperiencia?: number | null
}
interface Filtros { busca?: string }

const router = useRouter()
const lista = useApiList<Candidato, Filtros>('/rh/recrutamento/candidatos', {
  filtrosIniciais: { busca: '' },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Candidato>[] = [
  { key: 'primeiroNome', label: 'Nome', sortable: true },
  { key: 'sobrenome', label: 'Sobrenome', sortable: true },
  { key: 'email', label: 'E-mail', sortable: false },
  { key: 'anosExperiencia', label: 'Experiência (anos)', sortable: false, align: 'right' }
]
const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Nome ou e-mail...', grow: true }
]
function novo() {
  router.push('/erp/rh/recrutamento/candidatos/novo')
}
onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Candidatos" subtitle="Banco de candidatos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo candidato</button>
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
      row-key="id"
      empty-text="Nenhum candidato encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-email="{ value }">
        <a v-if="value" :href="`mailto:${value}`" @click.stop>{{ value }}</a>
      </template>
    </DataTable>
  </div>
</template>
