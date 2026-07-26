<script setup lang="ts">
/**
 * Listagem de Paradas — Manutenção / Paradas.
 * Fonte: GET /manutencao/paradas. Ação por linha: finalizar (POST /paradas/{id}/finalizar).
 */
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import { tipoParadaOpcoes, rotuloStatusRegistro } from '~/components/manutencao-shared/opcoes'

definePageMeta({ layout: 'default' })

interface Parada {
  id: string
  codigo?: string | null
  descricao?: string | null
  status?: number | null
  tipoParada?: number | null
  dataHoraInicio?: string | null
  dataHoraFim?: string | null
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Parada>('/manutencao/paradas', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Parada>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '120px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'tipoParada', label: 'Tipo', sortable: true, width: '140px' },
  { key: 'dataHoraInicio', label: 'Início', sortable: true, width: '160px' },
  { key: 'dataHoraFim', label: 'Fim', sortable: false, width: '160px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '130px' }
]

function rotuloTipo(v: unknown): string {
  return tipoParadaOpcoes.find((o) => o.value === Number(v))?.label ?? ''
}
function formatarDataHora(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleString('pt-BR')
}

function novo() {
  router.push('/erp/manutencao/paradas/novo')
}
function abrir(item: Parada) {
  router.push(`/erp/manutencao/paradas/${item.id}`)
}
function irMotivos() {
  router.push('/erp/manutencao/paradas/motivos')
}

// ---- Finalizar ----
const finalizarVisivel = ref(false)
const finalizando = ref(false)
const paradaAtual = ref<Parada | null>(null)
const formFinalizar = reactive<{ dataHoraFim: string | null }>({ dataHoraFim: null })

function abrirFinalizar(item: Parada) {
  paradaAtual.value = item
  formFinalizar.dataHoraFim = null
  finalizarVisivel.value = true
}

async function confirmarFinalizar() {
  if (!paradaAtual.value) return
  if (!formFinalizar.dataHoraFim) {
    toast.error('Informe a data/hora de fim.')
    return
  }
  finalizando.value = true
  try {
    await useApi(`/manutencao/paradas/${paradaAtual.value.id}/finalizar`, {
      method: 'POST',
      body: { paradaId: paradaAtual.value.id, dataHoraFim: formFinalizar.dataHoraFim }
    })
    toast.success('Parada finalizada.')
    finalizarVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    finalizando.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Paradas" subtitle="Registro e finalização de paradas de equipamentos e linhas" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="irMotivos">Motivos de parada</button>
        <button type="button" class="btn btn-primary" @click="novo">+ Nova parada</button>
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
      empty-text="Nenhuma parada encontrada."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
      @row-click="abrir"
    >
      <template #cell-tipoParada="{ value }"><span>{{ rotuloTipo(value) }}</span></template>
      <template #cell-dataHoraInicio="{ value }"><span>{{ formatarDataHora(value) }}</span></template>
      <template #cell-dataHoraFim="{ value }"><span>{{ formatarDataHora(value) }}</span></template>
      <template #cell-status="{ value }"><span class="badge badge-info">{{ rotuloStatusRegistro(value) }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Finalizar" @click.stop="abrirFinalizar(row)">Finalizar</button>
      </template>
    </DataTable>

    <AppDialog v-model="finalizarVisivel" title="Finalizar parada" width="440px" persistent>
      <DateTimeField v-model="formFinalizar.dataHoraFim" label="Data/hora de fim" mode="datetime" required />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="finalizando" @click="finalizarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="finalizando" @click="confirmarFinalizar">
          <span v-if="finalizando" class="spinner"></span>
          <span v-else>Finalizar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>
