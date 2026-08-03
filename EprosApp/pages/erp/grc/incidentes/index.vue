<script setup lang="ts">
/**
 * Listagem de Incidentes de Compliance — GRC.
 * Fonte: GET /api/v1/grc/incidentes (lista completa). Só há GET e POST no backend.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Incidente {
  id: string
  titulo?: string | null
  origem?: string | null
  gravidade?: string | null
  status?: string | null
  dataAbertura?: string | null
}

const router = useRouter()
const lista = useApiList<Incidente>('/grc/incidentes', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Incidente>[] = [
  { key: 'titulo', label: 'Título', sortable: true },
  { key: 'origem', label: 'Origem', sortable: true, width: '140px' },
  { key: 'gravidade', label: 'Gravidade', sortable: true, align: 'center', width: '120px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '140px' },
  { key: 'dataAbertura', label: 'Abertura', sortable: true, width: '160px' }
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
    <PageToolbar title="Incidentes de Compliance" subtitle="Registro e acompanhamento de incidentes" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="router.push('/erp/grc/incidentes/novo')">+ Novo incidente</button>
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
      empty-text="Nenhum incidente registrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-gravidade="{ value }">
        <span v-if="value" class="badge badge-info">{{ value }}</span>
      </template>
      <template #cell-status="{ value }">
        <span v-if="value" class="badge badge-info">{{ value }}</span>
      </template>
      <template #cell-dataAbertura="{ value }">{{ formatarData(value) }}</template>
    </DataTable>
  </div>
</template>
