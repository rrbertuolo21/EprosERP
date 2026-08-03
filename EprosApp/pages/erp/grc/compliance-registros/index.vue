<script setup lang="ts">
/**
 * Listagem de Registros Regulatórios — GRC / Compliance Regulatório.
 * Fonte: GET /api/v1/grc/compliance/registros, POST criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Registro {
  id: string
  codigo?: string | null
  descricao?: string | null
  norma?: string | null
  status?: string | null
}

const router = useRouter()
const lista = useApiList<Registro>('/grc/compliance/registros', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Registro>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'norma', label: 'Norma', sortable: true, width: '160px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Registros Regulatórios" subtitle="Obrigações e registros de compliance regulatório" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="router.push('/erp/grc/compliance-registros/novo')">+ Novo registro</button>
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
      empty-text="Nenhum registro regulatório cadastrado."
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
