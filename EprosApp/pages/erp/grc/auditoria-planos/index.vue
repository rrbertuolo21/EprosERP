<script setup lang="ts">
/**
 * Listagem de Planos de Auditoria — GRC / Controles Internos e Auditoria.
 * Fonte: GET /api/v1/grc/auditoria/planos, POST criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Plano {
  id: string
  codigo?: string | null
  titulo?: string | null
  ciclo?: string | null
  status?: string | null
}

const router = useRouter()
const lista = useApiList<Plano>('/grc/auditoria/planos', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Plano>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '140px' },
  { key: 'titulo', label: 'Título', sortable: true },
  { key: 'ciclo', label: 'Ciclo', sortable: true, width: '140px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Planos de Auditoria" subtitle="Planejamento de ciclos de auditoria interna" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="router.push('/erp/grc/auditoria-planos/novo')">+ Novo plano</button>
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
      empty-text="Nenhum plano de auditoria cadastrado."
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
