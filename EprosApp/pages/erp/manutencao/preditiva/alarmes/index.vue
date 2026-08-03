<script setup lang="ts">
/**
 * Listagem de Alarmes Preditivos — Manutenção / Preditiva / Alarmes.
 * Fonte: GET /manutencao/preditiva/alarmes. Ações por linha:
 *  - Converter em ordem: POST /preditiva/alarmes/{alarmeId}/converter-ordem
 *  - Descartar: POST /preditiva/alarmes/{alarmeId}/descartar
 */
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '~/composables/useApi'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'

definePageMeta({ layout: 'default' })

interface Alarme {
  id: string
  severidade?: string | null
  descricao?: string | null
  status?: number | null
  dataHoraDisparo?: string | null
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Alarme>('/manutencao/preditiva/alarmes', { tamanhoPaginaInicial: 20 })

const colunas: DataTableColumn<Alarme>[] = [
  { key: 'severidade', label: 'Severidade', sortable: true, width: '140px' },
  { key: 'descricao', label: 'Descrição', sortable: false },
  { key: 'dataHoraDisparo', label: 'Disparo', sortable: true, width: '160px' }
]

function formatarDataHora(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleString('pt-BR')
}

function novo() {
  router.push('/erp/manutencao/preditiva/alarmes/novo')
}

// ---- Converter em ordem ----
const converterVisivel = ref(false)
const convertendo = ref(false)
const alarmeAtual = ref<Alarme | null>(null)
const formConverter = reactive({ ordemTrabalhoId: '', statusRetorno: '', payloadRetorno: '' })

function abrirConverter(item: Alarme) {
  alarmeAtual.value = item
  Object.assign(formConverter, { ordemTrabalhoId: '', statusRetorno: '', payloadRetorno: '' })
  converterVisivel.value = true
}
async function confirmarConverter() {
  if (!alarmeAtual.value) return
  if (!formConverter.ordemTrabalhoId) {
    toast.error('Informe a ordem de trabalho.')
    return
  }
  convertendo.value = true
  try {
    await useApi(`/manutencao/preditiva/alarmes/${alarmeAtual.value.id}/converter-ordem`, {
      method: 'POST',
      body: {
        alarmeId: alarmeAtual.value.id, ordemTrabalhoId: formConverter.ordemTrabalhoId,
        statusRetorno: formConverter.statusRetorno || null, payloadRetorno: formConverter.payloadRetorno || null
      }
    })
    toast.success('Alarme convertido em ordem.')
    converterVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    convertendo.value = false
  }
}

// ---- Descartar ----
const descartarVisivel = ref(false)
const descartando = ref(false)
const motivoDescarte = ref('')
function abrirDescartar(item: Alarme) {
  alarmeAtual.value = item
  motivoDescarte.value = ''
  descartarVisivel.value = true
}
async function confirmarDescartar() {
  if (!alarmeAtual.value) return
  descartando.value = true
  try {
    await useApi(`/manutencao/preditiva/alarmes/${alarmeAtual.value.id}/descartar`, {
      method: 'POST',
      body: { alarmeId: alarmeAtual.value.id, motivo: motivoDescarte.value || null }
    })
    toast.success('Alarme descartado.')
    descartarVisivel.value = false
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    descartando.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Alarmes preditivos" subtitle="Alarmes disparados por regras de monitoramento" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo alarme</button>
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
      empty-text="Nenhum alarme encontrado."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-dataHoraDisparo="{ value }"><span>{{ formatarDataHora(value) }}</span></template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Converter em ordem" @click.stop="abrirConverter(row)">Converter</button>
        <button type="button" class="btn btn-ghost btn-sm btn-danger-action" title="Descartar" @click.stop="abrirDescartar(row)">Descartar</button>
      </template>
    </DataTable>

    <AppDialog v-model="converterVisivel" title="Converter alarme em ordem" width="520px" persistent>
      <div class="dialog-form">
        <!-- TODO: ordemTrabalhoId sem endpoint de listagem no módulo — texto até integração. -->
        <TextField v-model="formConverter.ordemTrabalhoId" label="Ordem de trabalho (ID)" placeholder="UUID" required />
        <TextField v-model="formConverter.statusRetorno" label="Status de retorno" />
        <TextField v-model="formConverter.payloadRetorno" label="Payload de retorno" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="convertendo" @click="converterVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="convertendo" @click="confirmarConverter">Converter</button>
      </template>
    </AppDialog>

    <AppDialog v-model="descartarVisivel" title="Descartar alarme" width="440px" persistent>
      <TextField v-model="motivoDescarte" label="Motivo" placeholder="Descreva o motivo" />
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="descartando" @click="descartarVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-danger" :disabled="descartando" @click="confirmarDescartar">Descartar</button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-form { display: flex; flex-direction: column; gap: 16px; }
</style>
