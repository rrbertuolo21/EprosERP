<script setup lang="ts">
/**
 * Listagem de Inventários GEE — ESG / GHG / Inventários.
 *
 * Contrato real (EsgGhgController):
 *   GET  /esg/ghg/inventarios                     (lista)
 *   POST /esg/ghg/inventarios                     (criar → página de formulário)
 *   POST /esg/ghg/inventarios/{id}/submeter|aprovar|consolidar   (workflow → ações por linha)
 *   POST /esg/ghg/fontes                          (adicionar fonte → diálogo por linha)
 *   POST /esg/ghg/dados-atividade                 (registrar dado → diálogo)
 *   POST /esg/ghg/calcular                        (calcular emissão → diálogo)
 * Não há GET por id / PUT / DELETE.
 */
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApiList, obterMensagemErro } from '~/composables/useApiList'
import { useApi, extrairDados } from '~/composables/useApi'
import { useToast } from '~/composables/useToast'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import ConfirmDialog from '~/components/shared/ConfirmDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import SelectField from '~/components/shared/fields/SelectField.vue'
import QuantityInput from '~/components/shared/fields/QuantityInput.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import type { SelectOption } from '~/composables/useEnum'
import { statusEsgOptions, formatarStatusEsg, escopoGhgOptions, origemDadoGhgOptions } from '~/components/esg-comum/statusEsg'

definePageMeta({ layout: 'default' })

interface Inventario {
  id: string
  codigo?: string | null
  versao?: number | null
  periodoInicio?: string | null
  periodoFim?: string | null
  criterioFronteira?: string | null
  metodologia?: string | null
  status?: number | string | null
}

interface InventarioFiltros {
  busca?: string
  status?: number | null
}

const router = useRouter()
const toast = useToast()
const confirmRef = ref<InstanceType<typeof ConfirmDialog>>()

const lista = useApiList<Inventario, InventarioFiltros>('/esg/ghg/inventarios', {
  filtrosIniciais: { busca: '', status: null },
  tamanhoPaginaInicial: 25
})

const colunas: DataTableColumn<Inventario>[] = [
  { key: 'codigo', label: 'Código', sortable: true, width: '150px' },
  { key: 'versao', label: 'Versão', sortable: true, align: 'center', width: '90px' },
  { key: 'periodoInicio', label: 'Período início', sortable: true, width: '130px' },
  { key: 'periodoFim', label: 'Período fim', sortable: true, width: '130px' },
  { key: 'metodologia', label: 'Metodologia', sortable: false },
  { key: 'status', label: 'Status', sortable: true, align: 'center', width: '120px' }
]

const camposFiltro: FilterField[] = [
  { key: 'busca', label: 'Buscar', type: 'text', placeholder: 'Código ou metodologia...', grow: true },
  { key: 'status', label: 'Status', type: 'select', options: statusEsgOptions }
]

function formatarData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  return isNaN(d.getTime()) ? String(v) : d.toLocaleDateString('pt-BR')
}

function novo() {
  router.push('/erp/esg/ghg/inventarios/novo')
}

// --- Workflow (submeter / aprovar / consolidar) --------------------------------
async function acaoWorkflow(item: Inventario, acao: 'submeter' | 'aprovar' | 'consolidar') {
  const titulos: Record<string, string> = {
    submeter: 'Submeter inventário',
    aprovar: 'Aprovar inventário',
    consolidar: 'Consolidar inventário'
  }
  const ok = await confirmRef.value?.open(
    titulos[acao],
    `Confirma "${acao}" o inventário ${item.codigo ?? ''}?`,
    { textoConfirmar: 'Confirmar' }
  )
  if (!ok) return
  try {
    await useApi(`/esg/ghg/inventarios/{id}/${acao}`, { method: 'POST', params: { id: item.id } })
    toast.success('Ação executada com sucesso.')
    await lista.buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  }
}

