<script setup lang="ts">
/**
 * Listagem de Versões Orçamentárias — Planejamento/Orçamento.
 * GET /planejamento-orcamento/versoes, POST. Detalhe com ações em /{id}.
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList } from '~/composables/useApiList'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Versao {
  id: string
  nome?: string | null
  periodoInicio?: string | null
  periodoFim?: string | null
  statusDescricao?: string | null
}

const router = useRouter()
const { formatarData } = useHelper()

const lista = useApiList<Versao>('/planejamento-orcamento/versoes', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Versao>[] = [
  { key: 'nome', label: 'Nome', sortable: true },
  { key: 'periodoInicio', label: 'Início', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'periodoFim', label: 'Fim', sortable: false, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'statusDescricao', label: 'Status', sortable: false, align: 'center' }
]

function nova() {
  router.push('/erp/financeiro/planejamento-orcamento/versoes/novo')
}
function abrir(item: Versao) {
  router.push(`/erp/financeiro/planejamento-orcamento/versoes/${item.id}`)
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Versões Orçamentárias" subtitle="Versões de planejamento orçamentário" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="nova">+ Nova versão</button>
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
      empty-text="Nenhuma versão cadastrada."
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
