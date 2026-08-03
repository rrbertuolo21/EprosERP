<script setup lang="ts">
/**
 * Gestão de Serviços — Faturas de serviço.
 * Contrato real: base `/vendas/servicos`.
 *   GET faturas · POST faturas/{id}/confirmar|faturar|cancelar|estornar.
 * A criação exige linhas/impostos (fluxo dedicado) — aqui é lista + transições de estado.
 * Apresentação — sem regra nova.
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

const STATUS = ['Rascunho', 'Confirmado', 'Faturado', 'Cancelado', 'Estornado']
function statusLabel(v: unknown) {
  const n = Number(v)
  return STATUS[n] ?? String(v ?? '')
}

interface Fatura {
  id: string
  numeroFatura?: string | number | null
  clienteId?: string | null
  dataFatura?: string | null
  status?: number | null
  totalGeral?: number | null
  valorPago?: number | null
  saldoDevido?: number | null
}

const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()
const lista = useApiList<Fatura>('/vendas/servicos/faturas', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Fatura>[] = [
  { key: 'numeroFatura', label: 'Nº' },
  { key: 'dataFatura', label: 'Data', formatter: formatData },
  { key: 'status', label: 'Status', formatter: statusLabel },
  { key: 'totalGeral', label: 'Total', align: 'right', formatter: formatMoeda },
  { key: 'valorPago', label: 'Pago', align: 'right', formatter: formatMoeda },
  { key: 'saldoDevido', label: 'Saldo', align: 'right', formatter: formatMoeda }
]

async function acao(f: Fatura, rota: string, titulo: string, msg: string) {
  const ok = await confirmRef.value!.open(titulo, msg)
  if (!ok) return
  try {
    await useApi(`/vendas/servicos/faturas/{id}/${rota}`, { method: 'POST', params: { id: f.id } })
    toast.success('Operação concluída.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Faturas de serviço" subtitle="Gestão de Serviços — faturamento" :loading="lista.carregando.value" />

    <DataTable
      :items="lista.itens.value"
      :columns="colunas"
      :total="lista.total.value"
      :page="lista.pagina.value"
      :page-size="lista.tamanhoPagina.value"
      :loading="lista.carregando.value"
      :sort="lista.ordenacao.value"
      empty-text="Nenhuma fatura encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" :disabled="Number(row.status) !== 0" @click.stop="acao(row, 'confirmar', 'Confirmar fatura', 'Confirmar esta fatura?')">Confirmar</button>
        <button type="button" class="btn btn-ghost btn-sm" :disabled="Number(row.status) !== 1" @click.stop="acao(row, 'faturar', 'Faturar', 'Faturar esta fatura?')">Faturar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" :disabled="Number(row.status) >= 3" @click.stop="acao(row, 'cancelar', 'Cancelar fatura', 'Cancelar esta fatura?')">Cancelar</button>
        <button type="button" class="btn btn-ghost btn-sm" :disabled="Number(row.status) !== 2" @click.stop="acao(row, 'estornar', 'Estornar', 'Estornar esta fatura?')">Estornar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>
