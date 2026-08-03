<script setup lang="ts">
/**
 * Listagem de Administração da Qualidade (QLD-ADM) — Qualidade / Administração.
 *
 * Fonte: GET /qualidade/administracao (paginado, filtro por status).
 * Backend expõe apenas leitura + criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import { STATUS_REGISTRO, STATUS_REGISTRO_OPCOES_FILTRO, rotuloEnum } from '~/components/qualidade-shared/enums'

definePageMeta({ layout: 'default' })

interface RegistroAdm {
  id: string
  codigo?: string | null
  descricao?: string | null
  status: number
  criadoEm?: string | null
}

interface AdmFiltros {
  status?: string | null
}

const router = useRouter()

const lista = useApiList<RegistroAdm, AdmFiltros>('/qualidade/administracao', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<RegistroAdm>[] = [
  { key: 'codigo', label: 'Código', sortable: false, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: false },
  { key: 'status', label: 'Status', sortable: false, align: 'center', width: '120px' }
]

const camposFiltro: FilterField[] = [
  { key: 'status', label: 'Status', type: 'select', options: STATUS_REGISTRO_OPCOES_FILTRO }
]

function novoRegistro() {
  router.push('/erp/qualidade/administracao/novo')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Administração da Qualidade" subtitle="Registros administrativos do módulo de qualidade" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoRegistro">+ Novo registro</button>
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
      empty-text="Nenhum registro encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
    >
      <template #cell-status="{ value }">
        <span class="badge badge-neutral">{{ rotuloEnum(STATUS_REGISTRO, value) }}</span>
      </template>
    </DataTable>
  </div>
</template>
