<script setup lang="ts">
/**
 * Listagem de Fluxos Circulares — ESG / ECO / Fluxos.
 *
 * Contrato real (EsgEcoController):
 *   GET  /esg/eco/fluxos      (lista)
 *   POST /esg/eco/fluxos      (criar → formulário)
 *   POST /esg/eco/triagens    (triagem → diálogo por fluxo)
 *   POST /esg/eco/metas       (meta → diálogo por fluxo)
 *   POST /esg/eco/medicoes    (medição → diálogo por fluxo)
 *   POST /esg/eco/destinos    (destino → diálogo)
 * Sem GET por id / PUT / DELETE.
 */
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import { statusEsgOptions, formatarStatusEsg } from '~/components/esg-comum/statusEsg'

definePageMeta({ layout: 'default' })

interface Fluxo {
  id: string
  codigo?: string | null
  descricao?: string | null
  tipo?: string | null
  status?: number | string | null
}

interface FluxoFiltros {
  busca?: string
  status?: number | null
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Fluxo, FluxoFiltros>('/esg/eco/fluxos', {
  filtrosIniciais: { busca: '', status: null },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Fluxo>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '150px' },
  { key: 'descricao', label: 'Descrição', sortable: true },
  { key: 'tipo', label: 'Tipo', sortable: true, width: '150px' },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '120px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Código ou descrição...', grow: true },
  { key: 'status', label: 'Status', type: 'select', options: statusEsgOptions }
]

function novo() {
  router.push('/erp/esg/eco/fluxos/novo')
}

// --- Diálogo: Triagem ----------------------------------------------------------
const triaVisivel = ref(false)
const salvandoTria = ref(false)
const triaForm = reactive({
  fluxoId: '' as string,
  itemDevolucaoId: null as string | null,
  quantidadeRecebida: null as number | null,
  unidade: null as string | null,
  condicao: null as string | null,
  destinoProposto: null as string | null,
  motivo: null as string | null,
  responsavelId: '' as string
})

function abrirTriagem(item: Fluxo) {
  Object.assign(triaForm, {
    fluxoId: item.id, itemDevolucaoId: null, quantidadeRecebida: null, unidade: null,
    condicao: null, destinoProposto: null, motivo: null, responsavelId: ''
  })
  triaVisivel.value = true
}

async function salvarTriagem() {
  if (triaForm.quantidadeRecebida == null || !triaForm.responsavelId) {
    toast.error('Informe a quantidade recebida e o responsável.')
    return
  }
  salvandoTria.value = true
  try {
    await useApi('/esg/eco/triagens', { method: 'POST', body: triaForm })
    toast.success('Triagem registrada.')
    triaVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoTria.value = false
  }
}

// --- Diálogo: Meta -------------------------------------------------------------
const metaVisivel = ref(false)
const salvandoMeta = ref(false)
const metaForm = reactive({
  fluxoId: '' as string,
  tipoIndicador: null as string | null,
  periodoInicio: null as string | null,
  periodoFim: null as string | null,
  valorMeta: null as number | null,
  unidade: null as string | null,
  formula: null as string | null,
  responsavelId: '' as string
})

function abrirMeta(item: Fluxo) {
  Object.assign(metaForm, {
    fluxoId: item.id, tipoIndicador: null, periodoInicio: null, periodoFim: null,
    valorMeta: null, unidade: null, formula: null, responsavelId: ''
  })
  metaVisivel.value = true
}

async function salvarMeta() {
  if (!metaForm.periodoInicio || !metaForm.periodoFim || metaForm.valorMeta == null || !metaForm.responsavelId) {
    toast.error('Preencha período, valor da meta e responsável.')
    return
  }
  salvandoMeta.value = true
  try {
    await useApi('/esg/eco/metas', { method: 'POST', body: metaForm })
    toast.success('Meta definida.')
    metaVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoMeta.value = false
  }
}

// --- Diálogo: Medição ----------------------------------------------------------
const medVisivel = ref(false)
const salvandoMed = ref(false)
const medForm = reactive({
  fluxoId: '' as string,
  tipoIndicador: null as string | null,
  periodo: null as string | null,
  numerador: null as number | null,
  denominador: null as number | null,
  unidade: null as string | null,
  fonte: null as string | null
})

function abrirMedicao(item: Fluxo) {
  Object.assign(medForm, {
    fluxoId: item.id, tipoIndicador: null, periodo: null, numerador: null,
    denominador: null, unidade: null, fonte: null
  })
  medVisivel.value = true
}

async function salvarMedicao() {
  salvandoMed.value = true
  try {
    await useApi('/esg/eco/medicoes', { method: 'POST', body: medForm })
    toast.success('Medição registrada.')
    medVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoMed.value = false
  }
}

// --- Diálogo: Destino ----------------------------------------------------------
const destVisivel = ref(false)
const salvandoDest = ref(false)
const destForm = reactive({
  triagemId: '' as string,
  tipoDestino: null as string | null,
  quantidade: null as number | null,
  unidade: null as string | null,
  dataExecucao: null as string | null,
  responsavelId: '' as string,
  evidenciaArquivoId: null as string | null,
  observacao: null as string | null
})

