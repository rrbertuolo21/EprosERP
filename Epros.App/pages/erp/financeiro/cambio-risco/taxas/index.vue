<script setup lang="ts">
/**
 * Listagem de Taxas de Câmbio — Câmbio/Risco.
 * A API expõe apenas GET /cambio-risco/taxas e POST (sem edição/exclusão).
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import { OPCOES_ORIGEM_TAXA } from '~/components/financeiro-avancado/enums'

definePageMeta({ layout: 'default' })

interface Taxa {
  id: string
  moedaId?: string | null
  moedaNome?: string | null
  dataTaxa?: string | null
  taxaCompra?: number | null
  taxaVenda?: number | null
  origemTaxa?: number | null
}

const router = useRouter()
const { formatarData } = useHelper()

const lista = useApiList<Taxa>('/cambio-risco/taxas', { tamanhoPaginaInicial: 25 })

function origemLabel(v: unknown): string {
  return OPCOES_ORIGEM_TAXA.find((o) => o.value === v)?.label ?? ''
}

const colunas: DataTableColumn<Taxa>[] = [
  { key: 'moedaNome', label: 'Moeda', sortable: true },
  { key: 'dataTaxa', label: 'Data', sortable: true, formatter: (v) => formatarData(v as string) },
  { key: 'taxaCompra', label: 'Compra', sortable: true, align: 'right', formatter: (v) => (v != null ? Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 4 }) : '') },
  { key: 'taxaVenda', label: 'Venda', sortable: true, align: 'right', formatter: (v) => (v != null ? Number(v).toLocaleString('pt-BR', { minimumFractionDigits: 4 }) : '') },
  { key: 'origemTaxa', label: 'Origem', sortable: false, align: 'center', formatter: (v) => origemLabel(v) }
]

function nova() {
  router.push('/erp/financeiro/cambio-risco/taxas/novo')
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Taxas de Câmbio" subtitle="Cotações de compra e venda por moeda" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="nova">+ Nova taxa</button>
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
      empty-text="Nenhuma taxa registrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    />
  </div>
</template>