// --- Diálogo: Nova fonte de emissão --------------------------------------------
const fonteVisivel = ref(false)
const salvandoFonte = ref(false)
const fonteForm = reactive({
  inventarioId: '' as string,
  codigo: null as string | null,
  descricao: null as string | null,
  escopo: null as number | null,
  categoria: null as string | null,
  unidadeOrganizacionalId: '' as string
})

function abrirFonte(item: Inventario) {
  fonteForm.inventarioId = item.id
  fonteForm.codigo = null
  fonteForm.descricao = null
  fonteForm.escopo = null
  fonteForm.categoria = null
  fonteForm.unidadeOrganizacionalId = ''
  fonteVisivel.value = true
}

async function salvarFonte() {
  if (!fonteForm.inventarioId || fonteForm.escopo == null || !fonteForm.unidadeOrganizacionalId) {
    toast.error('Preencha escopo e unidade organizacional.')
    return
  }
  salvandoFonte.value = true
  try {
    await useApi('/esg/ghg/fontes', { method: 'POST', body: fonteForm })
    toast.success('Fonte de emissão adicionada.')
    fonteVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoFonte.value = false
  }
}

// --- Diálogo: Registrar dado de atividade --------------------------------------
const dadoVisivel = ref(false)
const salvandoDado = ref(false)
const dadoForm = reactive({
  fonteEmissaoId: '' as string,
  dataReferencia: null as string | null,
  quantidade: null as number | null,
  unidade: null as string | null,
  origemDado: 'Manual' as string,
  referenciaOperacional: null as string | null
})

function abrirDado() {
  dadoForm.fonteEmissaoId = ''
  dadoForm.dataReferencia = null
  dadoForm.quantidade = null
  dadoForm.unidade = null
  dadoForm.origemDado = 'Manual'
  dadoForm.referenciaOperacional = null
  dadoVisivel.value = true
}

async function salvarDado() {
  if (!dadoForm.fonteEmissaoId || !dadoForm.dataReferencia || dadoForm.quantidade == null) {
    toast.error('Preencha fonte, data e quantidade.')
    return
  }
  salvandoDado.value = true
  try {
    await useApi('/esg/ghg/dados-atividade', { method: 'POST', body: dadoForm })
    toast.success('Dado de atividade registrado.')
    dadoVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoDado.value = false
  }
}

// --- Diálogo: Calcular emissão -------------------------------------------------
const calcVisivel = ref(false)
const salvandoCalc = ref(false)
const opcoesFatores = ref<SelectOption[]>([])
const calcForm = reactive({
  dadoAtividadeId: '' as string,
  fatorEmissaoId: '' as string | number,
  formulaVersao: null as string | null
})

async function abrirCalcular() {
  calcForm.dadoAtividadeId = ''
  calcForm.fatorEmissaoId = ''
  calcForm.formulaVersao = null
  calcVisivel.value = true
  if (opcoesFatores.value.length === 0) {
    try {
      const resp = await useApi('/esg/ghg/fatores')
      const arr = extrairDados<Array<{ id: string; codigo?: string; versao?: string }>>(resp) ?? []
      opcoesFatores.value = arr.map((f) => ({ label: `${f.codigo ?? f.id}${f.versao ? ' · ' + f.versao : ''}`, value: f.id }))
    } catch (e) {
      console.error('[inventarios] fatores', e)
    }
  }
}

async function salvarCalcular() {
  if (!calcForm.dadoAtividadeId || !calcForm.fatorEmissaoId) {
    toast.error('Informe o dado de atividade e o fator de emissão.')
    return
  }
  salvandoCalc.value = true
  try {
    await useApi('/esg/ghg/calcular', { method: 'POST', body: calcForm })
    toast.success('Emissão calculada com sucesso.')
    calcVisivel.value = false
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvandoCalc.value = false
  }
}

onMounted(() => {
  void lista.buscar()
})
</script>