function abrirDestino() {
  Object.assign(destForm, {
    triagemId: '', tipoDestino: null, quantidade: null, unidade: null,
    dataExecucao: null, responsavelId: '', evidenciaArquivoId: null, observacao: null
  })
  destVisivel.value = true
}

async function salvarDestino() {
  if (!destForm.triagemId || destForm.quantidade == null || !destForm.dataExecucao || !destForm.responsavelId) {
    toast.error('Preencha triagem, quantidade, data de execução e responsável.')
    return
  }
  salvandoDest.value = true
  try {
    await useApi('/esg/eco/destinos', { method: 'POST', body: destForm })
    toast.success('Destino registrado.')
    destVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoDest.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Fluxos Circulares" subtitle="Fluxos de economia circular: triagem, metas, medições e destinação" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="abrirDestino">Destino</button>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo fluxo</button>
      </template>
    </PageToolbar>

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
      empty-text="Nenhum fluxo circular encontrado. Crie um novo fluxo para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-status="{ value }">
        <span class="badge">{{ formatarStatusEsg(value) }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Triagem" @click.stop="abrirTriagem(row)">Triagem</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Meta" @click.stop="abrirMeta(row)">Meta</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Medição" @click.stop="abrirMedicao(row)">Medição</button>
      </template>
    </DataTable>

    <!-- Diálogo: Triagem -->
    <AppDialog v-model="triaVisivel" title="Registrar triagem" width="560px">
      <div class="dialog-grid">
        <QuantityInput v-model="triaForm.quantidadeRecebida" label="Quantidade recebida" required />
        <TextField v-model="triaForm.unidade" label="Unidade" />
        <TextField v-model="triaForm.condicao" label="Condição" />
        <TextField v-model="triaForm.destinoProposto" label="Destino proposto" />
        <TextField v-model="triaForm.motivo" label="Motivo" />
        <!-- TODO: itemDevolucaoId / responsavelId são uuid; sem endpoint de listagem, texto. -->
        <TextField v-model="triaForm.responsavelId" label="Responsável (UUID)" required />
        <TextField v-model="triaForm.itemDevolucaoId" label="Item de devolução (UUID)" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="triaVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoTria" @click="salvarTriagem">
          <span v-if="salvandoTria" class="spinner"></span><span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo: Meta -->
    <AppDialog v-model="metaVisivel" title="Definir meta" width="560px">
      <div class="dialog-grid">
        <TextField v-model="metaForm.tipoIndicador" label="Tipo de indicador" />
        <DateTimeField v-model="metaForm.periodoInicio" label="Período início" required />
        <DateTimeField v-model="metaForm.periodoFim" label="Período fim" required />
        <QuantityInput v-model="metaForm.valorMeta" label="Valor da meta" required />
        <TextField v-model="metaForm.unidade" label="Unidade" />
        <TextField v-model="metaForm.formula" label="Fórmula" />
        <!-- TODO: responsavelId é uuid; sem endpoint de listagem, texto. -->
        <TextField v-model="metaForm.responsavelId" label="Responsável (UUID)" required />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="metaVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoMeta" @click="salvarMeta">
          <span v-if="salvandoMeta" class="spinner"></span><span v-else>Definir</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo: Medição -->
    <AppDialog v-model="medVisivel" title="Registrar medição" width="560px">
      <div class="dialog-grid">
        <TextField v-model="medForm.tipoIndicador" label="Tipo de indicador" />
        <TextField v-model="medForm.periodo" label="Período" placeholder="Ex.: 2026-Q1" />
        <QuantityInput v-model="medForm.numerador" label="Numerador" />
        <QuantityInput v-model="medForm.denominador" label="Denominador" />
        <TextField v-model="medForm.unidade" label="Unidade" />
        <TextField v-model="medForm.fonte" label="Fonte" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="medVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoMed" @click="salvarMedicao">
          <span v-if="salvandoMed" class="spinner"></span><span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo: Destino -->
    <AppDialog v-model="destVisivel" title="Registrar destino" width="560px">
      <div class="dialog-grid">
        <!-- TODO: triagemId é uuid; a listagem de triagens não é exposta pela API. -->
        <TextField v-model="destForm.triagemId" label="Triagem (UUID)" required />
        <TextField v-model="destForm.tipoDestino" label="Tipo de destino" placeholder="Ex.: Reciclagem, Aterro" />
        <QuantityInput v-model="destForm.quantidade" label="Quantidade" required />
        <TextField v-model="destForm.unidade" label="Unidade" />
        <DateTimeField v-model="destForm.dataExecucao" label="Data de execução" mode="datetime" required />
        <TextField v-model="destForm.observacao" label="Observação" />
        <!-- TODO: responsavelId / evidenciaArquivoId são uuid; sem endpoint de listagem, texto. -->
        <TextField v-model="destForm.responsavelId" label="Responsável (UUID)" required />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="destVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoDest" @click="salvarDestino">
          <span v-if="salvandoDest" class="spinner"></span><span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 14px; }
</style>
