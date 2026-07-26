<script setup lang="ts">
/**
 * Listagem de Programas de Subsídio/Fundo.
 * GET /subsidios-fundos/programas, POST. Detalhe/edição em /{id}.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Programa {
  id: string
  orgao?: string | null
  valorTotal?: number | null
  vigenciaInicio?: string | null
  vigenciaFim?: string | null
  statusDescricao?: string | null
}

const router = useRouter()
const { formatarData, formatarMoeda } = useHelper()

const lista = useApiList<Programa>('/subsidios-fundos/programas', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Programa>[] = [
  { key: 'orgao', label: 'Órgão', sortable: true },
  { key: 'valorTotal', label: 'Valor Total', sortable: true, align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'vigenciaInicio', label: 'Início', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'vigenciaFim', label: 'Fim', sortable: false, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'statusDescricao', label: 'Status', sortable: false, align: 'center' }
]

function novo() {
  router.push('/erp/financeiro/subsidios-fundos/programas/novo')
}
function abrir(item: Programa) {
  router.push(`/erp/financeiro/subsidios-fundos/programas/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Programas de Subsídio" subtitle="Programas de subsídio e fundos" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo programa</button>
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
      empty-text="Nenhum programa cadastrado."
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
