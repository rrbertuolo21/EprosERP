<script setup lang="ts">
/**
 * Logística de Saída — Expedições (picking / expedição / entrega).
 * Contrato real: base `/vendas/expedicoes`.
 *   GET (lista) ?pedidoId=&status= · GET {id}
 *   POST {id}/confirmar · POST {id}/cancelar
 * A criação exige EmpresaId/PedidoId (origem em pedido de venda) — ver relatório.
 * Lista + transições. Apresentação — sem regra nova.
 */
import { onMounted, ref } from 'vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { formatData } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default', middleware: 'auth' })

const STATUS = [
  { value: 0, label: 'Rascunho' },
  { value: 1, label: 'Confirmado' },
  { value: 2, label: 'Faturado' },
  { value: 3, label: 'Cancelado' },
  { value: 4, label: 'Estornado' }
]
function statusLabel(v: unknown) {
  return STATUS.find((s) => s.value === Number(v))?.label ?? String(v ?? '')
}

interface Expedicao {
  id: string
  pedidoId?: string | null
  documentoFiscalId?: string | null
  status?: number | null
  dataExpedicao?: string | null
  dataConfirmacao?: string | null
}
interface Filtros { status?: number | null }

const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
const lista = useApiList<Expedicao, Filtros>('/vendas/expedicoes', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Expedicao>[] = [
  { key: 'id', label: 'Expedição', width: '120px' },
  { key: 'pedidoId', label: 'Pedido' },
  { key: 'status', label: 'Status', formatter: statusLabel },
  { key: 'dataExpedicao', label: 'Expedição', formatter: formatData },
  { key: 'dataConfirmacao', label: 'Confirmação', formatter: formatData }
]
const camposFiltro: FilterField[] = [{ key: 'status', label: 'Status', type: 'select', options: STATUS }]

async function confirmar(e: Expedicao) {
  const ok = await confirmRef.value!.open('Confirmar expedição', 'Confirmar esta expedição?')
  if (!ok) return
  try {
    await useApi('/vendas/expedicoes/{id}/confirmar', { method: 'POST', params: { id: e.id } })
    toast.success('Expedição confirmada.')
    await lista.buscar()
  } catch (err) {
    toast.error(obterMensagemErro(err))
  }
}
async function cancelar(e: Expedicao) {
  const ok = await confirmRef.value!.open('Cancelar expedição', 'Cancelar esta expedição?')
  if (!ok) return
  try {
    await useApi('/vendas/expedicoes/{id}/cancelar', { method: 'POST', params: { id: e.id } })
    toast.success('Expedição cancelada.')
    await lista.buscar()
  } catch (err) {
    toast.error(obterMensagemErro(err))
  }
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Logística de saída" subtitle="Expedições, picking e entregas" :loading="lista.carregando.value" />

    <FilterBar
      :fields="camposFiltro"
      :model-value="lista.filtros.value"
      :loading="lista.carregando.value"
      @update:model-value="(v) => (lista.filtros.value = v as typeof lista.filtros.value)"
      @search="lista.aplicarFiltros($event as Partial<typeof lista.filtros.value>)"
      @clear="lista.limpar()"
    />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma expedição encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" :disabled="Number(row.status) !== 0" @click.stop="confirmar(row)">Confirmar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" :disabled="Number(row.status) >= 3" @click.stop="cancelar(row)">Cancelar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
