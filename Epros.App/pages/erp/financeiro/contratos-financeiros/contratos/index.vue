<script setup lang="ts">
/**
 * Listagem de Contratos Financeiros.
 * GET /contratos-financeiros, POST (criar). Detalhe com ações em /{id}.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Contrato {
  id: string
  planoDescricao?: string | null
  pessoaNome?: string | null
  dataInicio?: string | null
  dataFim?: string | null
  statusDescricao?: string | null
  valorAtual?: number | null
}

const router = useRouter()
const { formatarData, formatarMoeda } = useHelper()

const lista = useApiList<Contrato>('/contratos-financeiros', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Contrato>[] = [
  { key: 'pessoaNome', label: 'Pessoa', sortable: true },
  { key: 'planoDescricao', label: 'Plano', sortable: false },
  { key: 'dataInicio', label: 'Início', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'valorAtual', label: 'Valor Atual', sortable: false, align: 'right', formatter: (v) => (v != null ? formatarMoeda(v as number) : '') },
  { key: 'statusDescricao', label: 'Status', sortable: false, align: 'center' }
]

function novo() {
  router.push('/erp/financeiro/contratos-financeiros/contratos/novo')
}
function abrir(item: Contrato) {
  router.push(`/erp/financeiro/contratos-financeiros/contratos/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Contratos Financeiros" subtitle="Contratos recorrentes de cobrança" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo contrato</button>
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
      empty-text="Nenhum contrato cadastrado."
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
