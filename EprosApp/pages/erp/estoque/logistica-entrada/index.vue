<script setup lang="ts">
/**
 * Logística de Entrada — recebimento físico vinculado a compras (LogisticaEntradaController).
 *
 * Contrato (rota `api/v1/logistica-entrada`):
 *   GET  /logistica-entrada?compraId&fornecedorId&situacao&pagina&tamanhoPagina
 *   GET  /logistica-entrada/{id}
 *   POST /logistica-entrada/{id}/confirmar | /cancelar | /estornar
 */
import { ref, onMounted } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import type { SelectOption } from '~/composables/useEnum'

definePageMeta({ layout: 'default' })

const toast = useToast()
const { formatarData } = useHelper()

interface Entrada {
  id: string
  numero?: string | number | null
  compraId?: string | null
  fornecedorId?: string | null
  fornecedorNome?: string | null
  situacao: number
  dataEntrada?: string | null
  localEntrega?: string | null
}

const SITUACAO: Record<number, string> = { 0: 'Rascunho', 1: 'Confirmado', 2: 'Faturado', 3: 'Cancelado', 4: 'Estornado' }

const itens = ref<Entrada[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)
const filtroSituacao = ref<number | null>(null)
const processando = ref<string | null>(null)

const colunas: DataTableColumn<Entrada>[] = [
  { key: 'numero', label: 'Nº', sortable: false, width: '90px' },
  { key: 'fornecedorNome', label: 'Fornecedor', sortable: false },
  { key: 'localEntrega', label: 'Local de entrega', sortable: false },
  { key: 'dataEntrada', label: 'Data', sortable: false, width: '120px', align: 'center' },
  { key: 'situacao', label: 'Situação', sortable: false, width: '140px', align: 'center' }
]
const opcoesSituacao: SelectOption[] = Object.entries(SITUACAO).map(([v, label]) => ({ label, value: Number(v) }))

async function buscar() {
  carregando.value = true
  try {
    const query: Record<string, unknown> = { pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    if (filtroSituacao.value !== null) query.situacao = filtroSituacao.value
    const resp = await useApi('/logistica-entrada', { query })
    const d = extrairDados<{ itens?: Entrada[]; total?: number }>(resp)
    itens.value = Array.isArray(d) ? (d as Entrada[]) : d?.itens ?? []
    total.value = (d && !Array.isArray(d) ? d.total : undefined) ?? itens.value.length
  } catch (e) {
    toast.error(obterMensagemErro(e))
    itens.value = []; total.value = 0
  } finally {
    carregando.value = false
  }
}

async function acao(item: Entrada, verbo: 'confirmar' | 'cancelar' | 'estornar') {
  processando.value = item.id
  try {
    const body = verbo === 'confirmar' ? undefined : { id: item.id, motivo: '' }
    await useApi(`/logistica-entrada/{id}/${verbo}`, { method: 'POST', params: { id: item.id }, body })
    toast.success(`Entrada ${verbo === 'confirmar' ? 'confirmada' : verbo === 'cancelar' ? 'cancelada' : 'estornada'}.`)
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    processando.value = null
  }
}

onMounted(buscar)
</script>

<template>
  <div>
    <PageToolbar title="Logística de Entrada" subtitle="Recebimento físico de mercadorias vinculado às compras" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-ghost" @click="buscar">Atualizar</button>
      </template>
    </PageToolbar>

    <div class="filtros">
      <SelectField v-model="filtroSituacao" :options="opcoesSituacao" label="Situação" placeholder="Todas" @update:model-value="pagina = 1; buscar()" />
    </div>

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="total"
      :page="pagina"
      :page-size="tamanhoPagina"
      :loading="carregando"
      empty-text="Nenhuma entrada registrada."
      @update:page="pagina = $event; buscar()"
      @update:page-size="tamanhoPagina = $event; pagina = 1; buscar()"
    >
      <template #cell-numero="{ value }">{{ value ?? '—' }}</template>
      <template #cell-fornecedorNome="{ row }">{{ row.fornecedorNome ?? (row.fornecedorId ? String(row.fornecedorId).slice(0, 8) : '—') }}</template>
      <template #cell-localEntrega="{ value }">{{ value ?? '—' }}</template>
      <template #cell-dataEntrada="{ value }">{{ value ? formatarData(String(value)) : '—' }}</template>
      <template #cell-situacao="{ value }">
        <span class="badge" :class="Number(value) === 1 ? 'badge-success' : Number(value) === 0 ? 'badge-secondary' : 'badge-warning'">
          {{ SITUACAO[Number(value)] ?? value }}
        </span>
      </template>
      <template #actions="{ row }">
        <button v-if="row.situacao === 0" type="button" class="btn btn-ghost btn-sm" :disabled="processando === row.id" @click.stop="acao(row, 'confirmar')">Confirmar</button>
        <button v-if="row.situacao === 0 || row.situacao === 1" type="button" class="btn btn-ghost btn-sm btn-danger-action" :disabled="processando === row.id" @click.stop="acao(row, 'cancelar')">Cancelar</button>
        <button v-if="row.situacao === 1" type="button" class="btn btn-ghost btn-sm" :disabled="processando === row.id" @click.stop="acao(row, 'estornar')">Estornar</button>
      </template>
    </DataTable>
  </div>
</template>

<style scoped>
.filtros { display: flex; gap: 1rem; margin-bottom: 1rem; max-width: 320px; }
</style>
