<script setup lang="ts">
/**
 * Itens de um Relatório ESG — ESG / Relatórios / {id} / Itens.
 *
 * Contrato real (EsgRelController):
 *   GET  /esg/relatorios/{relatorioId}/itens   (lista de itens do relatório)
 *   POST /esg/relatorios/indicadores           (vincular indicador a um item → diálogo)
 *   POST /esg/relatorios/snapshots             (capturar snapshot de indicador → diálogo)
 * Tela de leitura dos itens + ações de vínculo/snapshot.
 */
import { onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi, extrairDados, extrairLista} from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import type { SelectOption } from '~/composables/useEnum'
import SelectField from '~/components/shared/fields/SelectField.vue'

definePageMeta({ layout: 'default' })

interface ItemRelatorio {
  id: string
  sequencia?: number | null
  tipoConteudo?: string | null
  quantidade?: number | null
  statusPreenchimento?: string | null
  observacao?: string | null
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const relatorioId = route.params.id as string
const carregando = ref(false)
const itens = ref<ItemRelatorio[]>([])
const opcoesItens = ref<SelectOption[]>([])

const colunas: DataTableColumn<ItemRelatorio>[] = [
  { key: 'sequencia', label: 'Seq.', sortable: false, width: '80px', align: 'center' },
  { key: 'tipoConteudo', label: 'Tipo de conteúdo', sortable: false },
  { key: 'quantidade', label: 'Quantidade', sortable: false, align: 'right' },
  { key: 'statusPreenchimento', label: 'Preenchimento', sortable: false, align: 'center', width: '150px' },
  { key: 'observacao', label: 'Observação', sortable: false }
]

async function carregar() {
  carregando.value = true
  try {
    const resp = await useApi('/esg/relatorios/{relatorioId}/itens', { params: { relatorioId } })
    itens.value = extrairLista<ItemRelatorio>(resp) ?? []
    opcoesItens.value = itens.value.map((i) => ({
      label: `#${i.sequencia ?? '?'} · ${i.tipoConteudo ?? i.id}`,
      value: i.id
    }))
  } catch (e) {
    toast.error(obterMensagemErro(e))
    itens.value = []
  } finally {
    carregando.value = false
  }
}

function voltar() {
  router.push('/erp/esg/relatorios')
}

// --- Diálogo: Vincular indicador -----------------------------------------------
const indVisivel = ref(false)
const salvandoInd = ref(false)
const indForm = reactive({
  itemId: '' as string | number,
  origemDominio: null as string | null,
  origemEntidade: null as string | null,
  origemId: null as string | null,
  codigoIndicador: null as string | null,
  regraComposicao: null as string | null
})

function abrirIndicador() {
  Object.assign(indForm, {
    itemId: '', origemDominio: null, origemEntidade: null, origemId: null,
    codigoIndicador: null, regraComposicao: null
  })
  indVisivel.value = true
}

async function salvarIndicador() {
  if (!indForm.itemId) {
    toast.error('Selecione o item.')
    return
  }
  salvandoInd.value = true
  try {
    await useApi('/esg/relatorios/indicadores', { method: 'POST', body: indForm })
    toast.success('Indicador vinculado.')
    indVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoInd.value = false
  }
}

// --- Diálogo: Capturar snapshot ------------------------------------------------
const snapVisivel = ref(false)
const salvandoSnap = ref(false)
const snapForm = reactive({
  indicadorReferenciaId: '' as string,
  origemVersao: null as string | null,
  dataCorte: null as string | null,
  valorNumerico: null as number | null,
  valorTexto: null as string | null,
  unidade: null as string | null,
  dimensoes: null as string | null,
  statusOrigem: null as string | null
})

function abrirSnapshot() {
  Object.assign(snapForm, {
    indicadorReferenciaId: '', origemVersao: null, dataCorte: null, valorNumerico: null,
    valorTexto: null, unidade: null, dimensoes: null, statusOrigem: null
  })
  snapVisivel.value = true
}

async function salvarSnapshot() {
  if (!snapForm.indicadorReferenciaId || !snapForm.dataCorte) {
    toast.error('Informe o indicador de referência e a data de corte.')
    return
  }
  salvandoSnap.value = true
  try {
    await useApi('/esg/relatorios/snapshots', { method: 'POST', body: snapForm })
    toast.success('Snapshot capturado.')
    snapVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoSnap.value = false
  }
}

onMounted(() => {
  void carregar()
})
</script>

<template>
  <div>
    <PageToolbar title="Itens do relatório" subtitle="Itens, indicadores e snapshots do relatório ESG" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="voltar">Voltar</button>
        <button type="button" class="btn btn-secondary" @click="abrirSnapshot">Capturar snapshot</button>
        <button type="button" class="btn btn-primary" @click="abrirIndicador">Vincular indicador</button>
      </template>
    </PageToolbar>

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="itens.length"
      :page="1"
      :page-size="itens.length || 1"
      :loading="carregando"
      empty-text="Nenhum item neste relatório."
    >
      <template #cell-quantidade="{ value }">
        <span v-if="value != null">{{ Number(value).toLocaleString('pt-BR') }}</span>
      </template>
      <template #cell-statusPreenchimento="{ value }">
        <span class="badge">{{ value }}</span>
      </template>
    </DataTable>

    <!-- Diálogo: Vincular indicador -->
    <AppDialog v-model="indVisivel" title="Vincular indicador de referência" width="560px">
      <div class="dialog-grid">
        <SelectField v-model="indForm.itemId" label="Item" required :options="opcoesItens" />
        <TextField v-model="indForm.codigoIndicador" label="Código do indicador" />
        <TextField v-model="indForm.origemDominio" label="Domínio de origem" placeholder="Ex.: GHG, EHS" />
        <TextField v-model="indForm.origemEntidade" label="Entidade de origem" />
        <TextField v-model="indForm.origemId" label="ID de origem" />
        <TextField v-model="indForm.regraComposicao" label="Regra de composição" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="indVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoInd" @click="salvarIndicador">
          <span v-if="salvandoInd" class="spinner"></span><span v-else>Vincular</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo: Capturar snapshot -->
    <AppDialog v-model="snapVisivel" title="Capturar snapshot de indicador" width="560px">
      <div class="dialog-grid">
        <!-- TODO: indicadorReferenciaId é uuid; a listagem de indicadores de referência não é exposta pela API. -->
        <TextField v-model="snapForm.indicadorReferenciaId" label="Indicador de referência (UUID)" required />
        <DateTimeField v-model="snapForm.dataCorte" label="Data de corte" mode="datetime" required />
        <QuantityInput v-model="snapForm.valorNumerico" label="Valor numérico" />
        <TextField v-model="snapForm.valorTexto" label="Valor texto" />
        <TextField v-model="snapForm.unidade" label="Unidade" />
        <TextField v-model="snapForm.origemVersao" label="Versão de origem" />
        <TextField v-model="snapForm.dimensoes" label="Dimensões" />
        <TextField v-model="snapForm.statusOrigem" label="Status de origem" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="snapVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoSnap" @click="salvarSnapshot">
          <span v-if="salvandoSnap" class="spinner"></span><span v-else>Capturar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 14px; }
</style>
