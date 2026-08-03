<script setup lang="ts">
/**
 * Listagem de Achados de Auditoria — GRC / Controles Internos e Auditoria.
 * Fonte: GET /api/v1/grc/auditoria/achados, POST criação.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Achado {
  id: string
  titulo?: string | null
  severidade?: string | null
  prazoRemediacao?: string | null
  status?: string | null
}

const router = useRouter()
const lista = useApiList<Achado>('/grc/auditoria/achados', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Achado>[] = [
  { key: 'titulo', label: 'Título', sortable: true },
  { key: 'severidade', label: 'Severidade', sortable: true, align: 'center', width: '120px' },
  { key: 'prazoRemediacao', label: 'Prazo remediação', sortable: true, width: '160px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]

function formatarData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleDateString('pt-BR')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Achados de Auditoria" subtitle="Não conformidades encontradas em testes de controle" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="router.push('/erp/grc/auditoria-achados/novo')">+ Novo achado</button>
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
      empty-text="Nenhum achado registrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-severidade="{ value }">
        <span v-if="value" class="badge badge-info">{{ value }}</span>
      </template>
      <template #cell-prazoRemediacao="{ value }">{{ formatarData(value) }}</template>
      <template #cell-status="{ value }">
        <span v-if="value" class="badge badge-info">{{ value }}</span>
      </template>
    </DataTable>
  </div>
</template>
