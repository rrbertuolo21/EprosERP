<script setup lang="ts">
/**
 * Listagem de Políticas — GRC / Gestão de Políticas.
 * Fonte: GET /api/v1/grc/politicas. Backend com GET (lista) e POST (criação).
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Politica {
  id: string
  codigo?: string | null
  titulo?: string | null
  categoria?: string | null
  moduloAplicavel?: string | null
  status?: string | null
}

const router = useRouter()
const lista = useApiList<Politica>('/grc/politicas', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Politica>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '130px' },
  { key: 'titulo', label: 'Título', sortable: true },
  { key: 'categoria', label: 'Categoria', sortable: true, width: '140px' },
  { key: 'moduloAplicavel', label: 'Módulo', sortable: true, width: '140px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Políticas" subtitle="Gestão de políticas corporativas e versões" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="router.push('/erp/grc/politicas/novo')">+ Nova política</button>
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
      empty-text="Nenhuma política cadastrada."
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
