<script setup lang="ts">
/**
 * CRM Comercial — Oportunidades.
 * Contrato real: base `/vendas/crm`.
 *   GET  oportunidades?status=&etapaId=&clientePrincipalId=&pagina=&tamanhoPagina=
 *   POST oportunidades/{id}/ganhar
 *   POST oportunidades/{id}/perder      (PerderCrmOportunidadeCommand: { oportunidadeId, motivoPerda })
 * A criação exige PipelineId+EtapaId (setup de funil sem endpoint de listagem) — ver relatório.
 * Apresentação — sem regra nova.
 */
import { onMounted, reactive, ref } from 'vue'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import { formatMoeda, formatData } from '~/components/concessionarias-shared/formatadores'

definePageMeta({ layout: 'default', middleware: 'auth' })

const STATUS = [
  { value: 0, label: 'Ativa' },
  { value: 1, label: 'Ganha' },
  { value: 2, label: 'Perdida' },
  { value: 3, label: 'Arquivada' }
]
function statusLabel(v: unknown) {
  return STATUS.find((s) => s.value === Number(v))?.label ?? ''
}

interface Oportunidade {
  id: string
  nome?: string | null
  valor?: number | null
  status?: number | null
  dataFechamentoPrevista?: string | null
}
interface Filtros { status?: number | null }

const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const lista = useApiList<Oportunidade, Filtros>('/vendas/crm/oportunidades', {
  filtrosIniciais: { status: null },
  tamanhoPaginaInicial: 20
})

const colunas: DataTableColumn<Oportunidade>[] = [
  { key: 'nome', label: 'Oportunidade' },
  { key: 'valor', label: 'Valor', align: 'right', formatter: formatMoeda },
  { key: 'dataFechamentoPrevista', label: 'Fechamento previsto', formatter: formatData },
  { key: 'status', label: 'Status', formatter: statusLabel }
]
const camposFiltro: FilterField[] = [{ key: 'status', label: 'Status', type: 'select', options: STATUS }]

async function ganhar(o: Oportunidade) {
  const ok = await confirmRef.value!.open('Ganhar oportunidade', `Marcar "${o.nome ?? ''}" como ganha?`)
  if (!ok) return
  try {
    await useApi('/vendas/crm/oportunidades/{id}/ganhar', { method: 'POST', params: { id: o.id } })
    toast.success('Oportunidade ganha.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

// Perder (com motivo)
const dlgPerder = ref(false)
const perdendo = ref(false)
const alvo = ref<Oportunidade | null>(null)
const motivo = reactive({ texto: '' })

function abrirPerder(o: Oportunidade) {
  alvo.value = o
  motivo.texto = ''
  dlgPerder.value = true
}
async function confirmarPerder() {
  if (!alvo.value) return
  perdendo.value = true
  try {
    await useApi('/vendas/crm/oportunidades/{id}/perder', {
      method: 'POST',
      params: { id: alvo.value.id },
      body: { oportunidadeId: alvo.value.id, motivoPerda: motivo.texto || null }
    })
    toast.success('Oportunidade marcada como perdida.')
    dlgPerder.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    perdendo.value = false
  }
}

onMounted(() => void lista.buscar())
</script>

<template>
  <div>
    <PageToolbar title="Oportunidades" subtitle="CRM Comercial — pipeline de vendas" :loading="lista.carregando.value" />

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
      empty-text="Nenhuma oportunidade encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" :disabled="Number(row.status) !== 0" @click.stop="ganhar(row)">Ganhar</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" :disabled="Number(row.status) !== 0" @click.stop="abrirPerder(row)">Perder</button>
      </template>
    </DataTable>

    <AppDialog v-model="dlgPerder" title="Perder oportunidade" width="480px" persistent>
      <div class="form-grid">
        <TextField v-model="motivo.texto" label="Motivo da perda" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="perdendo" @click="dlgPerder = false">Cancelar</button>
        <button type="button" class="btn btn-danger" :disabled="perdendo" @click="confirmarPerder">
          <span v-if="perdendo" class="spinner"></span><span v-else>Confirmar</span>
        </button>
      </template>
    </AppDialog>

    <ConfirmDialog ref="confirmRef" />
  </div>
</template>

<style scoped>
.form-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; }
</style>
