<script setup lang="ts">
/**
 * Contratos GCC — Gestão de Contratos de Compra (erp/compras/contratos-gcc).
 *
 * Camada de apresentação sobre `GccContratosCompraController` (`/api/v1/estoque-gcc-contratos`):
 *   - listagem paginada (envelope `dados = { total, pagina, itens }`) com filtro por situação;
 *   - criação de contrato (cabeçalho: fornecedor, número, vigência, valor);
 *   - abrir detalhe (vigência/aditivo/consumo/performance).
 *
 * Endpoints: estoque-gcc-contratos (GET, POST).
 */
import { onMounted, ref } from 'vue'
import { useApi, extrairDados } from '~/composables/useApi'
import { obterMensagemErro } from '~/composables/useApiList'
import { useToast } from '~/composables/useToast'
import { useHelper } from '~/composables/useHelper'
import DataTable, { type DataTableColumn } from '~/components/shared/DataTable.vue'
import PageToolbar from '~/components/shared/PageToolbar.vue'
import FilterBar, { type FilterField } from '~/components/shared/FilterBar.vue'
import AppDialog from '~/components/shared/AppDialog.vue'
import TextField from '~/components/shared/fields/TextField.vue'
import DateTimeField from '~/components/shared/fields/DateTimeField.vue'
import MoneyInput from '~/components/shared/fields/MoneyInput.vue'

definePageMeta({ middleware: 'auth', layout: 'default' })

/** Situação do contrato (ESituacaoContratoCompra). */
interface ContratoGccListagem {
  id: string
  fornecedorId: string | null
  numeroContrato: string | null
  vigenciaInicio: string | null
  vigenciaFim: string | null
  valorTotal: number | null
  situacao: number | string | null
  criadoEm: string | null
}

const SITUACAO_CONTRATO_GCC_OPTIONS = [
  { label: 'Rascunho', value: '0' },
  { label: 'Em Aprovação', value: '1' },
  { label: 'Aprovado', value: '2' },
  { label: 'Suspenso', value: '3' },
  { label: 'Encerrado', value: '4' },
  { label: 'Cancelado', value: '5' },
  { label: 'Expirado', value: '6' }
]

function situacaoContratoGccInfo(v: number | string | null): { texto: string; classe: string } {
  const mapa: Record<string, { texto: string; classe: string }> = {
    '0': { texto: 'Rascunho', classe: 'pendente' },
    '1': { texto: 'Em Aprovação', classe: 'pendente' },
    '2': { texto: 'Aprovado', classe: 'ok' },
    '3': { texto: 'Suspenso', classe: 'erro' },
    '4': { texto: 'Encerrado', classe: 'cancelado' },
    '5': { texto: 'Cancelado', classe: 'cancelado' },
    '6': { texto: 'Expirado', classe: 'erro' }
  }
  return mapa[String(v)] ?? { texto: String(v ?? '-'), classe: 'pendente' }
}

interface ContratosDados {
  total: number
  pagina: number
  itens: ContratoGccListagem[]
}

const toast = useToast()
const { formatarData, formatarMoeda } = useHelper()

const itens = ref<ContratoGccListagem[]>([])
const total = ref(0)
const pagina = ref(1)
const tamanhoPagina = ref(20)
const carregando = ref(false)
const filtroSituacao = ref<string>('')

const filtrosForm = ref<Record<string, unknown>>({ situacao: '' })

const camposFiltro: FilterField[] = [
  { key: 'situacao', label: 'Situação', type: 'select', options: SITUACAO_CONTRATO_GCC_OPTIONS, grow: true }
]

const colunas: DataTableColumn<ContratoGccListagem>[] = [
  { key: 'numeroContrato', label: 'Contrato', width: '140px', formatter: (v) => (v as string | null) ?? '-' },
  { key: 'fornecedorId', label: 'Fornecedor', formatter: (v) => (v ? `Forn. ${String(v).slice(0, 8)}` : '-') },
  { key: 'vigenciaInicio', label: 'Início', width: '110px', formatter: (v) => formatarData(v as string | null) },
  { key: 'vigenciaFim', label: 'Fim', width: '110px', formatter: (v) => formatarData(v as string | null) },
  { key: 'valorTotal', label: 'Valor', align: 'right', width: '140px', formatter: (v) => formatarMoeda(v as number | null) },
  { key: 'situacao', label: 'Situação', align: 'center', width: '130px' }
]

async function buscar(): Promise<void> {
  carregando.value = true
  try {
    const query: Record<string, unknown> = { pagina: pagina.value, tamanhoPagina: tamanhoPagina.value }
    if (filtroSituacao.value !== '') query.situacao = Number(filtroSituacao.value)
    const resposta = await useApi('/estoque-gcc-contratos', { query })
    const dados = extrairDados<ContratosDados>(resposta)
    itens.value = dados?.itens ?? []
    total.value = dados?.total ?? itens.value.length
  } catch (e) {
    itens.value = []
    total.value = 0
    toast.error(obterMensagemErro(e))
  } finally {
    carregando.value = false
  }
}

