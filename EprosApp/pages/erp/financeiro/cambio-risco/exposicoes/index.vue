<script setup lang="ts">
/**
 * Listagem de Exposições Cambiais — Câmbio/Risco.
 * GET /cambio-risco/exposicoes, POST (criar). Ações: encerrar e hedgear (POST /{id}/...).
 */
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'

definePageMeta({ layout: 'default' })

interface Exposicao {
  id: string
  moedaNome?: string | null
  valorExposto?: number | null
  valorMoedaBase?: number | null
  dataReferencia?: string | null
  origemExposicao?: string | null
  status?: number | null
  statusDescricao?: string | null
}

const router = useRouter()
const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const lista = useApiList<Exposicao>('/cambio-risco/exposicoes', { tamanhoPaginaInicial: 25 })

const colunas: DataTableColumn<Exposicao>[] = [
  { key: 'moedaNome', label: 'Moeda', sortable: true },
  { key: 'valorExposto', label: 'Valor Exposto', sortable: true, align: 'right', formatter: (v) => formatarMoeda(v as number) },
  { key: 'valorMoedaBase', label: 'Valor Moeda Base', sortable: false, align: 'right', formatter: (v) => (v != null ? formatarMoeda(v as number) : '') },
  { key: 'dataReferencia', label: 'Referência', sortable: true, formatter: (v) => (v ? formatarData(v as string) : '') },
  { key: 'origemExposicao', label: 'Origem', sortable: false },
  { key: 'statusDescricao', label: 'Status', sortable: false, align: 'center' }
]

function nova() {
  router.push('/erp/financeiro/cambio-risco/exposicoes/novo')
}

async function encerrar(item: Exposicao) {
  const ok = await confirmRef.value!.open('Encerrar exposição', 'Confirma o encerramento desta exposição cambial?')
  if (!ok) return
  try {
    await useApi('/cambio-risco/exposicoes/{id}/encerrar', { method: 'POST', params: { id: item.id } })
    toast.success('Exposição encerrada.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

async function hedgear(item: Exposicao) {
  const ok = await confirmRef.value!.open('Hedgear exposição', 'Confirma o hedge desta exposição cambial?')
  if (!ok) return
  try {
    await useApi('/cambio-risco/exposicoes/{id}/hedgear', { method: 'POST', params: { id: item.id } })
    toast.success('Exposição marcada como hedgeada.')
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
    <PageToolbar title="Exposições Cambiais" subtitle="Exposições em moeda estrangeira e hedge" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="nova">+ Nova exposição</button>
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
      empty-text="Nenhuma exposição registrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Hedgear" @click.stop="hedgear(row)">Hedgear</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Encerrar" @click.stop="encerrar(row)">Encerrar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
