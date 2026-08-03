<script setup lang="ts">
/**
 * Listagem de Riscos Corporativos — GRC / Gestão de Riscos.
 *
 * Fonte: GET /api/v1/grc/riscos (lista completa, sem paginação server-side).
 * O backend só expõe GET (lista) e POST (criação); não há detalhe/edição/exclusão.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Risco {
  id: string
  titulo?: string | null
  descricao?: string | null
  categoria?: string | null
  probabilidade?: number | null
  impacto?: number | null
  nivelRisco?: number | null
  status?: string | null
}

const router = useRouter()

const lista = useApiList<Risco>('/grc/riscos', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Risco>[] = [
  { key: 'titulo', label: 'Título', sortable: true },
  { key: 'categoria', label: 'Categoria', sortable: true, width: '140px' },
  { key: 'probabilidade', label: 'Probabilidade', align: 'center', width: '120px' },
  { key: 'impacto', label: 'Impacto', align: 'center', width: '100px' },
  { key: 'nivelRisco', label: 'Nível', align: 'center', width: '90px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '140px' }
]

function novoRisco() {
  router.push('/erp/grc/riscos/novo')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Riscos Corporativos" subtitle="Identificação e classificação de riscos (probabilidade × impacto)" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novoRisco">+ Novo risco</button>
      </template>
    </PageToolbar>

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum risco cadastrado. Adicione um novo risco para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-status="{ value }">
        <span v-if="value" class="badge badge-info">{{ value }}</span>
      </template>
    </DataTable>
  </div>
</template>
