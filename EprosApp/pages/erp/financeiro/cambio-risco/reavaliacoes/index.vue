<script setup lang="ts">
/**
 * Listagem de Reavaliações Cambiais — Câmbio/Risco.
 * GET /cambio-risco/reavaliacoes, POST (criar). Ações no detalhe: aprovar/cancelar/contabilizar.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Reavaliacao {
  id: string
  dataReavaliacao?: string | null
  observacao?: string | null
  statusDescricao?: string | null
}

const router = useRouter()
const { formatarData } = useHelper()

const lista = useApiList<Reavaliacao>('/cambio-risco/reavaliacoes', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Reavaliacao>[] = [
  { key: 'dataReavaliacao', label: 'Data', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'observacao', label: 'Observação', sortable: false },
  { key: 'statusDescricao', label: 'Status', sortable: false, align: 'center' }
]

function nova() {
  router.push('/erp/financeiro/cambio-risco/reavaliacoes/novo')
}
function abrir(item: Reavaliacao) {
  router.push(`/erp/financeiro/cambio-risco/reavaliacoes/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Reavaliações Cambiais" subtitle="Reavaliação de títulos em moeda estrangeira" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="nova">+ Nova reavaliação</button>
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
      empty-text="Nenhuma reavaliação registrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrir"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Abrir" @click.stop="abrir(row)">Abrir</button>
      </template>
    </DataTable>
  </div>
</template>
