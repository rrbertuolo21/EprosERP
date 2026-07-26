<script setup lang="ts">
/**
 * Listagem de Movimentos Financeiros (Tesouraria).
 * GET /tesouraria/movimentos, POST. Ação: conciliar (POST /{id}/conciliar).
 */
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'

definePageMeta({ layout: 'default' })

interface Movimento {
  id: string
  emissao?: string | null
  credito?: number | null
  debito?: number | null
  contaNome?: string | null
  caixaNome?: string | null
  conciliado?: boolean | null
}

const router = useRouter()
const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()

const lista = useApiList<Movimento>('/tesouraria/movimentos', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Movimento>[] = [
  { key: 'emissao', label: 'Emissão', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'contaNome', label: 'Conta', sortable: false },
  { key: 'caixaNome', label: 'Caixa', sortable: false },
  { key: 'credito', label: 'Crédito', sortable: true, align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'debito', label: 'Débito', sortable: true, align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'conciliado', label: 'Conciliado', sortable: false, align: 'center' }
]

function novo() {
  router.push('/erp/financeiro/tesouraria/movimentos/novo')
}

async function conciliar(item: Movimento) {
  try {
    await useApi('/tesouraria/movimentos/{id}/conciliar', { method: 'POST', params: { id: item.id } })
    toast.success('Movimento conciliado.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Movimentos Financeiros" subtitle="Movimentação de caixa e conta" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo movimento</button>
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
      empty-text="Nenhum movimento registrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-conciliado="{ value }">
        <span class="badge" :class="value ? 'badge-success' : 'badge-danger'">{{ value ? 'Sim' : 'Não' }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" :disabled="!!row.conciliado" title="Conciliar" @click.stop="conciliar(row)">Conciliar</button>
      </template>
    </DataTable>
  </div>
</template>
