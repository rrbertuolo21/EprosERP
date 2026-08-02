<script setup lang="ts">
/**
 * Faturamento Comercial Internacional (FCI) — Documentos.
 * Contrato real: base `/vendas/faturamento-internacional`.
 *   GET documentos · GET documentos/{id} · GET contas-em-aberto
 *   POST documentos/{id}/aprovar
 * A criação exige itens/impostos (fluxo dedicado) — ver relatório.
 * Lista + aprovação. Apresentação — sem regra nova.
 */
import { onMounted, ref } from 'vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { formatMoeda, formatData } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default', middleware: 'auth' })

const STATUS = ['Rascunho', 'Aprovada', 'Paga', 'Excluída']
function statusLabel(v: unknown) {
  return STATUS[Number(v)] ?? String(v ?? '')
}

interface Documento {
  id: string
  numero?: string | null
  dataDocumento?: string | null
  clienteId?: string | null
  totalGeral?: number | null
  saldoEmAberto?: number | null
  status?: number | null
}

const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
const lista = useApiList<Documento>('/vendas/faturamento-internacional/documentos', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Documento>[] = [
  { key: 'numero', label: 'Nº' },
  { key: 'dataDocumento', label: 'Data', formatter: formatData },
  { key: 'totalGeral', label: 'Total', align: 'right', formatter: formatMoeda },
  { key: 'saldoEmAberto', label: 'Em aberto', align: 'right', formatter: formatMoeda },
  { key: 'status', label: 'Status', formatter: statusLabel }
]

async function aprovar(d: Documento) {
  const ok = await confirmRef.value!.open('Aprovar documento', `Aprovar o documento "${d.numero ?? ''}"?`)
  if (!ok) return
  try {
    await useApi('/vendas/faturamento-internacional/documentos/{id}/aprovar', {
      method: 'POST',
      params: { id: d.id },
      body: { documentoId: d.id }
    })
    toast.success('Documento aprovado.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Faturamento internacional — documentos" subtitle="FCI — cotações, faturas e notas de crédito" :loading="lista.carregando.value" />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhum documento encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" :disabled="Number(row.status) !== 0" @click.stop="aprovar(row)">Aprovar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
