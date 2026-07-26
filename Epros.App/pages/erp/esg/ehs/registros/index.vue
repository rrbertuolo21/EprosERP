<script setup lang="ts">
/**
 * Listagem de Registros EHS — ESG / EHS / Registros.
 *
 * Contrato real (EsgEhsController):
 *   GET  /esg/ehs/registros       (lista)
 *   POST /esg/ehs/registros       (criar → formulário)
 *   POST /esg/ehs/atividades      (atividade ocupacional → diálogo por registro)
 *   POST /esg/ehs/residuos        (movimento de resíduo → diálogo por registro)
 *   POST /esg/ehs/fatores-risco   (fator de risco → diálogo)
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

interface Registro {
  id: string
  codigo?: string | null
  descricao?: string | null
  tipo?: string | null
  status?: number | string | null
}

interface RegistroFiltros {
  busca?: string
  status?: number | null
}

const router = useRouter()
const toast = useToast()

const lista = useApiList<Registro, RegistroFiltros>('/esg/ehs/registros', {
  filtrosIniciais: { busca: '', status: null },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Registro>[] = [
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
  router.push('/erp/esg/ehs/registros/novo')
}

// --- Diálogo: Atividade ocupacional --------------------------------------------
const ativVisivel = ref(false)
const salvandoAtiv = ref(false)
const ativForm = reactive({
  registroEhsId: '' as string,
  idFolhaPpp: null as string | null,
  dataInicio: null as string | null,
  dataFim: null as string | null,
  descricao: null as string | null
})

function abrirAtividade(item: Registro) {
  ativForm.registroEhsId = item.id
  ativForm.idFolhaPpp = null
  ativForm.dataInicio = null
  ativForm.dataFim = null
  ativForm.descricao = null
  ativVisivel.value = true
}

async function salvarAtividade() {
  salvandoAtiv.value = true
  try {
    await useApi('/esg/ehs/atividades', { method: 'POST', body: ativForm })
    toast.success('Atividade ocupacional registrada.')
    ativVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoAtiv.value = false
  }
}

// --- Diálogo: Movimento de resíduo ---------------------------------------------
const resVisivel = ref(false)
const salvandoRes = ref(false)
const resForm = reactive({
  registroEhsId: '' as string,
  tipoResiduo: null as string | null,
  classificacao: null as string | null,
  origem: null as string | null,
  quantidade: null as number | null,
  unidade: null as string | null,
  data: null as string | null,
  localId: null as string | null,
  tipoMovimento: null as string | null,
  destinoId: null as string | null,
  evidenciaArquivoId: null as string | null
})

function abrirResiduo(item: Registro) {
  resForm.registroEhsId = item.id
  resForm.tipoResiduo = null
  resForm.classificacao = null
  resForm.origem = null
  resForm.quantidade = null
  resForm.unidade = null
  resForm.data = null
  resForm.localId = null
  resForm.tipoMovimento = null
  resForm.destinoId = null
  resForm.evidenciaArquivoId = null
  resVisivel.value = true
}

async function salvarResiduo() {
  if (resForm.quantidade == null || !resForm.data) {
    toast.error('Informe quantidade e data.')
    return
  }
  salvandoRes.value = true
  try {
    await useApi('/esg/ehs/residuos', { method: 'POST', body: resForm })
    toast.success('Movimento de resíduo registrado.')
    resVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoRes.value = false
  }
}

// --- Diálogo: Fator de risco ---------------------------------------------------
const fatorVisivel = ref(false)
const salvandoFator = ref(false)
const fatorForm = reactive({
  atividadeId: null as string | null,
  idFolhaPpp: null as string | null,
  dataInicio: null as string | null,
  dataFim: null as string | null,
  tipo: null as string | null,
  fatorRiscoDescricao: null as string | null,
  intensidade: null as string | null,
  tecnicaUtilizada: null as string | null,
  epcEficaz: null as string | null,
  epiEficaz: null as string | null,
  caEpi: null as string | null
})

function abrirFator() {
  Object.assign(fatorForm, {
    atividadeId: null, idFolhaPpp: null, dataInicio: null, dataFim: null, tipo: null,
    fatorRiscoDescricao: null, intensidade: null, tecnicaUtilizada: null,
    epcEficaz: null, epiEficaz: null, caEpi: null
  })
  fatorVisivel.value = true
}

async function salvarFator() {
  salvandoFator.value = true
  try {
    await useApi('/esg/ehs/fatores-risco', { method: 'POST', body: fatorForm })
    toast.success('Fator de risco registrado.')
    fatorVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoFator.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Registros EHS" subtitle="Registros de gestão ambiental, saúde e segurança do trabalho" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="abrirFator">Fator de risco</button>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo registro</button>
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
      empty-text="Nenhum registro EHS encontrado. Crie um novo registro para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-status="{ value }">
        <span class="badge">{{ formatarStatusEsg(value) }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Atividade ocupacional" @click.stop="abrirAtividade(row)">Atividade</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Movimento de resíduo" @click.stop="abrirResiduo(row)">Resíduo</button>
      </template>
    </DataTable>

    <!-- Diálogo: Atividade ocupacional -->
    <AppDialog v-model="ativVisivel" title="Registrar atividade ocupacional" width="560px">
      <div class="dialog-grid">
        <DateTimeField v-model="ativForm.dataInicio" label="Data início" mode="datetime" />
        <DateTimeField v-model="ativForm.dataFim" label="Data fim" mode="datetime" />
        <TextField v-model="ativForm.descricao" label="Descrição" />
        <!-- TODO: idFolhaPpp é uuid; sem endpoint de listagem no módulo, mantido como texto. -->
        <TextField v-model="ativForm.idFolhaPpp" label="Folha PPP (UUID)" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="ativVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoAtiv" @click="salvarAtividade">
          <span v-if="salvandoAtiv" class="spinner"></span><span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo: Movimento de resíduo -->
    <AppDialog v-model="resVisivel" title="Registrar movimento de resíduo" width="640px">
      <div class="dialog-grid">
        <TextField v-model="resForm.tipoResiduo" label="Tipo de resíduo" />
        <TextField v-model="resForm.classificacao" label="Classificação" />
        <TextField v-model="resForm.origem" label="Origem" />
        <QuantityInput v-model="resForm.quantidade" label="Quantidade" required />
        <TextField v-model="resForm.unidade" label="Unidade" />
        <DateTimeField v-model="resForm.data" label="Data" required />
        <TextField v-model="resForm.tipoMovimento" label="Tipo de movimento" placeholder="Ex.: Entrada, Saída" />
        <!-- TODO: localId / destinoId / evidenciaArquivoId são uuid; sem endpoint de listagem, mantidos como texto. -->
        <TextField v-model="resForm.localId" label="Local (UUID)" />
        <TextField v-model="resForm.destinoId" label="Destino (UUID)" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="resVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoRes" @click="salvarResiduo">
          <span v-if="salvandoRes" class="spinner"></span><span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo: Fator de risco -->
    <AppDialog v-model="fatorVisivel" title="Registrar fator de risco" width="640px">
      <div class="dialog-grid">
        <TextField v-model="fatorForm.tipo" label="Tipo" />
        <TextField v-model="fatorForm.fatorRiscoDescricao" label="Descrição do fator" />
        <TextField v-model="fatorForm.intensidade" label="Intensidade" />
        <TextField v-model="fatorForm.tecnicaUtilizada" label="Técnica utilizada" />
        <TextField v-model="fatorForm.epcEficaz" label="EPC eficaz" />
        <TextField v-model="fatorForm.epiEficaz" label="EPI eficaz" />
        <TextField v-model="fatorForm.caEpi" label="CA do EPI" />
        <DateTimeField v-model="fatorForm.dataInicio" label="Data início" mode="datetime" />
        <DateTimeField v-model="fatorForm.dataFim" label="Data fim" mode="datetime" />
        <!-- TODO: atividadeId / idFolhaPpp são uuid; sem endpoint de listagem, mantidos como texto. -->
        <TextField v-model="fatorForm.atividadeId" label="Atividade (UUID)" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="fatorVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoFator" @click="salvarFator">
          <span v-if="salvandoFator" class="spinner"></span><span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 14px; }
</style>