<template>
  <div>
    <PageToolbar title="Inventários GEE" subtitle="Inventários de gases de efeito estufa por período e fronteira" :loading="lista.carregando.value">
      <template #actions>
        <button type="button" class="btn btn-secondary" @click="abrirDado">Dado de atividade</button>
        <button type="button" class="btn btn-secondary" @click="abrirCalcular">Calcular emissão</button>
        <button type="button" class="btn btn-primary" @click="novo">+ Novo inventário</button>
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
      empty-text="Nenhum inventário encontrado. Crie um novo inventário para começar."
      @update:page="lista.irParaPagina($event)"
      @update:page-size="lista.buscar({ tamanhoPagina: $event, pagina: 1 })"
      @update:sort="lista.buscar({ ordenacao: $event })"
    >
      <template #cell-periodoInicio="{ value }">{{ formatarData(value) }}</template>
      <template #cell-periodoFim="{ value }">{{ formatarData(value) }}</template>
      <template #cell-status="{ value }">
        <span class="badge">{{ formatarStatusEsg(value) }}</span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Adicionar fonte" @click.stop="abrirFonte(row)">Fonte+</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Submeter" @click.stop="acaoWorkflow(row, 'submeter')">Submeter</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Aprovar" @click.stop="acaoWorkflow(row, 'aprovar')">Aprovar</button>
        <button type="button" class="btn btn-ghost btn-sm" title="Consolidar" @click.stop="acaoWorkflow(row, 'consolidar')">Consolidar</button>
      </template>
    </DataTable>

    <ConfirmDialog ref="confirmRef" />

    <!-- Diálogo: Nova fonte -->
    <AppDialog v-model="fonteVisivel" title="Nova fonte de emissão" width="560px">
      <div class="dialog-grid">
        <TextField v-model="fonteForm.codigo" label="Código" />
        <SelectField v-model="fonteForm.escopo" label="Escopo" required :options="escopoGhgOptions" />
        <TextField v-model="fonteForm.descricao" label="Descrição" />
        <TextField v-model="fonteForm.categoria" label="Categoria" />
        <!-- TODO: unidadeOrganizacionalId é uuid; sem endpoint de listagem no módulo, mantido como texto. -->
        <TextField v-model="fonteForm.unidadeOrganizacionalId" label="Unidade organizacional (UUID)" required />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="fonteVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoFonte" @click="salvarFonte">
          <span v-if="salvandoFonte" class="spinner"></span><span v-else>Adicionar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo: Dado de atividade -->
    <AppDialog v-model="dadoVisivel" title="Registrar dado de atividade" width="560px">
      <div class="dialog-grid">
        <!-- TODO: fonteEmissaoId é uuid; a listagem de fontes por inventário não é exposta pela API. -->
        <TextField v-model="dadoForm.fonteEmissaoId" label="Fonte de emissão (UUID)" required />
        <DateTimeField v-model="dadoForm.dataReferencia" label="Data de referência" required />
        <QuantityInput v-model="dadoForm.quantidade" label="Quantidade" required />
        <TextField v-model="dadoForm.unidade" label="Unidade" />
        <SelectField v-model="dadoForm.origemDado" label="Origem do dado" :options="origemDadoGhgOptions" :clearable="false" />
        <TextField v-model="dadoForm.referenciaOperacional" label="Referência operacional" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="dadoVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoDado" @click="salvarDado">
          <span v-if="salvandoDado" class="spinner"></span><span v-else>Registrar</span>
        </button>
      </template>
    </AppDialog>

    <!-- Diálogo: Calcular emissão -->
    <AppDialog v-model="calcVisivel" title="Calcular emissão" width="560px">
      <div class="dialog-grid">
        <!-- TODO: dadoAtividadeId é uuid; a listagem de dados de atividade não é exposta pela API. -->
        <TextField v-model="calcForm.dadoAtividadeId" label="Dado de atividade (UUID)" required />
        <SelectField v-model="calcForm.fatorEmissaoId" label="Fator de emissão" required :options="opcoesFatores" />
        <TextField v-model="calcForm.formulaVersao" label="Versão da fórmula" />
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" @click="calcVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvandoCalc" @click="salvarCalcular">
          <span v-if="salvandoCalc" class="spinner"></span><span v-else>Calcular</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.dialog-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 14px; }
</style>
