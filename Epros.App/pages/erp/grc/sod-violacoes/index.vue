<script setup lang="ts">
/**
 * Listagem de Violações de Segregação de Funções (SoD) — GRC.
 * Fonte: GET /api/v1/grc/sod/violacoes. Somente leitura (não há POST de violação;
 * violações são geradas por simulações/detecção no backend).
 */
import { onMounted } from 'vue'
import { useApiList } from '~/composables/useApiList'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Violacao {
  id: string
  regraId?: string | null
  perfilId?: string | null
  usuarioId?: string | null
  status?: string | null
  dataDeteccao?: string | null
}

const lista = useApiList<Violacao>('/grc/sod/violacoes', { tamanhoPaginaInicial: 50 })

const colunas: DataTableColumn<Violacao>[] = [
  { key: 'regraId', label: 'Regra', width: '280px' },
  { key: 'usuarioId', label: 'Usuário', width: '280px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' },
  { key: 'dataDeteccao', label: 'Detecção', sortable: true, width: '160px' }
]

function formatarData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleString('pt-BR')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Violações SoD" subtitle="Conflitos de segregação de funções detectados (somente leitura)" :loading="lista.carregando.value" />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma violação SoD detectada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-status="{ value }">
        <span v-if="value" class="badge badge-danger">{{ value }}</span>
      </template>
      <template #cell-dataDeteccao="{ value }">{{ formatarData(value) }}</template>
    </DataTable>
  </div>
</template>