let debounceTimer: ReturnType<typeof setTimeout> | undefined
function aoMudarFiltros(valores: Record<string, unknown>) {
  filtrosForm.value = valores
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    filtroSituacao.value = (valores.situacao as string) ?? ''
    pagina.value = 1
    void buscar()
  }, 300)
}
function aoBuscar(valores: Record<string, unknown>) {
  if (debounceTimer) clearTimeout(debounceTimer)
  filtroSituacao.value = (valores.situacao as string) ?? ''
  pagina.value = 1
  void buscar()
}
function aoLimpar() {
  filtrosForm.value = { situacao: '' }
  filtroSituacao.value = ''
  pagina.value = 1
  void buscar()
}
function irParaPagina(p: number) {
  pagina.value = p
  void buscar()
}
function mudarTamanho(ps: number) {
  tamanhoPagina.value = ps
  pagina.value = 1
  void buscar()
}

function abrir(item: ContratoGccListagem) {
  navigateTo(`/erp/compras/contratos-gcc/${item.id}`)
}

// --- Novo contrato ---
const novoVisivel = ref(false)
const salvando = ref(false)
const novo = ref({ fornecedorId: '', numeroContrato: '', vigenciaInicio: '', vigenciaFim: '', valorTotal: null as number | null, observacao: '' })

function abrirNovo() {
  novo.value = { fornecedorId: '', numeroContrato: '', vigenciaInicio: '', vigenciaFim: '', valorTotal: null, observacao: '' }
  novoVisivel.value = true
}

async function salvarNovo() {
  if (!novo.value.fornecedorId.trim()) {
    toast.error('Informe o fornecedor (ID)')
    return
  }
  salvando.value = true
  try {
    await useApi('/estoque-gcc-contratos', {
      method: 'POST',
      body: {
        fornecedorId: novo.value.fornecedorId.trim(),
        numeroContrato: novo.value.numeroContrato.trim() || null,
        vigenciaInicio: novo.value.vigenciaInicio || null,
        vigenciaFim: novo.value.vigenciaFim || null,
        valorTotal: novo.value.valorTotal,
        observacao: novo.value.observacao.trim() || null,
        itens: []
      }
    })
    toast.success('Contrato criado')
    novoVisivel.value = false
    await buscar()
  } catch (e) {
    toast.error(obterMensagemErro(e))
  } finally {
    salvando.value = false
  }
}

onMounted(() => void buscar())
</script>

<template>
  <div>
    <PageToolbar title="Contratos GCC" subtitle="Gestão de contratos de compra (vigência, aditivo, consumo, performance)" :loading="carregando">
      <template #actions>
        <button type="button" class="btn btn-primary" @click="abrirNovo">+ Novo Contrato</button>
      </template>
    </PageToolbar>

    <FilterBar
      :fields="camposFiltro"
      :model-value="filtrosForm"
      :loading="carregando"
      @update:model-value="aoMudarFiltros"
      @search="aoBuscar"
      @clear="aoLimpar"
    />

    <DataTable
      :items="itens"
      :columns="colunas"
      :total="total"
      :page="pagina"
      :page-size="tamanhoPagina"
      :loading="carregando"
      empty-text="Nenhum contrato encontrado"
      @update:page="irParaPagina"
      @update:page-size="mudarTamanho"
      @row-click="abrir"
    >
      <template #cell-situacao="{ row }">
        <span class="badge" :class="`st-${situacaoContratoGccInfo(row.situacao).classe}`">
          {{ situacaoContratoGccInfo(row.situacao).texto }}
        </span>
      </template>
      <template #actions="{ row }">
        <button type="button" class="btn btn-ghost btn-sm" title="Abrir" @click.stop="abrir(row)">➔</button>
      </template>
    </DataTable>

    <AppDialog v-model="novoVisivel" title="Novo Contrato de Compra" width="560px" persistent>
      <div class="form-grid">
        <div class="col-12">
          <TextField v-model="novo.fornecedorId" label="Fornecedor (ID)" required placeholder="ID do fornecedor" />
        </div>
        <div class="col-6">
          <TextField v-model="novo.numeroContrato" label="Nº do contrato" />
        </div>
        <div class="col-6">
          <MoneyInput v-model="novo.valorTotal" label="Valor total" />
        </div>
        <div class="col-6">
          <DateTimeField v-model="novo.vigenciaInicio" label="Vigência início" />
        </div>
        <div class="col-6">
          <DateTimeField v-model="novo.vigenciaFim" label="Vigência fim" />
        </div>
        <div class="col-12">
          <TextField v-model="novo.observacao" label="Observação" />
        </div>
      </div>
      <template #footer>
        <button type="button" class="btn btn-secondary" :disabled="salvando" @click="novoVisivel = false">Cancelar</button>
        <button type="button" class="btn btn-primary" :disabled="salvando" @click="salvarNovo">
          <span v-if="salvando" class="spinner"></span>
          <span v-else>Criar</span>
        </button>
      </template>
    </AppDialog>
  </div>
</template>

<style scoped>
.badge.st-ok { background: rgba(16, 185, 129, 0.1); border: 1px solid rgba(16, 185, 129, 0.3); color: var(--success); }
.badge.st-erro { background: rgba(239, 68, 68, 0.1); border: 1px solid rgba(239, 68, 68, 0.3); color: var(--danger); }
.badge.st-pendente { background: rgba(245, 158, 11, 0.1); border: 1px solid rgba(245, 158, 11, 0.3); color: var(--warning); }
.badge.st-cancelado { background: rgba(113, 113, 122, 0.1); border: 1px solid rgba(113, 113, 122, 0.3); color: #a1a1aa; }
</style>
